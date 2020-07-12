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
            bool wobbleDialog = false;
            Entity movingEnemy = null;
            Program.Level = 5;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Action> deck = new Stack<Action>();

            Action spawnPowerups = () =>
            {
                Program.Engine.AddEntity(Powerup.Create("platformer", 224 - 8, Program.ScreenHeight / 2 + 80));
                Program.Engine.AddEntity(Powerup.Create("top-down", 256 - 8, Program.ScreenHeight / 2 + 80));
                Program.Engine.AddEntity(Powerup.Create("platformer", 288 - 8, Program.ScreenHeight / 2 + 80));
                Entity ent = Powerup.Create("top-down", 314 - 8, Program.ScreenHeight / 2 + 80);
                ent.TickAction = (loc, e) =>
                {
                    if (!wobbleDialog && ((Description2D)movingEnemy.Description).Distance((Description2D)e.Description) < 12)
                    {
                        Program.Engine.AddEntity(DialogBox.Create("It'll be hard to keep my orientation.\nBetter keep an eye on it."));
                        wobbleDialog = true;
                    }
                };
                Program.Engine.AddEntity(ent);
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
            Program.Referee.AddRule(Rule.Rules["platformer"]);

            Program.Engine.AddEntity(DialogBox.Create("Something seems different."));

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Program.Engine.Location.GetEntities<DialogBox>().Any() || Program.Engine.Location.GetEntities<Banner>().Any())
                {
                    return;
                }

                if (Program.Referee.IsStarted && deck.Any() && timer++ % (Program.TPS * 3.3) == 0)
                {
                    deck.Pop().Invoke();
                }
            };

            Program.Engine.AddEntity(deckFlipper);

            Program.Engine.AddEntity(Player.Create(Program.ScreenWidth / 2 - 32, Program.ScreenHeight - 24));

            Program.Engine.AddEntity(Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2));

            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 - 16, Program.ScreenHeight / 2));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 - 16));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 + 16, Program.ScreenHeight / 2));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 + 16));

            //border
            Program.Engine.AddEntity(Wall.Create(0, 0, 16, Program.ScreenHeight));
            Program.Engine.AddEntity(Wall.Create(Program.ScreenWidth, 0, 16, Program.ScreenHeight));
            Program.Engine.AddEntity(Wall.Create(0, 0, Program.ScreenWidth, 16));
            Program.Engine.AddEntity(Wall.Create(0, Program.ScreenHeight, Program.ScreenWidth + 16, 16));

            //others
            Program.Engine.AddEntity(Wall.Create(0, Program.ScreenHeight / 2 + 32, 80, 16));

            Program.Engine.AddEntity(Wall.Create(80, Program.ScreenHeight / 2 + 80, 112, 16));

            Program.Engine.AddEntity(Wall.Create(112, Program.ScreenHeight / 2 + 32, 64, 16));
            Program.Engine.AddEntity(Wall.Create(176, Program.ScreenHeight / 2 - 80, 16, 208));

            Entity ent = Powerup.Create("clicky attack", 32, 32);
            bool dialogShown = false;
            ent.TickAction = (loc, e) =>
            {
                if (!dialogShown && loc.GetEntities<Player>().First().Distance((Description2D)e.Description) < 12)
                {
                    Program.Engine.AddEntity(DialogBox.Create("I bet I can use this to clear a path."));
                    dialogShown = true;
                }
            };
            Program.Engine.AddEntity(ent);

            Program.Engine.AddEntity(Powerup.Create("Enemy pickup Powerup", 176, 24));

            movingEnemy = Enemy.Create(224, Program.ScreenHeight / 2 + 80);
            double deltaX = 0.5f;
            movingEnemy.TickAction += (loc, ent) =>
            {
                if (Program.Engine.Location.GetEntities<DialogBox>().Any())
                {
                    return;
                }

                Description2D d2d = movingEnemy.Description as Description2D;
                if (d2d.X == Program.ScreenWidth - 16 || d2d.X == 208)
                {
                    deltaX = -deltaX;
                }

                d2d.ChangeCoordsDelta(deltaX, 0);
            };

            Program.Engine.AddEntity(movingEnemy);

            Program.Engine.AddEntity(HeadsUpDisplay.Create());

            Program.Referee.Start();
        }
    }
}
