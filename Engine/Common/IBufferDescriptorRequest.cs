﻿
namespace Engine.Common
{
    /// <summary>
    /// Buffer decriptor request interface
    /// </summary>
    interface IBufferDescriptorRequest
    {
        /// <summary>
        /// Requester Id
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Request action
        /// </summary>
        BufferDescriptorRequestActions Action { get; }
        /// <summary>
        /// Gets wheter the descriptor is processed into the buffer manager or not
        /// </summary>
        bool Processed { get; }
        /// <summary>
        /// Updates the buffer descriptor
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        void Process(BufferManager bufferManager);
    }
}
