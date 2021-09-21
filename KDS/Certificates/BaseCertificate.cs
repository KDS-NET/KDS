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
    public abstract class BaseCertificate<TNode> : ISimulationCertificate<TNode> where TNode : INode, new()
    {
        /// <summary>
        /// The point u
        /// </summary>
        private readonly SimulationPoint<TNode> u;

        /// <summary>
        /// The point v
        /// </summary>
        private readonly SimulationPoint<TNode> v;

        /// <summary>
        /// The failure time of the certificate computed at certificate creation
        /// </summary>
        private double? FailureTimeAtCreation;

        /// <summary>
        /// Gets the failure time of a certificate, computed at its creation
        /// </summary>
        /// <returns></returns>
        public double? GetFailureTimeAtCreation()
        {
            return FailureTimeAtCreation;
        }

        /// <summary>
        /// The construction for a basic certificate
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="CurrentTime"></param>
        protected BaseCertificate(SimulationPoint<TNode> u, SimulationPoint<TNode> v, double CurrentTime)
        {
            this.u = u;
            this.v = v;
            FailureTimeAtCreation = GetFailureTime(CurrentTime);
            this.u.PredictionChanged += Data_PredictionChanged;
            this.v.PredictionChanged += Data_PredictionChanged;
        }

        /// <summary>
        /// Called whenever the prediction of a point has been changed during the simulation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="XPol"></param>
        /// <param name="CurrentTime"></param>
        private void Data_PredictionChanged(SimulationPoint<TNode> sender, MathNet.Numerics.Polynomial[] XPol, double CurrentTime)
        {
            FailureTimeAtCreation = GetFailureTime(CurrentTime);
        }

        /// <summary>
        /// Gets the point U
        /// </summary>
        /// <returns></returns>
        public SimulationPoint<TNode> GetU()
        {
            return u;
        }

        /// <summary>
        /// Gets the point V
        /// </summary>
        /// <returns></returns>
        public SimulationPoint<TNode> GetV()
        {
            return v;
        }

        /// <summary>
        /// Dispose the base certificate
        /// </summary>
        public void Dispose()
        {
            u.PredictionChanged -= Data_PredictionChanged;
            v.PredictionChanged -= Data_PredictionChanged;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the failure time of a certificate
        /// </summary>
        /// <param name="CurrentTime"></param>
        /// <returns></returns>
        public abstract double? GetFailureTime(double CurrentTime);

        /// <summary>
        /// Evaluates the certificate predicate validity at a given time in a simulation, using static data, and not polynomials
        /// </summary>
        /// <param name="CurrentTime"></param>
        /// <returns></returns>
        public abstract bool EvaluateValidity(double CurrentTime);
    }
}
