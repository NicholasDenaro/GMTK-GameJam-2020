using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Game.Levels
{
    public class Level7 : Level
    {
        public override void SetupLevel()
        {
            Program.Level = 7;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Action> deck = new Stack<Action>();
            deck.Push(() => { });

            Program.Referee.ClearRules();

            Program.Referee.AddRule(Rule.Rules["Enemy hurty"]);
            Program.Referee.AddRule(Rule.Rules["shoot Boss"]);
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

            Program.Engine.AddEntity(Wall.Create(0, 0, 16, Program.ScreenHeight));
            Program.Engine.AddEntity(Wall.Create(Program.ScreenWidth, 0, 16, Program.ScreenHeight));
            Program.Engine.AddEntity(Wall.Create(0, 0, Program.ScreenWidth, 16));
            Program.Engine.AddEntity(Wall.Create(0, Program.ScreenHeight, Program.ScreenWidth + 16, 16));

            Program.Engine.AddEntity(HeadsUpDisplay.Create());

            Program.Engine.AddEntity(Boss.Create(Program.ScreenHeight - 16, Program.ScreenHeight / 2));

            Program.Referee.Start();
        }
    }
}
