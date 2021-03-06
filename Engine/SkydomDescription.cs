﻿
namespace Engine
{
    /// <summary>
    /// Skydom descriptor
    /// </summary>
    public class SkydomDescription : CubemapDescription
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SkydomDescription()
            : base()
        {
            this.CastShadow = false;
            this.DeferredEnabled = true;
            this.DepthEnabled = false;
            this.AlphaEnabled = false;

            this.Geometry = CubeMapGeometry.Sphere;
            this.ReverseFaces = true;
        }
    }
}
