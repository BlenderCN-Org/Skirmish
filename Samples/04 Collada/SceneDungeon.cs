﻿using Engine;
using Engine.Content;
using Engine.PathFinding.RecastNavigation;
using SharpDX;

namespace Collada
{
    public class SceneDungeon : Scene
    {
        private const int layerHUD = 99;

        private SceneObject<TextDrawer> fps = null;

        private Player agent = null;

        public SceneDungeon(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

#if DEBUG
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = false;
#else
            this.Game.VisibleMouse = false;
            this.Game.LockMouse = true;
#endif

            var title = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Tahoma", 18, Color.White), SceneObjectUsages.UI, layerHUD);
            title.Instance.Text = "Collada Dungeon Scene";
            title.Instance.Position = Vector2.Zero;

            this.fps = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            this.fps.Instance.Text = null;
            this.fps.Instance.Position = new Vector2(0, 24);

            var picks = this.AddComponent<TextDrawer>(TextDrawerDescription.Generate("Lucida Casual", 12, Color.Yellow), SceneObjectUsages.UI, layerHUD);
            picks.Instance.Text = null;
            picks.Instance.Position = new Vector2(0, 48);

            var spDesc = new SpriteDescription()
            {
                AlphaEnabled = true,
                Width = this.Game.Form.RenderWidth,
                Height = picks.Instance.Top + picks.Instance.Height + 3,
                Color = new Color4(0, 0, 0, 0.75f),
            };

            this.AddComponent<Sprite>(spDesc, SceneObjectUsages.UI, layerHUD - 1);

            this.agent = new Player()
            {
                Name = "Player",
                Height = 0.5f,
                Radius = 0.15f,
                MaxClimb = 0.225f,
            };

            var dungeon = this.AddComponent<Scenery>(
                new GroundDescription()
                {
                    Name = "room1",
                    Quadtree = new GroundDescription.QuadtreeDescription()
                    {
                        MaximumDepth = 2,
                    },
                    Content = new ContentDescription()
                    {
                        ContentFolder = "Resources/SceneDungeon",
                        ModelContentFilename = "Dungeon.xml",
                    },
                });

            this.SetGround(dungeon, true);

            var settings = new BuildSettings()
            {
                Agents = new[] { agent },
            };

            var input = new InputGeometry(GetTrianglesForNavigationGraph);

            this.PathFinderDescription = new Engine.PathFinding.PathFinderDescription(settings, input);

            this.Lights.AddRange(dungeon.Instance.Lights);

            this.InitializeCamera();
            this.InitializeEnvironment();
        }
        private void InitializeCamera()
        {
            this.Camera.NearPlaneDistance = 0.1f;
            this.Camera.FarPlaneDistance = 500;
            this.Camera.MovementDelta = this.agent.Velocity;
            this.Camera.SlowMovementDelta = this.agent.VelocitySlow;
            this.Camera.Mode = CameraModes.Free;
            this.Camera.Position = new Vector3(0, this.agent.Height, 0);
            this.Camera.Interest = new Vector3(0, this.agent.Height, 1);
        }
        private void InitializeEnvironment()
        {
            GameEnvironment.Background = Color.DarkGray;

            this.Lights.KeyLight.Enabled = false;
            this.Lights.BackLight.Enabled = false;
            this.Lights.FillLight.Enabled = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.Game.Input.KeyJustReleased(Keys.Escape))
            {
                this.Game.SetScene<SceneStart>();
            }

            this.UpdateCamera();

            this.fps.Instance.Text = this.Game.RuntimeText;
        }
        private void UpdateCamera()
        {
            bool slow = this.Game.Input.KeyPressed(Keys.LShiftKey);

            var prevPos = this.Camera.Position;

            if (this.Game.Input.KeyPressed(Keys.A))
            {
                this.Camera.MoveLeft(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.D))
            {
                this.Camera.MoveRight(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.W))
            {
                this.Camera.MoveForward(this.Game.GameTime, slow);
            }

            if (this.Game.Input.KeyPressed(Keys.S))
            {
                this.Camera.MoveBackward(this.Game.GameTime, slow);
            }

#if DEBUG
            if (this.Game.Input.RightMouseButtonPressed)
            {
                this.Camera.RotateMouse(
                    this.Game.GameTime,
                    this.Game.Input.MouseXDelta,
                    this.Game.Input.MouseYDelta);
            }
#else
            this.Camera.RotateMouse(
                this.Game.GameTime,
                this.Game.Input.MouseXDelta,
                this.Game.Input.MouseYDelta);
#endif

            if (this.Walk(this.agent, prevPos, this.Camera.Position, out Vector3 walkerPos))
            {
                this.Camera.Goto(walkerPos);
            }
            else
            {
                this.Camera.Goto(prevPos);
            }
        }
    }
}
