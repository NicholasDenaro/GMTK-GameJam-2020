using Game.Rules;
using GameEngine;
using GameEngine._2D;
using GameEngine.UI;
using GameEngine.UI.AvaloniaUI;
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
                .GameView(new GameView2D(ScreenWidth, ScreenHeight, Scale, Scale, Color.DimGray))
                .GameFrame(new GameFrame(new AvaloniaWindowBuilder(), 0, 0, ScreenWidth, ScreenHeight, Scale, Scale))
                .Controller(Keyboard)
                .Controller(Mouse)
                .StartingLocation(new Location(new Description2D(0, 0, ScreenWidth, ScreenHeight)))
                .Build();

            Engine = Builder.Engine;

            Referee = new Referee();
            Engine.TickEnd += Referee.Tick;

            Sprites();

            Rules();

            Setup();

            Engine.TickEnd += (s, gs) =>
            {
                if (Program.Keyboard[(int)Actions.RESTART].IsPress())
                {
                    if (Engine.Location.GetEntities<Banner>().FirstOrDefault()?.Text == "you win")
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Referee.AddRule(Rule.GetNameRandomRule());
                        }
                    }

                    Referee.ResetTimer();
                    Engine.SetLocation(new Location(new Description2D(0, 0, ScreenWidth, ScreenHeight)));
                    Setup();
                }
            };

            Engine.Start();

            while (true) { }
        }

        public static void Setup()
        {
            Engine.AddEntity(Player.Create(16, 16));

            Engine.AddEntity(Goal.Create(Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenWidth - 16)));

            for (int i = 0; i < 20; i++)
            {
                Engine.AddEntity(Enemy.Create(Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenWidth - 16)));
            }

            Engine.AddEntity(HeadsUpDisplay.Create());
        }

        public static void Rules()
        {
            new TouchRule<Player, Goal>("Goal victory", Rule.RuleType.VICTORY, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any())
                {
                    location.AddEntity(Banner.Create("you win"));
                }
            });

            new TouchRule<Player, Enemy>("Enemy victory", Rule.RuleType.VICTORY, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any())
                {
                    location.AddEntity(Banner.Create("you win"));
                }
            });

            new TouchRule<Player, Powerup>("Powerup victory", Rule.RuleType.VICTORY, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any())
                {
                    location.AddEntity(Banner.Create("you win"));
                }
            });

            new TouchRule<Player, Enemy>("Enemy hurty", Rule.RuleType.DEATH, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any())
                {
                    location.AddEntity(Banner.Create("you lose"));
                }
            });

            new TouchRule<Player, Goal>("Goal hurty", Rule.RuleType.DEATH, (location, obj) =>
            {
                if (!location.GetEntities<Banner>().Any())
                {
                    location.AddEntity(Banner.Create("you lose"));
                }
            });

            new TouchRule<Player, Powerup>("Player pickup Powerup", Rule.RuleType.POWERUP, (location, obj) =>
            {
                Powerup powerup = obj as Powerup;
                Program.Referee.AddRule(powerup.Rule);
                location.RemoveEntity(powerup.Id);
                Referee.AddRule(Rule.Rules["Enemy pickup Powerup"]);
            });

            new TouchRule<Enemy, Powerup>("Enemy pickup Powerup", Rule.RuleType.POWERUP, (location, obj) =>
            {
                Powerup powerup = obj as Powerup;
                Program.Referee.AddRule(powerup.Rule);
                location.RemoveEntity(powerup.Id);
                Referee.AddRule(Rule.Rules["Player pickup Powerup"]);
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

            new Rule("pop SPAWN", Rule.RuleType.POP, null);

            Referee.AddRule(Rule.Rules["Goal victory"]);
            Referee.AddRule(Rule.Rules["Enemy hurty"]);
            Referee.AddRule(Rule.Rules["Player pickup Powerup"]);
            Referee.AddRule(Rule.Rules["clicky attack"]);
            Referee.AddRule(Rule.Rules["spawn Powerup"]);

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
