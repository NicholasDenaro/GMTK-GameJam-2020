using GameEngine;
using GameEngine._2D;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Drawing;
using System.IO;

namespace Game
{
    public class DialogBox : Description2D, IIdentifiable
    {        
        public string Text { get; private set; }
        public string Shown { get; private set; }

        private int index = 0;

        private const int SKIP = 2;

        private static int tick;

        private Entity chain;

        private bool IsFinished => Shown.Length == Text.Length;

        public Guid Id { get; private set; }

        SinWaveSound sound;

        public DialogBox(string text, Entity chain = null) : base(Sprite.Sprites["banner"], 0, (Program.ScreenHeight - 64) * Program.Scale)
        {
            this.Text = text;
            this.DrawInOverlay = true;
            this.Shown = "";
            this.chain = chain;
        }

        private void Bloop()
        {
            sound = new SinWaveSound((int)(44100 / Program.TPS * 1.5), 250, 500);
            sound.SetWaveFormat(44100, 2);

            Program.WavProvider.AddMixerInput((ISampleProvider)sound);
            Program.WavPlayer.Play();
        }

        private void Tick(Location location, Entity entity)
        {
            if (index == Text.Length)
            {
                //Program.WavProvider.RemoveMixerInput(sound);
                //Program.WavProvider.RemoveAllMixerInputs();
                sound.Amplitude = 0;
                //Program.WavPlayer.Stop();
            }

            if (sound == null)
            {
                Bloop();
            }

            if (Program.Mouse[(int)Program.Actions.ACTION].IsDown())
            {
                if (index < Text.Length)
                {
                    index++;
                }
            }

            if (IsFinished && Program.Mouse[(int)Program.Actions.ACTION].IsPress())
            {
                location.RemoveEntity(Id);
                if (chain != null)
                {
                    location.AddEntity(chain);
                }
            }           
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(Program.ScreenWidth * Program.Scale, 64 * Program.Scale);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.ScaleTransform(Program.Scale, Program.Scale);
            gfx.FillRectangle(Brushes.Gray, 0, 0, Program.ScreenWidth, 64);
            Font f = new Font("Arial", 12);
            StringFormat format = new StringFormat();
            gfx.DrawString(Shown, f, Brushes.Black, 8, 8, format);

            if (index < Text.Length && tick++ % SKIP == 0)
            {
                index++;
            }

            Shown = Text.Substring(0, index);

            return bmp;
        }

        public static Entity Create(string text, Entity chain = null)
        {
            DialogBox dialog = new DialogBox(text, chain);
            dialog.DrawAction = dialog.Draw;
            Entity entity = new Entity(dialog);
            entity.TickAction = dialog.Tick;
            dialog.Id = entity.Id;
            return entity;
        }
    }
}
