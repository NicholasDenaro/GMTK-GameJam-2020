using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Levels
{
    public class Level6 : Level
    {
        public override void SetupLevel()
        {
            Program.Level = 6;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Action> deck = new Stack<Action>();
            deck.Push(() =>
            {
                Program.Engine.AddEntity(Powerup.Create("pop DEATH", 24, Program.ScreenHeight / 2 - 48));
                Program.Engine.AddEntity(Powerup.Create("Enemy hurty", 24, Program.ScreenHeight / 2 - 16));
            });
            deck.Push(() =>  Program.Engine.AddEntity(Powerup.Create("shoot Enemy", Program.ScreenWidth / 2, Program.ScreenHeight / 2 + 16)));
            deck.Push(() => { });

            Program.Referee.ClearRules();

            Program.Referee.AddRule(Rule.Rules["Goal victory"]);
            Program.Referee.AddRule(Rule.Rules["Enemy hurty"]);
            Program.Referee.AddRule(Rule.Rules["Powerup hurty"]);
            Program.Referee.AddRule(Rule.Rules["Player pickup Powerup"]);
            Program.Referee.AddRule(Rule.Rules["control Player"]);
            Program.Referee.AddRule(Rule.Rules["vvvvvv-platformer"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Program.Engine.Location.GetEntities<DialogBox>().Any())
                {
                    return;
                }

                if (Program.Referee.IsStarted && deck.Any() && timer++ % (Program.TPS * 5) == 0)
                {
                    deck.Pop().Invoke();
                }
            };

            Program.Engine.AddEntity(deckFlipper);

            //top
            Program.Engine.AddEntity(Wall.Create(0, 0, 64, 16));
            Program.Engine.AddEntity(Enemy.Create(64, 0));
            Program.Engine.AddEntity(Enemy.Create(80, 0));
            Program.Engine.AddEntity(Wall.Create(96, 0, 64, 16));
            Program.Engine.AddEntity(Enemy.Create(160, 0));
            Program.Engine.AddEntity(Enemy.Create(176, 0));
            Program.Engine.AddEntity(Enemy.Create(192, 0));
            Program.Engine.AddEntity(Wall.Create(208, 0, 128, 16));

            //bottom
            Program.Engine.AddEntity(Wall.Create(0, Program.ScreenHeight, 64, 16));
            Program.Engine.AddEntity(Enemy.Create(64, Program.ScreenHeight));
            Program.Engine.AddEntity(Enemy.Create(80, Program.ScreenHeight));
            Program.Engine.AddEntity(Wall.Create(96, Program.ScreenHeight, 64, 16));
            Program.Engine.AddEntity(Enemy.Create(160, Program.ScreenHeight));
            Program.Engine.AddEntity(Enemy.Create(176, Program.ScreenHeight));
            Program.Engine.AddEntity(Enemy.Create(192, Program.ScreenHeight));
            Program.Engine.AddEntity(Enemy.Create(208, Program.ScreenHeight));

            //walls and enemies
            Program.Engine.AddEntity(Wall.Create(Program.ScreenWidth, 0, 16, Program.ScreenHeight));
            Program.Engine.AddEntity(Wall.Create(0, 0, 16, Program.ScreenHeight));

            Program.Engine.AddEntity(Powerup.Create("pop SPEED", Program.ScreenWidth - 96, 16));
            Program.Engine.AddEntity(Powerup.Create("pop SPEED", Program.ScreenWidth - 96, 32));
            Program.Engine.AddEntity(Wall.Create(Program.ScreenWidth - 96, 48, 16, Program.ScreenHeight - 32));

            for (int i = 16; i < Program.ScreenHeight; i += 16)
            {
                Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 80, i));
            }

            // box and player
            Program.Engine.AddEntity(Enemy.Create(16, Program.ScreenHeight / 2 - 32));
            Program.Engine.AddEntity(Enemy.Create(32, Program.ScreenHeight / 2 - 32));
            Program.Engine.AddEntity(Wall.Create(48, Program.ScreenHeight / 2 - 32, 64, 16));
            Program.Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2));
            Program.Engine.AddEntity(Wall.Create(48, Program.ScreenHeight / 2 + 32, 64, 16));
            Program.Engine.AddEntity(Wall.Create(112, Program.ScreenHeight / 2 - 32, 16, 80));


            //horizontal split

            Entity trigger = Powerup.Create("Enemy hurty", 160, Program.ScreenHeight / 2 - 16);
            Guid triggerId = trigger.Id;
            trigger.AddTickAction((loc, ent) =>
            {
                if (loc.GetEntities<Player>().First().Distance((Description2D)ent.Description) < 20)
                {
                    Program.Referee.AddRule("pop DEATH");
                    loc.RemoveEntity(triggerId);
                }
            });

            Program.Engine.AddEntity(trigger);

            Program.Engine.AddEntity(Wall.Create(112, Program.ScreenHeight / 2, 112, 16));
            Program.Engine.AddEntity(Powerup.Create("pop SPEED", 128, Program.ScreenHeight / 2 + 32));
            Program.Engine.AddEntity(Powerup.Create("pop SPEED", 144, Program.ScreenHeight / 2 + 32));
            Program.Engine.AddEntity(Powerup.Create("pop SPEED", 160, Program.ScreenHeight / 2 + 32));
            Program.Engine.AddEntity(Powerup.Create("pop SPEED", 176, Program.ScreenHeight / 2 + 32));
            Program.Engine.AddEntity(Powerup.Create("pop SPEED", 192, Program.ScreenHeight / 2 + 32));
            Program.Engine.AddEntity(Powerup.Create("pop SPEED", 208, Program.ScreenHeight / 2 + 32));


            Program.Engine.AddEntity(Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2));

            Program.Engine.AddEntity(HeadsUpDisplay.Create());

            Program.Referee.Start();
        }
    }
}
