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

            Stack<Action> deck = new Stack<Action>();
            deck.Push(() =>
            {
                Program.Referee.AddRule(Rule.Rules["Goal victory"]);
                Program.Engine.AddEntity(DialogBox.Create("Oh, look! I can win by touching the goal."));
            });
            deck.Push(() =>
            {
                Program.Referee.AddRule(Rule.Rules["control Player"]);
                Program.Engine.AddEntity(DialogBox.Create("Aha! I can move now! But what to do?"));
            });
            deck.Push(() => Program.Engine.AddEntity(DialogBox.Create("What is this? I can't move...")));
            deck.Push(() => Program.Referee.AddRule(Rule.Rules["top-down"]));

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Program.Engine.Location.GetEntities<DialogBox>().Any())
                {
                    return;
                }

                if (Program.Referee.IsStarted && deck.Any() && timer++ % (Program.TPS * 2.5) == 0)
                {
                    deck.Pop().Invoke();
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
