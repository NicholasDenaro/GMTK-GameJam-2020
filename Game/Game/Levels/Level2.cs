using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Levels
{
    public class Level2 : Level
    {

        public override void SetupLevel()
        {
            Program.Level = 2;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

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

            Program.Referee.ClearRules();

            Program.Referee.AddRule(Rule.Rules["Goal victory"]);
            Program.Referee.AddRule(Rule.Rules["control Player"]);
            Program.Referee.AddRule(Rule.Rules["top-down"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (deck.Any() && timer++ % (Program.TPS * 1) == 0)
                {
                    Rule rule = deck.Pop();
                    Program.Referee.AddRule(rule);
                }
            };

            Program.Engine.AddEntity(deckFlipper);

            Program.Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2));

            Program.Engine.AddEntity(Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2));

            Program.Engine.AddEntity(HeadsUpDisplay.Create());

            Program.Referee.Start();
        }
    }
}
