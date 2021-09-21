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
    /// This class is called for initializing the data structures in the simulation
    /// </summary>
    /// <typeparam name="TNode">The data structure in the simulation</typeparam>
    public interface INodeInitializer<TNode> where TNode : INode, new()
    {
        /// <summary>
        /// This function will compute the values of the data structure / initialize them
        /// </summary>
        /// <param name="PointStructureList">The list of points in the simulation</param>
        public void ComputeNodeValues(IEnumerable<SimulationPoint<TNode>> PointStructureList);
    }
}
