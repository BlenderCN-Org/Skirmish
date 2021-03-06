﻿using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Base shadow effect
    /// </summary>
    public abstract class EffectShadowBase : Drawer, IShadowMapDrawer
    {
        #region Technique variables

        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionColor = null;
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionColorInstanced = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionColorSkinned = null;
        /// <summary>
        /// Position color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionColorSkinnedInstanced = null;

        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalColor = null;
        /// <summary>
        /// Position normal color drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalColorInstanced = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalColorSkinned = null;
        /// <summary>
        /// Position normal color skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalColorSkinnedInstanced = null;

        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTexture = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureInstanced = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureSkinned = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureSkinnedInstanced = null;

        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTexture = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureInstanced = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureSkinned = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureSkinnedInstanced = null;

        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangent = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentInstanced = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentSkinned = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentSkinnedInstanced = null;

        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureTransparent = null;
        /// <summary>
        /// Position texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureTransparentInstanced = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureTransparentSkinned = null;
        /// <summary>
        /// Position texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionTextureTransparentSkinnedInstanced = null;

        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTransparent = null;
        /// <summary>
        /// Position normal texture drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTransparentInstanced = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTransparentSkinned = null;
        /// <summary>
        /// Position normal texture skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTransparentSkinnedInstanced = null;

        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentTransparent = null;
        /// <summary>
        /// Position normal texture tangent drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentTransparentInstanced = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentTransparentSkinned = null;
        /// <summary>
        /// Position normal texture tangent skinned drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapPositionNormalTextureTangentTransparentSkinnedInstanced = null;

        #endregion

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        protected readonly EngineEffectVariableMatrix WorldViewProjectionVariable = null;
        /// <summary>
        /// Animation data effect variable
        /// </summary>
        protected readonly EngineEffectVariableScalar AnimationOffsetVariable = null;
        /// <summary>
        /// Texture index effect variable
        /// </summary>
        protected readonly EngineEffectVariableScalar TextureIndexVariable = null;
        /// <summary>
        /// Animation palette width effect variable
        /// </summary>
        protected readonly EngineEffectVariableScalar AnimationPaletteWidthVariable = null;
        /// <summary>
        /// Animation palette
        /// </summary>
        protected readonly EngineEffectVariableTexture AnimationPaletteVariable = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        protected readonly EngineEffectVariableTexture DiffuseMapVariable = null;

        /// <summary>
        /// Current animation palette
        /// </summary>
        protected EngineShaderResourceView CurrentAnimationPalette = null;
        /// <summary>
        /// Current diffuse map
        /// </summary>
        protected EngineShaderResourceView CurrentDiffuseMap = null;

        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.WorldViewProjectionVariable.GetMatrix();
            }
            set
            {
                this.WorldViewProjectionVariable.SetMatrix(value);
            }
        }
        /// <summary>
        /// Animation data
        /// </summary>
        protected uint AnimationOffset
        {
            get
            {
                return this.AnimationOffsetVariable.GetUInt();
            }
            set
            {
                this.AnimationOffsetVariable.Set(value);
            }
        }
        /// <summary>
        /// Texture index
        /// </summary>
        protected uint TextureIndex
        {
            get
            {
                return this.TextureIndexVariable.GetUInt();
            }
            set
            {
                this.TextureIndexVariable.Set(value);
            }
        }
        /// <summary>
        /// Animation palette width
        /// </summary>
        protected uint AnimationPaletteWidth
        {
            get
            {
                return this.AnimationPaletteWidthVariable.GetUInt();
            }
            set
            {
                this.AnimationPaletteWidthVariable.Set(value);
            }
        }
        /// <summary>
        /// Animation palette
        /// </summary>
        protected EngineShaderResourceView AnimationPalette
        {
            get
            {
                return this.AnimationPaletteVariable.GetResource();
            }
            set
            {
                if (this.CurrentAnimationPalette != value)
                {
                    this.AnimationPaletteVariable.SetResource(value);

                    this.CurrentAnimationPalette = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Diffuse map
        /// </summary>
        protected EngineShaderResourceView DiffuseMap
        {
            get
            {
                return this.DiffuseMapVariable.GetResource();
            }
            set
            {
                if (this.CurrentDiffuseMap != value)
                {
                    this.DiffuseMapVariable.SetResource(value);

                    this.CurrentDiffuseMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        protected EffectShadowBase(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.ShadowMapPositionColor = this.Effect.GetTechniqueByName("ShadowMapPositionColor");
            this.ShadowMapPositionColorInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionColorI");
            this.ShadowMapPositionColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionColorSkinned");
            this.ShadowMapPositionColorSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionColorSkinnedI");

            this.ShadowMapPositionNormalColor = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColor");
            this.ShadowMapPositionNormalColorInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorI");
            this.ShadowMapPositionNormalColorSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorSkinned");
            this.ShadowMapPositionNormalColorSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalColorSkinnedI");

            this.ShadowMapPositionTexture = this.Effect.GetTechniqueByName("ShadowMapPositionTexture");
            this.ShadowMapPositionTextureInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionTextureI");
            this.ShadowMapPositionTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionTextureSkinned");
            this.ShadowMapPositionTextureSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionTextureSkinnedI");

            this.ShadowMapPositionNormalTexture = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTexture");
            this.ShadowMapPositionNormalTextureInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureI");
            this.ShadowMapPositionNormalTextureSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureSkinned");
            this.ShadowMapPositionNormalTextureSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureSkinnedI");

            this.ShadowMapPositionNormalTextureTangent = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangent");
            this.ShadowMapPositionNormalTextureTangentInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentI");
            this.ShadowMapPositionNormalTextureTangentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentSkinned");
            this.ShadowMapPositionNormalTextureTangentSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentSkinnedI");

            this.ShadowMapPositionTextureTransparent = this.Effect.GetTechniqueByName("ShadowMapPositionTextureTransparent");
            this.ShadowMapPositionTextureTransparentInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionTextureTransparentI");
            this.ShadowMapPositionTextureTransparentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionTextureTransparentSkinned");
            this.ShadowMapPositionTextureTransparentSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionTextureTransparentSkinnedI");

            this.ShadowMapPositionNormalTextureTransparent = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparent");
            this.ShadowMapPositionNormalTextureTransparentInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparentI");
            this.ShadowMapPositionNormalTextureTransparentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparentSkinned");
            this.ShadowMapPositionNormalTextureTransparentSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTransparentSkinnedI");

            this.ShadowMapPositionNormalTextureTangentTransparent = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparent");
            this.ShadowMapPositionNormalTextureTangentTransparentInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparentI");
            this.ShadowMapPositionNormalTextureTangentTransparentSkinned = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparentSkinned");
            this.ShadowMapPositionNormalTextureTangentTransparentSkinnedInstanced = this.Effect.GetTechniqueByName("ShadowMapPositionNormalTextureTangentTransparentSkinnedI");

            this.AnimationPaletteWidthVariable = this.Effect.GetVariableScalar("gAnimationPaletteWidth");
            this.AnimationPaletteVariable = this.Effect.GetVariableTexture("gAnimationPalette");
            this.WorldViewProjectionVariable = this.Effect.GetVariableMatrix("gVSWorldViewProjection");
            this.AnimationOffsetVariable = this.Effect.GetVariableScalar("gVSAnimationOffset");
            this.DiffuseMapVariable = this.Effect.GetVariableTexture("gPSDiffuseMapArray");
            this.TextureIndexVariable = this.Effect.GetVariableScalar("gPSTextureIndex");
        }

        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="transparent">Use transparent textures</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        public EngineEffectTechnique GetTechnique(
            VertexTypes vertexType,
            bool instanced,
            bool transparent)
        {
            if (transparent)
            {
                return GetTechniqueTransparent(vertexType, instanced);
            }
            else
            {
                return GetTechniqueOpaque(vertexType, instanced);
            }
        }
        /// <summary>
        /// Get technique by vertex type for opaque objects
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        private EngineEffectTechnique GetTechniqueOpaque(
            VertexTypes vertexType,
            bool instanced)
        {
            if (!instanced)
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return this.ShadowMapPositionColor;
                    case VertexTypes.PositionColorSkinned:
                        return this.ShadowMapPositionColorSkinned;

                    case VertexTypes.PositionTexture:
                        return this.ShadowMapPositionTexture;
                    case VertexTypes.PositionTextureSkinned:
                        return this.ShadowMapPositionTextureSkinned;

                    case VertexTypes.PositionNormalColor:
                        return this.ShadowMapPositionNormalColor;
                    case VertexTypes.PositionNormalColorSkinned:
                        return this.ShadowMapPositionNormalColorSkinned;

                    case VertexTypes.PositionNormalTexture:
                        return this.ShadowMapPositionNormalTexture;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return this.ShadowMapPositionNormalTextureSkinned;

                    case VertexTypes.PositionNormalTextureTangent:
                        return this.ShadowMapPositionNormalTextureTangent;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return this.ShadowMapPositionNormalTextureTangentSkinned;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect. {0}; Instaced: {1}; Opaque", vertexType, instanced));
                }
            }
            else
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return this.ShadowMapPositionColorInstanced;
                    case VertexTypes.PositionColorSkinned:
                        return this.ShadowMapPositionColorSkinnedInstanced;

                    case VertexTypes.PositionTexture:
                        return this.ShadowMapPositionTextureInstanced;
                    case VertexTypes.PositionTextureSkinned:
                        return this.ShadowMapPositionTextureSkinnedInstanced;

                    case VertexTypes.PositionNormalColor:
                        return this.ShadowMapPositionNormalColorInstanced;
                    case VertexTypes.PositionNormalColorSkinned:
                        return this.ShadowMapPositionNormalColorSkinnedInstanced;

                    case VertexTypes.PositionNormalTexture:
                        return this.ShadowMapPositionNormalTextureInstanced;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return this.ShadowMapPositionNormalTextureSkinnedInstanced;

                    case VertexTypes.PositionNormalTextureTangent:
                        return this.ShadowMapPositionNormalTextureTangentInstanced;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return this.ShadowMapPositionNormalTextureTangentSkinnedInstanced;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect. {0}; Instaced: {1}; Opaque", vertexType, instanced));
                }
            }
        }
        /// <summary>
        /// Get technique by vertex type for transparent objects
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        private EngineEffectTechnique GetTechniqueTransparent(
            VertexTypes vertexType,
            bool instanced)
        {
            if (!instanced)
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return this.ShadowMapPositionColor;
                    case VertexTypes.PositionColorSkinned:
                        return this.ShadowMapPositionColorSkinned;

                    case VertexTypes.PositionTexture:
                        return this.ShadowMapPositionTextureTransparent;
                    case VertexTypes.PositionTextureSkinned:
                        return this.ShadowMapPositionTextureTransparentSkinned;

                    case VertexTypes.PositionNormalColor:
                        return this.ShadowMapPositionNormalColor;
                    case VertexTypes.PositionNormalColorSkinned:
                        return this.ShadowMapPositionNormalColorSkinned;

                    case VertexTypes.PositionNormalTexture:
                        return this.ShadowMapPositionNormalTextureTransparent;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return this.ShadowMapPositionNormalTextureTransparentSkinned;

                    case VertexTypes.PositionNormalTextureTangent:
                        return this.ShadowMapPositionNormalTextureTangentTransparent;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return this.ShadowMapPositionNormalTextureTangentTransparentSkinned;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect. {0}; Instaced: {1}; Transparent", vertexType, instanced));
                }
            }
            else
            {
                switch (vertexType)
                {
                    case VertexTypes.PositionColor:
                        return this.ShadowMapPositionColorInstanced;
                    case VertexTypes.PositionColorSkinned:
                        return this.ShadowMapPositionColorSkinnedInstanced;

                    case VertexTypes.PositionTexture:
                        return this.ShadowMapPositionTextureTransparentInstanced;
                    case VertexTypes.PositionTextureSkinned:
                        return this.ShadowMapPositionTextureTransparentSkinnedInstanced;

                    case VertexTypes.PositionNormalColor:
                        return this.ShadowMapPositionNormalColorInstanced;
                    case VertexTypes.PositionNormalColorSkinned:
                        return this.ShadowMapPositionNormalColorSkinnedInstanced;

                    case VertexTypes.PositionNormalTexture:
                        return this.ShadowMapPositionNormalTextureTransparentInstanced;
                    case VertexTypes.PositionNormalTextureSkinned:
                        return this.ShadowMapPositionNormalTextureTransparentSkinnedInstanced;

                    case VertexTypes.PositionNormalTextureTangent:
                        return this.ShadowMapPositionNormalTextureTangentTransparentInstanced;
                    case VertexTypes.PositionNormalTextureTangentSkinned:
                        return this.ShadowMapPositionNormalTextureTangentTransparentSkinnedInstanced;
                    default:
                        throw new EngineException(string.Format("Bad vertex type for effect. {0}; Instaced: {1}; Transparent", vertexType, instanced));
                }
            }
        }

        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWith">Animation palette texture width</param>
        public abstract void UpdateGlobals(
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth);
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="context">Context</param>
        public abstract void UpdatePerFrame(
            Matrix world,
            DrawContextShadows context);
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="animationOffset">Animation index</param>
        /// <param name="material">Material</param>
        /// <param name="textureIndex">Texture index</param>
        public abstract void UpdatePerObject(
            uint animationOffset,
            MeshMaterial material,
            uint textureIndex);
    }
}
