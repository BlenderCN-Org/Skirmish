﻿namespace Engine.PathFinding.NavMesh.Crowds
{
    /// <summary>
    /// This state changes depending on what the crowd agent has to do next
    /// </summary>
    public enum TargetState
    {
        /// <summary>
        /// Not in any state
        /// </summary>
        None,
        /// <summary>
        /// Failed to find a new path
        /// </summary>
        Failed,
        /// <summary>
        /// Target destination reached.
        /// </summary>
        Valid,
        /// <summary>
        /// Requesting a new path
        /// </summary>
        Requesting,
        /// <summary>
        /// Add this agent to the crowd manager's path queue
        /// </summary>
        WaitingForQueue,
        /// <summary>
        /// The agent is in the path queue
        /// </summary>
        WaitingForPath,
        /// <summary>
        /// Changing its velocity
        /// </summary>
        Velocity
    }
}
