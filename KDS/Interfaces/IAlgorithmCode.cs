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
using System.Collections.Generic;

namespace KDS.Interfaces
{
    /// <summary>
    /// The interface for the implementation of an algorithm (from pseudocode to code)
    /// </summary>
    /// <typeparam name="TNode">The data structure used by the algorithm</typeparam>
    public interface IAlgorithmCode<TNode> where TNode : INode, new()
    {
        /// <summary>
        /// Gets the number of rounds this algorithm expects. This is helpful for synchronism if required
        /// </summary>
        /// <returns></returns>
        public int GetMaxIterationCount();

        /// <summary>
        /// Runs the algorithm at a given round with a list of failed certificate and the current point we are handling
        /// </summary>
        /// <param name="i">The current round number, starts from 0</param>
        /// <param name="Fail">The list of failed certificates right now</param>
        /// <param name="u">The current point</param>
        /// <param name="CurrentTime">The current time</param>
        public void RunAlgorithmAfterAllPointsMovedPerPoint(int i, IEnumerable<ISimulationCertificate<TNode>> Fail, SimulationPoint<TNode> u, double CurrentTime);

        /// <summary>
        /// Runs the algorithm at a given round with a list of failed certificate after all the points have moved
        /// </summary>
        /// <param name="Fail">The list of failed certificates right now</param>
        /// <param name="Points">All the points</param>
        /// <param name="CurrentTime">The current time</param>
        public void RunAlgorithmAfterAllPointsMoved(IEnumerable<ISimulationCertificate<TNode>> Fail, IEnumerable<SimulationPoint<TNode>> Points, double CurrentTime);

        /// <summary>
        /// Runs the algorithm at a given round with a list of failed certificate and all the points we are handling everytime a point moves
        /// </summary>
        /// <param name="Fail">The list of failed certificates right now</param>
        /// <param name="Points">All the points</param>
        /// <param name="CurrentTime">The current time</param>
        public void RunAlgorithmAfterSinglePointMoved(IEnumerable<ISimulationCertificate<TNode>> Fail, IEnumerable<SimulationPoint<TNode>> Points, double CurrentTime);
    }
}
