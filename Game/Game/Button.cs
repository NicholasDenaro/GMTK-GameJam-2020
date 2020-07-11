using GameEngine;
using GameEngine._2D;
using GameEngine.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Game
{
    public class Button : Description2D
    {
        private string text;
        private Action action;

        public Button(string text, Action action, int x, int y) : base(Sprite.Sprites["button"], x, y, 128, 48)
        {
            this.text = text;
            this.action = action;
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(this.Width, this.Height);
            Graphics gfx = Graphics.FromImage(bmp);

            Font f = new Font("Arial", 12, FontStyle.Bold);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            gfx.FillRectangle(Brushes.White, 0, 0, this.Width, this.Height);
            gfx.DrawRectangle(Pens.Black, 0, 0, this.Width - 1, this.Height - 1);
            gfx.DrawString(text, f, Brushes.Black, this.Width / 2, this.Height / 2, format);

            return bmp;
        }

        private void Tick(Location location, Entity entity)
        {
            if (Program.Mouse[(int)Program.Actions.ACTION].IsPress())
            {
                MouseControllerInfo mci = Program.Mouse[(int)Program.Actions.ACTION].Info as MouseControllerInfo;
                if (mci.X > this.X && mci.X < this.X + this.Width
                    && mci.Y > this.Y && mci.Y < this.Y + this.Height)
                {
                    action();
                }
            }
        }

        public static Entity Create(string text, Action action, int x, int y)
        {
            Button enemy = new Button(text, action, x, y);
            enemy.DrawAction = enemy.Draw;
            Entity entity = new Entity(enemy);
            entity.TickAction = enemy.Tick;
            return entity;
        }
    }
}
