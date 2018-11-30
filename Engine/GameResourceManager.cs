﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Engine resource manager
    /// </summary>
    public class GameResourceManager : IDisposable
    {
        /// <summary>
        /// Game instance
        /// </summary>
        private readonly Game game;
        /// <summary>
        /// Resource dictionary
        /// </summary>
        private readonly Dictionary<string, EngineShaderResourceView> resources = new Dictionary<string, EngineShaderResourceView>();
        /// <summary>
        /// Global resources dictionary
        /// </summary>
        private readonly Dictionary<string, EngineShaderResourceView> globalResources = new Dictionary<string, EngineShaderResourceView>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public GameResourceManager(Game game)
        {
            this.game = game;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~GameResourceManager()
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
                foreach (var item in resources)
                {
                    item.Value?.Dispose();
                }
                resources.Clear();

                foreach (var item in globalResources)
                {
                    item.Value?.Dispose();
                }
                globalResources.Clear();
            }
        }

        /// <summary>
        /// Generates the resource view
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateResource(ImageContent imageContent)
        {
            if (imageContent.Stream != null)
            {
                byte[] buffer = imageContent.Stream.GetBuffer();

                return this.Get(buffer);
            }
            else
            {
                if (imageContent.IsCubic)
                {
                    return CreateResourceCubic(imageContent);
                }
                else if (imageContent.IsArray)
                {
                    return CreateResourceArray(imageContent);
                }
                else
                {
                    return CreateResourceDefault(imageContent);
                }
            }
        }
        /// <summary>
        /// Creates a resource view from image content
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <returns>Returns the created resource view</returns>
        private EngineShaderResourceView CreateResourceDefault(ImageContent imageContent)
        {
            if (imageContent.Path != null)
            {
                return this.Get(imageContent.Path);
            }
            else if (imageContent.Stream != null)
            {
                return this.Get(imageContent.Stream);
            }

            return null;
        }
        /// <summary>
        /// Creates a resource view from image content array
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <returns>Returns the created resource view</returns>
        private EngineShaderResourceView CreateResourceArray(ImageContent imageContent)
        {
            if (imageContent.Paths != null && imageContent.Paths.Length > 0)
            {
                return this.Get(imageContent.Paths);
            }
            else if (imageContent.Streams != null && imageContent.Streams.Length > 0)
            {
                return this.Get(imageContent.Streams);
            }

            return null;
        }
        /// <summary>
        /// Creates a resource view from cubic image content
        /// </summary>
        /// <param name="imageContent">Image content</param>
        /// <returns>Returns the created resource view</returns>
        private EngineShaderResourceView CreateResourceCubic(ImageContent imageContent)
        {
            if (imageContent.IsArray)
            {
                if (imageContent.Paths != null)
                {
                    return this.Get(imageContent.Paths);
                }
                else if (imageContent.Streams != null)
                {
                    return this.Get(imageContent.Streams);
                }
            }
            else
            {
                if (imageContent.Path != null)
                {
                    return this.Get(imageContent.Path);
                }
                else if (imageContent.Stream != null)
                {
                    return this.Get(imageContent.Stream);
                }
            }

            return null;
        }
        /// <summary>
        /// Generates the resource view
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateResource(string path)
        {
            return this.Get(path);
        }
        /// <summary>
        /// Generates the resource view
        /// </summary>
        /// <param name="stream">Memory stream</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateResource(MemoryStream stream)
        {
            return this.Get(stream);
        }
        /// <summary>
        /// Creates a 2d texture of byte values
        /// </summary>
        /// <param name="values">Values</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateResource(byte[] values)
        {
            string md5 = values.GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                var view = this.game.Graphics.LoadTexture(values);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
        /// <summary>
        /// Creates a 2d texture of Vector4 values
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <param name="values">Values</param>
        /// <param name="size">Texture size (total pixels = size * size)</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateResource(Guid identifier, Vector4[] values, int size)
        {
            string md5 = identifier.ToByteArray().GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                var view = this.game.Graphics.CreateTexture2D(size, values);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
        /// <summary>
        /// Creates a 1d texture of random values
        /// </summary>
        /// <param name="identifier">Identifier</param>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateResource(Guid identifier, int size, float min, float max, int seed = 0)
        {
            string md5 = identifier.ToByteArray().GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                var view = this.game.Graphics.CreateRandomTexture(size, min, max, seed);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="values">Values</param>
        /// <param name="size">Texture size (total pixels = size * size)</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateGlobalResourceTexture2D(string name, Vector4[] values, int size)
        {
            var view = this.game.Graphics.CreateTexture2D(size, values);
            this.SetGlobalResource(name, view);
            return view;
        }
        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="bytes">Resource bytes</param>
        /// <returns></returns>
        public EngineShaderResourceView CreateGlobalResourceTexture2D(string name, byte[] bytes)
        {
            var view = this.game.Graphics.LoadTexture(bytes);
            this.SetGlobalResource(name, view);
            return view;
        }
        /// <summary>
        /// Creates a new global resource by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="size">Texture size</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns the created resource view</returns>
        public EngineShaderResourceView CreateGlobalResourceRandomTexture(string name, int size, float min, float max, int seed = 0)
        {
            var view = this.game.Graphics.CreateRandomTexture(size, min, max, seed);
            this.SetGlobalResource(name, view);
            return view;
        }
        /// <summary>
        /// Set global resource by name
        /// </summary>
        /// <param name="name">Resource name</param>
        /// <param name="resource">Resource content</param>
        public void SetGlobalResource(string name, EngineShaderResourceView resource)
        {
            if (this.globalResources.ContainsKey(name))
            {
                var cRes = this.globalResources[name];
                if (cRes != null)
                {
                    cRes.Dispose();
                }
                this.globalResources[name] = resource;
            }
            else
            {
                this.globalResources.Add(name, resource);
            }
        }

        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Returns the created resource view</returns>
        private EngineShaderResourceView Get(byte[] buffer)
        {
            string md5 = buffer.GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                var view = this.game.Graphics.LoadTexture(buffer);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>Returns the created resource view</returns>
        private EngineShaderResourceView Get(string path)
        {
            if (!this.resources.ContainsKey(path))
            {
                var view = this.game.Graphics.LoadTexture(path);
                this.resources.Add(path, view);
            }

            return this.resources[path];
        }
        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="stream">Memory stream</param>
        /// <returns>Returns the created resource view</returns>
        private EngineShaderResourceView Get(MemoryStream stream)
        {
            string md5 = stream.GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                stream.Position = 0;
                var view = this.game.Graphics.LoadTexture(stream);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="paths">Path list</param>
        /// <returns>Returns the created resource view</returns>
        private EngineShaderResourceView Get(string[] paths)
        {
            string md5 = paths.GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                var view = this.game.Graphics.LoadTextureArray(paths);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
        /// <summary>
        /// Gets the shader resource view or creates if not exists
        /// </summary>
        /// <param name="streams">Stream list</param>
        /// <returns>Returns the created resource view</returns>
        private EngineShaderResourceView Get(MemoryStream[] streams)
        {
            string md5 = streams.GetMd5Sum();
            if (!this.resources.ContainsKey(md5))
            {
                var view = this.game.Graphics.LoadTextureArray(streams);
                this.resources.Add(md5, view);
            }

            return this.resources[md5];
        }
    }
}
