using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Game
{
    public class Powerup : Description2D
    {
        public static Color Color { get; private set; } = Color.OrangeRed;

        public string Rule { get; private set; }

        public Guid Id { get; private set; }

        public Powerup(string rule, int x, int y) : base(Sprite.Sprites["powerup"], x, y)
        {
            this.Rule = rule;
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(8, 8);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.FillEllipse(new SolidBrush(Color), 0, 0, 8, 8);

            return bmp;
        }

        public static Entity Create(string rule, int x, int y)
        {
            Powerup powup = new Powerup(rule, x, y);
            powup.DrawAction = powup.Draw;
            Entity entity = new Entity(powup);
            powup.Id = entity.Id;
            return entity;
        }
    }
}
