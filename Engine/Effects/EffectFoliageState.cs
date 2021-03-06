﻿using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Effect foliage state
    /// </summary>
    public struct EffectFoliageState
    {
        /// <summary>
        /// Wind direction
        /// </summary>
        public Vector3 WindDirection { get; set; }
        /// <summary>
        /// Wind strength
        /// </summary>
        public float WindStrength { get; set; }
        /// <summary>
        /// Total time
        /// </summary>
        public float TotalTime { get; set; }
        /// <summary>
        /// Delta
        /// </summary>
        public Vector3 Delta { get; set; }
        /// <summary>
        /// Drawing start radius
        /// </summary>
        public float StartRadius { get; set; }
        /// <summary>
        /// Drawing end radius
        /// </summary>
        public float EndRadius { get; set; }
        /// <summary>
        /// Random texture
        /// </summary>
        public EngineShaderResourceView RandomTexture { get; set; }
        /// <summary>
        /// Material index
        /// </summary>
        public uint MaterialIndex { get; set; }
        /// <summary>
        /// Texture count
        /// </summary>
        public uint TextureCount { get; set; }
        /// <summary>
        /// Normal map count
        /// </summary>
        public uint NormalMapCount { get; set; }
        /// <summary>
        /// Texture
        /// </summary>
        public EngineShaderResourceView Texture { get; set; }
        /// <summary>
        /// Normal maps
        /// </summary>
        public EngineShaderResourceView NormalMaps { get; set; }
    }
}