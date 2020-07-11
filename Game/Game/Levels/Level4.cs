using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Game.Levels
{
    public class Level4 : Level
    {

        public override void SetupLevel()
        {
            Program.Level = 4;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Action> deck = new Stack<Action>();
            deck.Push(() => Program.Engine.AddEntity(Powerup.Create("pop DEATH", 32, Program.ScreenHeight / 2)));
            deck.Push(() => Program.Engine.AddEntity(Powerup.Create("pop CONTROL", Program.ScreenWidth - 32, Program.ScreenHeight / 2)));
            deck.Push(() => Program.Engine.AddEntity(Powerup.Create("pop VICTORY", Program.ScreenWidth / 2, Program.ScreenHeight / 2)));

            Program.Referee.ClearRules();

            Program.Referee.AddRule(Rule.Rules["Goal victory"]);
            Program.Referee.AddRule(Rule.Rules["Enemy hurty"]);
            Program.Referee.AddRule(Rule.Rules["Player pickup Powerup"]);
            Program.Referee.AddRule(Rule.Rules["control Player"]);
            Program.Referee.AddRule(Rule.Rules["top-down"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Program.Referee.IsStarted && deck.Any() && timer++ % (Program.TPS * 5) == 0)
                {
                    deck.Pop().Invoke();
                }
            };

            Program.Engine.AddEntity(deckFlipper);

            Program.Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2));

            Program.Engine.AddEntity(Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2));

            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 - 16, Program.ScreenHeight / 2));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 - 16));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 + 16, Program.ScreenHeight / 2));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 + 16));

            Program.Engine.AddEntity(HeadsUpDisplay.Create());

            Program.Referee.Start();
        }
    }
}
