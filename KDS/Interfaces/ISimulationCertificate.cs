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

namespace KDS.Certificates
{
    /// <summary>
    /// The interface for certificates in the simulation
    /// </summary>
    /// <typeparam name="TNode">The type of data structure used by the certificates/simulation</typeparam>
    public interface ISimulationCertificate<TNode> : IDisposable where TNode : INode, new()
    {
        /// <summary>
        /// Gets the expected failure time of the simulation. Must return null if unavailable
        /// </summary>
        /// <param name="CurrentTime">The current time of the simulation</param>
        /// <returns></returns>
        public double? GetFailureTime(double CurrentTime);

        /// <summary>
        /// Gets the failure time computed at the creation of the certificate
        /// </summary>
        /// <returns></returns>
        public double? GetFailureTimeAtCreation();

        /// <summary>
        /// Evaluates the validity of the certificate at a given moment using current available position data
        /// </summary>
        /// <param name="CurrentTime"></param>
        /// <returns></returns>
        public bool EvaluateValidity(double CurrentTime);

        /// <summary>
        /// Gets the point U
        /// </summary>
        /// <returns></returns>
        public SimulationPoint<TNode> GetU();

        /// <summary>
        /// Gets the point V
        /// </summary>
        /// <returns></returns>
        public SimulationPoint<TNode> GetV();
    }
}
