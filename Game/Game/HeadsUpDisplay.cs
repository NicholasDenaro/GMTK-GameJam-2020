using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace Game
{
    public class HeadsUpDisplay : Description2D
    {
        public HeadsUpDisplay() : base(Sprite.Sprites["HUD"], 0, 0)
        {
            this.DrawInOverlay = true;
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(Program.ScreenWidth * Program.Scale, Program.ScreenHeight * Program.Scale);
            Graphics gfx = Graphics.FromImage(bmp);

            Font f = new Font("Arial", 12);
            StringFormat format = new StringFormat();
            if (Program.ShowDiags)
            {
                gfx.DrawString($"tps: {Program.Builder.tps}", f, Brushes.Black, new Point(0, Program.ScreenHeight * Program.Scale - 48), format);
                gfx.DrawString($"tick: {Program.Builder.tickTime}", f, Brushes.Black, new Point(0, Program.ScreenHeight * Program.Scale - 32), format);
                gfx.DrawString($"draw: {Program.Builder.drawTime}", f, Brushes.Black, new Point(0, Program.ScreenHeight * Program.Scale - 16), format);
            }

            format.Alignment = StringAlignment.Far;
            int i = 0;

            string timeLeft = $"Time left: {Program.Referee.Timer / Program.TPS}";

            gfx.DrawString(timeLeft, f, Brushes.Black, new Point(Program.ScreenWidth * Program.Scale, 0), format);

            gfx.DrawString($"Lives: {new string('$', Program.Lives)}", f, Brushes.Black, new Point(Program.ScreenWidth * Program.Scale, 16), format);

            format.Alignment = StringAlignment.Near;
            foreach (Stack<Rule> stack in Program.Referee.Piles.Values)
            {
                if (stack.Any())
                {
                    string name = stack.Peek().Name;
                    Rule rule = stack.Peek();
                    string show = $"{stack.Peek().Type}: {name}";
                    float xPos = 0;
                    foreach (string piece in show.Split(" "))
                    {
                        Color color = Color.Black;
                        string classname = piece.Trim();
                        Assembly assembly = Assembly.GetAssembly(typeof(Program));
                        Type t = assembly.GetType($"Game.{classname}");
                        PropertyInfo p;
                        if (t != null && (p = t.GetProperty("Color")) != null)
                        {
                            color = (Color)p.GetValue(null);
                        }

                        gfx.DrawString(piece, f, new SolidBrush(color), new Point((int)xPos, i * 18), format);
                        xPos += gfx.MeasureString(piece, f).Width;
                    }

                    i++;
                }
            }

            return bmp;
        }

        public static Entity Create()
        {
            HeadsUpDisplay hud = new HeadsUpDisplay();
            hud.DrawAction = hud.Draw;
            Entity entity = new Entity(hud);
            return entity;
        }
    }
}
