using Game.Levels;
using Game.Rules;
using GameEngine;
using GameEngine._2D;
using GameEngine.UI;
using GameEngine.UI.AvaloniaUI;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public static int StartingLevel = 1;
        public static int Level = 0;

        public static bool CreditsFinished = true;

        public static int ArcadeWins = 0;

        public enum Difficulty { EASY, NORMAL, HARD };

        public static Difficulty Diff = Difficulty.NORMAL;

        public static int Lives { get; set; } = 1;

        public static int Iframe { get; set; } = 0;

        public static List<Level> Levels { get; private set; } = new List<Level>();

        public static Random Random { get; private set; } = new Random();

        public static Controller Keyboard { get; private set; }
        public static Controller Mouse { get; private set; }

        public static Engine Engine { get; private set; }

        public static GameBuilder Builder { get; private set; }

        public static Referee Referee { get; private set; }

        public static bool ShowDiags { get; private set; }



        public static IWavePlayer WavPlayer { get; private set; } = new WasapiOut(AudioClientShareMode.Shared, 0);
        public static MixingSampleProvider WavProvider { get; private set; } = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));

        public static void Main(string[] args)
        {
            WavPlayer.Init(WavProvider);
            WavPlayer.Play();
            Keyboard = new WindowsKeyController(keyMap.ToDictionary(kvp => (int)kvp.Key, kvp => (int)kvp.Value));
            Mouse = new WindowsMouseController(mouseMap.ToDictionary(kvp => (int)kvp.Key, kvp => (int)kvp.Value));

            Builder = new GameBuilder();
            Builder.GameEngine(new FixedTickEngine(TPS))
                .GameView(new GameView2D(ScreenWidth, ScreenHeight, Scale, Scale, Color.DarkSlateGray))
                .GameFrame(new GameFrame(new AvaloniaWindowBuilder(), 0, 0, ScreenWidth, ScreenHeight, Scale, Scale))
                .Controller(Keyboard)
                .Controller(Mouse)
                .Build();

            Engine = Builder.Engine;

            GameFrame frame = Builder.Frame;
            AvaloniaWindow window = frame.Window as AvaloniaWindow;

            Referee = new Referee();
            Engine.TickEnd += Referee.Tick;

            Sprites();

            Rules();

            SetupLevels();

            SetupTitleScreen();

            Engine.TickEnd += (s, gs) =>
            {
                if (Iframe > 0)
                {
                    Iframe--;
                }

                if (Program.Keyboard[(int)Actions.ESCAPE].IsPress())
                {
                    Program.WavPlayer.Stop();
                    Program.WavProvider.RemoveAllMixerInputs();
                    StopMovingWindow();
                    SetupTitleScreen();
                }

                if (Program.Keyboard[(int)Actions.DIAGS].IsPress())
                {
                    ShowDiags = !ShowDiags;
                }

                if (Program.Keyboard[(int)Actions.RESTART].IsPress())
                {
                    if ((Program.Referee.OutofControl || Program.Level == 7) && !Engine.Location.GetEntities<Banner>().Any())
                    {
                        return;
                    }

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

                    StopMovingWindow();
                    Program.WavPlayer.Stop();
                    Program.WavProvider.RemoveAllMixerInputs();
                    // Levels can call the reset with something different
                    Referee.ResetTimer();

                    switch (Level)
                    {
                        case -1:
                            SetupCrazyMode();
                            break;
                        case 0:
                            break;
                        case 8:
                            SetupCredits();
                            Level = 9;
                            break;
                        case 9:
                            if (CreditsFinished)
                            {
                                SetupThanksForPlaying();
                                Level = 10;
                            }
                            break;
                        case 10:
                            SetupTitleScreen();
                            break;
                        default:
                            if (CreditsFinished)
                            {
                                StartingLevel = Level;
                                Levels[Level - 1].SetupLevel();
                            }
                            break;

                    }

                    if (Program.Diff == Difficulty.NORMAL && Level == 7)
                    {
                        Lives = 3;
                    }
                    else if (Program.Diff == Difficulty.EASY && Level == 7)
                    {
                        Lives = 2;
                    }
                    else
                    {
                        Lives = 1;
                    }
                }
            };

            Engine.Start();

            while (true) { }
        }

        private static void StopMovingWindow()
        {
            Boss boss = Program.Engine.Location.GetEntities<Boss>().FirstOrDefault();
            if (boss != null)
            {
                if (boss.windowAction != null)
                {
                    Program.Engine.TickEnd -= boss.windowAction;
                }
            }
        }

        public static void SetupCrazyMode()
        {
            Level = -1;

            Program.Referee.ClearRules();

            Program.Referee.OutofControl = true;

            for (int i = 0; i < 50; i++)
            {
                string rule = Rule.GetNameRandomRule();
                while(Rule.Rules[rule].Type == Rule.RuleType.POP)
                {
                    rule = Rule.GetNameRandomRule();
                }

                Referee.AddRule(rule);
            }

            Program.Referee.AddRule(Rule.Rules["top-down"]);
            Program.Referee.AddRule(Rule.Rules["Enemy hurty"]);
            Program.Referee.AddRule(Rule.Rules["control Player"]);
            Program.Referee.AddRule(Rule.Rules["Goal victory"]);

            Engine.SetLocation(new Location(new Description2D(0, 0, ScreenWidth, ScreenHeight)));
            Engine.AddEntity(Player.Create(Program.ScreenWidth / 4, Program.ScreenHeight / 2));

            Point center = new Point(Program.ScreenWidth * 3 / 4, Program.ScreenHeight / 2);

            Engine.AddEntity(Goal.Create(center.X + Program.Random.Next(-32, 48), center.Y + Program.Random.Next(-48, 48)));

            Action<Location, Entity> moving = (location, entity) =>
            {
                Enemy d2d = entity.Description as Enemy;
                d2d.ChangeCoordsDelta(d2d.VelX, Math.Sin(d2d.VelY));
                Description2D locd2d = location.Description as Description2D;

                if (d2d.X < 0 || d2d.X > locd2d.Width)
                {
                    d2d.VelX = -d2d.VelX;
                }
                if (d2d.Y < 0 || d2d.Y > locd2d.Height)
                {
                    d2d.VelY = -d2d.VelY;
                }
            };
            
            for (int i = 0; i < 20; i++)
            {
                Entity enemy = Enemy.Create(center.X + (int)(48 * Math.Cos(i / 20.0 * Math.PI * 2)), center.Y + (int)(48 * Math.Sin(i / 20.0 * Math.PI * 2)));
                enemy.TickAction = moving;
                //Engine.AddEntity(Enemy.Create(Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenHeight - 16)));
                Engine.AddEntity(enemy);
            }

            Engine.AddEntity(HeadsUpDisplay.Create());

            Referee.Start();
        }

        public static void SetupCredits()
        {
            Engine.SetLocation(new Location(new Description2D(0, 0, ScreenWidth, ScreenHeight)));

            int time = Program.ScreenHeight / 2 + 8;
            TickHandler t = null;

            CreditsFinished = false;

            t = (s, o) =>
            {
                if (time-- <= 0)
                {
                    Engine.TickEnd -= t;
                    CreditsFinished = true;
                }
            };

            Engine.TickEnd += t;

            Action<Location, Entity> scroll = (loc, ent) =>
            {
                Description2D entd = ent.Description as Description2D;

                if (time > 0)
                {
                    entd.ChangeCoordsDelta(0, -1);
                }
            };

            int y = Program.ScreenHeight / 2;

            Engine.AddEntity(Text.Create("Credits", new Font("Arial", 32, FontStyle.Underline), ScreenWidth / 2, y + 16).AddTickAction(scroll));

            Engine.AddEntity(Text.Create("Developed by", new Font("Arial", 16, FontStyle.Underline), ScreenWidth / 2, y + 64).AddTickAction(scroll));
            Engine.AddEntity(Text.Create("Nicholas Denaro", new Font("Arial", 12), ScreenWidth / 2, y + 96).AddTickAction(scroll));

            Engine.AddEntity(Text.Create("Art by", new Font("Arial", 16, FontStyle.Underline), ScreenWidth / 2, y + 128).AddTickAction(scroll));
            Engine.AddEntity(Text.Create("System.Drawing.Common", new Font("Arial", 12), ScreenWidth / 2, y + 160).AddTickAction(scroll));

            Engine.AddEntity(Text.Create("Special thanks to", new Font("Arial", 16, FontStyle.Underline), ScreenWidth / 2, y + 192).AddTickAction(scroll));
            Engine.AddEntity(Text.Create("My faithful Twitch chat <3", new Font("Arial", 10), ScreenWidth / 2, y + 224).AddTickAction(scroll));
        }

        public static void SetupThanksForPlaying()
        {
            Engine.SetLocation(new Location(new Description2D(0, 0, ScreenWidth, ScreenHeight)));

            Engine.AddEntity(Text.Create("Thanks for playing!", new Font("Arial", 24), ScreenWidth / 2, ScreenHeight / 2 - 24));
        }

        public static void SetupTitleScreen()
        {
            Referee.OutofControl = false;
            Level = 0;
            Referee.Stop();
            Engine.SetLocation(new Location(new Description2D(0, 0, ScreenWidth, ScreenHeight)));
            Description2D logoDescription = new Description2D(Sprite.Sprites["gmtklogo"], ScreenWidth / 2, ScreenHeight / 2);
            Entity logo = new Entity(logoDescription);
            int timer = TPS * 2;
            logo.TickAction = (loc, ent) =>
            {
                bool isPress = Program.Mouse[(int)Actions.ACTION].IsPress();

                if (logoDescription.ImageIndex < Sprite.Sprites["gmtklogo"].HImages - 1 && timer == TPS * 2)
                {
                    if (isPress)
                    {
                        logoDescription.ImageIndex = Sprite.Sprites["gmtklogo"].HImages - 1;
                    }
                    else
                    {
                        logoDescription.ImageIndex++;
                    }
                }
                else
                {
                    if (timer-- == 0 || timer > 0 && isPress)
                    {
                        timer = -1;
                        logoDescription.ImageIndex = 0;

                        Engine.AddEntity(Text.Create("RuleScramble", new Font("", 24, FontStyle.Italic | FontStyle.Bold | FontStyle.Underline), ScreenWidth / 2, 16));
                        Engine.AddEntity(Button.Create("Story Mode", () =>
                        {
                            Levels[StartingLevel - 1].SetupLevel();

                            if (Program.Diff == Difficulty.NORMAL && Level == 7)
                            {
                                Lives = 3;
                            }
                            else if (Program.Diff == Difficulty.EASY && Level == 7)
                            {
                                Lives = 2;
                            }
                            else
                            {
                                Lives = 1;
                            }
                        }, ScreenWidth / 2 - 128 / 2, ScreenHeight / 2 - 48));
                        Engine.AddEntity(Button.Create("Arcade Mode", SetupCrazyMode, ScreenWidth / 2 - 128 / 2, ScreenHeight / 2 + 8));
                        Engine.AddEntity(Button.Create("Credits", SetupCredits, ScreenWidth / 2 - 128 / 2, ScreenHeight / 2 + 64));

                        Button easyButton = null;
                        Button normalButton = null; 
                        Button hardButton = null;

                        Entity button = Button.Create("easy", () =>
                        {
                            Program.Diff = Difficulty.EASY;
                            easyButton.IsSelected = true;
                            normalButton.IsSelected = false;
                            hardButton.IsSelected = false;
                        }, ScreenWidth / 2 + 80, ScreenHeight / 2 - 48, 64, 32);
                        Engine.AddEntity(button);
                        easyButton = button.Description as Button;
                        if (Program.Diff == Difficulty.EASY)
                        {
                            easyButton.IsSelected = true;
                        }

                        button = Button.Create("normal", () =>
                        {
                            Program.Diff = Difficulty.NORMAL;
                            easyButton.IsSelected = false;
                            normalButton.IsSelected = true;
                            hardButton.IsSelected = false;
                        }, ScreenWidth / 2 + 80, ScreenHeight / 2 - 16, 64, 32);
                        Engine.AddEntity(button);
                        normalButton = button.Description as Button;
                        if (Program.Diff == Difficulty.NORMAL)
                        {
                            normalButton.IsSelected = true;
                        }

                        button = Button.Create("hard", () =>
                        {
                            Program.Diff = Difficulty.HARD;
                            easyButton.IsSelected = false;
                            normalButton.IsSelected = false;
                            hardButton.IsSelected = true;
                        }, ScreenWidth / 2 + 80, ScreenHeight / 2 + 16, 64, 32);
                        Engine.AddEntity(button);
                        hardButton = button.Description as Button;
                        if (Program.Diff == Difficulty.HARD)
                        {
                            hardButton.IsSelected = true;
                        }
                    }
                }
            };

            Engine.AddEntity(logo);
        }

        public static void SetupLevels()
        {
            Levels.Add(new Level1());
            Levels.Add(new Level2());
            Levels.Add(new Level3());
            Levels.Add(new Level4());
            Levels.Add(new Level5());
            Levels.Add(new Level6());
            Levels.Add(new Level7());
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
            return d1.IsCollision(d2);

            /*double d1Size = Math.Sqrt(d1.Width * d1.Width + d1.Height * d1.Height / 4);
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

            return false;*/
        }

        public static void Rules()
        {
            // TODO: Make it so the "Player" is what is being controlled
            new TouchRule<Player, Goal>("Goal victory", Rule.RuleType.VICTORY, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any() && Referee.IsStarted)
                {
                    Referee.Stop();
                    Program.Engine.AddEntity(Banner.Create("you win"));
                    if (Referee.OutofControl)
                    {
                        ArcadeWins++;
                    }
                }
            });

            new TouchRule<Player, Enemy>("Enemy victory", Rule.RuleType.VICTORY, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any() && Referee.IsStarted)
                {
                    Referee.Stop();
                    Program.Engine.AddEntity(Banner.Create("you win"));
                    if (Referee.OutofControl)
                    {
                        ArcadeWins++;
                    }
                }
            });

            new TouchRule<Player, Powerup>("Powerup victory", Rule.RuleType.VICTORY, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any() && Referee.IsStarted)
                {
                    Referee.Stop();
                    Program.Engine.AddEntity(Banner.Create("you win"));
                    if (Referee.OutofControl)
                    {
                        ArcadeWins++;
                    }
                }
            });

            new TouchRule<Player, Enemy>("Enemy hurty", Rule.RuleType.DEATH, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any() && Referee.IsStarted)
                {
                    if (Iframe == 0 && Lives-- == 1)
                    {
                        Referee.Stop();
                        if (!Referee.OutofControl)
                        {
                            Program.Engine.AddEntity(Banner.Create("you lose"));
                        }
                        else
                        {
                            Program.Engine.AddEntity(Banner.Create($"you lose. Score: {ArcadeWins}"));
                            ArcadeWins = 0;
                        }
                    }

                    if (Iframe == 0)
                    {
                        Iframe = TPS;
                    }
                }
            });

            new TouchRule<Player, Powerup>("Powerup hurty", Rule.RuleType.DEATH, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any() && Referee.IsStarted)
                {
                    if (Iframe == 0 && Lives-- == 1)
                    {
                        Referee.Stop();
                        if (!Referee.OutofControl)
                        {
                            Program.Engine.AddEntity(Banner.Create("you lose"));
                        }
                        else
                        {
                            Program.Engine.AddEntity(Banner.Create($"you lose. Score: {ArcadeWins}"));
                            ArcadeWins = 0;
                        }
                    }

                    if (Iframe == 0)
                    {
                        Iframe = TPS;
                    }
                }
            });

            new TouchRule<Player, Goal>("Goal hurty", Rule.RuleType.DEATH, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any() && Referee.IsStarted)
                {
                    if (Iframe == 0 && Lives-- == 1)
                    {
                        Referee.Stop();
                        if (!Referee.OutofControl)
                        {
                            Program.Engine.AddEntity(Banner.Create("you lose"));
                        }
                        else
                        {
                            Program.Engine.AddEntity(Banner.Create($"you lose. Score: {ArcadeWins}"));
                            ArcadeWins = 0;
                        }
                    }

                    if (Iframe == 0)
                    {
                        Iframe = TPS;
                    }
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

                    foreach (Enemy enemy in location.GetEntities<Enemy>())
                    {
                        if (enemy.Distance(new Point(mci.X, mci.Y)) < 8)
                        {
                            location.RemoveEntity(enemy.Id);
                        }
                    }
                }
            });

            new Rule("shoot Enemy", Rule.RuleType.ATTACK, (location, obj) =>
            {
                if (location.GetEntities<DialogBox>().Any())
                {
                    return;
                }

                if (Program.Mouse[(int)Program.Actions.ACTION].IsPress())
                {
                    MouseControllerInfo mci = Program.Mouse[(int)Program.Actions.ACTION].Info as MouseControllerInfo;

                    Player player = location.GetEntities<Player>().FirstOrDefault();
                    if (player == null)
                    {
                        return;
                    }
                    double dir = player.Direction(new Point(mci.X, mci.Y));

                    int spawnX = (int)(player.X + Math.Cos(dir) * 8);
                    int spawnY = (int)(player.Y + Math.Sin(dir) * 8);

                    location.AddEntity(Bullet<Enemy>.Create(spawnX, spawnY, dir));
                }
            });

            int shootTimer = 15;
            new Rule("shoot Boss", Rule.RuleType.ATTACK, (location, obj) =>
            {
                if (location.GetEntities<DialogBox>().Any())
                {
                    return;
                }

                if (shootTimer-- <= 0 && Program.Mouse[(int)Program.Actions.ACTION].IsDown())
                {
                    shootTimer = 15;
                    MouseControllerInfo mci = Program.Mouse[(int)Program.Actions.ACTION].Info as MouseControllerInfo;

                    Player player = location.GetEntities<Player>().FirstOrDefault();
                    if (player == null)
                    {
                        return;
                    }
                    double dir = player.Direction(new Point(mci.X, mci.Y));

                    int spawnX = (int)(player.X + Math.Cos(dir) * 8);
                    int spawnY = (int)(player.Y + Math.Sin(dir) * 8);

                    location.AddEntity(Bullet<BulletNull>.Create(spawnX, spawnY, dir));
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
                    if (Referee.Piles[Rule.RuleType.PERSPECTIVE].Any())
                    {
                        Referee.Piles[Rule.RuleType.PERSPECTIVE].Peek().Action(location, player);
                    }
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
            new Sprite("bullet", 2, 2);
            new Sprite("boss", 0, 40);

            new Sprite("wall", 8, 8);
            new Sprite("text", 0, 0);
            new Sprite("gmtklogo", "Sprites/gmtklogo.png", 12804 / 66, 128, 12804 / 66 / 2, 128 / 2);
        }

        public static Dictionary<Avalonia.Input.Key, Actions> keyMap 
            = new Dictionary<Avalonia.Input.Key, Actions>(new[]
            {
                new KeyValuePair<Avalonia.Input.Key, Actions>(Avalonia.Input.Key.F2, Actions.DIAGS),
                new KeyValuePair<Avalonia.Input.Key, Actions>(Avalonia.Input.Key.Escape, Actions.ESCAPE),
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

        public enum Actions { UP, DOWN, LEFT, RIGHT, ACTION, CANCEL, MOUSEINFO, ESCAPE, RESTART, DIAGS };
    }
}
