using GameEngine;
using GameEngine._2D;
using System;
using System.Drawing;

namespace Game
{
    public class Enemy : Description2D
    {
        private double direction;
        private double xVel;
        private double yVel;

        public static Color Color { get; private set; } = Color.MediumVioletRed;

        public Guid Id { get; private set; }

        public Enemy(int x, int y) : base(Sprite.Sprites["enemy"], x, y)
        {
            direction = Program.Random.NextDouble();
            xVel = Math.Cos(direction * Math.PI * 2);
            yVel = Math.Sin(direction * Math.PI * 2);
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