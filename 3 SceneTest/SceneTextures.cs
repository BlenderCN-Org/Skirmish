﻿using Engine;
using Engine.Animation;
using Engine.Common;
using Engine.Content;
using SharpDX;
using SharpDX.Direct3D;
using System;

namespace SceneTest
{
    public class SceneTextures : Scene
    {
        private float spaceSize = 40;

        private TextDrawer title = null;
        private TextDrawer runtime = null;

        private LensFlare lensFlare = null;

        private Model floorAsphalt = null;
        private ModelInstanced floorAsphaltI = null;

        private Model buildingObelisk = null;
        private ModelInstanced buildingObeliskI = null;

        private Model characterSoldier = null;
        private ModelInstanced characterSoldierI = null;

        private Model vehicleLeopard = null;
        private ModelInstanced vehicleLeopardI = null;

        private Model lamp = null;
        private ModelInstanced lampI = null;

        private Model streetlamp = null;
        private ModelInstanced streetlampI = null;

        private SkyScattering sky = null;

        private LineListDrawer lightsVolumeDrawer = null;

        public SceneTextures(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.Goto(-20, 10, -40f);
            this.Camera.LookTo(0, 0, 0);

            this.InitializeTextBoxes();
            this.InitializeSkyEffects();
            this.InitializeFloorAsphalt();
            this.InitializeBuildingObelisk();
            this.InitializeCharacterSoldier();
            this.InitializeVehiclesLeopard();
            this.InitializeLamps();
            this.InitializeStreetLamps();

            this.lightsVolumeDrawer = this.AddLineListDrawer(new LineListDrawerDescription() { AlwaysVisible = false, EnableDepthStencil = true }, 5000);

            this.TimeOfDay.BeginAnimation(new TimeSpan(4, 30, 00), 0.001f);

            this.SceneVolume = new BoundingSphere(Vector3.Zero, 150f);
        }

        private void InitializeTextBoxes()
        {
            this.title = this.AddText(TextDrawerDescription.Generate("Tahoma", 18, Color.White, Color.Orange));
            this.runtime = this.AddText(TextDrawerDescription.Generate("Tahoma", 10, Color.Yellow, Color.Orange));

            this.title.Text = "Scene Test - Textures";
            this.runtime.Text = "";

            this.title.Position = Vector2.Zero;
            this.runtime.Position = new Vector2(5, this.title.Top + this.title.Height + 3);
        }
        private void InitializeSkyEffects()
        {
            this.lensFlare = this.AddLensFlare(new LensFlareDescription()
            {
                Name = "Flares",
                ContentPath = @"Common/lensFlare",
                GlowTexture = "lfGlow.png",
                Flares = new[]
                {
                    new LensFlareDescription.Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 0.3f, 0.4f, new Color(100, 255, 200), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 1.2f, 1.0f, new Color(100,  50,  50), "lfFlare1.png"),
                    new LensFlareDescription.Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "lfFlare1.png"),

                    new LensFlareDescription.Flare(-0.3f, 0.7f, new Color(200,  50,  50), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "lfFlare2.png"),
                    new LensFlareDescription.Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "lfFlare2.png"),

                    new LensFlareDescription.Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "lfFlare3.png"),
                    new LensFlareDescription.Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "lfFlare3.png"),
                    new LensFlareDescription.Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "lfFlare3.png"),
                }
            });

            this.sky = this.AddSkyScattering(new SkyScatteringDescription() { Name = "Sky" });
        }
        private void InitializeFloorAsphalt()
        {
            float l = spaceSize;
            float h = 0f;

            VertexData[] vertices = new VertexData[]
            {
                new VertexData{ Position = new Vector3(-l, h, -l), Normal = Vector3.Up, Texture0 = new Vector2(0.0f, 0.0f) },
                new VertexData{ Position = new Vector3(-l, h, +l), Normal = Vector3.Up, Texture0 = new Vector2(0.0f, 1.0f) },
                new VertexData{ Position = new Vector3(+l, h, -l), Normal = Vector3.Up, Texture0 = new Vector2(1.0f, 0.0f) },
                new VertexData{ Position = new Vector3(+l, h, +l), Normal = Vector3.Up, Texture0 = new Vector2(1.0f, 1.0f) },
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            MaterialContent mat = MaterialContent.Default;
            mat.DiffuseTexture = "SceneTextures/floors/asphalt/d_road_asphalt_stripes_diffuse.dds";
            mat.NormalMapTexture = "SceneTextures/floors/asphalt/d_road_asphalt_stripes_normal.dds";
            mat.SpecularTexture = "SceneTextures/floors/asphalt/d_road_asphalt_stripes_specular.dds";

            var content = ModelContent.Generate(PrimitiveTopology.TriangleList, VertexTypes.PositionNormalTexture, vertices, indices, mat);

            var desc = new ModelDescription()
            {
                Name = "Floor",
                Static = true,
                CastShadow = true,
                AlwaysVisible = false,
                DeferredEnabled = true,
                EnableDepthStencil = true,
                EnableAlphaBlending = false,
            };

            var descI = new ModelInstancedDescription()
            {
                Name = "FloorI",
                Static = true,
                CastShadow = true,
                AlwaysVisible = false,
                DeferredEnabled = true,
                EnableDepthStencil = true,
                EnableAlphaBlending = false,
                Instances = 8,
            };

            this.floorAsphalt = this.AddModel(content, desc);

            this.floorAsphaltI = this.AddInstancingModel(content, descI);

            this.floorAsphaltI.Instances[0].Manipulator.SetPosition(-l * 2, 0, 0);
            this.floorAsphaltI.Instances[1].Manipulator.SetPosition(l * 2, 0, 0);
            this.floorAsphaltI.Instances[2].Manipulator.SetPosition(0, 0, -l * 2);
            this.floorAsphaltI.Instances[3].Manipulator.SetPosition(0, 0, l * 2);

            this.floorAsphaltI.Instances[4].Manipulator.SetPosition(-l * 2, 0, -l * 2);
            this.floorAsphaltI.Instances[5].Manipulator.SetPosition(l * 2, 0, -l * 2);
            this.floorAsphaltI.Instances[6].Manipulator.SetPosition(-l * 2, 0, l * 2);
            this.floorAsphaltI.Instances[7].Manipulator.SetPosition(l * 2, 0, l * 2);
        }
        private void InitializeBuildingObelisk()
        {
            this.buildingObelisk = this.AddModel(
                "SceneTextures/buildings/obelisk",
                "Obelisk.xml",
                new ModelDescription()
                {
                    Name = "Obelisk",
                    CastShadow = true,
                    Static = true,
                });

            this.buildingObeliskI = this.AddInstancingModel(
                "SceneTextures/buildings/obelisk",
                "Obelisk.xml",
                new ModelInstancedDescription()
                {
                    Name = "ObeliskI",
                    CastShadow = true,
                    Static = true,
                    Instances = 4,
                });

            this.buildingObelisk.Manipulator.SetPosition(0, 0, 0);
            this.buildingObelisk.Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.buildingObelisk.Manipulator.SetScale(10);

            this.buildingObeliskI.Instances[0].Manipulator.SetPosition(-spaceSize * 2, 0, 0);
            this.buildingObeliskI.Instances[1].Manipulator.SetPosition(spaceSize * 2, 0, 0);
            this.buildingObeliskI.Instances[2].Manipulator.SetPosition(0, 0, -spaceSize * 2);
            this.buildingObeliskI.Instances[3].Manipulator.SetPosition(0, 0, spaceSize * 2);

            this.buildingObeliskI.Instances[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.buildingObeliskI.Instances[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.buildingObeliskI.Instances[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.buildingObeliskI.Instances[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.buildingObeliskI.Instances[0].Manipulator.SetScale(10);
            this.buildingObeliskI.Instances[1].Manipulator.SetScale(10);
            this.buildingObeliskI.Instances[2].Manipulator.SetScale(10);
            this.buildingObeliskI.Instances[3].Manipulator.SetScale(10);
        }
        private void InitializeCharacterSoldier()
        {
            this.characterSoldier = this.AddModel(
                @"SceneTextures/character/soldier",
                @"soldier_anim2.xml",
                new ModelDescription()
                {
                    Name = "Soldier",
                    TextureIndex = 1,
                    CastShadow = true,
                    Static = false,
                });

            this.characterSoldierI = this.AddInstancingModel(
                @"SceneTextures/character/soldier",
                @"soldier_anim2.xml",
                new ModelInstancedDescription()
                {
                    Name = "SoldierI",
                    CastShadow = true,
                    Static = false,
                    Instances = 4,
                });

            float s = spaceSize / 2f;

            AnimationPath p1 = new AnimationPath();
            p1.AddLoop("idle1");

            this.characterSoldier.Manipulator.SetPosition(s - 10, 0, -s);
            this.characterSoldier.Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.characterSoldier.AnimationController.AddPath(p1);
            this.characterSoldier.AnimationController.Start(0);

            this.characterSoldierI.Instances[0].Manipulator.SetPosition(-spaceSize * 2 + s, 0, -s);
            this.characterSoldierI.Instances[1].Manipulator.SetPosition(spaceSize * 2 + s, 0, -s);
            this.characterSoldierI.Instances[2].Manipulator.SetPosition(s, 0, -spaceSize * 2 - s);
            this.characterSoldierI.Instances[3].Manipulator.SetPosition(s, 0, spaceSize * 2 - s);

            this.characterSoldierI.Instances[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.characterSoldierI.Instances[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.characterSoldierI.Instances[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.characterSoldierI.Instances[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.characterSoldierI.Instances[0].AnimationController.AddPath(p1);
            this.characterSoldierI.Instances[1].AnimationController.AddPath(p1);
            this.characterSoldierI.Instances[2].AnimationController.AddPath(p1);
            this.characterSoldierI.Instances[3].AnimationController.AddPath(p1);

            this.characterSoldierI.Instances[0].AnimationController.Start(1);
            this.characterSoldierI.Instances[1].AnimationController.Start(2);
            this.characterSoldierI.Instances[2].AnimationController.Start(3);
            this.characterSoldierI.Instances[3].AnimationController.Start(4);
        }
        private void InitializeVehiclesLeopard()
        {
            this.vehicleLeopard = this.AddModel(
                "SceneTextures/vehicles/leopard",
                "Leopard.xml",
                new ModelDescription()
                {
                    Name = "Leopard",
                    CastShadow = true,
                    Static = false,
                });

            this.vehicleLeopardI = this.AddInstancingModel(
                "SceneTextures/vehicles/leopard",
                "Leopard.xml",
                new ModelInstancedDescription()
                {
                    Name = "LeopardI",
                    CastShadow = true,
                    Static = false,
                    Instances = 4,
                });

            float s = -spaceSize / 2f;

            this.vehicleLeopard.Manipulator.SetPosition(s, 0, 0);
            this.vehicleLeopard.Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);

            this.vehicleLeopardI.Instances[0].Manipulator.SetPosition(-spaceSize * 2, 0, -spaceSize * 2);
            this.vehicleLeopardI.Instances[1].Manipulator.SetPosition(spaceSize * 2, 0, -spaceSize * 2);
            this.vehicleLeopardI.Instances[2].Manipulator.SetPosition(-spaceSize * 2, 0, spaceSize * 2);
            this.vehicleLeopardI.Instances[3].Manipulator.SetPosition(spaceSize * 2, 0, spaceSize * 2);

            this.vehicleLeopardI.Instances[0].Manipulator.SetRotation(MathUtil.PiOverTwo * 0, 0, 0);
            this.vehicleLeopardI.Instances[1].Manipulator.SetRotation(MathUtil.PiOverTwo * 1, 0, 0);
            this.vehicleLeopardI.Instances[2].Manipulator.SetRotation(MathUtil.PiOverTwo * 2, 0, 0);
            this.vehicleLeopardI.Instances[3].Manipulator.SetRotation(MathUtil.PiOverTwo * 3, 0, 0);

            this.Lights.AddRange(this.vehicleLeopard.Lights);
            this.Lights.AddRange(this.vehicleLeopardI.Instances[0].Lights);
            this.Lights.AddRange(this.vehicleLeopardI.Instances[1].Lights);
            this.Lights.AddRange(this.vehicleLeopardI.Instances[2].Lights);
            this.Lights.AddRange(this.vehicleLeopardI.Instances[3].Lights);
        }
        private void InitializeLamps()
        {
            this.lamp = this.AddModel(
                "SceneTextures/lamps",
                "lamp.xml",
                new ModelDescription()
                {
                    Name = "Lamp",
                    CastShadow = true,
                    Static = true,
                });

            this.lampI = this.AddInstancingModel(
                "SceneTextures/lamps",
                "lamp.xml",
                new ModelInstancedDescription()
                {
                    Name = "LampI",
                    CastShadow = true,
                    Static = true,
                    Instances = 4,
                });

            float dist = 0.23f;
            float pitch = MathUtil.DegreesToRadians(165) * -1;

            this.lamp.Manipulator.SetPosition(0, spaceSize, -spaceSize * dist, true);
            this.lamp.Manipulator.SetRotation(0, pitch, 0, true);

            this.lampI.Instances[0].Manipulator.SetPosition(-spaceSize * 2, spaceSize, -spaceSize * dist, true);
            this.lampI.Instances[1].Manipulator.SetPosition(spaceSize * 2, spaceSize, -spaceSize * dist, true);
            this.lampI.Instances[2].Manipulator.SetPosition(-spaceSize * dist, spaceSize, -spaceSize * 2, true);
            this.lampI.Instances[3].Manipulator.SetPosition(-spaceSize * dist, spaceSize, spaceSize * 2, true);

            this.lampI.Instances[0].Manipulator.SetRotation(0, pitch, 0, true);
            this.lampI.Instances[1].Manipulator.SetRotation(0, pitch, 0, true);
            this.lampI.Instances[2].Manipulator.SetRotation(MathUtil.PiOverTwo, pitch, 0, true);
            this.lampI.Instances[3].Manipulator.SetRotation(MathUtil.PiOverTwo, pitch, 0, true);

            this.Lights.AddRange(this.lamp.Lights);
            this.Lights.AddRange(this.lampI.Instances[0].Lights);
            this.Lights.AddRange(this.lampI.Instances[1].Lights);
            this.Lights.AddRange(this.lampI.Instances[2].Lights);
            this.Lights.AddRange(this.lampI.Instances[3].Lights);
        }
        private void InitializeStreetLamps()
        {
            this.streetlamp = this.AddModel(
                "SceneTextures/lamps",
                "streetlamp.xml",
                new ModelDescription()
                {
                    Name = "Street Lamp",
                    CastShadow = true,
                    Static = true,
                });

            this.streetlampI = this.AddInstancingModel(
                "SceneTextures/lamps",
                "streetlamp.xml",
                new ModelInstancedDescription()
                {
                    Name = "Street LampI",
                    CastShadow = true,
                    Static = true,
                    Instances = 9,
                });

            this.streetlamp.Manipulator.SetPosition(-spaceSize, 0, -spaceSize * -2f, true);
            this.streetlampI.Instances[0].Manipulator.SetPosition(-spaceSize, 0, -spaceSize * -1f, true);
            this.streetlampI.Instances[1].Manipulator.SetPosition(-spaceSize, 0, 0, true);
            this.streetlampI.Instances[2].Manipulator.SetPosition(-spaceSize, 0, -spaceSize * 1f, true);
            this.streetlampI.Instances[3].Manipulator.SetPosition(-spaceSize, 0, -spaceSize * 2f, true);

            this.streetlampI.Instances[4].Manipulator.SetPosition(+spaceSize, 0, -spaceSize * -2f, true);
            this.streetlampI.Instances[5].Manipulator.SetPosition(+spaceSize, 0, -spaceSize * -1f, true);
            this.streetlampI.Instances[6].Manipulator.SetPosition(+spaceSize, 0, 0, true);
            this.streetlampI.Instances[7].Manipulator.SetPosition(+spaceSize, 0, -spaceSize * 1f, true);
            this.streetlampI.Instances[8].Manipulator.SetPosition(+spaceSize, 0, -spaceSize * 2f, true);

            this.streetlampI.Instances[4].Manipulator.SetRotation(MathUtil.Pi, 0, 0, true);
            this.streetlampI.Instances[5].Manipulator.SetRotation(MathUtil.Pi, 0, 0, true);
            this.streetlampI.Instances[6].Manipulator.SetRotation(MathUtil.Pi, 0, 0, true);
            this.streetlampI.Instances[7].Manipulator.SetRotation(MathUtil.Pi, 0, 0, true);
            this.streetlampI.Instances[8].Manipulator.SetRotation(MathUtil.Pi, 0, 0, true);

            this.Lights.AddRange(this.streetlamp.Lights);
            this.Lights.AddRange(this.streetlampI.Instances[0].Lights);
            this.Lights.AddRange(this.streetlampI.Instances[1].Lights);
            this.Lights.AddRange(this.streetlampI.Instances[2].Lights);
            this.Lights.AddRange(this.streetlampI.Instances[3].Lights);
            this.Lights.AddRange(this.streetlampI.Instances[4].Lights);
            this.Lights.AddRange(this.streetlampI.Instances[5].Lights);
            this.Lights.AddRange(this.streetlampI.Instances[6].Lights);
            this.Lights.AddRange(this.streetlampI.Instances[7].Lights);
            this.Lights.AddRange(this.streetlampI.Instances[8].Lights);
        }

        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            bool shift = this.Game.Input.KeyPressed(Keys.LShiftKey);
            bool rightBtn = this.Game.Input.RightMouseButtonPressed;

            #region Camera

            this.UpdateCamera(gameTime, shift, rightBtn);

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F1))
            {
                this.lightsVolumeDrawer.Clear();

                foreach (var spot in this.Lights.SpotLights)
                {
                    var lines = spot.GetVolume();

                    this.lightsVolumeDrawer.AddLines(new Color4(spot.DiffuseColor.RGB(), 0.15f), lines);
                }

                this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F2))
            {
                this.lightsVolumeDrawer.Clear();

                foreach (var point in this.Lights.PointLights)
                {
                    var lines = point.GetVolume();

                    this.lightsVolumeDrawer.AddLines(new Color4(point.DiffuseColor.RGB(), 0.15f), lines);
                }

                this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = true;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F3))
            {
                this.lightsVolumeDrawer.Active = this.lightsVolumeDrawer.Visible = false;
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.RenderMode = this.RenderMode == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning;
            }

            #endregion

            base.Update(gameTime);

            this.runtime.Text = this.Game.RuntimeText;
        }

        private void UpdateCamera(GameTime gameTime, bool shift, bool rightBtn)
        {
#if DEBUG
            if (rightBtn)
#endif
            {
                this.Camera.RotateMouse(
                    gameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, shift);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, shift);
            }
        }
    }
}
