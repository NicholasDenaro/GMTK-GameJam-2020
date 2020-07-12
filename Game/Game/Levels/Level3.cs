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
            int delay = Program.TPS * 1;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            bool dialogShown = false;
            Stack<Action> deck = new Stack<Action>();
            deck.Push(() =>
            {
                Program.Referee.AddRule(Rule.Rules["Player pickup Powerup"]);
                Program.Engine.AddEntity(DialogBox.Create("Oh, maybe now I can collect it."));
                dialogShown = false;
            });

            deck.Push(() =>
            {
                Entity ent = Powerup.Create("pop DEATH", Program.ScreenWidth - 32, Program.ScreenHeight / 2);
                delay = Program.TPS * 5;
                ent.TickAction = (loc, e) =>
                {
                    if (!dialogShown && loc.GetEntities<Player>().First().Distance((Description2D)e.Description) < 12)
                    {
                        if (!Program.Referee.Piles[Rule.RuleType.POWERUP].Any())
                        {
                            Program.Engine.AddEntity(DialogBox.Create("I guess nothing happens?"));
                        }
                        else
                        {
                            Program.Engine.AddEntity(DialogBox.Create("Oh, now I can use the goal."));
                        }
                        dialogShown = true;
                    }
                };
                Program.Engine.AddEntity(ent);
                Program.Engine.AddEntity(DialogBox.Create("What's that? Better go check it out."));
            });
            deck.Push(() => Program.Referee.AddRule(Rule.Rules["Goal hurty"]));
            deck.Push(() => Program.Referee.AddRule(Rule.Rules["Goal victory"]));
            deck.Push(() =>
            {
                Program.Engine.AddEntity(DialogBox.Create("I better watch out for any tricks."));
            });

            Program.Referee.ClearRules();

            Program.Referee.AddRule(Rule.Rules["control Player"]);
            Program.Referee.AddRule(Rule.Rules["top-down"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Program.Engine.Location.GetEntities<DialogBox>().Any() || Program.Engine.Location.GetEntities<Banner>().Any())
                {
                    return;
                }

                if (Program.Referee.IsStarted && deck.Any() && timer++ % delay == 0)
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
