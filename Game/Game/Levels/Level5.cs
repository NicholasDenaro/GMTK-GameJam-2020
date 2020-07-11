using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Levels
{
    public class Level5 : Level
    {

        public override void SetupLevel()
        {
            Program.Level = 5;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Action> deck = new Stack<Action>();

            Action spawnPowerups = () =>
            {
                Program.Engine.AddEntity(Powerup.Create("platformer", 224 - 8, Program.ScreenHeight / 2 + 80));
                Program.Engine.AddEntity(Powerup.Create("top-down", 256 - 8, Program.ScreenHeight / 2 + 80));
                Program.Engine.AddEntity(Powerup.Create("platformer", 288 - 8, Program.ScreenHeight / 2 + 80));
                Program.Engine.AddEntity(Powerup.Create("top-down", 314 - 8, Program.ScreenHeight / 2 + 80));
            };

            deck.Push(spawnPowerups);
            deck.Push(spawnPowerups);
            deck.Push(spawnPowerups);
            deck.Push(spawnPowerups);
            deck.Push(spawnPowerups);
            deck.Push(spawnPowerups);

            Program.Referee.ClearRules();

            Program.Referee.AddRule(Rule.Rules["Goal victory"]);
            Program.Referee.AddRule(Rule.Rules["Enemy hurty"]);
            Program.Referee.AddRule(Rule.Rules["Any pickup Powerup"]);
            Program.Referee.AddRule(Rule.Rules["control Player"]);
            Program.Referee.AddRule(Rule.Rules["top-down"]);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Program.Referee.IsStarted && deck.Any() && timer++ % (Program.TPS * 3.3) == 0)
                {
                    deck.Pop().Invoke();
                }
            };

            Program.Engine.AddEntity(deckFlipper);

            Program.Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2 + 64));

            Program.Engine.AddEntity(Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2));

            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 - 16, Program.ScreenHeight / 2));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 - 16));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 + 16, Program.ScreenHeight / 2));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 + 16));

            Program.Engine.AddEntity(Wall.Create(0, Program.ScreenHeight / 2 + 32, 80, 16));

            Program.Engine.AddEntity(Wall.Create(80, Program.ScreenHeight / 2 + 80, 112, 16));

            Program.Engine.AddEntity(Wall.Create(112, Program.ScreenHeight / 2 + 32, 64, 16));
            Program.Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 - 80, 16, 160));

            Program.Engine.AddEntity(Powerup.Create("clicky attack", 32, 32));

            Program.Engine.AddEntity(Powerup.Create("Enemy pickup Powerup", 176, 12));
            Program.Engine.AddEntity(Powerup.Create("Enemy pickup Powerup", 176, 24));

            for (int i = 0; i < Program.ScreenWidth; i += 16)
            {
                Program.Engine.AddEntity(Wall.Create(i, Program.ScreenHeight - 16));
            }

            Entity enemy = Enemy.Create(224, Program.ScreenHeight / 2 + 80);
            int deltaX = 1;
            enemy.TickAction += (loc, ent) =>
            {
                Description2D d2d = enemy.Description as Description2D;
                if (d2d.X == Program.ScreenWidth - 16 || d2d.X == 208)
                {
                    deltaX = -deltaX;
                }

                d2d.ChangeCoordsDelta(deltaX, 0);
            };

            Program.Engine.AddEntity(enemy);

            Program.Engine.AddEntity(HeadsUpDisplay.Create());

            Program.Referee.Start();
        }
    }
}
