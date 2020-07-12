using GameEngine;
using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NAudio.Wave;

namespace Game.Levels
{
    public class Level4 : Level
    {

        public override void SetupLevel()
        {
            Program.Level = 4;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Stack<Action> deck = new Stack<Action>();
            deck.Push(() =>
            {
                bool dialogShown = false;
                Entity ent = Powerup.Create("pop DEATH", 32, Program.ScreenHeight / 2);
                ent.TickAction = (loc, e) =>
                {
                    if (!dialogShown && loc.GetEntities<Player>().First().Distance((Description2D)e.Description) < 12)
                    {
                        Program.Engine.AddEntity(DialogBox.Create("Now I can sneak by them!"));
                        dialogShown = true;
                    }
                };
                Program.Engine.AddEntity(ent);
            });

            deck.Push(() =>
            {
                bool dialogShown = false;
                Entity ent = Powerup.Create("pop CONTROL", Program.ScreenWidth - 32, Program.ScreenHeight / 2);
                ent.TickAction = (loc, e) =>
                {
                    if (!dialogShown && loc.GetEntities<Player>().First().Distance((Description2D)e.Description) < 12)
                    {
                        Program.Engine.AddEntity(DialogBox.Create("Oh no, I can't move. Better restart.\n(Press R)"));
                        dialogShown = true;
                    }
                };
                Program.Engine.AddEntity(ent);
            });
            deck.Push(() => Program.Engine.AddEntity(Powerup.Create("pop VICTORY", Program.ScreenWidth / 2, Program.ScreenHeight / 2)));

            Program.Referee.ClearRules();

            Program.Referee.AddRule(Rule.Rules["Goal victory"]);
            Program.Referee.AddRule(Rule.Rules["Enemy hurty"]);
            Program.Referee.AddRule(Rule.Rules["Player pickup Powerup"]);
            Program.Referee.AddRule(Rule.Rules["control Player"]);
            Program.Referee.AddRule(Rule.Rules["top-down"]);

            SinWaveSound sound = new SinWaveSound(true,
                80, 44100 / Program.TPS * 30, 0, 44100 / Program.TPS * 5,
                150, 44100 / Program.TPS * 30, 0, 44100 / Program.TPS * 5,
                100, 44100 / Program.TPS * 30, 0, 44100 / Program.TPS * 5,
                120, 44100 / Program.TPS * 30, 0, 44100 / Program.TPS * 5);
            sound.SetWaveFormat(44100, 2);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Program.Engine.Location.GetEntities<DialogBox>().Any() || Program.Engine.Location.GetEntities<Banner>().Any())
                {
                    return;
                }

                if (Program.Referee.IsStarted && deck.Any() && timer++ % (Program.TPS * 5) == 0)
                {
                    deck.Pop().Invoke();
                }
            };

            Program.Engine.AddEntity(DialogBox.Create("What are those? They don't look\ntoo friendly."));

            Program.Engine.AddEntity(deckFlipper);

            Program.Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2));

            bool dialogShown = false;
            Entity entity = Goal.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2);
            entity.TickAction = (loc, e) =>
            {
                if (!dialogShown && !Program.Referee.Piles[Rule.RuleType.VICTORY].Any() && loc.GetEntities<Player>().First().Distance((Description2D)e.Description) < 12)
                {
                    Program.Engine.AddEntity(DialogBox.Create("Hmm? Didn't I start with a victory\ncondition? Better try this again.\n(Press R)"));
                    dialogShown = true;
                }
            };
            Program.Engine.AddEntity(entity);

            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 - 16, Program.ScreenHeight / 2));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 - 16));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64 + 16, Program.ScreenHeight / 2));
            Program.Engine.AddEntity(Enemy.Create(Program.ScreenWidth - 64, Program.ScreenHeight / 2 + 16));

            Program.Engine.AddEntity(HeadsUpDisplay.Create());

            Program.Referee.Start();

            Program.WavProvider.AddMixerInput((ISampleProvider)sound);
            Program.WavPlayer.Play();
        }
    }
}
