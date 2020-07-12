using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Game.Levels
{
    public class Level2 : Level
    {

        public override void SetupLevel()
        {
            Program.Level = 2;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Action good = () =>
            {
                Program.Referee.AddRule(Rule.Rules["Goal victory"]);
                Program.Referee.Piles[Rule.RuleType.DEATH].Pop();
            };

            Action bad = () =>
            {
                Program.Referee.Piles[Rule.RuleType.VICTORY].Pop();
                Program.Referee.AddRule("Goal hurty");
            };

            Stack<Action> deck = new Stack<Action>();
            for (int i = 0; i < 3; i++)
            {
                deck.Push(bad);
                deck.Push(good);
            }

            deck.Push(bad);
            deck.Push(() =>
            {
                good();
                Program.Engine.Location.AddEntity(DialogBox.Create("And now it's back! Better time this right."));
            });
            deck.Push(() =>
            {
                bad();
                Program.Engine.Location.AddEntity(DialogBox.Create("Wait... the victory condition is removed?\nAnd the goal will hurt me?!"));
            });
            deck.Push(() => Program.Engine.Location.AddEntity(DialogBox.Create("Let's make it to the goal again.")));

            Program.Referee.ClearRules();

            Program.Referee.AddRule(Rule.Rules["Goal victory"]);
            Program.Referee.AddRule(Rule.Rules["control Player"]);
            Program.Referee.AddRule(Rule.Rules["top-down"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Program.Engine.Location.GetEntities<DialogBox>().Any())
                {
                    return;
                }

                if (deck.Any() && timer++ % (Program.TPS * 1) == 0)
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
