﻿using System;
using SharpDX.Direct3D11;

namespace Common.Utils
{
    public class Drawable : IDisposable
    {
        public Game Game { get; private set; }
        public Graphics Graphics { get { return this.Game.Graphics; } }
        public DeviceContext DeviceContext { get { return this.Game.Graphics.DeviceContext; } }
        public string Name { get; set; }
        public bool Visible { get; set; }
        public bool Active { get; set; }
        public int Order { get; set; }

        public Drawable(Game game)
        {
            this.Game = game;
            this.Active = true;
            this.Visible = true;
            this.Order = 0;
        }
        public virtual void Update()
        {

        }
        public virtual void Draw()
        {

        }
        public virtual void Dispose()
        {

        }
        public override string ToString()
        {
            return string.Format("Type: {0}; Name: {1}; Order: {2}", this.GetType(), this.Name, this.Order);
        }
    }
}
