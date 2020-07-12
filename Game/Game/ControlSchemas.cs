using GameEngine;
using GameEngine._2D;
using GameEngine.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using static Game.Program;

namespace Game
{
    public static class ControlSchemas
    {
        private static double velocity;
        private static int velocityDirection = 1;

        public static void Reset()
        {
            velocity = 0;
            velocityDirection = 1;
        }

        public static void TopDown(Location location, object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (location.GetEntities<Banner>().Any())
            {
                return;
            }

            Description2D d2d = obj as Description2D;

            MouseControllerInfo mci = Program.Mouse[(int)Actions.MOUSEINFO].Info as MouseControllerInfo;

            double dir = 0;
            if (mci != null)
            {
                dir = d2d.Direction(new Point(mci.X, mci.Y));
            }

            List<Description2D> walls = location.GetEntities<Wall>().Select(w => w as Description2D).ToList();

            double speed = 3 * (Program.Referee.Piles[Rule.RuleType.SPEED].FirstOrDefault()?.Action(location, obj) ?? 1);

            if (Program.Keyboard[(int)Actions.RIGHT].IsDown())
            {
                Move(d2d, speed * Math.Cos(dir + Math.PI / 2), speed * Math.Sin(dir + Math.PI / 2), walls);
            }
            if (Program.Keyboard[(int)Actions.UP].IsDown())
            {
                Move(d2d, speed * Math.Cos(dir), speed * Math.Sin(dir), walls);
            }
            if (Program.Keyboard[(int)Actions.LEFT].IsDown())
            {
                Move(d2d, speed * Math.Cos(dir - Math.PI / 2), speed * Math.Sin(dir - Math.PI / 2), walls);
            }
            if (Program.Keyboard[(int)Actions.DOWN].IsDown())
            {
                Move(d2d, speed * Math.Cos(dir + Math.PI), speed * Math.Sin(dir + Math.PI), walls);
            }
        }

        public static bool Move(Description2D d2d, double deltaX, double deltaY, List<Description2D> walls)
        {
            bool undo = false;
            d2d.ChangeCoordsDelta(deltaX, 0);
            if (Program.Collision(d2d, walls))
            {
                undo = true;
                for (int i = 0; i < Math.Abs(deltaX); i++)
                {
                    d2d.ChangeCoordsDelta(-Math.Clamp(deltaX, -1, 1), 0);
                    if (!Program.Collision(d2d, walls))
                    {
                        break;
                    }
                }
            }

            d2d.ChangeCoordsDelta(0, deltaY);
            if (Program.Collision(d2d, walls))
            {
                undo = true;
                for (int i = 0; i < Math.Abs(deltaY); i++)
                {
                    d2d.ChangeCoordsDelta(0, - Math.Clamp(deltaY, -1, 1));
                    if (!Program.Collision(d2d, walls))
                    {
                        break;
                    }
                }
            }

            return !undo;
        }

        public static void Platformer(Location location, object obj)
        {
            if (obj == null)
            {
                return;
            }

            Description2D d2d = obj as Description2D;

            double speed = 3 * (Program.Referee.Piles[Rule.RuleType.SPEED].FirstOrDefault()?.Action(location, obj) ?? 1);

            List<Description2D> walls = location.GetEntities<Wall>().Select(w => w as Description2D).ToList();

            d2d.ChangeCoordsDelta(0, 1);
            bool onGround = Program.Collision(d2d, walls);
            d2d.ChangeCoordsDelta(0, -1);

            if (!onGround)
            {
                if (velocity < 10)
                {
                    velocity += 1.0;
                }
            }

            if (Program.Keyboard[(int)Actions.RIGHT].IsDown())
            {
                Move(d2d, speed, 0, walls);
            }
            if (Program.Keyboard[(int)Actions.UP].IsPress())
            {

                if (d2d.Y + velocity >= ((Description2D)location.Description).Height || onGround)
                {
                    velocity = -10;
                }
            }
            if (Program.Keyboard[(int)Actions.LEFT].IsDown())
            {
                Move(d2d, -speed, 0, walls);
            }

            if (d2d.Y + velocity <= ((Description2D)location.Description).Height)
            {
                //d2d.ChangeCoordsDelta(0, velocity);
                if (!Move(d2d, 0, velocity, walls))
                {
                    velocity = 0;
                }
            }
            else
            {
                d2d.SetCoords(d2d.X, ((Description2D)location.Description).Height);
                velocity = 0;
            }
        }

        public static void VVVVVVPlatformer(Location location, object obj)
        {
            if (obj == null)
            {
                return;
            }

            Description2D d2d = obj as Description2D;

            List<Description2D> walls = location.GetEntities<Wall>().Select(w => w as Description2D).ToList();

            d2d.ChangeCoordsDelta(0, Math.Clamp(velocityDirection, -1, 1));
            bool onGround = Program.Collision(d2d, walls);
            d2d.ChangeCoordsDelta(0, Math.Clamp(-velocityDirection, -1, 1));

            double speed = 3 * (Program.Referee.Piles[Rule.RuleType.SPEED].FirstOrDefault()?.Action(location, obj) ?? 1); //.Aggregate(1.0, (a, rule) => rule.Action(location) * a );

            if (!onGround)
            {
                velocity += velocityDirection;
            }

            velocity = Math.Clamp(velocity, -10, 10);

            // Rule.List.Contains(playstyle type value=top/down)
            if (Program.Keyboard[(int)Actions.RIGHT].IsDown())
            {
                Move(d2d, speed, 0, walls);
            }
            if (Program.Keyboard[(int)Actions.UP].IsPress())
            {
                if (onGround || d2d.Y + velocity >= ((Description2D)location.Description).Height || d2d.Y + velocity <= 0)
                {
                    velocityDirection = -velocityDirection;
                    velocity = velocityDirection * 4;
                }
            }
            if (Program.Keyboard[(int)Actions.LEFT].IsDown())
            {
                Move(d2d, -speed, 0, walls);
            }

            if (velocityDirection > 0)
            {
                if (d2d.Y + velocity <= ((Description2D)location.Description).Height)
                {
                    if (!Move(d2d, 0, velocity, walls))
                    {
                        velocity = 0;
                    }
                }
                else
                {
                    d2d.SetCoords(d2d.X, ((Description2D)location.Description).Height);
                }
            }
            else
            {
                if (d2d.Y + velocity >= 0)
                {
                    if (!Move(d2d, 0, velocity, walls))
                    {
                        velocity = 0;
                    }
                }
                else
                {
                    d2d.SetCoords(d2d.X, 0);
                }
            }
        }
    }
}
