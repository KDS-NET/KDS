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
    /// The interface for the data structure used by the simulation
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// This function gets the collection of objects that we are trying to maintain in the data structure, this will count for the internal/external event metrics
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfChanges();

        /// <summary>
        /// This functions gets called whenever the point object is made available by the simulator for usage if needed.
        /// This function will thus have as point parameter, the class SimulationPoint<INode>
        /// The caller is expected to cast the object to this class. It is not properly specified in this function declaration
        /// given the data type would depend on this very own class.
        /// </summary>
        /// <param name="point">SimulationPoint<INode></param>
        public void SetAttachedSimulationPoint(object point);
    }
}
