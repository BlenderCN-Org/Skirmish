﻿using SharpDX;
using System;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Instaced model
    /// </summary>
    public class ModelInstanced : ModelBase
    {
        /// <summary>
        /// Instancing data per instance
        /// </summary>
        private VertexInstancingData[] instancingData = null;
        /// <summary>
        /// Model instance list
        /// </summary>
        private ModelInstance[] instances = null;
        /// <summary>
        /// Temporal instance listo for rendering
        /// </summary>
        private ModelInstance[] instancesTmp = null;

        /// <summary>
        /// Gets manipulator per instance list
        /// </summary>
        /// <returns>Gets manipulator per instance list</returns>
        public ModelInstance[] Instances
        {
            get
            {
                return this.instances;
            }
        }
        /// <summary>
        /// Gets instance count
        /// </summary>
        public int Count
        {
            get
            {
                return this.instances.Length;
            }
        }
        /// <summary>
        /// Gets visible instance count
        /// </summary>
        public int VisibleCount
        {
            get
            {
                return Array.FindAll(this.instances, i => i.Visible == true && i.Cull == false).Length;
            }
        }
        /// <summary>
        /// Instances
        /// </summary>
        public int InstanceCount { get; protected set; }
        /// <summary>
        /// Maximum number of instances
        /// </summary>
        public override int MaxInstances
        {
            get
            {
                return this.InstanceCount;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        /// <param name="description">Description</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public ModelInstanced(Game game, ModelContent content, ModelInstancedDescription description, bool dynamic = false)
            : base(game, content, description, true, description.Instances, true, true, dynamic)
        {
            if (description.Instances <= 0) throw new ArgumentException(string.Format("Instances parameter must be more than 0: {0}", instances));

            this.InstanceCount = description.Instances;

            this.instances = Helper.CreateArray(this.InstanceCount, () => new ModelInstance(this));
            this.instancingData = new VertexInstancingData[this.InstanceCount];
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        /// <param name="description">Description</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public ModelInstanced(Game game, LODModelContent content, ModelInstancedDescription description, bool dynamic = false)
            : base(game, content, description, true, description.Instances, true, true, dynamic)
        {
            if (description.Instances <= 0) throw new ArgumentException(string.Format("Instances parameter must be more than 0: {0}", instances));

            this.InstanceCount = description.Instances;

            this.instances = Helper.CreateArray(this.InstanceCount, () => new ModelInstance(this));
            this.instancingData = new VertexInstancingData[this.InstanceCount];
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            if (this.instances != null && this.instances.Length > 0)
            {
                Array.ForEach(this.instances, i =>
                {
                    if (i.Active) i.Update(context);
                });
            }
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="context">Context</param>
        public override void Draw(DrawContext context)
        {
            int count = 0;
            int instanceCount = 0;

            if (this.VisibleCount > 0)
            {
                #region Update instancing data

                //Process only visible instances
                if (this.instancesTmp != null && this.instancesTmp.Length > 0)
                {
                    LevelOfDetailEnum lastLod = LevelOfDetailEnum.None;
                    DrawingData drawingData = null;
                    int instanceIndex = 0;

                    for (int i = 0; i < this.instancesTmp.Length; i++)
                    {
                        var current = this.instancesTmp[i];
                        if (current != null)
                        {
                            if (lastLod != current.LevelOfDetail)
                            {
                                lastLod = current.LevelOfDetail;
                                drawingData = this.GetDrawingData(lastLod);
                            }

                            this.instancingData[instanceIndex].Local = current.Manipulator.LocalTransform;
                            this.instancingData[instanceIndex].TextureIndex = current.TextureIndex;

                            if (drawingData != null && drawingData.SkinningData != null)
                            {
                                current.AnimationController.Update(context.GameTime.ElapsedSeconds, drawingData.SkinningData);

                                this.instancingData[instanceIndex].ClipIndex = 0;
                                this.instancingData[instanceIndex].AnimationOffset = current.AnimationController.GetAnimationOffset(drawingData.SkinningData);

                                current.InvalidateCache();
                            }

                            instanceIndex++;
                        }
                    }

                    //Writes instancing data
                    if (instanceIndex > 0)
                    {
                        this.BufferManager.WriteInstancingData(this.Game.Graphics, this.instancingData);
                    }
                }

                #endregion

                Drawer effect = null;
                if (context.DrawerMode == DrawerModesEnum.Forward) effect = DrawerPool.EffectDefaultBasic;
                else if (context.DrawerMode == DrawerModesEnum.Deferred) effect = DrawerPool.EffectDeferredBasic;
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap) effect = DrawerPool.EffectShadowBasic;

                if (effect != null)
                {
                    this.BufferManager.SetVertexBuffers(this.Game.Graphics);

                    #region Per frame update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        ((EffectDefaultBasic)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection,
                            context.EyePosition,
                            context.Lights,
                            context.ShadowMaps,
                            context.ShadowMapStatic,
                            context.ShadowMapDynamic,
                            context.FromLightViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        ((EffectDeferredBasic)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        ((EffectShadowBasic)effect).UpdatePerFrame(
                            context.World,
                            context.ViewProjection);
                    }

                    #endregion

                    //Render by level of detail
                    for (int l = 1; l < (int)LevelOfDetailEnum.Minimum + 1; l *= 2)
                    {
                        LevelOfDetailEnum lod = (LevelOfDetailEnum)l;

                        //Get instances in this LOD
                        var ins = Array.FindAll(this.instancesTmp, i => i != null && i.LevelOfDetail == lod);
                        if (ins != null && ins.Length > 0)
                        {
                            var drawingData = this.GetDrawingData(lod);
                            if (drawingData != null)
                            {
                                var index = Array.IndexOf(this.instancesTmp, ins[0]);
                                var length = ins.Length;

                                instanceCount += length;

                                foreach (string meshName in drawingData.Meshes.Keys)
                                {
                                    var dictionary = drawingData.Meshes[meshName];

                                    foreach (string material in dictionary.Keys)
                                    {
                                        #region Per object update

                                        var mat = drawingData.Materials[material];

                                        if (context.DrawerMode == DrawerModesEnum.Forward)
                                        {
                                            ((EffectDefaultBasic)effect).UpdatePerObject(
                                                mat.DiffuseTexture,
                                                mat.NormalMap,
                                                mat.SpecularTexture,
                                                mat.ResourceIndex,
                                                0,
                                                0);
                                        }
                                        else if (context.DrawerMode == DrawerModesEnum.Deferred)
                                        {
                                            ((EffectDeferredBasic)effect).UpdatePerObject(
                                                mat.DiffuseTexture,
                                                mat.NormalMap,
                                                mat.SpecularTexture,
                                                mat.ResourceIndex,
                                                0,
                                                0);
                                        }
                                        else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                                        {
                                            ((EffectShadowBasic)effect).UpdatePerObject(
                                                0,
                                                0);
                                        }

                                        #endregion

                                        var mesh = dictionary[material];
                                        this.BufferManager.SetIndexBuffer(this.Game.Graphics, mesh.IndexBufferSlot);

                                        var technique = effect.GetTechnique(mesh.VertextType, mesh.Instanced, DrawingStages.Drawing, context.DrawerMode);
                                        this.BufferManager.SetInputAssembler(this.Game.Graphics, technique, mesh.VertextType, false, mesh.Topology);

                                        count += mesh.IndexCount > 0 ? mesh.IndexCount / 3 : mesh.VertexCount / 3;
                                        count *= instanceCount;

                                        for (int p = 0; p < technique.Description.PassCount; p++)
                                        {
                                            technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                                            mesh.Draw(this.Game.Graphics, index, length);

                                            Counters.DrawCallsPerFrame++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (context.DrawerMode != DrawerModesEnum.ShadowMap)
            {
                Counters.InstancesPerFrame += instanceCount;
                Counters.PrimitivesPerFrame += count;
            }
        }
        /// <summary>
        /// Culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public override void Culling(BoundingFrustum frustum)
        {
            //Cull was made per instance
            this.Cull = true;

            Array.ForEach(this.Instances, i =>
            {
                if (i.Visible)
                {
                    i.Culling(frustum);

                    if (!i.Cull)
                    {
                        this.Cull = false;
                    }
                }
            });

            var par = frustum.GetCameraParams();

            this.UpdateInstacesTmp(par.Position);
        }
        /// <summary>
        /// Culling test
        /// </summary>
        /// <param name="sphere">Sphere</param>
        public override void Culling(BoundingSphere sphere)
        {
            //Cull was made per instance
            this.Cull = true;

            Array.ForEach(this.Instances, i =>
            {
                if (i.Visible)
                {
                    i.Culling(sphere);

                    if (!i.Cull)
                    {
                        this.Cull = false;
                    }
                }
            });

            this.UpdateInstacesTmp(sphere.Center);
        }
        /// <summary>
        /// Sets cull value
        /// </summary>
        /// <param name="value">New value</param>
        public override void SetCulling(bool value)
        {
            base.SetCulling(value);

            if (this.instances != null && this.instances.Length > 0)
            {
                Array.ForEach(this.instances, i => i.SetCulling(value));
            }

            this.UpdateInstacesTmp(Vector3.Zero);
        }
        /// <summary>
        /// Updates the instance tmp list
        /// </summary>
        private void UpdateInstacesTmp(Vector3 origin)
        {
            this.instancesTmp = Array.FindAll(this.instances, i => i.Visible && !i.Cull && i.LevelOfDetail != LevelOfDetailEnum.None);

            //Sort by LOD
            Array.Sort(this.instancesTmp, (i1, i2) =>
            {
                ModelInstance a;
                ModelInstance b;

                if (this.AlphaEnabled)
                {
                    a = i2;
                    b = i1;
                }
                else
                {
                    a = i1;
                    b = i2;
                }

                var i = a.LevelOfDetail.CompareTo(b.LevelOfDetail);
                if (i == 0)
                {
                    var da = Vector3.DistanceSquared(a.Manipulator.Position, origin);
                    var db = Vector3.DistanceSquared(b.Manipulator.Position, origin);
                    i = da.CompareTo(db);
                }
                if (i == 0)
                {
                    i = a.Id.CompareTo(b.Id);
                }

                return i;
            });
        }
        /// <summary>
        /// Set instance positions
        /// </summary>
        /// <param name="positions">New positions</param>
        public void SetPositions(Vector3[] positions)
        {
            if (positions != null && positions.Length > 0)
            {
                if (this.Instances != null && this.Instances.Length > 0)
                {
                    for (int i = 0; i < this.Instances.Length; i++)
                    {
                        if (i < positions.Length)
                        {
                            this.Instances[i].Manipulator.SetPosition(positions[i], true);
                            this.Instances[i].Active = true;
                            this.Instances[i].Visible = true;
                        }
                        else
                        {
                            this.Instances[i].Active = false;
                            this.Instances[i].Visible = false;
                        }
                    }
                }
            }
        }
    }
}
