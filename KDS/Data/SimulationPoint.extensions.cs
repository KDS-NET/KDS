using KDS.Interfaces;
using MathNet.Numerics;
using System;
using System.Linq;

#nullable enable

namespace KDS
{
    /// <summary>
    /// This class represents a point in the simulator
    /// </summary>
    /// <typeparam name="TNode">The data structure bound to the point</typeparam>
    public partial class SimulationPoint<TNode> where TNode : INode, new()
    {
        /// <summary>
        /// The X axis, if available
        /// </summary>
        public SimulationPointAxis X => Axis[0];

        /// <summary>
        /// The Y axis, if available
        /// </summary>
        public SimulationPointAxis Y => Axis[1];

        /// <summary>
        /// The Z axis, if available
        /// </summary>
        public SimulationPointAxis Z => Axis[2];

        /// <summary>
        /// Returns the distance between this point and d.
        /// </summary>
        /// <param name="d">The other point</param>
        /// <returns></returns>
        public double Distance(SimulationPoint<TNode> d)
        {
            return PredictedDistance(d) ?? StaticDistance(d);
        }

        /// <summary>
        /// Returns the distance polynomial between this point and d.
        /// </summary>
        /// <param name="d">The other point</param>
        /// <returns></returns>
        public Polynomial SquareDistance(SimulationPoint<TNode> d)
        {
            return SquarePredictedDistance(d) ?? SquareStaticDistance(d);
        }

        /// <summary>
        /// Returns the static distance between this point and d.
        /// </summary>
        /// <param name="d">The other point</param>
        /// <returns></returns>
        public double StaticDistance(SimulationPoint<TNode> d)
        {
            double pol = 0;
            for (int i = 0; i < Axis.Length; i++)
            {
                SimulationPointAxis? axis = Axis[i];
                SimulationPointAxis? axis2 = d.Axis[i];
                pol += (axis.Static - axis2.Static) * (axis.Static - d.Axis[i].Static);
            }

            return Math.Sqrt(pol);
        }

        /// <summary>
        /// Returns the static distance polynomial between this point and d.
        /// </summary>
        /// <param name="d">The other point</param>
        /// <returns></returns>
        internal Polynomial SquareStaticDistance(SimulationPoint<TNode> d)
        {
            Polynomial pol = new();
            for (int i = 0; i < Axis.Length; i++)
            {
                SimulationPointAxis? axis = Axis[i];
                SimulationPointAxis? axis2 = d.Axis[i];
                pol += (axis.PolStatic - axis2.PolStatic) * (axis.PolStatic - d.Axis[i].PolStatic);
            }

            return pol;
        }

        /// <summary>
        /// Gets the predicted distance between this point and d, using trajectory predictions. Returns null if trajectory predictions are unavailable.
        /// </summary>
        /// <param name="d">The other point</param>
        /// <returns></returns>
        internal double? PredictedDistance(SimulationPoint<TNode> d)
        {
            if (Axis.Any(x => x.Predicted == null) || d.Axis.Any(x => x.Predicted == null))
            {
                return null;
            }

            double pol = 0;
            for (int i = 0; i < Axis.Length; i++)
            {
                SimulationPointAxis axis = Axis[i];
                SimulationPointAxis axis2 = d.Axis[i];
                if (axis.Predicted == null || axis2.Predicted == null)
                {
                    return null;
                }

                pol += (axis.Predicted.Value - axis2.Predicted.Value) * (axis.Predicted.Value - axis2.Predicted.Value);
            }

            return Math.Sqrt(pol);
        }

        /// <summary>
        /// Gets the predicted distance polynomial between this point and d, using trajectory predictions. Returns null if trajectory predictions are unavailable.
        /// </summary>
        /// <param name="d">The other point</param>
        /// <returns></returns>
        internal Polynomial? SquarePredictedDistance(SimulationPoint<TNode> d)
        {
            if (Axis.Any(x => x.PolPredicted == null) || d.Axis.Any(x => x.PolPredicted == null))
            {
                return null;
            }

            Polynomial pol = new();
            for (int i = 0; i < Axis.Length; i++)
            {
                SimulationPointAxis? axis = Axis[i];
                SimulationPointAxis? axis2 = d.Axis[i];
                pol += (axis.PolPredicted - axis2.PolPredicted) * (axis.PolPredicted - d.Axis[i].PolPredicted);
            }

            return pol;
        }
    }
}
