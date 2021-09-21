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
using KDS.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace KDS
{
    /// <summary>
    /// The current state of the simulation
    /// </summary>
    internal class SimulatorState
    {
        /// <summary>
        /// The beginning of time
        /// </summary>
        internal double StartTime { get; set; } = 0;

        /// <summary>
        /// The current time
        /// </summary>
        internal double CurrentTime { get; set; } = 0;

        /// <summary>
        /// The end of time
        /// </summary>
        internal double EndTime { get; set; } = 1800;

        /// <summary>
        /// The step at which the simulation progresses
        /// </summary>
        internal double TimeStep { get; set; } = 1;

        /// <summary>
        /// The Epsillon variable so that the prediction minus the real position is less or equal than epsillon
        /// </summary>
        internal double TrajectoryPredictionEpsillon { get; set; } = 20;

        /// <summary>
        /// The history size for prediction functionality, the higher, the finer the prediction gets,
        /// but also the more often it gets recomputed
        /// </summary>
        internal int TrajectoryPredictionHistorySize { get; set; } = 3;

        /// <summary>
        /// The number of axises present in the simulation
        /// </summary>
        internal int AxisCount { get; set; } = 2;

        /// <summary>
        /// Are trajectory predictions enabled in the simulation?
        /// </summary>
        internal bool EnablePredictions { get; set; } = false;
    }

    public class Simulator<TNode> where TNode : INode, new()
    {
        internal readonly SimulatorState State = new();

        public delegate void SimulationPointsChangedEventDelegate(double CurrentTime, IEnumerable<SimulationPoint<TNode>> Points, IEnumerable<SimulationPoint<TNode>> ChangedPoints);
        public event SimulationPointsChangedEventDelegate? SimulationPointsChanged;

        public delegate void SimulationTickEventDelegate(double CurrentTime, IEnumerable<SimulationPoint<TNode>> Points);
        public event SimulationTickEventDelegate? SimulationTick;

        public Simulator(
            double StartTime = 0,
            double EndTime = 1800,
            double TimeStep = 1,
            double TrajectoryPredictionEpsillon = 20,
            int TrajectoryPredictionHistorySize = 3,
            int AxisCount = 2,
            bool EnablePredictions = false
        )
        {
            State.StartTime = StartTime;
            State.EndTime = EndTime;
            State.TimeStep = TimeStep;
            State.TrajectoryPredictionEpsillon = TrajectoryPredictionEpsillon;
            State.TrajectoryPredictionHistorySize = TrajectoryPredictionHistorySize;
            State.AxisCount = AxisCount;
            State.EnablePredictions = EnablePredictions;
        }

        /// <summary>
        /// The list of points in the simulation
        /// </summary>
        internal HashSet<SimulationPoint<TNode>> Points = new();

        /// <summary>
        /// Draws a nice progress bar string
        /// </summary>
        /// <param name="perc">Percentage for the progress bar</param>
        /// <returns></returns>
        private static string GetProgressBarString(int perc)
        {
            if (perc < 0)
            {
                perc = 0;
            }

            if (perc > 100)
            {
                perc = 100;
            }

            int eqsLength = (int)((double)perc / 100 * 55);
            string bases = new string('=', eqsLength) + new string(' ', 55 - eqsLength);
            bases = bases.Insert(28, perc + "%");
            if (perc == 100)
            {
                bases = bases[1..];
            }
            else if (perc < 10)
            {
                bases = bases.Insert(28, " ");
            }

            return "[" + bases + "]";
        }

        /// <summary>
        /// This function sets the next current time in the simulation
        /// </summary>
        internal void SetCurrentTime()
        {
            // If we contain only points, or have a point with no prediction available, do not use failuretime
            bool AllPointsHavePredictions = Points.All(x => x.ArePredictionsAvailable());
            bool NoPointsAreStaticallyDefined = !Points.Any(x => x.Axis.Any(y => y.IsStaticallyDefinedPoint));

            bool UseFailureTime = AllPointsHavePredictions || NoPointsAreStaticallyDefined;

            if (UseFailureTime)
            {
                // We need to get the list of certificates whose failure time is higher than the current time
                IEnumerable<Certificates.ISimulationCertificate<TNode>>? globalCertificateList = Points.SelectMany(x => x.Certificates).Where(x => x.GetFailureTimeAtCreation() > State.CurrentTime).OrderBy(x => x.GetFailureTimeAtCreation());

                // If we found no certificate, this is the end.
                if (!globalCertificateList.Any())
                {
                    State.CurrentTime = State.EndTime;
                }
                // Otherwise, given this is meant to be ordered by failure time, get the first one, this will be our new current failure time.
                else
                {
                    State.CurrentTime = globalCertificateList.First().GetFailureTimeAtCreation()!.Value;
                }
            }
            else
            {
                // Move by the defined step given failuretime is unavailable.
                State.CurrentTime += State.TimeStep;
            }

            Console.Title = $"{GetProgressBarString((int)Math.Round(State.CurrentTime / State.EndTime * 100d))} - {State.CurrentTime}/{State.EndTime}";
        }

        private bool HasRegenerateCertificateForAllPoints = true;
        private bool HasRegenerateCertificateForSinglePoint = true;

        /// <summary>
        /// This function performs general initialization for the points, initial data structure values, initial certificates
        /// </summary>
        /// <param name="nodeInitializer">The interface implementing NodeInitializer</param>
        /// <param name="regenerateCertificates">The interface implementing RegenerateCertificates</param>
        /// <param name="DataFiles">The list of data files</param>
        /// <param name="moveAction">The function that will move the points at a given time</param>
        /// <returns></returns>
        internal async Task InitializePointsAsync(
            INodeInitializer<TNode> nodeInitializer,
            ICertificateGenerator<TNode>? regenerateCertificates,
            IEnumerable<string> DataFiles,
            Func<IEnumerable<SimulationPoint<TNode>>, double, double?, Task> moveAction)
        {
            int i = 0;
            foreach (string? file in DataFiles)
            {
                SimulationPoint<TNode> data = new(State) { Identifier = (uint)i };
                data.DataFile = file;
                Points.Add(data);
                i++;
            }

            await MovePoints(Points, State.CurrentTime, moveAction);

            nodeInitializer.ComputeNodeValues(Points);

            await RebuildCertificatesAsync(regenerateCertificates);
        }

        /// <summary>
        /// Rebuilds all certificates in the simulation
        /// </summary>
        /// <param name="regenerateCertificates"></param>
        /// <returns></returns>
        private async Task RebuildCertificatesAsync(ICertificateGenerator<TNode>? regenerateCertificates)
        {
            if (regenerateCertificates != null && HasRegenerateCertificateForAllPoints)
            {
                try
                {
                    regenerateCertificates.RebuildCertificates(Points, State.CurrentTime);
                }
                catch (NotImplementedException) { HasRegenerateCertificateForAllPoints = false; }
            }

            if (regenerateCertificates != null && HasRegenerateCertificateForSinglePoint)
            {
                await Parallel2.ForEachAsync(Points, (x, y) => new ValueTask(Task.Run(() =>
                {
                    try
                    {
                        x.RebuildCertificates(regenerateCertificates);
                    }
                    catch (NotImplementedException) { HasRegenerateCertificateForSinglePoint = false; }
                }, y)));
            }
        }

        /// <summary>
        /// This function starts the simulation
        /// </summary>
        /// <param name="algorithmCode">The algorithm code to run when certificates fail</param>
        /// <param name="nodeInitializer">The interface that will initialize the data structure</param>
        /// <param name="regenerateCertificates">The interface that will regenerate certificates</param>
        /// <param name="DataFiles">The list of data files to forward to the SimulationPoint<typeparamref name="TNode"/> class</param>
        /// <param name="moveAction">The function designed to move points at a given time</param>
        /// <returns></returns>
        public async Task StartSimulationAsync(IAlgorithmCode<TNode> algorithmCode,
            INodeInitializer<TNode> nodeInitializer,
            ICertificateGenerator<TNode>? regenerateCertificates,
            IAudit<TNode>? audit,
            IEnumerable<string> DataFiles,
            Func<IEnumerable<SimulationPoint<TNode>>, double, double?, Task> moveAction)
        {
            await InitializePointsAsync(nodeInitializer, regenerateCertificates, DataFiles, moveAction);

            foreach (SimulationPoint<TNode>? Point in Points)
            {
                Point.PositionChanged += (SimulationPoint<TNode> _, double[] __, double ___) => SimulatorRunLocalized(algorithmCode);
            }

            while (State.CurrentTime < State.EndTime)
            {
                SetCurrentTime();

                bool NoPointsAreStaticallyDefined = !Points.Any(x => x.Axis.Any(y => y.IsStaticallyDefinedPoint));

                if (!NoPointsAreStaticallyDefined)
                {
                    await MovePoints(Points, State.CurrentTime, moveAction);
                }

                SimulatorRunGlobal(algorithmCode);
                await SimulatorRunPointRoundsAsync(algorithmCode);

                foreach (SimulationPoint<TNode>? point in Points)
                {
                    // Removed failed certificates
                    foreach (Certificates.ISimulationCertificate<TNode>? itm in point.RemovedCertificatesList)
                    {
                        point.RemoveCertificate(itm);
                        itm.Dispose();
                    }
                    point.RemovedCertificatesList.Clear();

                    int ext = point.Node.GetNumberOfChanges();

                    if (point.RemovedCertificates > ext)
                    {
                        point.ExternalEvents += (uint)ext;
                        point.InternalEvents += point.RemovedCertificates - (uint)ext;
                    }

                    point.RemovedCertificates = 0;

                    point.Changed = ext != 0;
                }

                await RebuildCertificatesAsync(regenerateCertificates);

                audit?.AuditDataStructures(Points);

                SimulationTick?.Invoke(State.CurrentTime, Points);

                if (Points.Any(x => x.Changed))
                {
                    SimulationPointsChanged?.Invoke(State.CurrentTime, Points, Points.Where(x => x.Changed));
                }
            }
        }

        /// <summary>
        /// Executes the algorithm for all points
        /// </summary>
        /// <param name="algorithmCode"></param>
        /// <param name="Localized"></param>
        /// <returns></returns>
        internal bool RunAlgorithm(IAlgorithmCode<TNode> algorithmCode, bool Localized = false)
        {
            Certificates.ISimulationCertificate<TNode>[] Fail = Points.SelectMany(point => point.GetFailedCertificates()).ToArray();

            if (Fail.Length == 0)
            {
                return false;
            }

            foreach (SimulationPoint<TNode>? point in Points)
            {
                point.MessageQueue.Clear();
            }

            if (!Localized)
            {
                algorithmCode.RunAlgorithmAfterAllPointsMoved(Fail, Points, State.CurrentTime);
            }
            else
            {
                algorithmCode.RunAlgorithmAfterSinglePointMoved(Fail, Points, State.CurrentTime);
            }

            return true;
        }

        /// <summary>
        /// Executes the algorithm after a single movement
        /// </summary>
        /// <param name="code"></param>
        private void SimulatorRunLocalized(IAlgorithmCode<TNode> code)
        {
            //Console.WriteLine($"Executing localized round");
            try
            {
                RunAlgorithm(code, true);
            }
            catch (NotImplementedException) { }
            //Console.WriteLine($"Finished Executing localized round");
        }

        /// <summary>
        /// Executes the algorithm after all movements
        /// </summary>
        /// <param name="algorithmCode"></param>
        private void SimulatorRunGlobal(IAlgorithmCode<TNode> algorithmCode)
        {
            //Console.WriteLine($"Executing global round");
            try
            {
                RunAlgorithm(algorithmCode);
            }
            catch (NotImplementedException) { }
            //Console.WriteLine($"Finished Executing global round");
        }

        /// <summary>
        /// Executes all rounds of the algorithm for each point in parallel
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private async Task SimulatorRunPointRoundsAsync(IAlgorithmCode<TNode> code)
        {
            for (int i = 0; i < code.GetMaxIterationCount(); i++)
            {
                //Console.WriteLine($"Executing round {i}");
                await Parallel2.ForEachAsync(Points, (x, y) => new ValueTask(Task.Run(() =>
                {
                    try
                    {
                        x.RunAlgorithm(i, code);
                    }
                    catch (NotImplementedException) { }
                }, y)));

                //Console.WriteLine($"Finished Executing round {i}");
            }
        }

        /// <summary>
        /// The previous simulation time before it changed
        /// </summary>
        private double? OldCurrentTime = null;

        /// <summary>
        /// This function calls the provided move point function to the simulator and updates the oldcurrentime variable
        /// </summary>
        /// <param name="Points">The list of points in the simulatio,</param>
        /// <param name="CurrentTime">The current time of the simulatio,</param>
        /// <param name="moveAction">The action to move the points</param>
        /// <returns></returns>
        internal async Task MovePoints(IEnumerable<SimulationPoint<TNode>> Points, double CurrentTime, Func<IEnumerable<SimulationPoint<TNode>>, double, double?, Task> moveAction)
        {
            await moveAction(Points, CurrentTime, OldCurrentTime);
            OldCurrentTime = CurrentTime;
        }
    }
}
