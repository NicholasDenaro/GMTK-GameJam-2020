using GameEngine;
using GameEngine._2D;
using NAudio.Wave;
using System.Drawing;

namespace Game
{
    public class Banner : Description2D
    {
        public string Text { get; set; }

        private SinWaveSound sound;

        public Banner(string text) : base(Sprite.Sprites["banner"], 0, Program.ScreenHeight * Program.Scale / 2)
        {
            this.Text = text;
            this.DrawInOverlay = true;
        }

        private void Jingle()
        {
            if (Text == "you win")
            {
                sound = new SinWaveSound(
                    500f, 44100 / Program.TPS, 750, 44100 / Program.TPS * 2, 1000, 44100 / Program.TPS * 4, 0, 44100 / Program.TPS,
                    750, 44100 / Program.TPS, 1000, 44100 / Program.TPS * 2, 1250, 44100 / Program.TPS * 4, 0, 44100 / Program.TPS,
                    1000, 44100 / Program.TPS, 1250, 44100 / Program.TPS * 2, 1500, 44100 / Program.TPS * 4);
            }
            else
            {
                sound = new SinWaveSound(
                    500f, 44100 / Program.TPS, 750, 44100 / Program.TPS * 2, 1000, 44100 / Program.TPS * 4, 0, 44100 / Program.TPS,
                    500, 44100 / Program.TPS, 400, 44100 / Program.TPS * 2, 300, 44100 / Program.TPS * 10);
            }

            sound.SetWaveFormat(44100, 2);

            Program.WavProvider.AddMixerInput((ISampleProvider)sound);
            Program.WavPlayer.Play();
        }

        private void Tick(Location location, Entity entity)
        {
            if (sound == null)
            {
                Jingle();
            }
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(Program.ScreenWidth * Program.Scale, 16 * Program.Scale);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.ScaleTransform(Program.Scale, Program.Scale);
            gfx.FillRectangle(Brushes.Gray, 0, 0, Program.ScreenWidth, 16);
            Font f = new Font("Arial", 12);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            gfx.DrawString(Text, f, Brushes.Black, Program.ScreenWidth / 2, 0, format);

            gfx.ScaleTransform(1.0f / Program.Scale, 1.0f / Program.Scale);
            string continueText = "Press R to continue";
            if (Text != "you win")
            {
                continueText = "Press R to try again";
            }

            format.Alignment = StringAlignment.Far;
            gfx.DrawString(continueText, f, Brushes.Black, Program.ScreenWidth * Program.Scale, 16, format);

            return bmp;
        }

        public static Entity Create(string text)
        {
            Banner banner = new Banner(text);
            banner.DrawAction = banner.Draw;
            Entity entity = new Entity(banner);
            entity.TickAction = banner.Tick;
            return entity;
        }
    }
}
