﻿using System;
using SharpDX;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// 3D manipulator
    /// </summary>
    public class Manipulator3D
    {
        /// <summary>
        /// State updated event
        /// </summary>
        public event EventHandler Updated;

        /// <summary>
        /// One radian
        /// </summary>
        private const float RADIAN = 0.0174532924f;

        /// <summary>
        /// Transform update needed flag
        /// </summary>
        protected bool transformUpdateNeeded = true;
        /// <summary>
        /// Final transform for the controller
        /// </summary>
        protected Matrix localTransform = Matrix.Identity;
        /// <summary>
        /// Rotation component
        /// </summary>
        protected Quaternion rotation = Quaternion.Identity;
        /// <summary>
        /// Scaling component
        /// </summary>
        protected Vector3 scaling = Vector3.Zero;
        /// <summary>
        /// Position component
        /// </summary>
        protected Vector3 position = Vector3.Zero;

        /// <summary>
        /// Ground
        /// </summary>
        protected IGround Ground = null;

        /// <summary>
        /// Following path
        /// </summary>
        protected ICurve Curve = null;
        /// <summary>
        /// Path time
        /// </summary>
        protected float CurveTime = 0f;
        /// <summary>
        /// Following time delta
        /// </summary>
        protected float CurveTimeDelta = 1f;

        /// <summary>
        /// Target
        /// </summary>
        protected Vector3[] Path = null;
        /// <summary>
        /// Target position
        /// </summary>
        protected int PathTarget = -1;
        /// <summary>
        /// Path velocity
        /// </summary>
        protected float PathVelocity = 0f;

        /// <summary>
        /// Gets Position component
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return this.position;
            }
        }
        /// <summary>
        /// Gets Scaling component
        /// </summary>
        public Vector3 Scaling
        {
            get
            {
                return this.scaling;
            }
        }
        /// <summary>
        /// Rotation component
        /// </summary>
        public Quaternion Rotation
        {
            get
            {
                return this.rotation;
            }
        }
        /// <summary>
        /// Gets final transform of controller
        /// </summary>
        public Matrix LocalTransform
        {
            get
            {
                return this.localTransform;
            }
        }
        /// <summary>
        /// Gets Forward vector
        /// </summary>
        public Vector3 Forward { get; private set; }
        /// <summary>
        /// Gets Backward vector
        /// </summary>
        public Vector3 Backward { get; private set; }
        /// <summary>
        /// Gets Left vector
        /// </summary>
        public Vector3 Left { get; private set; }
        /// <summary>
        /// Gets Right vector
        /// </summary>
        public Vector3 Right { get; private set; }
        /// <summary>
        /// Gets Up vector
        /// </summary>
        public Vector3 Up { get; private set; }
        /// <summary>
        /// Gets Down vector
        /// </summary>
        public Vector3 Down { get; private set; }
        /// <summary>
        /// Linear velocity modifier
        /// </summary>
        public float LinearVelocity = 1f;
        /// <summary>
        /// Angular velocity modifier
        /// </summary>
        public float AngularVelocity = 1f;
        /// <summary>
        /// Gets whether the model is following a path or not
        /// </summary>
        public bool IsFollowingPath
        {
            get
            {
                return this.Curve != null || this.Path != null;
            }
        }

        /// <summary>
        /// Contructor
        /// </summary>
        public Manipulator3D()
        {
            this.position = Vector3.Zero;
            this.rotation = Quaternion.Identity;
            this.scaling = new Vector3(1);

            this.UpdateLocalTransform();
        }
        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Update(GameTime gameTime)
        {
            if (this.Curve != null)
            {
                #region Following a curve

                this.UpdateFollowCurve(gameTime);

                #endregion
            }
            else if (this.Path != null)
            {
                #region Following a path

                this.UpdateFollowPath(gameTime);

                #endregion
            }

            this.UpdateLocalTransform();
        }
        /// <summary>
        /// Update internal state
        /// </summary>
        /// <param name="force">If true, local transforms were forced to update</param>
        public void UpdateInternals(bool force)
        {
            if (force) this.transformUpdateNeeded = true;

            this.UpdateLocalTransform();
        }
        /// <summary>
        /// Computes current position and orientation in the curve
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void UpdateFollowCurve(GameTime gameTime)
        {
            if (this.CurveTime <= this.Curve.Length)
            {
                Vector3 newPosition = this.Curve.GetPosition(this.CurveTime);

                if (this.CurveTime != 0f)
                {
                    Vector3 view = Vector3.Normalize(this.position - newPosition);

                    this.position = newPosition;
                    this.LookAt(newPosition + view);
                }

                this.CurveTime += gameTime.ElapsedMilliseconds * 0.01f * this.CurveTimeDelta;
            }
            else
            {
                this.Curve = null;
                this.CurveTime = 0f;
            }
        }
        /// <summary>
        /// Computes current position and orientation in the path
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void UpdateFollowPath(GameTime gameTime)
        {
            if (this.PathTarget < this.Path.Length)
            {
                var velocity = this.PathVelocity * gameTime.ElapsedSeconds;

                Vector3 p = this.Path[this.PathTarget];

                if (Helper.WithinEpsilon(this.position, p, velocity))
                {
                    this.PathTarget++;
                }
                else
                {
                    Vector3 newPosition = this.position + (Vector3.Normalize(p - this.position) * velocity);
                    Vector3 newNormal = this.Up;

                    if (this.Ground != null)
                    {
                        Vector3 pos;
                        Triangle tri;
                        float d;
                        if (this.Ground.FindNearestGroundPosition(newPosition, out pos, out tri, out d))
                        {
                            newPosition = pos;
                            newNormal = tri.Normal;
                        }
                    }

                    Vector3 view = Vector3.Normalize(this.position - newPosition);

                    this.position = newPosition;
                    this.LookAt(newPosition + view, newNormal, false, 0.1f);
                }
            }
            else
            {
                this.Path = null;
                this.PathVelocity = 0f;
                this.PathTarget = -1;
            }
        }
        /// <summary>
        /// Update internal state
        /// </summary>
        protected virtual void UpdateLocalTransform()
        {
            if (this.transformUpdateNeeded)
            {
                Matrix sca = Matrix.Scaling(this.scaling);
                Matrix rot = Matrix.RotationQuaternion(this.rotation);
                Matrix tra = Matrix.Translation(this.position);

                this.localTransform = sca * rot * tra;

                this.Forward = rot.Forward;
                this.Backward = rot.Backward;
                this.Left = rot.Left;
                this.Right = rot.Right;
                this.Up = rot.Up;
                this.Down = rot.Down;

                this.transformUpdateNeeded = false;

                if (this.Updated != null)
                {
                    this.Updated.Invoke(this, new EventArgs());
                }

                Counters.UpdatesPerFrame++;
            }
        }
        /// <summary>
        /// Sets if transform update needed
        /// </summary>
        /// <param name="needed">Needed</param>
        protected virtual void SetUpdateNeeded(bool needed)
        {
            this.transformUpdateNeeded = needed;
        }

        /// <summary>
        /// Increments position component d length along d vector
        /// </summary>
        /// <param name="d">Distance</param>
        private void Move(Vector3 d)
        {
            if (d != Vector3.Zero)
            {
                this.position += d;

                this.SetUpdateNeeded(true);
            }
        }
        /// <summary>
        /// Increments rotation component by axis
        /// </summary>
        /// <param name="yaw">Yaw (Y) amount (radians)</param>
        /// <param name="pitch">Pitch (X) amount (radians)</param>
        /// <param name="roll">Roll (Z) amount (radians)</param>
        public void Rotate(float yaw, float pitch, float roll)
        {
            if (yaw != 0f || pitch != 0f || roll != 0f)
            {
                this.rotation *= Quaternion.RotationYawPitchRoll(yaw, pitch, roll);

                this.SetUpdateNeeded(true);
            }
        }

        /// <summary>
        /// Increments position component d distance along forward vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveForward(GameTime gameTime, float d = 1f)
        {
            this.Move(this.Forward * d * this.LinearVelocity * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along backward vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveBackward(GameTime gameTime, float d = 1f)
        {
            this.Move(this.Backward * d * this.LinearVelocity * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along left vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveLeft(GameTime gameTime, float d = 1f)
        {
            this.Move(this.Left * -d * this.LinearVelocity * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along right vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveRight(GameTime gameTime, float d = 1f)
        {
            this.Move(this.Right * -d * this.LinearVelocity * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along up vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveUp(GameTime gameTime, float d = 1f)
        {
            this.Move(this.Up * d * this.LinearVelocity * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments position component d distance along down vector
        /// </summary>
        /// <param name="d">Distance</param>
        public void MoveDown(GameTime gameTime, float d = 1f)
        {
            this.Move(this.Down * d * this.LinearVelocity * gameTime.ElapsedSeconds);
        }

        /// <summary>
        /// Increments rotation yaw (Y) to the left
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void YawLeft(GameTime gameTime, float a = RADIAN)
        {
            this.Rotate(-a * this.AngularVelocity * gameTime.ElapsedSeconds, 0, 0);
        }
        /// <summary>
        /// Increments rotation yaw (Y) to the right
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void YawRight(GameTime gameTime, float a = RADIAN)
        {
            this.Rotate(a * this.AngularVelocity * gameTime.ElapsedSeconds, 0, 0);
        }
        /// <summary>
        /// Increments rotation pitch (X) up
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void PitchUp(GameTime gameTime, float a = RADIAN)
        {
            this.Rotate(0, a * this.AngularVelocity * gameTime.ElapsedSeconds, 0);
        }
        /// <summary>
        /// Increments rotation pitch (X) down
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void PitchDown(GameTime gameTime, float a = RADIAN)
        {
            this.Rotate(0, -a * this.AngularVelocity * gameTime.ElapsedSeconds, 0);
        }
        /// <summary>
        /// Increments rotation roll (Z) left
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void RollLeft(GameTime gameTime, float a = RADIAN)
        {
            this.Rotate(0, 0, -a * this.AngularVelocity * gameTime.ElapsedSeconds);
        }
        /// <summary>
        /// Increments rotation roll (Z) right
        /// </summary>
        /// <param name="a">Amount (radians)</param>
        public void RollRight(GameTime gameTime, float a = RADIAN)
        {
            this.Rotate(0, 0, a * this.AngularVelocity * gameTime.ElapsedSeconds);
        }

        /// <summary>
        /// Clamped scale increment
        /// </summary>
        /// <param name="scale">Scale amount (percent 0 to x)</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        public void Scale(GameTime gameTime, float scale, Vector3? minSize = null, Vector3? maxSize = null)
        {
            this.Scale(gameTime, new Vector3(scale), minSize, maxSize);
        }
        /// <summary>
        /// Clamped scale increment
        /// </summary>
        /// <param name="scaleX">X axis scale amount (percent 0 to x)</param>
        /// <param name="scaleY">Y axis scale amount (percent 0 to x)</param>
        /// <param name="scaleZ">Z axis scale amount (percent 0 to x)</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        public void Scale(GameTime gameTime, float scaleX, float scaleY, float scaleZ, Vector3? minSize = null, Vector3? maxSize = null)
        {
            this.Scale(gameTime, new Vector3(scaleX, scaleY, scaleZ), minSize, maxSize);
        }
        /// <summary>
        /// Clamped scale increment
        /// </summary>
        /// <param name="scale">Scaling component</param>
        /// <param name="minSize">Min scaling component</param>
        /// <param name="maxSize">Max scaling component</param>
        public void Scale(GameTime gameTime, Vector3 scale, Vector3? minSize = null, Vector3? maxSize = null)
        {
            Vector3 newScaling = this.scaling + (scale * gameTime.ElapsedSeconds);

            if (maxSize.HasValue)
            {
                if (newScaling.LengthSquared() > maxSize.Value.LengthSquared()) newScaling = maxSize.Value;
            }

            if (minSize.HasValue)
            {
                if (newScaling.LengthSquared() < minSize.Value.LengthSquared()) newScaling = minSize.Value;
            }

            this.SetScale(newScaling);
        }

        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="x">X component of position</param>
        /// <param name="y">Y component of position</param>
        /// <param name="z">Z component of position</param>
        /// <param name="updateState">Update internal state</param>
        public void SetPosition(float x, float y, float z, bool updateState = false)
        {
            this.SetPosition(new Vector3(x, y, z), updateState);
        }
        /// <summary>
        /// Sets position
        /// </summary>
        /// <param name="position">Position component</param>
        /// <param name="updateState">Update internal state</param>
        public void SetPosition(Vector3 position, bool updateState = false)
        {
            if (this.position != position)
            {
                this.position = position;

                this.SetUpdateNeeded(true);

                if (updateState) this.UpdateLocalTransform();
            }
        }
        /// <summary>
        /// Sets rotation
        /// </summary>
        /// <param name="yaw">Yaw (Y)</param>
        /// <param name="pitch">Pitch (X)</param>
        /// <param name="roll">Roll (Z)</param>
        /// <param name="updateState">Update internal state</param>
        public void SetRotation(float yaw, float pitch, float roll, bool updateState = false)
        {
            this.SetRotation(Quaternion.RotationYawPitchRoll(yaw, pitch, roll), updateState);
        }
        /// <summary>
        /// Sets rotation
        /// </summary>
        /// <param name="rotation">Rotation component</param>
        /// <param name="updateState">Update internal state</param>
        public void SetRotation(Quaternion rotation, bool updateState = false)
        {
            if (this.rotation != rotation)
            {
                this.rotation = rotation;

                this.SetUpdateNeeded(true);

                if (updateState) this.UpdateLocalTransform();
            }
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale amount (0 to x)</param>
        /// <param name="updateState">Update internal state</param>
        public void SetScale(float scale, bool updateState = false)
        {
            this.SetScale(new Vector3(scale), updateState);
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scaleX">Scale along X axis</param>
        /// <param name="scaleY">Scale along Y axis</param>
        /// <param name="scaleZ">Scale along Z axis</param>
        /// <param name="updateState">Update internal state</param>
        public void SetScale(float scaleX, float scaleY, float scaleZ, bool updateState = false)
        {
            this.SetScale(new Vector3(scaleX, scaleY, scaleZ), updateState);
        }
        /// <summary>
        /// Sets scale
        /// </summary>
        /// <param name="scale">Scale vector</param>
        /// <param name="updateState">Update internal state</param>
        public void SetScale(Vector3 scale, bool updateState = false)
        {
            if (this.scaling != scale)
            {
                this.scaling = scale;

                this.SetUpdateNeeded(true);

                if (updateState) this.UpdateLocalTransform();
            }
        }
        /// <summary>
        /// Look at target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="yAxisOnly">Rotate Y axis only</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        public void LookAt(Vector3 target, bool yAxisOnly = true, float interpolationAmount = 0)
        {
            LookAt(target, Vector3.Up, yAxisOnly, interpolationAmount);
        }
        /// <summary>
        /// Look at target
        /// </summary>
        /// <param name="target">Target</param>
        /// <param name="up">Up vector</param>
        /// <param name="yAxisOnly">Rotate Y axis only</param>
        /// <param name="interpolationAmount">Interpolation amount for linear interpolation</param>
        public void LookAt(Vector3 target, Vector3 up, bool yAxisOnly = true, float interpolationAmount = 0)
        {
            if (!Vector3.NearEqual(this.position, target, new Vector3(MathUtil.ZeroTolerance)))
            {
                if (interpolationAmount == 0)
                {
                    this.rotation = Helper.LookAt(this.position, target, up, yAxisOnly);
                }
                else
                {
                    this.rotation = Quaternion.Slerp(this.rotation, Helper.LookAt(this.position, target, up, yAxisOnly), interpolationAmount);
                }

                this.SetUpdateNeeded(true);
            }
        }
        /// <summary>
        /// Set model aligned to normal
        /// </summary>
        /// <param name="normal">Normal</param>
        public void SetNormal(Vector3 normal)
        {
            float angle = Helper.Angle(Vector3.Up, normal);
            if (angle != 0)
            {
                Vector3 axis = Vector3.Cross(Vector3.Up, normal);

                this.SetRotation(Quaternion.RotationAxis(axis, angle));
            }
            else
            {
                this.SetRotation(Quaternion.RotationAxis(Vector3.Left, 0f));
            }
        }

        /// <summary>
        /// Follow specified curve
        /// </summary>
        /// <param name="curve">Path</param>
        /// <param name="delta">Delta to apply to time increment</param>
        public void Follow(ICurve curve, float delta = 1f)
        {
            this.Curve = curve;
            this.CurveTime = 0f;
            this.CurveTimeDelta = delta;
        }
        /// <summary>
        /// Follow specified path
        /// </summary>
        /// <param name="path">Path to follow</param>
        /// <param name="velocity">Velocity</param>
        /// <param name="ground">Terrain</param>
        public void Follow(Vector3[] path, float velocity, IGround ground)
        {
            this.Path = path;
            this.PathTarget = 0;
            this.PathVelocity = velocity;
            this.Ground = ground;
        }

        /// <summary>
        /// Gets manipulator text representation
        /// </summary>
        /// <returns>Returns manipulator text description</returns>
        public override string ToString()
        {
            return string.Format("{0}", this.localTransform.GetDescription());
        }
    }
}
