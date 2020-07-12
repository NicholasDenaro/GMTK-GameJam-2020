using GameEngine;
using GameEngine._2D;
using GameEngine.UI;
using System;
using System.Drawing;
using System.Linq;
using static Game.Program;

namespace Game
{
    public class Player : Description2D
    {
        public static Color Color { get; private set; } = Color.White;

        private Player(int x, int y) : base(Sprite.Sprites["player"], x, y, 16, 16)
        {
        }

        public void Tick(Location location, Entity entity)
        {
        }

        public Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(16, 16);
            Graphics gfx = Graphics.FromImage(bmp);

            Color color = Color;

            if (Program.Iframe > 0)
            {
                color = Color.PaleVioletRed;
            }

            gfx.FillEllipse(new SolidBrush(color), 0, 0, 16, 16);

            return bmp;
        }

        public static Entity Create(int x, int y)
        {
            Player player = new Player(x , y);
            player.DrawAction = player.Draw;
            Entity entity = new Entity(player);
            entity.TickAction = player.Tick;
            return entity;
        }
    }
}
