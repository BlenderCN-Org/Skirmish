﻿using SharpDX;

namespace Engine.PathFinding.NavMesh2
{
    /// <summary>
    /// Represents a simple, non-overlapping contour in field space.
    /// </summary>
    public class Contour
    {
        /// <summary>
        /// Simplified contour vertex and connection data. [Size: 4 * #nverts]
        /// </summary>
        public Int4[] verts;
        /// <summary>
        /// The number of vertices in the simplified contour. 
        /// </summary>
        public int nverts;
        /// <summary>
        /// Raw contour vertex and connection data. [Size: 4 * #nrverts]
        /// </summary>
        public Int4[] rverts;
        /// <summary>
        /// The number of vertices in the raw contour. 
        /// </summary>
        public int nrverts;
        /// <summary>
        /// The region id of the contour.
        /// </summary>
        public int reg;
        /// <summary>
        /// The area id of the contour.
        /// </summary>
        public TileCacheAreas area;
    };
}
