using GameEngine;
using GameEngine._2D;
using System.Drawing;

namespace Game
{
    public class Text : Description2D
    {
        private string text;
        private Font font;
        private bool moved;
        private int width;
        private int height;

        public Text(string text, Font font, int x, int y) : base(Sprite.Sprites["text"], x, y)
        {
            this.text = text;
            this.font = font;
            width = 1;
            height = 1;
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(width, height);
            Graphics gfx = Graphics.FromImage(bmp);
            //gfx.FillRectangle(Brushes.Gray, 0, 0, Program.ScreenWidth, height);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            if(!moved)
            {
                width = (int)gfx.MeasureString(text, font).Width;
                height = (int)gfx.MeasureString(text, font).Height;
                ChangeCoordsDelta(-width / 2, 0);
                moved = true;
            }
            gfx.DrawString(text, font, Brushes.Black, width / 2, 0, format);

            return bmp;
        }

        public static Entity Create(string txt, Font font, int x, int y)
        {
            Text text = new Text(txt, font, x, y);
            text.DrawAction = text.Draw;
            return new Entity(text);
        }
    }
}
