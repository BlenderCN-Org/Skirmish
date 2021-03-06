﻿using SharpDX.DXGI;
using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Render target
    /// </summary>
    public class RenderTarget : IDisposable
    {
        /// <summary>
        /// Game class
        /// </summary>
        protected Game Game { get; private set; }

        /// <summary>
        /// Render target format
        /// </summary>
        public Format RenderTargetFormat { get; protected set; }
        /// <summary>
        /// Buffer count
        /// </summary>
        public int BufferCount { get; protected set; }
        /// <summary>
        /// Use samples if available
        /// </summary>
        public bool UseSamples { get; protected set; }
        /// <summary>
        /// Buffer textures
        /// </summary>
        public EngineShaderResourceView[] Textures { get; protected set; }
        /// <summary>
        /// Render targets
        /// </summary>
        public EngineRenderTargetView Targets { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="format">Format</param>
        /// <param name="useSamples">Use samples if available</param>
        /// <param name="count">Buffer count</param>
        public RenderTarget(Game game, Format format, bool useSamples, int count)
        {
            this.Game = game;
            this.RenderTargetFormat = format;
            this.UseSamples = useSamples;
            this.BufferCount = count;

            this.CreateTargets();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~RenderTarget()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DisposeTargets();
            }
        }

        /// <summary>
        /// Resizes geometry buffer using render form size
        /// </summary>
        public void Resize()
        {
            this.DisposeTargets();
            this.CreateTargets();
        }

        /// <summary>
        /// Creates render targets, depth buffer and viewport
        /// </summary>
        private void CreateTargets()
        {
            int width = this.Game.Form.RenderWidth;
            int height = this.Game.Form.RenderHeight;

            this.Game.Graphics.CreateRenderTargetTexture(
                this.RenderTargetFormat,
                width, height, this.BufferCount, this.UseSamples,
                out EngineRenderTargetView targets,
                out EngineShaderResourceView[] textures);

            this.Targets = targets;
            this.Textures = textures;
        }
        /// <summary>
        /// Disposes all targets and depth buffer
        /// </summary>
        private void DisposeTargets()
        {
            if (this.Targets != null)
            {
                this.Targets.Dispose();
                this.Targets = null;
            }

            if (this.Textures != null)
            {
                for (int i = 0; i < this.Textures.Length; i++)
                {
                    this.Textures[i]?.Dispose();
                    this.Textures[i] = null;
                }

                this.Textures = null;
            }
        }
    }
}
