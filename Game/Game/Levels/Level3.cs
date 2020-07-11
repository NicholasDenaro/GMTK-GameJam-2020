using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Levels
{
    public class Level3 : Level
    {

        public override void SetupLevel()
        {
            Program.Level = 3;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Action> deck = new Stack<Action>();
            deck.Push(() => Program.Referee.AddRule(Rule.Rules["Player pickup Powerup"]));
            deck.Push(() => Program.Engine.AddEntity(Powerup.Create("pop DEATH", Program.ScreenWidth - 32, Program.ScreenHeight / 2)));
            deck.Push(() => Program.Referee.AddRule(Rule.Rules["Goal hurty"]));
            deck.Push(() => Program.Referee.AddRule(Rule.Rules["Goal victory"]));
            deck.Push(() => Program.Referee.AddRule(Rule.Rules["pop VICTORY"]));

            Program.Referee.ClearRules();

            Program.Referee.AddRule(Rule.Rules["Goal victory"]);
            Program.Referee.AddRule(Rule.Rules["control Player"]);
            Program.Referee.AddRule(Rule.Rules["top-down"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Program.Referee.IsStarted && deck.Any() && timer++ % (Program.TPS * 1) == 0)
                {
                    deck.Pop().Invoke();
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
