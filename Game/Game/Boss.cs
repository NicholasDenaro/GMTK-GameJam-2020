using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Game
{
    public class Boss : Description2D, IIdentifiable
    {
        public static Color Color { get; private set; } = Color.LightGray;

        public Guid Id { get; private set; }
        int health = 61;

        private int attackTimerMax = Program.TPS * 2;
        private int attackTimer = Program.TPS * 3;

        private int stealAttack = 0;

        private bool shiftRoom;

        private Guid centerWall;

        public Boss(int x, int y) : base(Sprite.Sprites["boss"], x, y, 80, 80)
        {
        }

        private Bitmap Draw()
        {
            Bitmap bmp = BitmapExtensions.CreateBitmap(80, 80);
            Graphics gfx = Graphics.FromImage(bmp);

            gfx.FillRectangle(new SolidBrush(Color), 0, 0, 80, 80);

            return bmp;
        }

        private void Tick(Location location, Entity entity)
        {
            foreach (Bullet<BulletNull> bullet in location.GetEntities<Bullet<BulletNull>>())
            {
                if (bullet.IsCollision(this))
                {
                    health -= 1;
                    location.RemoveEntity(bullet.Id);
                }
            }

            if (health == 80)
            {
                Program.Referee.AddRule("platformer");
                attackTimerMax = 30;
                Program.Referee.AddRule(Rule.Rules["Goal hurty"]);
            }

            if (health == 70 && stealAttack == 0)
            {
                Program.Referee.AddRule("no attack. haha.");
                location.AddEntity(Powerup.Create("shoot Boss", Program.ScreenWidth - 64, Program.ScreenHeight - 16));
                stealAttack++;
            }

            if (health == 69)
            {
                attackTimerMax = 60;
            }

            if (health == 60 && !shiftRoom)
            {
                Program.Referee.AddRule("vvvvvv-platformer");
                attackTimerMax = 60;
                Program.Referee.AddRule(Rule.Rules["Powerup hurty"]);

                shiftRoom = true;

                Entity wall = Wall.Create(0, Program.ScreenHeight / 2, 128, 16);
                centerWall = wall.Id;
                location.AddEntity(wall);
            }

            if (health == 50 && stealAttack == 1)
            {
                Program.Referee.AddRule("no attack. haha.");
                location.AddEntity(Powerup.Create("shoot Boss", Program.ScreenWidth - 96, Program.ScreenHeight / 2));
                stealAttack++;
            }

            if (health == 40 && shiftRoom)
            {
                Program.Referee.AddRule("top-down");
                shiftRoom = false;
                location.RemoveEntity(centerWall);
            }

            if (health == 20)
            {

            }

            if (health <= 0)
            {
                location.RemoveEntity(this.Id);
                location.AddEntity(Banner.Create("you win"));
            }

            if (attackTimer-- == 0)
            {
                attackTimer = attackTimerMax;

                //top-down
                if (health > 80)
                {
                    int count = 6;
                    for (int i = 0; i < count; i++)
                    {
                        double dir =
                            this.Direction(location.GetEntities<Player>().First())
                            + (Math.PI / 3)
                            - (i * 1.0 / count) * (Math.PI * 2.0 / 3)
                            + (Program.Random.NextDouble() - 0.5) * Math.PI / 10;
                        location.AddEntity(Enemy.Create((int)this.X, (int)this.Y).AddTickAction((l, e) =>
                        {
                            ((Description2D)e.Description).ChangeCoordsDelta(2 * Math.Cos(dir), 2 * Math.Sin(dir));
                        }));
                    }
                }
                // platformer
                else if (health > 60)
                {
                    attackTimer += Program.Random.Next(-10, 10);

                    for (int i = 0; i < 3; i++)
                    {
                        location.AddEntity(Goal.Create(Program.ScreenWidth, Program.ScreenHeight - 16 - 16 * i).AddTickAction((l, e) =>
                        {
                            ((Description2D)e.Description).ChangeCoordsDelta(-5, 0);
                        }));
                    }
                }
                // vvvvvv-platformer
                else if (health > 40)
                {

                }
                // top-down spinny
                else if (health > 20)
                {

                }
                else
                {

                }
            }
        }

        public static Entity Create(int x, int y)
        {
            Boss boss = new Boss(x, y);
            boss.DrawAction = boss.Draw;
            Entity entity = new Entity(boss);
            entity.TickAction = boss.Tick;
            boss.Id = entity.Id;
            return entity;
        }
    }
}
