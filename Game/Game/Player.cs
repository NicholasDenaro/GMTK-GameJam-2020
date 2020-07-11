using GameEngine;
using GameEngine._2D;
using GameEngine.UI;
using System;
using System.Drawing;
using System.Linq;
using static Game.Program;

namespace Game
{
    public class Player : Description2D
    {
        public static Color Color { get; private set; } = Color.White;

        private Player(int x, int y) : base(Sprite.Sprites["player"], x, y)
        {
        }

        public void Tick(Location location, Entity entity)
        {
            if (location.GetEntities<Banner>().Any())
            {
                return;
            }

            Description2D d2d = entity.Description as Description2D;

            MouseControllerInfo mci = Program.Mouse[(int)Actions.MOUSEINFO].Info as MouseControllerInfo;

            double dir = 0;
            if (mci != null)
            {
                dir = d2d.Direction(new Point(mci.X, mci.Y));
            }

            // Rule.List.Contains(speed type value=x)
            double speed = 3 * (Program.Referee.Piles[Rule.RuleType.SPEED].FirstOrDefault()?.Action(location, this) ?? 1); //.Aggregate(1.0, (a, rule) => rule.Action(location) * a );

            // Rule.List.Contains(playstyle type value=top/down)
            if (Program.Keyboard[(int)Actions.RIGHT].IsDown())
            {
                d2d.ChangeCoordsDelta(speed * Math.Cos(dir + Math.PI / 2), speed * Math.Sin(dir + Math.PI / 2));
            }
            if (Program.Keyboard[(int)Actions.UP].IsDown())
            {
                d2d.ChangeCoordsDelta(speed * Math.Cos(dir), speed * Math.Sin(dir));
            }
            if (Program.Keyboard[(int)Actions.LEFT].IsDown())
            {
                d2d.ChangeCoordsDelta(speed * Math.Cos(dir - Math.PI / 2), speed * Math.Sin(dir - Math.PI / 2));
            }
            if (Program.Keyboard[(int)Actions.DOWN].IsDown())
            {
                d2d.ChangeCoordsDelta(speed * Math.Cos(dir + Math.PI), speed * Math.Sin(dir + Math.PI));
            }
        }

        public Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(16, 16);
            Graphics gfx = Graphics.FromImage(bmp);
            gfx.FillEllipse(new SolidBrush(Color), 0, 0, 16, 16);

            return bmp;
        }

        public static Entity Create(int x, int y)
        {
            Player player = new Player(x , y);
            player.DrawAction = player.Draw;
            Entity entity = new Entity(player);
            entity.TickAction = player.Tick;
            return entity;
        }
    }
}
