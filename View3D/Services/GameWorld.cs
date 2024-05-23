﻿using Microsoft.Xna.Framework;
using MonoGame.Framework.WpfInterop;
using SharedCore.Events;

namespace View3D.Services
{
    public class GameWorld : WpfGame
    {
        private bool _disposed;
        WpfGraphicsDeviceService _deviceServiceHandle;

        public GameWorld(EventHub eventHub, string contentDir = "BuiltContent") : base(eventHub, contentDir)
        {

        }

        protected override void Initialize()
        {
            _disposed = false;
            _deviceServiceHandle = new WpfGraphicsDeviceService(this);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void Draw(GameTime time)
        {
            base.Draw(time);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            _disposed = true;

            _deviceServiceHandle = null;
            base.Dispose(disposing);
            Components.Clear();
        }
    }
}
