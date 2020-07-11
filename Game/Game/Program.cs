using Game.Rules;
using GameEngine;
using GameEngine._2D;
using GameEngine.UI;
using GameEngine.UI.AvaloniaUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Engine = GameEngine.GameEngine;

namespace Game
{
    class Program
    {
        public const int TPS = 30;
        public const int ScreenHeight = 240;
        public const int ScreenWidth = 320;
        public const int Scale = 2;

        public static int Level = 0;

        public static Random Random { get; private set; } = new Random();

        public static Controller Keyboard { get; private set; }
        public static Controller Mouse { get; private set; }

        public static Engine Engine { get; private set; }

        public static GameBuilder Builder { get; private set; }

        public static Referee Referee { get; private set; }

        public static void Main(string[] args)
        {
            Keyboard = new WindowsKeyController(keyMap.ToDictionary(kvp => (int)kvp.Key, kvp => (int)kvp.Value));
            Mouse = new WindowsMouseController(mouseMap.ToDictionary(kvp => (int)kvp.Key, kvp => (int)kvp.Value));

            Builder = new GameBuilder();
            Builder.GameEngine(new FixedTickEngine(TPS))
                .GameView(new GameView2D(ScreenWidth, ScreenHeight, Scale, Scale, Color.DarkSlateGray))
                .GameFrame(new GameFrame(new AvaloniaWindowBuilder(), 0, 0, ScreenWidth, ScreenHeight, Scale, Scale))
                .Controller(Keyboard)
                .Controller(Mouse)
                //.StartingLocation(new Location(new Description2D(0, 0, ScreenWidth, ScreenHeight)))
                .Build();

            Engine = Builder.Engine;

            GameFrame frame = Builder.Frame;
            AvaloniaWindow window = frame.Window as AvaloniaWindow;

            Referee = new Referee();
            Engine.TickEnd += Referee.Tick;

            Sprites();

            Rules();

            SetupTitleScreen();
            //SetupCrazyMode();

            Engine.TickEnd += (s, gs) =>
            {
                if (Program.Keyboard[(int)Actions.RESTART].IsPress())
                {
                    if (Engine.Location.GetEntities<Banner>().FirstOrDefault()?.Text == "you win")
                    {
                        if (Level == -1)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                Referee.AddRule(Rule.GetNameRandomRule());
                            }
                        }
                        else
                        {
                            Level++;
                        }
                    }

                    switch (Level)
                    {
                        case -1:
                            SetupCrazyMode();
                            break;
                        case 2:
                            SetupLevel2();
                            break;
                        case 3:
                            SetupLevel3();
                            break;
                        case 4:
                            SetupLevel4();
                            break;
                        case 5:
                            SetupLevel5();
                            break;

                    }
                    Referee.ResetTimer();
                }

                ////window.Position = window.Position.WithX(window.Position.X + 1);
            };

            Engine.Start();

            while (true) { }
        }

        public static void SetupCrazyMode()
        {
            Level = -1;

            Engine.SetLocation(new Location(new Description2D(0, 0, ScreenWidth, ScreenHeight)));
            Engine.AddEntity(Player.Create(Program.ScreenWidth / 2, Program.ScreenHeight / 2));

            Engine.AddEntity(Goal.Create(Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenHeight - 16)));

            for (int i = 0; i < 20; i++)
            {
                Engine.AddEntity(Enemy.Create(Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenHeight - 16)));
            }

            Engine.AddEntity(HeadsUpDisplay.Create());

            Referee.Start();
        }

        public static void SetupTitleScreen()
        {
            Engine.SetLocation(new Location(new Description2D(0, 0, ScreenWidth, ScreenHeight)));
            Description2D logoDescription = new Description2D(Sprite.Sprites["gmtklogo"], ScreenWidth / 2, ScreenHeight / 2);
            Entity logo = new Entity(logoDescription);
            int timer = TPS * 2;
            logo.TickAction = (loc, ent) =>
            {
                if (logoDescription.ImageIndex < Sprite.Sprites["gmtklogo"].HImages - 1)
                {
                    logoDescription.ImageIndex++;
                }
                else
                {
                    if (timer-- == 0)
                    {
                        logoDescription.ImageIndex++;
                        Engine.AddEntity(Button.Create("Story Mode", SetupLevel1, ScreenWidth / 2 - 128 / 2, ScreenHeight / 2 - 16));
                        Engine.AddEntity(Button.Create("Crazy Mode", SetupCrazyMode, ScreenWidth / 2 - 128 / 2, ScreenHeight / 2 + 48));
                    }
                }
            };

            Engine.AddEntity(logo);
        }
        public static void SetupLevel1()
        {
            Level = 1;

            Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Rule> deck = new Stack<Rule>();
            deck.Push(Rule.Rules["Goal victory"]);
            deck.Push(Rule.Rules["control Player"]);
            deck.Push(Rule.Rules["top-down"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (deck.Any() && timer++ % (Program.TPS * 5) == 0)
                {
                    Referee.AddRule(deck.Pop());
                }
            };

            Engine.AddEntity(deckFlipper);

            Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2));

            Engine.AddEntity(Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2));

            Engine.AddEntity(HeadsUpDisplay.Create());

            Referee.ClearRules();

            Referee.Start();
        }

        public static void SetupLevel2()
        {
            Level = 2;

            Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Rule> deck = new Stack<Rule>();
            deck.Push(Rule.Rules["pop VICTORY"]);
            deck.Push(Rule.Rules["Goal victory"]);
            deck.Push(Rule.Rules["pop VICTORY"]);
            deck.Push(Rule.Rules["Goal victory"]);
            deck.Push(Rule.Rules["pop VICTORY"]);
            deck.Push(Rule.Rules["Goal victory"]);
            deck.Push(Rule.Rules["pop VICTORY"]);
            deck.Push(Rule.Rules["Goal victory"]);
            deck.Push(Rule.Rules["pop VICTORY"]);

            Referee.ClearRules();

            Referee.AddRule(Rule.Rules["Goal victory"]);
            Referee.AddRule(Rule.Rules["control Player"]);
            Referee.AddRule(Rule.Rules["top-down"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (deck.Any() && timer++ % (Program.TPS * 1) == 0)
                {
                    Rule rule = deck.Pop();
                    Referee.AddRule(rule);
                }
            };

            Engine.AddEntity(deckFlipper);

            Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2));

            Engine.AddEntity(Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2));

            Engine.AddEntity(HeadsUpDisplay.Create());

            Referee.Start();
        }

        public static void SetupLevel3()
        {
            Level = 3;

            Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Action> deck = new Stack<Action>();
            deck.Push(() => Referee.AddRule(Rule.Rules["Player pickup Powerup"]));
            deck.Push(() => Engine.AddEntity(Powerup.Create("pop DEATH", Program.ScreenWidth - 32, Program.ScreenHeight / 2)));
            deck.Push(() => Referee.AddRule(Rule.Rules["Goal hurty"]));
            deck.Push(() => Referee.AddRule(Rule.Rules["Goal victory"]));
            deck.Push(() => Referee.AddRule(Rule.Rules["pop VICTORY"]));

            Referee.ClearRules();

            Referee.AddRule(Rule.Rules["Goal victory"]);
            Referee.AddRule(Rule.Rules["control Player"]);
            Referee.AddRule(Rule.Rules["top-down"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Referee.IsStarted && deck.Any() && timer++ % (Program.TPS * 1) == 0)
                {
                    deck.Pop().Invoke();
                }
            };

            Engine.AddEntity(deckFlipper);

            Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2));

            Engine.AddEntity(Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2));

            Engine.AddEntity(HeadsUpDisplay.Create());

            Referee.Start();
        }

        public static void SetupLevel4()
        {
            Level = 4;

            Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Action> deck = new Stack<Action>();
            deck.Push(() => Engine.AddEntity(Powerup.Create("pop DEATH", 32, Program.ScreenHeight / 2)));
            deck.Push(() => Engine.AddEntity(Powerup.Create("pop CONTROL", Program.ScreenWidth - 32, Program.ScreenHeight / 2)));
            deck.Push(() => Engine.AddEntity(Powerup.Create("pop VICTORY", Program.ScreenWidth / 2, Program.ScreenHeight / 2)));

            Referee.ClearRules();

            Referee.AddRule(Rule.Rules["Goal victory"]);
            Referee.AddRule(Rule.Rules["Enemy hurty"]);
            Referee.AddRule(Rule.Rules["Player pickup Powerup"]);
            Referee.AddRule(Rule.Rules["control Player"]);
            Referee.AddRule(Rule.Rules["top-down"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Referee.IsStarted && deck.Any() && timer++ % (Program.TPS * 5) == 0)
                {
                    deck.Pop().Invoke();
                }
            };

            Engine.AddEntity(deckFlipper);

            Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2));

            Engine.AddEntity(Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2));

            Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 - 16, Program.ScreenHeight / 2));
            Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 - 16));
            Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 + 16, Program.ScreenHeight / 2));
            Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 + 16));

            Engine.AddEntity(HeadsUpDisplay.Create());

            Referee.Start();
        }

        public static void SetupLevel5()
        {
            Level = 5;

            Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Action> deck = new Stack<Action>();

            Action spawnPowerups = () =>
            {
                Engine.AddEntity(Powerup.Create("platformer", 224 - 8, Program.ScreenHeight / 2 + 80));
                Engine.AddEntity(Powerup.Create("top-down", 256 - 8, Program.ScreenHeight / 2 + 80));
                Engine.AddEntity(Powerup.Create("platformer", 288 - 8, Program.ScreenHeight / 2 + 80));
                Engine.AddEntity(Powerup.Create("top-down", 314 - 8, Program.ScreenHeight / 2 + 80));
            };

            deck.Push(spawnPowerups);
            deck.Push(spawnPowerups);
            deck.Push(spawnPowerups);
            deck.Push(spawnPowerups);
            deck.Push(spawnPowerups);
            deck.Push(spawnPowerups);

            Referee.ClearRules();

            Referee.AddRule(Rule.Rules["Goal victory"]);
            Referee.AddRule(Rule.Rules["Enemy hurty"]);
            Referee.AddRule(Rule.Rules["Any pickup Powerup"]);
            Referee.AddRule(Rule.Rules["control Player"]);
            Referee.AddRule(Rule.Rules["top-down"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Referee.IsStarted && deck.Any() && timer++ % (Program.TPS * 3.5) == 0)
                {
                    deck.Pop().Invoke();
                }
            };

            Engine.AddEntity(deckFlipper);

            Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2 + 64));

            Engine.AddEntity(Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2));

            Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 - 16, Program.ScreenHeight / 2));
            Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 - 16));
            Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 + 16, Program.ScreenHeight / 2));
            Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 + 16));

            Engine.AddEntity(Wall.Create(0, Program.ScreenHeight / 2 + 32));
            Engine.AddEntity(Wall.Create(16, Program.ScreenHeight / 2 + 32));
            Engine.AddEntity(Wall.Create(32, Program.ScreenHeight / 2 + 32));
            Engine.AddEntity(Wall.Create(48, Program.ScreenHeight / 2 + 32));
            Engine.AddEntity(Wall.Create(64, Program.ScreenHeight / 2 + 32));
            Engine.AddEntity(Wall.Create(80, Program.ScreenHeight / 2 + 80));
            Engine.AddEntity(Wall.Create(96, Program.ScreenHeight / 2 + 80));
            Engine.AddEntity(Wall.Create(112, Program.ScreenHeight / 2 + 32));
            Engine.AddEntity(Wall.Create(128, Program.ScreenHeight / 2 + 32));
            Engine.AddEntity(Wall.Create(144, Program.ScreenHeight / 2 + 32));
            Engine.AddEntity(Wall.Create(160, Program.ScreenHeight / 2 + 32));
            Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 + 32));
            Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 + 16));
            Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2));
            Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 - 16));
            Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 - 32));
            Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 - 48));
            Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 - 64));
            Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 - 80));
            Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 + 48));
            Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 + 64));
            Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 + 80));

            Engine.AddEntity(Powerup.Create("clicky attack", 32, 32));

            for (int i = 0; i < Program.ScreenWidth; i += 16)
            {
                Engine.AddEntity(Wall.Create(i, Program.ScreenHeight - 16));
            }

            Entity enemy = Enemy.Create(224, Program.ScreenHeight / 2 + 80);
            int deltaX = 1;
            enemy.TickAction += (loc, ent) =>
            {
                Description2D d2d = enemy.Description as Description2D;
                if (d2d.X == Program.ScreenWidth - 16 || d2d.X == 208)
                {
                    deltaX = -deltaX;
                }

                d2d.ChangeCoordsDelta(deltaX, 0);
            };

            Engine.AddEntity(enemy);

            Engine.AddEntity(HeadsUpDisplay.Create());

            Referee.Start();
        }

        public static bool Collision(Description2D d1, List<Description2D> d2s)
        {
            foreach(Description2D d2 in d2s)
            {
                if (Collision(d1, d2))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Collision(Description2D d1, Description2D d2)
        { 
            double d1Size = Math.Sqrt(d1.Width * d1.Width + d1.Height * d1.Height / 4);
            double d2Size = Math.Sqrt(d2.Width * d2.Width + d2.Height * d2.Height / 4);

            double dist = d1.Distance(d2);

            if (dist <= (d1Size + d2Size) / 1.5)
            {
                Bitmap bmp = BitmapExtensions.CreateBitmap((int)(d1Size + d2Size) + 20, (int)(d1Size + d2Size) + 20);
                Graphics gfx = Graphics.FromImage(bmp);
                float minX = (float)Math.Min(d1.X - d1.Sprite.X, d2.X - d2.Sprite.X);
                float minY = (float)Math.Min(d1.Y - d1.Sprite.X, d2.Y - d2.Sprite.X);
                gfx.TranslateTransform(-minX + 10, -minY + 10);

                Bitmap b1 = (Bitmap)d1.Image();
                Bitmap b2 = (Bitmap)d2.Image();

                gfx.DrawImage(b1, (float)d1.X - d1.Sprite.X, (float)d1.Y - d1.Sprite.X);
                gfx.DrawImage(b2, (float)d2.X - d2.Sprite.X, (float)d2.Y - d2.Sprite.Y);

                int total = 0;
                for (int i = 0; i < b1.Width; i++)
                {
                    for (int j = 0; j < b1.Height; j++)
                    {
                        if (b1.GetPixel(i, j).A != 0)
                        {
                            total++;
                        }
                    }
                }

                for (int i = 0; i < b2.Width; i++)
                {
                    for (int j = 0; j < b2.Height; j++)
                    {
                        if (b2.GetPixel(i, j).A != 0)
                        {
                            total++;
                        }
                    }
                }

                int count = 0;
                for (int i = 0; i < bmp.Width; i++)
                {
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        Color c = bmp.GetPixel(i, j);
                        if (c.A != 0)
                        {
                            count++;
                        }
                    }
                }

                return count != total;
            }

            return false;
        }

        public static void Rules()
        {
            // TODO: Make it so the "Player" is what is being controlled
            new TouchRule<Player, Goal>("Goal victory", Rule.RuleType.VICTORY, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any() && Referee.IsStarted)
                {
                    Referee.Stop();
                    location.AddEntity(Banner.Create("you win"));
                }
            });

            new TouchRule<Player, Enemy>("Enemy victory", Rule.RuleType.VICTORY, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any() && Referee.IsStarted)
                {
                    Referee.Stop();
                    location.AddEntity(Banner.Create("you win"));
                }
            });

            new TouchRule<Player, Powerup>("Powerup victory", Rule.RuleType.VICTORY, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any() && Referee.IsStarted)
                {
                    Referee.Stop();
                    location.AddEntity(Banner.Create("you win"));
                }
            });

            new TouchRule<Player, Enemy>("Enemy hurty", Rule.RuleType.DEATH, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any() && Referee.IsStarted)
                {
                    Referee.Stop();
                    location.AddEntity(Banner.Create("you lose"));
                }
            });

            new TouchRule<Player, Goal>("Goal hurty", Rule.RuleType.DEATH, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any() && Referee.IsStarted)
                {
                    Referee.Stop();
                    location.AddEntity(Banner.Create("you lose"));
                }
            });

            // TODO: Make it so the "Player" is what is being controlled
            new TouchRule<Player, Powerup>("Player pickup Powerup", Rule.RuleType.POWERUP, (location, obj) =>
            {
                Powerup powerup = obj as Powerup;
                Program.Referee.AddRule(powerup.Rule);
                location.RemoveEntity(powerup.Id);
                ////Referee.AddRule(Rule.Rules["Enemy pickup Powerup"]);
            });

            new TouchRule<Enemy, Powerup>("Enemy pickup Powerup", Rule.RuleType.POWERUP, (location, obj) =>
            {
                Powerup powerup = obj as Powerup;
                Program.Referee.AddRule(powerup.Rule);
                location.RemoveEntity(powerup.Id);
                ////Referee.AddRule(Rule.Rules["Player pickup Powerup"]);
            });

            new TouchRule<Description2D, Powerup>("Any pickup Powerup", Rule.RuleType.POWERUP, (location, obj) =>
            {
                Powerup powerup = obj as Powerup;
                Program.Referee.AddRule(powerup.Rule);
                location.RemoveEntity(powerup.Id);
                ////Referee.AddRule(Rule.Rules["Player pickup Powerup"]);
            });

            new Rule("clicky attack", Rule.RuleType.ATTACK, (location, obj) =>
            {
                if (Program.Mouse[(int)Program.Actions.ACTION].IsPress())
                {
                    MouseControllerInfo mci = Program.Mouse[(int)Program.Actions.ACTION].Info as MouseControllerInfo;

                    Player player = location.GetEntities<Player>().First();
                    foreach (Enemy enemy in location.GetEntities<Enemy>())
                    {
                        if (enemy.Distance(new Point(mci.X, mci.Y)) < 8)
                        {
                            location.RemoveEntity(enemy.Id);
                        }
                    }
                }
            });

            new TouchRule<Player, Enemy>("Player kill Enemy", Rule.RuleType.ATTACK, (location, obj) =>
            {
                Enemy enemy = obj as Enemy;
                location.RemoveEntity(enemy.Id);
            });

            new TouchRule<Player, Goal>("Player kill Goal", Rule.RuleType.ATTACK, (location, obj) =>
            {
                Goal goal = obj as Goal;
                location.RemoveEntity(goal.Id);
            });

            new Rule("no attack. haha.", Rule.RuleType.ATTACK, (location, obj) => { });

            new Rule("be fast", Rule.RuleType.SPEED, (location, obj) =>
            {
                return 3;
            });

            new Rule("be slow", Rule.RuleType.SPEED, (location, obj) =>
            {
                return 0.8;
            });

            new Rule("be normal", Rule.RuleType.SPEED, (location, obj) =>
            {
                return 1;
            });

            new Rule("spawn Powerup", Rule.RuleType.SPAWN, (location, obj) =>
            {
                location.AddEntity(Powerup.Create(Rule.GetNameRandomRule(), Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenWidth - 16)));
            });

            new Rule("spawn Enemy", Rule.RuleType.SPAWN, (location, obj) =>
            {
                location.AddEntity(Enemy.Create(Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenWidth - 16)));
            });

            new Rule("spawn Goal", Rule.RuleType.SPAWN, (location, obj) =>
            {
                location.AddEntity(Goal.Create(Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenWidth - 16)));
            });

            new Rule("control Player", Rule.RuleType.CONTROL, (location, obj) =>
            {
                foreach(Player player in location.GetEntities<Player>())
                {
                    Referee.Piles[Rule.RuleType.PERSPECTIVE].Peek().Action(location, player);
                }
            });

            new Rule("control Enemy", Rule.RuleType.CONTROL, (location, obj) =>
            {
                foreach (Enemy enemy in location.GetEntities<Enemy>())
                {
                    Referee.Piles[Rule.RuleType.PERSPECTIVE].Peek().Action(location, enemy);
                }
            });

            new Rule("control Goal", Rule.RuleType.CONTROL, (location, obj) =>
            {
                foreach (Goal goal in location.GetEntities<Goal>())
                {
                    Referee.Piles[Rule.RuleType.PERSPECTIVE].Peek().Action(location, goal);
                }
            });

            new Rule("top-down", Rule.RuleType.PERSPECTIVE, ControlSchemas.TopDown);

            new Rule("platformer", Rule.RuleType.PERSPECTIVE, ControlSchemas.Platformer);

            new Rule("vvvvvv-platformer", Rule.RuleType.PERSPECTIVE, ControlSchemas.VVVVVVPlatformer);

            ////new Rule("colorblind", Rule.RuleType.OVERLAY, (location, obj) =>
            ////{

            ////});

            ////new Rule("vertical-flip", Rule.RuleType.OVERLAY, (location, obj) =>
            ////{

            ////});


            foreach (Rule.RuleType type in Enum.GetValues(typeof(Rule.RuleType)))
            {
                new Rule($"pop {type}", Rule.RuleType.POP, null);
            }

            Referee.AddRule(Rule.Rules["Goal victory"]);
            Referee.AddRule(Rule.Rules["Enemy hurty"]);
            Referee.AddRule(Rule.Rules["Player pickup Powerup"]);
            Referee.AddRule(Rule.Rules["clicky attack"]);
            Referee.AddRule(Rule.Rules["spawn Powerup"]);
            Referee.AddRule(Rule.Rules["control Player"]);
            Referee.AddRule(Rule.Rules["vvvvvv-platformer"]);

            for (int i = 0; i < 10; i++)
            {
                Referee.AddRule(Rule.GetNameRandomRule());
            }
        }

        public static void Sprites()
        {
            new Sprite("goal", 8, 8);

            new Sprite("player", 8, 8);

            new Sprite("banner", 0, 8);

            new Sprite("enemy", 8, 8);

            new Sprite("powerup", 8, 8);

            // TODO in engine: Make it so you don't need a sprite. thanks
            new Sprite("HUD", 0, 0);
            new Sprite("button", 0, 0);

            new Sprite("wall", 8, 8);
            new Sprite("gmtklogo", "Sprites/gmtklogo.png", 12804 / 66, 128, 12804 / 66 / 2, 128 / 2);
        }

        public static Dictionary<Avalonia.Input.Key, Actions> keyMap 
            = new Dictionary<Avalonia.Input.Key, Actions>(new[]
            {
                new KeyValuePair<Avalonia.Input.Key, Actions>(Avalonia.Input.Key.LeftAlt, Actions.ALT),
                new KeyValuePair<Avalonia.Input.Key, Actions>(Avalonia.Input.Key.R, Actions.RESTART),
                new KeyValuePair<Avalonia.Input.Key, Actions>(Avalonia.Input.Key.W, Actions.UP),
                new KeyValuePair<Avalonia.Input.Key, Actions>(Avalonia.Input.Key.S, Actions.DOWN),
                new KeyValuePair<Avalonia.Input.Key, Actions>(Avalonia.Input.Key.A, Actions.LEFT),
                new KeyValuePair<Avalonia.Input.Key, Actions>(Avalonia.Input.Key.D, Actions.RIGHT),
            });

        public static Dictionary<Avalonia.Input.PointerUpdateKind, Actions> mouseMap
            = new Dictionary<Avalonia.Input.PointerUpdateKind, Actions>(new[]
            {
                new KeyValuePair<Avalonia.Input.PointerUpdateKind, Actions>(Avalonia.Input.PointerUpdateKind.LeftButtonPressed, Actions.ACTION),
                new KeyValuePair<Avalonia.Input.PointerUpdateKind, Actions>(Avalonia.Input.PointerUpdateKind.RightButtonPressed, Actions.CANCEL),
                new KeyValuePair<Avalonia.Input.PointerUpdateKind, Actions>(Avalonia.Input.PointerUpdateKind.Other, Actions.MOUSEINFO)
            });

        public enum Actions { UP, DOWN, LEFT, RIGHT, ACTION, CANCEL, MOUSEINFO, ALT, RESTART };
    }
}
