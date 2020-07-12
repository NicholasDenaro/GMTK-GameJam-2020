using GameEngine;
using GameEngine._2D;
using System;
using System.Drawing;

namespace Game
{
    public class Enemy : Description2D, IIdentifiable
    {
        public double MoveDirection { get; private set; }
        public double VelX { get; internal set; }
        public double VelY { get; internal set; }

        public static Color Color { get; private set; } = Color.MediumVioletRed;

        public Guid Id { get; private set; }

        public Enemy(int x, int y) : base(Sprite.Sprites["enemy"], x, y, 16, 16)
        {
            MoveDirection = Program.Random.NextDouble() * Math.PI * 2;
            VelX = Math.Cos(MoveDirection);
            VelY = Math.Sin(MoveDirection);
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(16, 16);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.FillPolygon(new SolidBrush(Color), new Point[] { new Point(0, 16), new Point(8, 0), new Point(16, 16), new Point(0, 8), new Point(16, 8) });

            return bmp;
        }

        private void Tick(Location location, Entity entity)
        {
        }

        public static Entity Create(int x, int y)
        {
            Enemy enemy = new Enemy(x, y);
            enemy.DrawAction = enemy.Draw;
            Entity entity = new Entity(enemy);
            entity.TickAction = enemy.Tick;
            enemy.Id = entity.Id;
            return entity;
        }
    }
}