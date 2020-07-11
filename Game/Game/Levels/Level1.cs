using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Levels
{
    public class Level1 : Level
    {
        public override void SetupLevel()
        {
            Program.Level = 1;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

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
                    Program.Referee.AddRule(deck.Pop());
                }
            };

            Program.Engine.AddEntity(deckFlipper);

            Program.Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2));

            Program.Engine.AddEntity(Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2));

            Program.Engine.AddEntity(HeadsUpDisplay.Create());

            Program.Referee.ClearRules();

            Program.Referee.Start();
        }
    }
}
