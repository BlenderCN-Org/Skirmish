﻿using SharpDX;

namespace Engine
{
    /// <summary>
    /// Water description
    /// </summary>
    public class WaterDescription : SceneObjectDescription
    {
        /// <summary>
        /// Creates a default water description
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="planeSize">Plane size</param>
        /// <param name="planeHeight">Plane height</param>
        /// <returns>Returns a water description</returns>
        public static WaterDescription CreateDefault(string name, float planeSize, float planeHeight)
        {
            return new WaterDescription()
            {
                Name = name,
                PlaneHeight = planeHeight,
                PlaneSize = planeSize,
            };
        }
        /// <summary>
        /// Creates a calm water description
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="planeSize">Plane size</param>
        /// <param name="planeHeight">Plane height</param>
        /// <returns>Returns a water description</returns>
        public static WaterDescription CreateCalm(string name, float planeSize, float planeHeight)
        {
            return new WaterDescription()
            {
                Name = name,
                PlaneHeight = planeHeight,
                PlaneSize = planeSize,
                WaveHeight = 0.2f,
                WaveChoppy = 1f,
                WaveSpeed = 1.5f,
                WaveFrequency = 0.08f,
            };
        }
        /// <summary>
        /// Creates a ocean water description
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="planeSize">Plane size</param>
        /// <param name="planeHeight">Plane height</param>
        /// <returns>Returns a water description</returns>
        public static WaterDescription CreateOcean(string name, float planeSize, float planeHeight)
        {
            return new WaterDescription()
            {
                Name = name,
                PlaneHeight = planeHeight,
                PlaneSize = planeSize,
                WaveHeight = 1.0f,
                WaveChoppy = 5f,
                WaveSpeed = 2.0f,
                WaveFrequency = 0.08f,
            };
        }

        /// <summary>
        /// Base color
        /// </summary>
        public Color BaseColor { get; set; } = new Color(0.1f, 0.19f, 0.22f, 1.0f);
        /// <summary>
        /// Water color
        /// </summary>
        public Color WaterColor { get; set; } = new Color(0.8f, 0.9f, 0.6f, 1.0f);
        /// <summary>
        /// Wave height
        /// </summary>
        /// <remarks>0.6f by default</remarks>
        public float WaveHeight { get; set; } = 0.6f;
        /// <summary>
        /// Wave choppy
        /// </summary>
        /// <remarks>4.0f by default</remarks>
        public float WaveChoppy { get; set; } = 4.0f;
        /// <summary>
        /// Wave speed
        /// </summary>
        /// <remarks>0.8f by default</remarks>
        public float WaveSpeed { get; set; } = 0.8f;
        /// <summary>
        /// Wave frequency
        /// </summary>
        /// <remarks>0.16f by default</remarks>
        public float WaveFrequency { get; set; } = 0.16f;
        /// <summary>
        /// Water plane size
        /// </summary>
        public float PlaneSize { get; set; } = 100f;
        /// <summary>
        /// Water plane height
        /// </summary>
        public float PlaneHeight { get; set; } = 0f;
        /// <summary>
        /// Number of iterations to obtain the wave height
        /// </summary>
        public int HeightmapIterations { get; set; } = 8;
        /// <summary>
        /// Number of iterations to obtain the wave shape
        /// </summary>
        public int GeometryIterations { get; set; } = 4;
        /// <summary>
        /// Number of iterations to obtain the wave color
        /// </summary>
        public int ColorIterations { get; set; } = 6;

        /// <summary>
        /// Constructor
        /// </summary>
        public WaterDescription()
            : base()
        {
            this.CastShadow = false;
            this.DeferredEnabled = true;
            this.DepthEnabled = true;
            this.AlphaEnabled = false;
        }
    }
}
