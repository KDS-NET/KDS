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
using MathNet.Numerics;
using System.Linq;

#nullable enable

namespace KDS
{
    /// <summary>
    /// This structure represents an axis
    /// </summary>
    public class SimulationPointAxis
    {
        /// <summary>
        /// The current state of the simulation
        /// </summary>
        private readonly SimulatorState SimulatorState;

        /// <summary>
        /// The constructor for an axis of a point
        /// </summary>
        /// <param name="Simulator"></param>
        internal SimulationPointAxis(SimulatorState Simulator)
        {
            SimulatorState = Simulator;
            LastPositions = new double?[Simulator.TrajectoryPredictionHistorySize];
            LastTimes = new double?[Simulator.TrajectoryPredictionHistorySize];
        }

        /// <summary>
        /// Is the point defined purely using static data?
        /// </summary>
        internal bool IsStaticallyDefinedPoint = false;

        /// <summary>
        /// The predicted axis polynomial
        /// </summary>
        internal Polynomial? PolPredicted { get; set; }

        /// <summary>
        /// The predicted current axis position
        /// </summary>
        internal double? Predicted => PolPredicted?.Evaluate(SimulatorState.CurrentTime);

        /// <summary>
        /// The polynomial object built using static position data
        /// </summary>
        private Polynomial polStatic = new();

        /// <summary>
        /// The real polynomial defining axis trajectory. In case of a static point, this will always be of degree 0.
        /// </summary>
        internal Polynomial PolStatic
        {
            get => polStatic; set
            {
                polStatic = value;
                IsStaticallyDefinedPoint = false;
            }
        }

        /// <summary>
        /// The last 3 positions of this point for the axis. Contains null if no recorded position was performed yet.
        /// </summary>
        internal double?[] LastPositions;

        /// <summary>
        /// The last 3 times of this point for the axis. Contains null if no recorded position was performed yet.
        /// </summary>
        internal double?[] LastTimes;

        /// <summary>
        /// The current position of the point, estimated or real
        /// </summary>
        public double Position { get => Predicted ?? Static; set => Static = value; }

        /// <summary>
        /// The current polynomial object representing the position, estimated or real
        /// </summary>
        public Polynomial Pol { get => PolPredicted ?? PolStatic; set => PolStatic = value; }

        /// <summary>
        /// The real axis position
        /// </summary>
        public double Static
        {
            get => PolStatic.Evaluate(SimulatorState.CurrentTime); set
            {
                PolStatic = new Polynomial(new double[] { value });
                IsStaticallyDefinedPoint = true;
            }
        }

        /// <summary>
        /// The current position in the Last Positions array variables.
        /// </summary>
        internal int counter = 0;

        /// <summary>
        /// Gets the list of last positions, in order
        /// </summary>
        /// <returns></returns>
        internal double?[] GetOrderedLastPositions()
        {
            return LastPositions.Skip(counter).Concat(LastPositions.Take(counter)).ToArray();
        }

        /// <summary>
        /// Gets the list of last times, in order
        /// </summary>
        /// <returns></returns>
        internal double?[] GetOrderedLastTimes()
        {
            return LastTimes.Skip(counter).Concat(LastTimes.Take(counter)).ToArray();
        }

        /// <summary>
        /// Records the last position into the structure. Recomputes predictions if required.
        /// </summary>
        /// <param name="x"></param>
        internal void AddLastPosition(double x, double t)
        {
            Static = x;
            LastPositions[counter] = x;
            LastTimes[counter] = t;
            counter = (counter + 1) % LastPositions.Length;
        }
    }
}
