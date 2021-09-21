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
using System.Collections.Generic;

namespace KDS.Interfaces
{
    /// <summary>
    /// The interface to maintain certificates in a structure. It is being called after the algorithm code has been executed and is optional.
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public interface ICertificateGenerator<TNode> where TNode : INode, new()
    {
        /// <summary>
        /// Rebuilds the certificates for a given point, the caller is expected to
        /// compute the differences in the data structure to then remove or add certificates at its own will
        /// </summary>
        /// <param name="u">The current point we are handling</param>
        /// <param name="Node">The current data structure</param>
        public void RebuildCertificates(SimulationPoint<TNode> u, TNode Node, double CurrentTime);

        /// <summary>
        /// Rebuilds the certificates for all points, the caller is expected to
        /// compute the differences in the data structure to then remove or add certificates at its own will
        /// </summary>
        /// <param name="Points">All the current points we are handling</param>
        public void RebuildCertificates(IEnumerable<SimulationPoint<TNode>> Points, double CurrentTime);
    }
}
