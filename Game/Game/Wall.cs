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

        public Wall(int x, int y, int w, int h) : base(Sprite.Sprites["wall"], x, y, w, h)
        {
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(this.Width, this.Height);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.FillRectangle(new SolidBrush(Color), 0, 0, this.Width, this.Height);

            return bmp;
        }

        private void Tick(Location location, Entity entity)
        {
        }

        public static Entity Create(int x, int y, int w = 16, int h = 16)
        {
            Wall wall = new Wall(x, y, w, h);
            wall.DrawAction = wall.Draw;
            Entity entity = new Entity(wall);
            entity.TickAction = wall.Tick;
            wall.Id = entity.Id;
            return entity;
        }
    }
}
