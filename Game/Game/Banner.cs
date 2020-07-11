using GameEngine;
using GameEngine._2D;
using System.Drawing;

namespace Game
{
    public class Banner : Description2D
    {
        public string Text { get; set; }

        public Banner(string text) : base(Sprite.Sprites["banner"], 0, Program.ScreenHeight / 2)
        {
            this.Text = text;
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(Program.ScreenWidth, 16);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.FillRectangle(Brushes.Gray, 0, 0, Program.ScreenWidth, 16);
            Font f = new Font("Arial", 12);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            gfx.DrawString(Text, f, Brushes.Black, Program.ScreenWidth / 2, 0, format);

            return bmp;
        }

        public static Entity Create(string text)
        {
            Banner banner = new Banner(text);
            banner.DrawAction = banner.Draw;
            return new Entity(banner);
        }
    }
}
