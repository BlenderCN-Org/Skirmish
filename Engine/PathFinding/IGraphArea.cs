﻿using SharpDX;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph area interface
    /// </summary>
    public interface IGraphArea
    {
        /// <summary>
        /// Area id
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Vertices (convex polygon)
        /// </summary>
        Vector3[] Vertices { get; set; }
        /// <summary>
        /// Minimum height
        /// </summary>
        float MinHeight { get; set; }
        /// <summary>
        /// Maximum height
        /// </summary>
        float MaxHeight { get; set; }
        /// <summary>
        /// Vertex count
        /// </summary>
        int VertexCount { get; set; }
        /// <summary>
        /// Area type
        /// </summary>
        GraphAreaTypes AreaType { get; set; }
    }
}
