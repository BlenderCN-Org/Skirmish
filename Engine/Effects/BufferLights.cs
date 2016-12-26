﻿using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    using Engine.Common;
    using System;

    /// <summary>
    /// Directional light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferDirectionalLight : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 3;

        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor;
        /// <summary>
        /// Specular color
        /// </summary>
        public Color4 SpecularColor;
        /// <summary>
        /// Light direction vector
        /// </summary>
        public Vector3 Direction;
        /// <summary>
        /// Light cast shadows
        /// </summary>
        public float CastShadow;

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
#if DEBUG
                int size = Marshal.SizeOf(typeof(BufferDirectionalLight));
                if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
                return size;
#else
                return Marshal.SizeOf(typeof(BufferDirectionalLight));
#endif
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferDirectionalLight(SceneLightDirectional light)
        {
            this.Direction = light.Direction;
            this.DiffuseColor = light.DiffuseColor * light.Brightness;
            this.SpecularColor = light.SpecularColor * light.Brightness;
            this.CastShadow = light.CastShadow ? 1 : 0;
        }
    }

    /// <summary>
    /// Point light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferPointLight : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 16;

        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor;
        /// <summary>
        /// Specular color
        /// </summary>
        public Color4 SpecularColor;
        /// <summary>
        /// Light position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity;
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad1;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad2;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad3;

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
#if DEBUG
                int size = Marshal.SizeOf(typeof(BufferPointLight));
                if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
                return size;
#else
                return Marshal.SizeOf(typeof(BufferPointLight));
#endif
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferPointLight(SceneLightPoint light)
        {
            this.Position = light.Position;
            this.DiffuseColor = light.DiffuseColor;
            this.SpecularColor = light.SpecularColor;
            this.Intensity = light.Intensity;
            this.Radius = light.Radius;

            this.Pad1 = 1000;
            this.Pad2 = 2000;
            this.Pad3 = 3000;
        }
    }

    /// <summary>
    /// Spot light buffer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferSpotLight : IBufferData
    {
        /// <summary>
        /// Maximum light count
        /// </summary>
        public const int MAX = 16;

        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor;
        /// <summary>
        /// Specular color
        /// </summary>
        public Color4 SpecularColor;
        /// <summary>
        /// Light position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Spot radius
        /// </summary>
        public float Angle;
        /// <summary>
        /// Light direction vector
        /// </summary>
        public Vector3 Direction;
        /// <summary>
        /// Intensity
        /// </summary>
        public float Intensity;
        /// <summary>
        /// Light radius
        /// </summary>
        public float Radius;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad1;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad2;
        /// <summary>
        /// Padding
        /// </summary>
        public float Pad3;

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
#if DEBUG
                int size = Marshal.SizeOf(typeof(BufferSpotLight));
                if (size % 8 != 0) throw new EngineException("Buffer strides must be divisible by 8 in order to be sent to shaders and effects as arrays");
                return size;
#else
                return Marshal.SizeOf(typeof(BufferSpotLight));
#endif
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="light">Light</param>
        public BufferSpotLight(SceneLightSpot light)
        {
            this.Position = light.Position;
            this.Direction = light.Direction;
            this.DiffuseColor = light.DiffuseColor;
            this.SpecularColor = light.SpecularColor;
            this.Intensity = light.Intensity;
            this.Intensity = light.Intensity;
            this.Angle = light.AngleRadians;
            this.Radius = light.Radius;

            this.Pad1 = 1000;
            this.Pad2 = 2000;
            this.Pad3 = 3000;
        }
    }
}
