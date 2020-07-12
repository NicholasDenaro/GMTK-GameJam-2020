using GameEngine;
using GameEngine._2D;
using System;
using System.Drawing;


namespace Game
{
    public class DialogBox : Description2D, IIdentifiable
    {        
        public string Text { get; private set; }
        public string Shown { get; private set; }

        private int index = 0;

        private const int SKIP = 2;

        private static int tick;

        private bool IsFinished => Shown.Length == Text.Length;

        public Guid Id { get; private set; }

        public DialogBox(string text) : base(Sprite.Sprites["banner"], 0, (Program.ScreenHeight - 64) * Program.Scale)
        {
            this.Text = text;
            this.DrawInOverlay = true;
            this.Shown = "";
        }

        private void Tick(Location location, Entity entity)
        {
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

        public static Entity Create(string text)
        {
            DialogBox dialog = new DialogBox(text);
            dialog.DrawAction = dialog.Draw;
            Entity entity = new Entity(dialog);
            entity.TickAction = dialog.Tick;
            dialog.Id = entity.Id;
            return entity;
        }
    }
}
