﻿using GameEngine;
using GameEngine._2D;
using GameEngine.UI.AvaloniaUI;
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
        int health = 21; // 100
        private int stealAttack = 3; // 0
        private bool shiftRoom = true; // false

        private int attackTimerMax = Program.TPS * 2;
        private int attackTimer = Program.TPS * 3;

        private AvaloniaWindow window = Program.Builder.Frame.Window as AvaloniaWindow;

        private List<Guid> centerWalls = new List<Guid>();

        private bool attackTop;

        private TickHandler windowAction = null;

        private Avalonia.PixelPoint originalWindowPosition;

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
                Program.Referee.AddRule("be fast");

                shiftRoom = true;

                for (int i = 0; i < 4; i++)
                {
                    Entity wall = Powerup.Create("pop SPEED", Program.ScreenWidth / 2 - 80 + i * 16, Program.ScreenHeight / 2);
                    centerWalls.Add(wall.Id);
                    location.AddEntity(wall);
                }

                attackTimerMax = 30;
            }

            if (health == 50 && stealAttack == 1)
            {
                Program.Referee.AddRule("no attack. haha.");
                Entity trigger = Powerup.Create("shoot Boss", Program.ScreenWidth - 128, Program.ScreenHeight / 2);
                Guid triggerId = trigger.Id;
                trigger.AddTickAction((loc, ent) =>
                {
                    if (loc.GetEntities<Player>().First().Distance((Description2D)ent.Description) < 20)
                    {
                        Program.Referee.AddRule("shoot Boss");
                        loc.RemoveEntity(triggerId);
                    }
                });

                location.AddEntity(trigger);
                stealAttack++;
            }

            if (health == 40 && shiftRoom)
            {
                Program.Referee.AddRule("top-down");
                Program.Referee.AddRule("Enemy hurty");
                attackTimer = 30;
                attackTimerMax = 30;
                shiftRoom = false;
                foreach (Guid guid in centerWalls)
                {
                    location.RemoveEntity(guid);
                }

                double dir = location.GetEntities<Player>().First().Direction(new Point(Program.ScreenWidth / 2, Program.ScreenHeight / 2)) + Math.PI / 4;
                centerWalls.Clear();
                for (int j = 0; j < 4; j++)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        int xoffset = (int)(Math.Cos(dir + j * Math.PI / 2) * i * 16);
                        int yoffset = (int)(Math.Sin(dir + j * Math.PI / 2) * i * 16);

                        Entity rotaty = Enemy.Create(Program.ScreenWidth / 2 + xoffset, Program.ScreenHeight / 2 + yoffset).AddTickAction((loc, ent) =>
                        {
                            Description2D enemyd = ent.Description as Description2D;

                            double instantdir = enemyd.Direction(new Point(Program.ScreenWidth / 2, Program.ScreenHeight / 2));
                            double dist = enemyd.Distance(new Point(Program.ScreenWidth / 2, Program.ScreenHeight / 2));

                            enemyd.ChangeCoordsDelta(Math.Cos(instantdir + Math.PI / 2) * dist / 50, Math.Sin(instantdir + Math.PI / 2) * dist / 50);
                        });

                        centerWalls.Add(rotaty.Id);
                        location.AddEntity(rotaty);
                    }
                }
            }

            if (health == 30 && stealAttack == 2)
            {
                Program.Referee.AddRule("no attack. haha.");
                location.AddEntity(Powerup.Create("shoot Boss", Program.ScreenWidth - 128, Program.ScreenHeight / 2));
                stealAttack++;
            }

            if (health == 20)
            {
                Program.Referee.AddRule("pop DEATH");
                Program.Referee.AddRule("pop DEATH");
                Program.Referee.AddRule("pop DEATH");
                Program.Referee.AddRule("pop DEATH");

                foreach (Guid guid in centerWalls)
                {
                    location.RemoveEntity(guid);
                }

                attackTimer = 10;
                attackTimerMax = 10;

                originalWindowPosition = window.Position;
            }

            if (health == 0)
            {
                location.RemoveEntity(this.Id);
                location.AddEntity(Banner.Create("you win"));

                if (windowAction != null)
                {
                    Program.Engine.TickEnd -= windowAction;
                }

                window.Position = originalWindowPosition;

                health--;
            }

            if (health <= 0)
            {
                window.Position = originalWindowPosition;
                if (windowAction != null)
                {
                    Program.Engine.TickEnd -= windowAction;
                }
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
                    int y = attackTop ? 0 : Program.ScreenHeight / 2;

                    for (int i = 0; i < 6; i++)
                    {
                        location.AddEntity(Powerup.Create("pop SPEED", Program.ScreenWidth, y + 16 + 16 * i).AddTickAction((l, e) =>
                        {
                            ((Description2D)e.Description).ChangeCoordsDelta(-5, 0);
                        }));
                    }

                    attackTop = !attackTop;
                }
                // top-down spinny
                else if (health > 20)
                {
                    int yPos = Program.Random.Next(16, Program.ScreenHeight - 16);
                    int delta = 0;
                    location.AddEntity(Enemy.Create(Program.ScreenWidth, yPos).AddTickAction((l, e) =>
                    {
                        ((Description2D)e.Description).ChangeCoordsDelta(-delta++, 0);
                    }));
                }
                // just wait
                else
                {
                    int velocity = (20 - health) * 2;
                    double direction = Program.Random.NextDouble() * Math.PI * 2;

                    if (windowAction != null)
                    {
                        Program.Engine.TickEnd -= windowAction;
                    }

                    location.AddEntity(Goal.Create(Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenHeight - 16)));
                    location.AddEntity(Powerup.Create("pop SPEED", Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenHeight - 16)));
                    location.AddEntity(Enemy.Create(Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenHeight - 16)));

                    windowAction = (s, gs) =>
                    {
                        double shake = Program.Random.NextDouble() * Math.PI / 8;

                        window.Position = window.Position
                            .WithX((int)Math.Clamp(window.Position.X + Math.Cos(direction + shake - Math.PI / 4) * velocity, 0, 1920 - Program.ScreenWidth * Program.Scale))
                            .WithY((int)Math.Clamp(window.Position.Y + Math.Sin(direction + shake - Math.PI / 4) * velocity, 0, 1080 - Program.ScreenHeight * Program.Scale));
                    };

                    Program.Engine.TickEnd += windowAction;
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