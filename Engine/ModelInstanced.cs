﻿using System;
using SharpDX;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

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
        /// Manipulator list per instance
        /// </summary>
        private ModelInstance[] instances = null;

        /// <summary>
        /// Enables transparent blending
        /// </summary>
        public bool EnableAlphaBlending { get; set; }
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
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="content">Content</param>
        /// <param name="instances">Number of instances</param>
        public ModelInstanced(Game game, ModelContent content, int instances)
            : base(game, content, true, instances, true, true)
        {
            this.instancingData = new VertexInstancingData[instances];

            this.instances = Helper.CreateArray(instances, () => new ModelInstance(this));
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Update(GameTime gameTime, Context context)
        {
            base.Update(gameTime, context);

            if (this.instances != null && this.instances.Length > 0)
            {
                for (int i = 0; i < this.instances.Length; i++)
                {
                    if (this.instances[i].Active)
                    {
                        this.instances[i].Manipulator.Update(gameTime);
                    }
                }
            }
        }
        /// <summary>
        /// Draw
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Draw(GameTime gameTime, Context context)
        {
            if (this.Meshes != null && this.VisibleCount > 0)
            {
                Drawer effect = null;
                if (context.DrawerMode == DrawerModesEnum.Forward) effect = DrawerPool.EffectInstancing;
                else if (context.DrawerMode == DrawerModesEnum.Deferred) effect = DrawerPool.EffectInstancingGBuffer;
                else if (context.DrawerMode == DrawerModesEnum.ShadowMap) effect = DrawerPool.EffectInstancingShadow;

                if (effect != null)
                {
                    if (this.EnableAlphaBlending)
                    {
                        this.Game.Graphics.SetBlendTransparent();
                    }
                    else
                    {
                        this.Game.Graphics.SetBlendAlphaToCoverage();
                    }

                    if (this.instances != null && this.instances.Length > 0)
                    {
                        int instanceIndex = 0;
                        for (int i = 0; i < this.instances.Length; i++)
                        {
                            if (this.instances[i].Visible && !this.instances[i].Cull)
                            {
                                this.instancingData[instanceIndex].Local = this.instances[i].Manipulator.LocalTransform;
                                this.instancingData[instanceIndex].TextureIndex = this.instances[i].TextureIndex;

                                instanceIndex++;
                            }
                        }
                    }

                    #region Per frame update

                    if (context.DrawerMode == DrawerModesEnum.Forward)
                    {
                        ((EffectInstancing)effect).FrameBuffer.World = context.World;
                        ((EffectInstancing)effect).FrameBuffer.WorldInverse = Matrix.Invert(context.World);
                        ((EffectInstancing)effect).FrameBuffer.WorldViewProjection = context.World * context.ViewProjection;
                        ((EffectInstancing)effect).FrameBuffer.ShadowTransform = context.ShadowTransform;
                        ((EffectInstancing)effect).FrameBuffer.Lights = new BufferLights(context.EyePosition, context.Lights);
                        ((EffectInstancing)effect).UpdatePerFrame(context.ShadowMap);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.Deferred)
                    {
                        ((EffectInstancingGBuffer)effect).FrameBuffer.World = context.World;
                        ((EffectInstancingGBuffer)effect).FrameBuffer.WorldInverse = Matrix.Invert(context.World);
                        ((EffectInstancingGBuffer)effect).FrameBuffer.WorldViewProjection = context.World * context.ViewProjection;
                        ((EffectInstancingGBuffer)effect).FrameBuffer.ShadowTransform = context.ShadowTransform;
                        ((EffectInstancingGBuffer)effect).UpdatePerFrame(context.ShadowMap);
                    }
                    else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                    {
                        ((EffectInstancingShadow)effect).FrameBuffer.WorldViewProjection = context.World * context.ViewProjection;
                        ((EffectInstancingShadow)effect).UpdatePerFrame();
                    }

                    #endregion

                    foreach (string meshName in this.Meshes.Keys)
                    {
                        #region Per skinning update

                        if (this.SkinningData != null)
                        {
                            if (context.DrawerMode == DrawerModesEnum.Forward)
                            {
                                ((EffectInstancing)effect).SkinningBuffer.FinalTransforms = this.SkinningData.GetFinalTransforms(meshName);
                                ((EffectInstancing)effect).UpdatePerSkinning();
                            }
                            else if (context.DrawerMode == DrawerModesEnum.Deferred)
                            {
                                ((EffectInstancingGBuffer)effect).SkinningBuffer.FinalTransforms = this.SkinningData.GetFinalTransforms(meshName);
                                ((EffectInstancingGBuffer)effect).UpdatePerSkinning();
                            }
                            else if (context.DrawerMode == DrawerModesEnum.ShadowMap)
                            {
                                ((EffectInstancingShadow)effect).SkinningBuffer.FinalTransforms = this.SkinningData.GetFinalTransforms(meshName);
                                ((EffectInstancingShadow)effect).UpdatePerSkinning();
                            }
                        }

                        #endregion

                        MeshMaterialsDictionary dictionary = this.Meshes[meshName];

                        foreach (string material in dictionary.Keys)
                        {
                            MeshInstanced mesh = (MeshInstanced)dictionary[material];
                            MeshMaterial mat = this.Materials[material];

                            #region Per object update

                            var matdata = mat != null ? mat.Material : Material.Default;
                            var texture = mat != null ? mat.DiffuseTexture : null;
                            var normalMap = mat != null ? mat.NormalMap : null;

                            if (context.DrawerMode == DrawerModesEnum.Forward)
                            {
                                ((EffectInstancing)effect).ObjectBuffer.Material.SetMaterial(matdata);
                                ((EffectInstancing)effect).UpdatePerObject(texture, normalMap);
                            }
                            else if (context.DrawerMode == DrawerModesEnum.Deferred)
                            {
                                ((EffectInstancingGBuffer)effect).ObjectBuffer.Material.SetMaterial(matdata);
                                ((EffectInstancingGBuffer)effect).UpdatePerObject(texture);
                            }

                            #endregion

                            EffectTechnique technique = effect.GetTechnique(mesh.VertextType, DrawingStages.Drawing);

                            mesh.SetInputAssembler(this.DeviceContext, effect.GetInputLayout(technique));

                            mesh.WriteInstancingData(this.DeviceContext, this.instancingData);

                            for (int p = 0; p < technique.Description.PassCount; p++)
                            {
                                technique.GetPassByIndex(p).Apply(this.DeviceContext, 0);

                                mesh.Draw(gameTime, this.DeviceContext, this.VisibleCount);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Frustum culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public override void FrustumCulling(BoundingFrustum frustum)
        {
            //Cull was made per instance
            this.Cull = false;

            for (int i = 0; i < this.Instances.Length; i++)
            {
                if (this.Instances[i].Visible)
                {
                    this.Instances[i].FrustumCulling(frustum);
                }
            }
        }
    }

    /// <summary>
    /// Instanced model description
    /// </summary>
    public class ModelInstancedDescription
    {
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Model file name
        /// </summary>
        public string ModelFileName = null;
        /// <summary>
        /// Instances
        /// </summary>
        public int Instances = 1;
        /// <summary>
        /// Drops shadow
        /// </summary>
        public bool Opaque = false;
    }

    /// <summary>
    /// Model instance
    /// </summary>
    public class ModelInstance
    {
        /// <summary>
        /// Model
        /// </summary>
        private ModelBase model = null;
        /// <summary>
        /// Update point cache flag
        /// </summary>
        private bool updatePoints = true;
        /// <summary>
        /// Update triangle cache flag
        /// </summary>
        private bool updateTriangles = true;
        /// <summary>
        /// Points caché
        /// </summary>
        private Vector3[] positionCache = null;
        /// <summary>
        /// Triangle list cache
        /// </summary>
        private Triangle[] triangleCache = null;
        /// <summary>
        /// Bounding sphere cache
        /// </summary>
        private BoundingSphere boundingSphere = new BoundingSphere();
        /// <summary>
        /// Bounding box cache
        /// </summary>
        private BoundingBox boundingBox = new BoundingBox();
        /// <summary>
        /// Oriented bounding box cache
        /// </summary>
        private OrientedBoundingBox orientedBoundingBox = new OrientedBoundingBox();
        /// <summary>
        /// Gets if model has volumes
        /// </summary>
        private bool hasVolumes
        {
            get
            {
                Vector3[] positions = this.GetPoints();

                return positions != null && positions.Length > 0;
            }
        }

        /// <summary>
        /// Manipulator
        /// </summary>
        public Manipulator3D Manipulator = null;
        /// <summary>
        /// Texture index
        /// </summary>
        public int TextureIndex = 0;
        /// <summary>
        /// Active
        /// </summary>
        public bool Active = true;
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible = true;
        /// <summary>
        /// Culling test flag
        /// </summary>
        public bool Cull = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model">Model</param>
        public ModelInstance(ModelBase model)
        {
            this.model = model;
            this.Manipulator = new Manipulator3D();
            this.Manipulator.Updated += new System.EventHandler(ManipulatorUpdated);
        }
        /// <summary>
        /// Occurs when manipulator transform updated
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void ManipulatorUpdated(object sender, EventArgs e)
        {
            this.updatePoints = true;

            this.updateTriangles = true;

            this.boundingSphere = new BoundingSphere();
            this.boundingBox = new BoundingBox();
            this.orientedBoundingBox = new OrientedBoundingBox();
        }

        /// <summary>
        /// Gets point list of mesh if the vertex type has position channel
        /// </summary>
        /// <returns>Returns null or position list</returns>
        public Vector3[] GetPoints()
        {
            if (this.updatePoints)
            {
                this.positionCache = this.model.GetPoints(this.Manipulator.LocalTransform);

                this.updatePoints = false;
            }

            return this.positionCache;
        }
        /// <summary>
        /// Gets triangle list of mesh if the vertex type has position channel
        /// </summary>
        /// <returns>Returns null or triangle list</returns>
        public Triangle[] GetTriangles()
        {
            if (this.updateTriangles)
            {
                this.triangleCache = this.model.GetTriangles(this.Manipulator.LocalTransform);

                this.updateTriangles = false;
            }

            return this.triangleCache;
        }
        /// <summary>
        /// Gets bounding sphere
        /// </summary>
        /// <returns>Returns bounding sphere. Empty if the vertex type hasn't position channel</returns>
        public BoundingSphere GetBoundingSphere()
        {
            if (this.boundingSphere == new BoundingSphere())
            {
                Vector3[] positions = this.GetPoints();
                if (positions != null && positions.Length > 0)
                {
                    this.boundingSphere = BoundingSphere.FromPoints(positions);
                }
            }

            return this.boundingSphere;
        }
        /// <summary>
        /// Gets bounding box
        /// </summary>
        /// <returns>Returns bounding box. Empty if the vertex type hasn't position channel</returns>
        public BoundingBox GetBoundingBox()
        {
            if (this.boundingBox == new BoundingBox())
            {
                Vector3[] positions = this.GetPoints();
                if (positions != null && positions.Length > 0)
                {
                    this.boundingBox = BoundingBox.FromPoints(positions);
                }
            }

            return this.boundingBox;
        }
        /// <summary>
        /// Gets oriented bounding box
        /// </summary>
        /// <returns>Returns oriented bounding box with identity transformation. Empty if the vertex type hasn't position channel</returns>
        public OrientedBoundingBox GetOrientedBoundingBox()
        {
            if (this.orientedBoundingBox == new OrientedBoundingBox())
            {
                Vector3[] positions = this.GetPoints();
                if (positions != null && positions.Length > 0)
                {
                    this.orientedBoundingBox = new OrientedBoundingBox(positions);
                    this.orientedBoundingBox.Transform(Matrix.Identity);
                }
            }

            return this.orientedBoundingBox;
        }

        /// <summary>
        /// Gets picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool Pick(ref Ray ray, out Vector3 position, out Triangle triangle)
        {
            position = new Vector3();
            triangle = new Triangle();

            BoundingSphere bsph = this.GetBoundingSphere();

            if (bsph.Intersects(ref ray))
            {
                Triangle[] triangles = this.GetTriangles();
                if (triangles != null && triangles.Length > 0)
                {
                    for (int i = 0; i < triangles.Length; i++)
                    {
                        Triangle tri = triangles[i];

                        Vector3 pos;
                        if (tri.Intersects(ref ray, out pos))
                        {
                            position = pos;
                            triangle = tri;

                            return true;
                        }
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Performs frustum culling test
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public virtual void FrustumCulling(BoundingFrustum frustum)
        {
            if (this.hasVolumes)
            {
                this.Cull = frustum.Contains(this.GetBoundingSphere()) == ContainmentType.Disjoint;
            }
            else
            {
                this.Cull = false;
            }
        }
    }
}
