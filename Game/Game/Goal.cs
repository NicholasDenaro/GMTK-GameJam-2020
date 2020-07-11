using GameEngine;
using GameEngine._2D;
using System;
using System.Drawing;

namespace Game
{
    public class Goal : Description2D
    {
        public Guid Id { get; private set; }
        public static Color Color { get; private set; } = Color.Cyan;

        public Goal(int x, int y) : base(Sprite.Sprites["goal"], x, y)
        {

        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(16, 16);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.FillPolygon(new SolidBrush(Color), new Point[] { new Point(0, 16), new Point(8, 0), new Point(16, 16) });

            return bmp;
        }

        public static Entity Create(int x, int y)
        {
            Goal goal = new Goal(x, y);
            goal.DrawAction = goal.Draw;
            Entity entity = new Entity(goal);
            goal.Id = entity.Id;
            return entity;
        }
    }
}
