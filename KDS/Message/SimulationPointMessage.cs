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

namespace KDS
{
    /// <summary>
    /// Represents a distributed message sent in simulation
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    internal class SimulationPointMessage<TNode> where TNode : INode, new()
    {
        /// <summary>
        /// The type of the message sent
        /// </summary>
        internal int Type { get; set; }

        /// <summary>
        /// The point sent along with the message
        /// </summary>
        internal SimulationPoint<TNode> Point { get; set; }
    }
}
