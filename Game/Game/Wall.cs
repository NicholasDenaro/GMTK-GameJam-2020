using GameEngine;
using GameEngine._2D;
using System;
using System.Drawing;

namespace Game
{
    public class Wall : Description2D
    {
        public static Color Color { get; private set; } = Color.DimGray;

        public Guid Id { get; private set; }

        public Wall(int x, int y) : base(Sprite.Sprites["wall"], x, y, 16, 16)
        {
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(16, 16);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.FillRectangle(new SolidBrush(Color), 0, 0, 16, 16);

            return bmp;
        }

        private void Tick(Location location, Entity entity)
        {
        }

        public static Entity Create(int x, int y)
        {
            Wall wall = new Wall(x, y);
            wall.DrawAction = wall.Draw;
            Entity entity = new Entity(wall);
            entity.TickAction = wall.Tick;
            wall.Id = entity.Id;
            return entity;
        }
    }
}
