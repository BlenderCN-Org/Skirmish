﻿using SharpDX;
using System;
using System.Text;

namespace Engine.Animation
{
    /// <summary>
    /// Animation keyframe
    /// </summary>
    public struct Keyframe
    {
        /// <summary>
        /// Frame transformation
        /// </summary>
        private Matrix transform;

        /// <summary>
        /// Time
        /// </summary>
        public float Time;
        /// <summary>
        /// Frame transformation
        /// </summary>
        public Matrix Transform
        {
            get
            {
                return this.transform;
            }
            set
            {
                Matrix trn = value;

                Vector3 location;
                Quaternion rotation;
                Vector3 scale;
                if (trn.Decompose(out scale, out rotation, out location))
                {
                    this.transform = trn;
                    this.Translation = location;
                    this.Rotation = rotation;
                    this.Scale = scale;
                }
                else
                {
                    throw new Exception("Bad transform");
                }
            }
        }
        /// <summary>
        /// Translation
        /// </summary>
        public Vector3 Translation { get; private set; }
        /// <summary>
        /// Rotation
        /// </summary>
        public Quaternion Rotation { get; private set; }
        /// <summary>
        /// Scale
        /// </summary>
        public Vector3 Scale { get; private set; }
        /// <summary>
        /// Interpolation type
        /// </summary>
        public string Interpolation;

        /// <summary>
        /// Interpolate between two keyframes
        /// </summary>
        /// <param name="from">From</param>
        /// <param name="to">To</param>
        /// <param name="amount">Amount</param>
        /// <returns>Returns the interpolated transformation</returns>
        public static Matrix Interpolate(Keyframe from, Keyframe to, float amount)
        {
            return
                Matrix.Scaling(Vector3.Lerp(from.Scale, to.Scale, amount)) *
                Matrix.RotationQuaternion(Quaternion.Slerp(from.Rotation, to.Rotation, amount)) *
                Matrix.Translation(Vector3.Lerp(from.Translation, to.Translation, amount));
        }

        /// <summary>
        /// Fills keyframe description into the specified StringBuilder
        /// </summary>
        /// <param name="desc">Description to fill</param>
        public void GetDescription(ref StringBuilder desc)
        {
            desc.AppendFormat("Time: {0,-12}: S:{1,-30} T:{2,-30} R:{3,-30}",
                this.Time,
                this.Scale.GetDescription(Vector3.One, "None"),
                this.Translation.GetDescription(Vector3.Zero, "Zero"),
                this.Rotation.GetDescription());
            desc.AppendLine();
        }
        /// <summary>
        /// Gets text representation
        /// </summary>
        /// <returns>Return text representation</returns>
        public override string ToString()
        {
            return string.Format("Time: {0,-12}: {1}", this.Time, this.Transform.GetDescription());
        }
    }
}
