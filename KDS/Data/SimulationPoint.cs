/*
 *    KDS .NET - A KDS algorithm simulator for .NET
 *
 *    (C) 2021, LaBRI - Laboratoire Bordelais de Recherche en Informatique
 *                      (Bordeaux's Computer Science Research Laboratory)
 *    (C) 2021, Gustave Monce
 *
 *    This library is free software; you can redistribute it and/or
 *    modify it under the terms of the GNU Lesser General Public
 *    License as published by the Free Software Foundation;
 *    version 2.1 of the License.
 *
 *    This library is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *    Lesser General Public License for more details.
 */
using KDS.Certificates;
using KDS.Interfaces;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#nullable enable

namespace KDS
{
    /// <summary>
    /// Simple comparer for the certificates in the simulation, required by the skip list object used to store certificates
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    internal class SimulationCertificateComparer<TNode> : IComparer<ISimulationCertificate<TNode>> where TNode : INode, new()
    {
        public int Compare(ISimulationCertificate<TNode>? x, ISimulationCertificate<TNode>? y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            return x.GetHashCode().CompareTo(y.GetHashCode());
        }
    }

    /// <summary>
    /// This class represents a point in the simulation, with the given TNode data structure as well as position data, messages and certificates
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public partial class SimulationPoint<TNode> where TNode : INode, new()
    {
        /// <summary>
        /// The current state of the simulator (time, end, start etc)
        /// </summary>
        private readonly SimulatorState SimulatorState;

        /// <summary>
        /// The queue for the messages
        /// </summary>
        internal HashSet<SimulationPointMessage<TNode>> MessageQueue { get; } = new();

        /// <summary>
        /// The list of currently valid certificates concerning this point
        /// </summary>
        public DataStructures.SkipList<ISimulationCertificate<TNode>> Certificates { get; } = new(new SimulationCertificateComparer<TNode>());

        /// <summary>
        /// The list of currently removed certificates, this is being kept track for counting internal/external events.
        /// </summary>
        public DataStructures.SkipList<ISimulationCertificate<TNode>> RemovedCertificatesList { get; } = new(new SimulationCertificateComparer<TNode>());

        /// <summary>
        /// The number of messages recieved so far by this point
        /// </summary>
        public uint ReceivedMessages { get; private set; } = 0;

        /// <summary>
        /// The number of messages sent so far by this point
        /// </summary>
        public uint SentMessages { get; private set; } = 0;

        /// <summary>
        /// The number of internal events concerning this point
        /// </summary>
        public uint InternalEvents { get; internal set; } = 0;

        /// <summary>
        /// The number of external events concerning this point
        /// </summary>
        public uint ExternalEvents { get; internal set; } = 0;

        /// <summary>
        /// The unique identifier of the point
        /// </summary>
        public uint Identifier { get; internal set; } = 0;

        /// <summary>
        /// Keeps track of the changes in the data structure
        /// </summary>
        internal bool Changed { get; set; } = false;

        /// <summary>
        /// Number of certificates removed
        /// </summary>
        internal uint RemovedCertificates { get; set; } = 0;

        /// <summary>
        /// Lock mutex for message queue
        /// </summary>
        private readonly Mutex mutex = new();

        /// <summary>
        /// The axises for this point (X, Y, Z and beyond if needed)
        /// </summary>
        public SimulationPointAxis[] Axis { get; }

        public TNode Node { get; } = new();

        /// <summary>
        /// The file holding the recorded trace data for this point. Might be empty if no file is being used.
        /// </summary>
        public string DataFile { get; set; } = "";

        /// <summary>
        /// The arguments for PredictionChanged
        /// </summary>
        /// <param name="sender">The sender of this event</param>
        /// <param name="YPol">The current predicted Y axis trajectory</param>
        internal delegate void PositionChangedEventArgs(SimulationPoint<TNode> sender, double[] x, double CurrentTime);

        /// <summary>
        /// Fires whenever the position changed
        /// </summary>
        internal event PositionChangedEventArgs? PositionChanged;

        /// <summary>
        /// The number of times the polynomial object got recomputed in the simulation
        /// </summary>
        public ulong RecomputedPolynomialCount { get; internal set; } = 0;

        /// <summary>
        /// The arguments for PredictionChanged
        /// </summary>
        /// <param name="sender">The sender of this event</param>
        /// <param name="YPol">The current predicted Y axis trajectory</param>
        internal delegate void PredictionChangedEventArgs(SimulationPoint<TNode> sender, Polynomial[] Pol, double CurrentTime);

        /// <summary>
        /// Fires whenever the prediction polynomials have changed
        /// </summary>
        internal event PredictionChangedEventArgs? PredictionChanged;

        /// <summary>
        /// Constructor of the simulation point
        /// </summary>
        /// <param name="Simulator">The state object of the simulation</param>
        internal SimulationPoint(SimulatorState Simulator)
        {
            this.SimulatorState = Simulator;

            Node.SetAttachedSimulationPoint(this);

            List<SimulationPointAxis> AxisList = new();
            for (int i = 0; i < Simulator.AxisCount; i++)
            {
                AxisList.Add(new SimulationPointAxis(Simulator));
            }

            Axis = AxisList.ToArray();
        }
        
        public void SetPolynomialPosition(Polynomial[] polynomials)
        {
            for (int i = 0; i < Axis.Length; i++)
            {
                SimulationPointAxis? axis = Axis[i];
                axis.Pol = polynomials[i];
            }

            RecomputedPolynomialCount++;
            PredictionChanged?.Invoke(this, Axis.Select(x => x.Pol!).ToArray(), SimulatorState.CurrentTime);
        }

        /// <summary>
        /// Records the last position into the structure. Recomputes predictions if required.
        /// </summary>
        /// <param name="x"></param>
        public void AddLastPosition(double[] x, double t)
        {
            for (int i = 0; i < Axis.Length; i++)
            {
                SimulationPointAxis? axis = Axis[i];
                axis.AddLastPosition(x[i], t);
            }

            if (SimulatorState.EnablePredictions)
            {
                // Recomputed predicted trajectory
                if (Axis.Any(x => x.PolPredicted == null) || !IsPolynomialTrajectoryCorrect())
                {
                    foreach (SimulationPointAxis? axis in Axis)
                    {
                        axis.PolPredicted = PredictedPolynomialTrajectory(axis.GetOrderedLastPositions(), axis.GetOrderedLastTimes());
                    }

                    if (ArePredictionsAvailable())
                    {
                        RecomputedPolynomialCount++;
                        PredictionChanged?.Invoke(this, Axis.Select(x => x.PolPredicted!).ToArray(), SimulatorState.CurrentTime);
                    }
                }
            }

            PositionChanged?.Invoke(this, x, t);
        }

        /// <summary>
        /// Checks if predictions are currently available
        /// </summary>
        /// <returns></returns>
        internal bool ArePredictionsAvailable()
        {
            return !Axis.Any(x => x.PolPredicted == null);
        }

        /// <summary>
        /// Gets a predicted trajectory polynomial for the given last position array.
        /// </summary>
        /// <param name="LastPositions"></param>
        /// <returns></returns>
        private static Polynomial? PredictedPolynomialTrajectory(double?[] LastPositions, double?[] LastTimes)
        {
            if (LastPositions.Any(x => x == null))
            {
                return null;
            }

            if (LastTimes.Any(x => x == null))
            {
                return null;
            }

            return Polynomial.Fit(LastTimes.Select(x => x!.Value).ToArray(), LastPositions.Select(x => x!.Value).ToArray(), LastPositions.Length - 1);
        }

        /// <summary>
        /// Checks if the predicted polynomial trajectories are currently correct given a constant threshold
        /// </summary>
        /// <param name="polY"></param>
        /// <returns></returns>
        private bool IsPolynomialTrajectoryCorrect()
        {
            foreach (SimulationPointAxis? axis in Axis)
            {
                if (axis.PolPredicted == null || Math.Abs(axis.Predicted!.Value - axis.Static) > SimulatorState.TrajectoryPredictionEpsillon)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Remove a certificate concerning this point
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveCertificate(ISimulationCertificate<TNode> item)
        {
            if (!Certificates.Contains(item))
            {
                return false;
            }

            RemovedCertificatesList.Add(item);
            Certificates.Remove(item);
            RemovedCertificates++;

            return true;
        }

        /// <summary>
        /// Send a message from a point (sender) to a point (recipient)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recipient"></param>
        /// <param name="type"></param>
        /// <param name="point"></param>
        public static void SendMessage(SimulationPoint<TNode> sender, SimulationPoint<TNode> recipient, int type, SimulationPoint<TNode> point)
        {
            sender.SentMessages++;
            recipient.mutex.WaitOne();
            recipient.MessageQueue.Add(new() { Type = type, Point = point });
            recipient.mutex.ReleaseMutex();
            //Console.WriteLine($"[{sender.Identifier}] Sent {type} message to {Identifier}.");
        }

        /// <summary>
        /// Send a message to this point
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="type"></param>
        /// <param name="point"></param>
        public void SendMessage(SimulationPoint<TNode> sender, int type, SimulationPoint<TNode> point)
        {
            SendMessage(sender, this, type, point);
        }

        /// <summary>
        /// Gets the list of messages recieved so far, when this function gets called, messages get removed from the queue permanently
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public SimulationPoint<TNode>[] ReceiveMessages(int type)
        {
            mutex.WaitOne();
            SimulationPoint<TNode>[] points = MessageQueue.Where(x => x != null && x.Type == type).Select(x => x.Point).ToArray();
            MessageQueue.RemoveWhere(x => x?.Type == type);
            //if (points.Count() > 0)
            //    Console.WriteLine($"[{Identifier}] Got {points.Count()} {type} messages.");
            mutex.ReleaseMutex();
            ReceivedMessages += (uint)points.Length;
            return points;
        }

        /// <summary>
        /// Get the list of failed certificates right now
        /// </summary>
        /// <returns></returns>
        public ISimulationCertificate<TNode>[] GetFailedCertificates()
        {
            // Get failed certificates
            ISimulationCertificate<TNode>[]? Fail = Certificates.Where(x => x?.EvaluateValidity(SimulatorState.CurrentTime) == false).ToArray();
            foreach (ISimulationCertificate<TNode>? item in Fail)
            {
                RemovedCertificatesList.Add(item);
            }
            return Fail;
        }

        /// <summary>
        /// This functions executes the algorithm code
        /// </summary>
        /// <param name="i"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        internal bool RunAlgorithm(int i, IAlgorithmCode<TNode> code)
        {
            // First gets the list of failed certificates
            ISimulationCertificate<TNode>[]? Fail = GetFailedCertificates();
            if (Fail == null || Fail.Length == 0)
            {
                // If none are found, no point in running the code
                return false;
            }

            // Clear all communication messages at the beginning
            if (i == 0)
            {
                MessageQueue.Clear();
            }

            // Run the algorithm
            code.RunAlgorithmAfterAllPointsMovedPerPoint(i, Fail, this, SimulatorState.CurrentTime);

            return true;
        }

        /// <summary>
        /// Rebuilds certificates for this point
        /// </summary>
        /// <param name="regenerateCertificates"></param>
        internal void RebuildCertificates(ICertificateGenerator<TNode> regenerateCertificates)
        {
            // Call the rebuild function implemented by the user
            regenerateCertificates.RebuildCertificates(this, Node, SimulatorState.CurrentTime);
        }
    }
}
