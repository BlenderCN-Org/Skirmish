﻿using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Particles effect
    /// </summary>
    public class EffectDefaultCpuParticles : Drawer
    {
        /// <summary>
        /// Non rotation particles drawing technique
        /// </summary>
        public readonly EngineEffectTechnique NonRotationDraw = null;
        /// <summary>
        /// Rotation particles drawing technique
        /// </summary>
        public readonly EngineEffectTechnique RotationDraw = null;

        /// <summary>
        /// World effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldVar = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private readonly EngineEffectVariableVector eyePositionWorldVar = null;
        /// <summary>
        /// Game time effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar totalTimeVar = null;
        /// <summary>
        /// Texture count effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureCountVar = null;
        /// <summary>
        /// Textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture textureArrayVar = null;

        /// <summary>
        /// Maximum particle duration variable
        /// </summary>
        private readonly EngineEffectVariableScalar maxDurationVar = null;
        /// <summary>
        /// Maximum particle duration randomness variable
        /// </summary>
        private readonly EngineEffectVariableScalar maxDurationRandomnessVar = null;
        /// <summary>
        /// End velocity variable
        /// </summary>
        private readonly EngineEffectVariableScalar endVelocityVar = null;
        /// <summary>
        /// Gravity variable
        /// </summary>
        private readonly EngineEffectVariableVector gravityVar = null;
        /// <summary>
        /// Starting size variable
        /// </summary>
        private readonly EngineEffectVariableVector startSizeVar = null;
        /// <summary>
        /// Ending size variable
        /// </summary>
        private readonly EngineEffectVariableVector endSizeVar = null;
        /// <summary>
        /// Minimum color variable
        /// </summary>
        private readonly EngineEffectVariableVector minColorVar = null;
        /// <summary>
        /// Maximum color variable
        /// </summary>
        private readonly EngineEffectVariableVector maxColorVar = null;
        /// <summary>
        /// Rotation speed variable
        /// </summary>
        private readonly EngineEffectVariableVector rotateSpeedVar = null;

        /// <summary>
        /// Current texture array
        /// </summary>
        private EngineShaderResourceView currentTextureArray = null;

        /// <summary>
        /// World matrix
        /// </summary>
        protected Matrix World
        {
            get
            {
                return this.worldVar.GetMatrix();
            }
            set
            {
                this.worldVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.worldViewProjectionVar.GetMatrix();
            }
            set
            {
                this.worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                return this.eyePositionWorldVar.GetVector<Vector3>();
            }
            set
            {
                this.eyePositionWorldVar.Set(value);
            }
        }
        /// <summary>
        /// Game time
        /// </summary>
        protected float TotalTime
        {
            get
            {
                return this.totalTimeVar.GetFloat();
            }
            set
            {
                this.totalTimeVar.Set(value);
            }
        }
        /// <summary>
        /// Texture count
        /// </summary>
        protected uint TextureCount
        {
            get
            {
                return this.textureCountVar.GetUInt();
            }
            set
            {
                this.textureCountVar.Set(value);
            }
        }
        /// <summary>
        /// Textures
        /// </summary>
        protected EngineShaderResourceView TextureArray
        {
            get
            {
                return this.textureArrayVar.GetResource();
            }
            set
            {
                if (this.currentTextureArray != value)
                {
                    this.textureArrayVar.SetResource(value);

                    this.currentTextureArray = value;

                    Counters.TextureUpdates++;
                }
            }
        }

        /// <summary>
        /// Maximum particle duration
        /// </summary>
        protected float MaxDuration
        {
            get
            {
                return this.maxDurationVar.GetFloat();
            }
            set
            {
                this.maxDurationVar.Set(value);
            }
        }
        /// <summary>
        /// Maximum particle duration randomness
        /// </summary>
        protected float MaxDurationRandomness
        {
            get
            {
                return this.maxDurationRandomnessVar.GetFloat();
            }
            set
            {
                this.maxDurationRandomnessVar.Set(value);
            }
        }
        /// <summary>
        /// End velocity
        /// </summary>
        protected float EndVelocity
        {
            get
            {
                return this.endVelocityVar.GetFloat();
            }
            set
            {
                this.endVelocityVar.Set(value);
            }
        }
        /// <summary>
        /// Gravity
        /// </summary>
        protected Vector3 Gravity
        {
            get
            {
                return this.gravityVar.GetVector<Vector3>();
            }
            set
            {
                this.gravityVar.Set(value);
            }
        }
        /// <summary>
        /// Starting size
        /// </summary>
        protected Vector2 StartSize
        {
            get
            {
                return this.startSizeVar.GetVector<Vector2>();
            }
            set
            {
                this.startSizeVar.Set(value);
            }
        }
        /// <summary>
        /// Ending size
        /// </summary>
        protected Vector2 EndSize
        {
            get
            {
                return this.endSizeVar.GetVector<Vector2>();
            }
            set
            {
                this.endSizeVar.Set(value);
            }
        }
        /// <summary>
        /// Minimum color
        /// </summary>
        protected Color4 MinColor
        {
            get
            {
                return this.minColorVar.GetVector<Color4>();
            }
            set
            {
                this.minColorVar.Set(value);
            }
        }
        /// <summary>
        /// Maximum color
        /// </summary>
        protected Color4 MaxColor
        {
            get
            {
                return this.maxColorVar.GetVector<Color4>();
            }
            set
            {
                this.maxColorVar.Set(value);
            }
        }
        /// <summary>
        /// Rotation speed
        /// </summary>
        protected Vector2 RotateSpeed
        {
            get
            {
                return this.rotateSpeedVar.GetVector<Vector2>();
            }
            set
            {
                this.rotateSpeedVar.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultCpuParticles(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.NonRotationDraw = this.Effect.GetTechniqueByName("NonRotationParticle");
            this.RotationDraw = this.Effect.GetTechniqueByName("RotationParticle");

            this.worldVar = this.Effect.GetVariableMatrix("gWorld");
            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.eyePositionWorldVar = this.Effect.GetVariableVector("gEyePositionWorld");
            this.totalTimeVar = this.Effect.GetVariableScalar("gTotalTime");
            this.textureCountVar = this.Effect.GetVariableScalar("gTextureCount");
            this.textureArrayVar = this.Effect.GetVariableTexture("gTextureArray");

            this.maxDurationVar = this.Effect.GetVariableScalar("gMaxDuration");
            this.maxDurationRandomnessVar = this.Effect.GetVariableScalar("gMaxDurationRandomness");
            this.endVelocityVar = this.Effect.GetVariableScalar("gEndVelocity");
            this.gravityVar = this.Effect.GetVariableVector("gGravity");
            this.startSizeVar = this.Effect.GetVariableVector("gStartSize");
            this.endSizeVar = this.Effect.GetVariableVector("gEndSize");
            this.minColorVar = this.Effect.GetVariableVector("gMinColor");
            this.maxColorVar = this.Effect.GetVariableVector("gMaxColor");
            this.rotateSpeedVar = this.Effect.GetVariableVector("gRotateSpeed");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="state">Particle state</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="textures">Texture</param>
        public void UpdatePerFrame(
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            EffectParticleState state,
            uint textureCount,
            EngineShaderResourceView textures)
        {
            this.World = Matrix.Identity;
            this.WorldViewProjection = viewProjection;
            this.EyePositionWorld = eyePositionWorld;
            this.TotalTime = state.TotalTime;
            this.MaxDuration = state.MaxDuration;
            this.MaxDurationRandomness = state.MaxDurationRandomness;
            this.EndVelocity = state.EndVelocity;
            this.Gravity = state.Gravity;
            this.StartSize = state.StartSize;
            this.EndSize = state.EndSize;
            this.MinColor = state.MinColor;
            this.MaxColor = state.MaxColor;
            this.RotateSpeed = state.RotateSpeed;
            this.TextureCount = textureCount;
            this.TextureArray = textures;
        }
    }
}
