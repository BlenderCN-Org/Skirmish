﻿using SharpDX;

namespace Engine
{
    /// <summary>
    /// Minimap description
    /// </summary>
    public class MinimapDescription : SceneObjectDescription
    {
        /// <summary>
        /// Top position
        /// </summary>
        public int Top { get; set; }
        /// <summary>
        /// Left position
        /// </summary>
        public int Left { get; set; }
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Terrain to draw
        /// </summary>
        public IDrawable[] Drawables { get; set; }
        /// <summary>
        /// Minimap render area
        /// </summary>
        public BoundingBox MinimapArea { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MinimapDescription()
            : base()
        {
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
