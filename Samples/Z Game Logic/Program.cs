﻿using Engine;
using Engine.Content.FmtCollada;
using SharpDX;
using System;
using System.IO;

namespace GameLogic
{
    static class Program
    {
        private static Game game = null;

        [STAThread]
        static void Main()
        {
            try
            {
#if DEBUG
                using (game = new Game("Game Logic", false, 1600, 900, true, 0, 0))
#else
                using (game = new Game("Game Logic", true, 0, 0, true, 0, 4))
#endif
                {
                    game.VisibleMouse = true;
                    game.LockMouse = false;

                    GameResourceManager.RegisterLoader<LoaderCollada>();

                    GameEnvironment.Background = Color.CornflowerBlue;

                    game.SetScene<SceneObjects>();

                    game.Run();
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText("dump.txt", ex.ToString());
            }
        }
    }
}
