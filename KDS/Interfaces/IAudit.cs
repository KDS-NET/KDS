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
    /// Class that implements the Audit functionality for the simulation (Optional)
    /// </summary>
    /// <typeparam name="TNode">The data structure used by the simulation</typeparam>
    public interface IAudit<TNode> where TNode : INode, new()
    {
        /// <summary>
        /// Function to audit all data structures in the simulation.
        /// The implementation is expected to throw exceptions whenever an inconsistency is detected in the data structures
        /// to halt the simulation and allow debugging
        /// </summary>
        /// <param name="Points"></param>
        public void AuditDataStructures(IEnumerable<SimulationPoint<TNode>> Points);
    }
}
