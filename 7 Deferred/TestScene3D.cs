﻿using Engine;
using Engine.Common;
using Engine.PathFinding.AStar;
using SharpDX;
using System;
using System.Diagnostics;

namespace DeferredTest
{
    public class TestScene3D : Scene
    {
        private string titleMask = "{0}: {1} directionals, {2} points and {3} spots. Shadows {4}";

        private const float near = 0.1f;
        private const float far = 1000f;
        private const float fogStart = 0.01f;
        private const float fogRange = 0.10f;

        private TextDrawer title = null;
        private TextDrawer load = null;
        private TextDrawer help = null;
        private TextDrawer statistics = null;

        private Model tank = null;
        private Model helicopter = null;
        private ModelInstanced helicopters = null;
        private Skydom skydom = null;
        private Terrain terrain = null;
        private ParticleSystem fire = null;

        private SpriteTexture bufferDrawer = null;
        private int textIntex = 0;
        private bool animateLights = false;
        private SceneLightSpot spotLight = null;

        private LineListDrawer lineDrawer = null;

        private Random rnd = new Random(0);

        public TestScene3D(Game game)
            : base(game, SceneModesEnum.DeferredLightning)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            this.Camera.NearPlaneDistance = near;
            this.Camera.FarPlaneDistance = far;

            #region Models

            string resources = @"Resources";

            Stopwatch sw = Stopwatch.StartNew();

            string loadingText = null;

            #region Skydom

            sw.Restart();
            this.skydom = this.AddSkydom(new SkydomDescription()
            {
                ContentPath = resources,
                Radius = far,
                Texture = "sunset.dds",
            });
            sw.Stop();
            loadingText += string.Format("skydom: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Terrain

            sw.Restart();
            this.terrain = this.AddTerrain(new TerrainDescription()
            {
                ContentPath = resources,
                Model = new TerrainDescription.ModelDescription()
                {
                    ModelFileName = "terrain.dae",
                },
                Quadtree = new TerrainDescription.QuadtreeDescription()
                {
                    MaxTrianglesPerNode = 2048,
                },
                PathFinder = new TerrainDescription.PathFinderDescription()
                {
                    Settings = new GridGenerationSettings()
                    {
                        NodeSize = 2f,
                        NodeInclination = MathUtil.DegreesToRadians(35),
                    },
                },
                Vegetation = new TerrainDescription.VegetationDescription()
                {
                    VegetarionTextures = new[] { "grass.png" },
                    Saturation = 20f,
                    StartRadius = 0f,
                    EndRadius = 50f,
                    MinSize = Vector2.One * 0.20f,
                    MaxSize = Vector2.One * 0.25f,
                },
            });
            sw.Stop();
            loadingText += string.Format("terrain: {0} ", sw.Elapsed.TotalSeconds);

            this.SceneVolume = this.terrain.GetBoundingSphere();

            #endregion

            #region Helicopter

            sw.Restart();
            this.helicopter = this.AddModel(new ModelDescription()
            {
                ContentPath = resources,
                ModelFileName = "helicopter.dae",
                Opaque = true,
                TextureIndex = 2,
            });
            sw.Stop();
            loadingText += string.Format("helicopter: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Helicopters

            sw.Restart();
            this.helicopters = this.AddInstancingModel(new ModelInstancedDescription()
            {
                ContentPath = resources,
                ModelFileName = "helicopter.dae",
                Opaque = true,
                Instances = 2,
            });
            sw.Stop();
            loadingText += string.Format("helicopters: {0} ", sw.Elapsed.TotalSeconds);

            #endregion

            #region Tank

            sw.Restart();
            this.tank = this.AddModel(new ModelDescription()
            {
                ContentPath = resources,
                ModelFileName = "leopard.dae",
                Opaque = true,
            });
            sw.Stop();
            loadingText += string.Format("tank: {0} ", sw.Elapsed.TotalSeconds);

            this.tank.Manipulator.SetScale(3);

            #endregion

            #region Moving fire

            var fireEmitter = new ParticleEmitter()
            {
                Color = Color.Yellow,
                Size = 0.5f,
                Position = new Vector3(0, 10, 0),
            };
            this.fire = this.AddParticleSystem(ParticleSystemDescription.Fire(fireEmitter, "flare2.png"));

            #endregion

            #endregion

            #region Debug Buffer Drawer

            int width = (int)(this.Game.Form.RenderWidth * 0.33f);
            int height = (int)(this.Game.Form.RenderHeight * 0.33f);
            int smLeft = this.Game.Form.RenderWidth - width;
            int smTop = this.Game.Form.RenderHeight - height;

            this.bufferDrawer = this.AddSpriteTexture(new SpriteTextureDescription()
            {
                Left = smLeft,
                Top = smTop,
                Width = width,
                Height = height,
                Channel = SpriteTextureChannelsEnum.NoAlpha,
            });

            this.bufferDrawer.Visible = false;

            #endregion

            #region Texts

            this.title = this.AddText("Tahoma", 18, Color.White);
            this.load = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.help = this.AddText("Lucida Casual", 12, Color.Yellow);
            this.statistics = this.AddText("Lucida Casual", 10, Color.Red);

            this.title.Text = "Deferred Ligthning test";
            this.load.Text = loadingText;
            this.help.Text = "";
            this.statistics.Text = "";

            this.title.Position = Vector2.Zero;
            this.load.Position = new Vector2(0, this.title.Top + this.title.Height + 2);
            this.help.Position = new Vector2(0, this.load.Top + this.load.Height + 2);
            this.statistics.Position = new Vector2(0, this.help.Top + this.help.Height + 2);

            #endregion

            #region Object locations

            Vector3 cameraPosition = Vector3.Zero;
            int modelCount = 0;

            Vector3 tankPosition;
            Triangle tankTriangle;
            if (this.terrain.FindTopGroundPosition(0, 0, out tankPosition, out tankTriangle))
            {
                //Inclination
                this.tank.Manipulator.SetPosition(tankPosition, true);
                this.tank.Manipulator.SetNormal(tankTriangle.Normal);
                cameraPosition += tankPosition;
                modelCount++;
            }

            Vector3 helicopterPosition;
            if (this.terrain.FindTopGroundPosition(20, -20, out helicopterPosition))
            {
                helicopterPosition.Y += 10f;
                this.helicopter.Manipulator.SetPosition(helicopterPosition, true);
                cameraPosition += helicopterPosition;
                modelCount++;
            }

            for (int i = 0; i < this.helicopters.Count; i++)
            {
                Vector3 heliPos;
                if (this.terrain.FindTopGroundPosition((i * 10) - 20, 20, out heliPos))
                {
                    heliPos.Y += 10f;
                    this.helicopters.Instances[i].Manipulator.SetPosition(heliPos, true);
                    cameraPosition += heliPos;
                    modelCount++;
                }
            }

            cameraPosition /= (float)modelCount;
            this.Camera.Goto(cameraPosition + new Vector3(-30, 30, -30));
            this.Camera.LookTo(cameraPosition + Vector3.Up);

            #endregion

            #region Lights

            //SceneLightDirectional primary = SceneLightDirectional.Primary;
            SceneLightDirectional primary = new SceneLightDirectional()
            {
                Name = "night has come",
                Enabled = true,
                LightColor = Color.LightBlue,
                AmbientIntensity = 0.1f,
                DiffuseIntensity = 0.1f,
                Direction = SceneLightDirectional.Primary.Direction,
                CastShadow = false,
            };

            //this.Lights.ClearDirectionalLights();
            //this.Lights.Add(primary);

            this.Lights.FogColor = Color.LightGray;
            this.Lights.FogStart = far * fogStart;
            this.Lights.FogRange = far * fogRange;

            #region Light Sphere Marker

            Line3[] axis = Line3.CreateAxis(Matrix.Identity, 5f);

            this.lineDrawer = this.AddLineListDrawer(axis, Color.Red);
            this.lineDrawer.Opaque = false;
            this.lineDrawer.Active = false;
            this.lineDrawer.Visible = false;

            #endregion

            #endregion
        }
        public override void Update(GameTime gameTime)
        {
            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.Exit();
            }

            if (this.Game.Input.KeyJustReleased(Keys.R))
            {
                this.RenderMode = this.RenderMode == SceneModesEnum.ForwardLigthning ?
                    SceneModesEnum.DeferredLightning :
                    SceneModesEnum.ForwardLigthning;
            }

            base.Update(gameTime);

            Ray cursorRay = this.GetPickingRay();

            #region Cursor picking and positioning

            Vector3 position;
            Triangle triangle;
            bool picked = this.terrain.PickNearest(ref cursorRay, out position, out triangle);

            #endregion

            #region Debug

            if (this.Game.Input.KeyJustReleased(Keys.F12))
            {
                if (this.bufferDrawer.Manipulator.Position == Vector2.Zero)
                {
                    int width = (int)(this.Game.Form.RenderWidth * 0.33f);
                    int height = (int)(this.Game.Form.RenderHeight * 0.33f);
                    int smLeft = this.Game.Form.RenderWidth - width;
                    int smTop = this.Game.Form.RenderHeight - height;

                    this.bufferDrawer.Manipulator.SetPosition(smLeft, smTop);
                    this.bufferDrawer.ResizeSprite(width, height);
                }
                else
                {
                    this.bufferDrawer.Manipulator.SetPosition(Vector2.Zero);
                    this.bufferDrawer.ResizeSprite(this.Game.Form.RenderWidth, this.Game.Form.RenderHeight);
                }
            }

            {
                if (this.Game.Input.KeyJustReleased(Keys.F1))
                {
                    var colorMap = this.Renderer.GetResource(SceneRendererResultEnum.ColorMap);

                    if (this.bufferDrawer.Texture == colorMap &&
                        this.bufferDrawer.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Specular Factor
                        this.bufferDrawer.Texture = colorMap;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Text = "Specular Factor";
                    }
                    else
                    {
                        //Colors
                        this.bufferDrawer.Texture = colorMap;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Text = "Colors";
                    }
                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F2))
                {
                    var normalMap = this.Renderer.GetResource(SceneRendererResultEnum.NormalMap);

                    if (this.bufferDrawer.Texture == normalMap &&
                        this.bufferDrawer.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Specular Power
                        this.bufferDrawer.Texture = normalMap;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Text = "Specular Power";
                    }
                    else
                    {
                        //Normals
                        this.bufferDrawer.Texture = normalMap;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Text = "Normals";
                    }
                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F3))
                {
                    var depthMap = this.Renderer.GetResource(SceneRendererResultEnum.DepthMap);

                    if (this.bufferDrawer.Texture == depthMap &&
                        this.bufferDrawer.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Depth
                        this.bufferDrawer.Texture = depthMap;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Text = "Depth";
                    }
                    else
                    {
                        //Position
                        this.bufferDrawer.Texture = depthMap;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Text = "Position";
                    }
                    this.bufferDrawer.Visible = true;
                }

                if (this.Game.Input.KeyJustReleased(Keys.F4))
                {
                    var other = this.Renderer.GetResource(SceneRendererResultEnum.Other);

                    if (this.bufferDrawer.Texture == other &&
                        this.bufferDrawer.Channels == SpriteTextureChannelsEnum.NoAlpha)
                    {
                        //Specular intensity
                        this.bufferDrawer.Texture = other;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Alpha;
                        this.help.Text = "Specular Intensity";
                    }
                    else
                    {
                        //Shadow positions
                        this.bufferDrawer.Texture = other;
                        this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                        this.help.Text = "Shadow Positions";
                    }
                    this.bufferDrawer.Visible = true;
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F5))
            {
                var shadowMap = this.Renderer.GetResource(SceneRendererResultEnum.ShadowMap);

                if (shadowMap != null)
                {
                    //Shadow map
                    this.bufferDrawer.Texture = shadowMap;
                    this.bufferDrawer.Channels = SpriteTextureChannelsEnum.Red;
                    this.bufferDrawer.Visible = true;
                    this.help.Text = "Shadow map";
                }
                else
                {
                    this.help.Text = "The Shadow map is empty";
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F6))
            {
                var lightMap = this.Renderer.GetResource(SceneRendererResultEnum.LightMap);

                if (lightMap != null)
                {
                    //Light map
                    this.bufferDrawer.Texture = lightMap;
                    this.bufferDrawer.Channels = SpriteTextureChannelsEnum.NoAlpha;
                    this.bufferDrawer.Visible = true;
                    this.help.Text = "Light map";
                }
                else
                {
                    this.help.Text = "The Light map is empty";
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.F7))
            {
                this.bufferDrawer.Visible = !this.bufferDrawer.Visible;
                this.help.Visible = this.bufferDrawer.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F8))
            {
                this.terrain.Active = this.terrain.Visible = !this.terrain.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F9))
            {
                this.tank.Active = this.tank.Visible = !this.tank.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F10))
            {
                this.helicopter.Active = this.helicopter.Visible = !this.helicopter.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.F11))
            {
                this.helicopters.Active = this.helicopters.Visible = !this.helicopters.Visible;
            }

            if (this.Game.Input.KeyJustReleased(Keys.Oemcomma))
            {
                this.textIntex--;
            }

            if (this.Game.Input.KeyJustReleased(Keys.OemPeriod))
            {
                this.textIntex++;
            }

            if (this.Game.Input.KeyJustReleased(Keys.T))
            {
                this.helicopter.TextureIndex++;

                if (this.helicopter.TextureIndex >= this.helicopter.TextureCount)
                {
                    //Loop
                    this.helicopter.TextureIndex = 0;
                }
            }

            #endregion

            #region Tank

            if (this.Game.Input.LeftMouseButtonPressed)
            {
                if (picked)
                {
                    var p = this.terrain.FindPath(this.tank.Manipulator.Position, position);
                    if (p != null)
                    {
                        this.tank.Manipulator.Follow(p.GenerateBezierPath(), 0.2f);
                    }
                }
            }

            if (this.tank.Manipulator.IsFollowingPath)
            {
                Vector3 pos = this.tank.Manipulator.Position;

                Vector3 tankPosition;
                Triangle tankTriangle;
                if (this.terrain.FindTopGroundPosition(pos.X, pos.Z, out tankPosition, out tankTriangle))
                {
                    this.tank.Manipulator.SetNormal(tankTriangle.Normal);
                }
            }

            #endregion

            #region Camera

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
#endif
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(gameTime, this.Game.Input.ShiftPressed);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(gameTime, this.Game.Input.ShiftPressed);
            }

            #endregion

            #region Lights

            if (this.Game.Input.KeyJustReleased(Keys.F))
            {
                this.Lights.FogStart = this.Lights.FogStart == 0f ? far * fogStart : 0f;
                this.Lights.FogRange = this.Lights.FogRange == 0f ? far * fogRange : 0f;
            }

            if (this.Game.Input.KeyJustReleased(Keys.G))
            {
                this.Lights.DirectionalLights[0].CastShadow = !this.Lights.DirectionalLights[0].CastShadow;
            }

            if (this.Game.Input.KeyJustReleased(Keys.K))
            {
                if (this.spotLight == null)
                {
                    this.CreateSpotLights();
                }
                else
                {
                    this.ClearSpotLights();
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.L))
            {
                if (this.Lights.EnabledPointLights.Length > 0)
                {
                    this.ClearPointLigths();
                }
                else
                {
                    this.CreatePointLigths();
                }
            }

            if (this.Game.Input.KeyJustReleased(Keys.P))
            {
                this.animateLights = !this.animateLights;
            }

            if (this.spotLight != null)
            {
                if (this.Game.Input.KeyPressed(Keys.Left))
                {
                    this.spotLight.Position += (Vector3.Left) * gameTime.ElapsedSeconds * 10f;
                }

                if (this.Game.Input.KeyPressed(Keys.Right))
                {
                    this.spotLight.Position += (Vector3.Right) * gameTime.ElapsedSeconds * 10f;
                }

                if (this.Game.Input.KeyPressed(Keys.Up))
                {
                    this.spotLight.Position += (Vector3.ForwardLH) * gameTime.ElapsedSeconds * 10f;
                }

                if (this.Game.Input.KeyPressed(Keys.Down))
                {
                    this.spotLight.Position += (Vector3.BackwardLH) * gameTime.ElapsedSeconds * 10f;
                }

                if (this.Game.Input.KeyPressed(Keys.PageUp))
                {
                    this.spotLight.Position += (Vector3.Up) * gameTime.ElapsedSeconds * 10f;
                }

                if (this.Game.Input.KeyPressed(Keys.PageDown))
                {
                    this.spotLight.Position += (Vector3.Down) * gameTime.ElapsedSeconds * 10f;
                }

                if (this.Game.Input.KeyPressed(Keys.Add))
                {
                    this.spotLight.DiffuseIntensity += gameTime.ElapsedSeconds * 10f;
                }

                if (this.Game.Input.KeyPressed(Keys.Subtract))
                {
                    this.spotLight.DiffuseIntensity -= gameTime.ElapsedSeconds * 10f;

                    this.spotLight.DiffuseIntensity = Math.Max(0f, this.spotLight.DiffuseIntensity);
                }

                this.lineDrawer.Manipulator.SetPosition(this.spotLight.Position);
                this.lineDrawer.Manipulator.LookAt(this.spotLight.Position + this.spotLight.Direction, false);
            }
            else
            {
                this.lineDrawer.Visible = false;
            }

            if (animateLights)
            {
                if (this.Lights.EnabledPointLights.Length > 0)
                {
                    for (int i = 1; i < this.Lights.EnabledPointLights.Length; i++)
                    {
                        var l = this.Lights.EnabledPointLights[i];

                        if ((int)l.State == 1) l.Radius += (0.5f * gameTime.ElapsedSeconds * 50f);
                        if ((int)l.State == -1) l.Radius -= (2f * gameTime.ElapsedSeconds * 50f);

                        if (l.Radius <= 0)
                        {
                            l.Radius = 0;
                            l.State = 1;
                        }

                        if (l.Radius >= 50)
                        {
                            l.Radius = 50;
                            l.State = -1;
                        }

                        l.DiffuseIntensity = l.Radius * 0.1f;
                    }
                }
            }

            #endregion
        }
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (this.Game.Form.IsFullscreen)
            {
                this.load.Text = this.Game.RuntimeText;
            }

            this.title.Text = string.Format(
                this.titleMask,
                this.RenderMode,
                this.Lights.EnabledDirectionalLights.Length,
                this.Lights.EnabledPointLights.Length,
                this.Lights.EnabledSpotLights.Length,
                this.Lights.ShadowCastingLights.Length);

            if (Counters.Statistics.Length == 0)
            {
                this.statistics.Text = "No statistics";
            }
            else if (this.textIntex < 0)
            {
                this.statistics.Text = "Press . for more statistics";
                this.textIntex = -1;
            }
            else if (this.textIntex >= Counters.Statistics.Length)
            {
                this.statistics.Text = "Press , for more statistics";
                this.textIntex = Counters.Statistics.Length;
            }
            else
            {
                this.statistics.Text = string.Format(
                    "{0} - {1}",
                    Counters.Statistics[this.textIntex],
                    Counters.GetStatistics(this.textIntex));
            }
        }

        private void CreateSpotLights()
        {
            this.Lights.ClearSpotLights();

            Vector3 lightPosition;
            if (this.terrain.FindTopGroundPosition(0, 1, out lightPosition))
            {
                lightPosition.Y += 10f;

                Vector3 direction = -Vector3.Normalize(lightPosition);

                this.spotLight = new SceneLightSpot()
                {
                    Name = "Spot the dog",
                    LightColor = Color.Yellow,
                    AmbientIntensity = 0.2f,
                    DiffuseIntensity = 25f,
                    Position = lightPosition,
                    Direction = direction,
                    Angle = 25,
                    Radius = 25,
                    Enabled = true,
                    CastShadow = false,
                };

                this.Lights.Add(this.spotLight);

                this.lineDrawer.Active = true;
                this.lineDrawer.Visible = true;
            }
        }
        private void ClearSpotLights()
        {
            this.Lights.ClearSpotLights();

            this.spotLight = null;

            this.lineDrawer.Active = false;
            this.lineDrawer.Visible = false;
        }

        private void CreatePointLigths()
        {
            this.Lights.ClearPointLights();

            int sep = 2;
            int f = 12;
            int l = (f - 1) * sep;

            for (int i = 0; i < f; i++)
            {
                for (int x = 0; x < f; x++)
                {
                    Vector3 lightPosition;
                    if (!this.terrain.FindTopGroundPosition((i * sep) - l, (x * sep) - l, out lightPosition))
                    {
                        lightPosition = new Vector3((i * sep) - l, 1f, (x * sep) - l);
                    }
                    else
                    {
                        lightPosition.Y += 1f;
                    }

                    SceneLightPoint pointLight = new SceneLightPoint()
                    {
                        Name = string.Format("Point {0}", this.Lights.PointLights.Length),
                        Enabled = true,
                        LightColor = new Color4(rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), rnd.NextFloat(0, 1), 1.0f),
                        AmbientIntensity = 0.1f,
                        DiffuseIntensity = 5f,
                        Position = lightPosition,
                        Radius = 5f,
                        State = rnd.NextFloat(0, 1) >= 0.5f ? 1 : -1,
                    };

                    this.Lights.Add(pointLight);

                    //if (this.Lights.PointLights.Length >= 4) return;
                }
            }
        }
        private void ClearPointLigths()
        {
            this.Lights.ClearPointLights();
        }
    }
}
