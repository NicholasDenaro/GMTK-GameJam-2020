using GameEngine;
using GameEngine._2D;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Game.Levels
{
    public class Level7 : Level
    {
        public static bool introDialogShown = false;

        public override void SetupLevel()
        {
            Program.Level = 7;

            Program.Engine.SetLocation(new Location(new Description2D(0, 0, Program.ScreenWidth, Program.ScreenHeight)));

            Entity entb = Boss.Create(Program.ScreenHeight - 16, Program.ScreenHeight / 2);
            Program.Engine.AddEntity(entb);

            Stack<Action> deck = new Stack<Action>();
            if (Boss.savedHealth == 100)
            {
                deck.Push(() =>
                {
                    Program.Engine.AddEntity(Powerup.Create("shoot Boss", Program.ScreenWidth - 128, Program.ScreenHeight / 2));
                    Program.Engine.AddEntity(DialogBox.Create("I just need to get close enough..."));
                });
                deck.Push(() => Program.Engine.AddEntity(DialogBox.Create("This isn't good. I have to do something.")));
                deck.Push(() => { });
            }

            Program.Referee.ClearRules();

            Program.Referee.AddRule(Rule.Rules["Enemy hurty"]);
            Program.Referee.AddRule(Rule.Rules["Player pickup Powerup"]);
            Program.Referee.AddRule(Rule.Rules["control Player"]);
            Program.Referee.AddRule(Rule.Rules["top-down"]);

            if(Program.Diff == Program.Difficulty.EASY && Boss.savedHealth < 100)
            {
                Program.Referee.AddRule(Rule.Rules["shoot Boss"]);
            }

            SinWaveSound sound = new SinWaveSound(true);
            sound.SetWaveFormat(44100, 2);

            Entity deckFlipper = new Entity(new Description2D(0, 0, 0, 0));
            int timer = 0;
            deckFlipper.TickAction = (loc, ent) =>
            {
                if (Program.Engine.Location.GetEntities<DialogBox>().Any() || Program.Engine.Location.GetEntities<Banner>().Any())
                {
                    return;
                }

                if (Program.Referee.IsStarted && deck.Any() && timer++ % (Program.TPS * 10) == 0)
                {
                    deck.Pop().Invoke();
                }
            };

            Program.Engine.AddEntity(deckFlipper);

            string machineName = Environment.MachineName;

            if (!introDialogShown)
            {
                Program.Engine.AddEntity(
                    DialogBox.Create("[???]: What where did you come from?",
                        DialogBox.Create("Who are you?",
                            DialogBox.Create($"[???] I am {machineName}.",
                                DialogBox.Create("That couldn't be true.",
                                    DialogBox.Create($"[{machineName}] It is. And you're not\nsupposed to be here. It's time\nto delete you."))))));
                introDialogShown = true;
            }
            else
            {
                Program.Engine.AddEntity(DialogBox.Create($"[{machineName}] It's time to delete you."));
            }

            Program.Engine.AddEntity(Player.Create(64, Program.ScreenHeight / 2));

            Program.Engine.AddEntity(Wall.Create(0, 0, 16, Program.ScreenHeight));
            Program.Engine.AddEntity(Wall.Create(Program.ScreenWidth, 0, 16, Program.ScreenHeight));
            Program.Engine.AddEntity(Wall.Create(0, 0, Program.ScreenWidth, 16));
            Program.Engine.AddEntity(Wall.Create(0, Program.ScreenHeight, Program.ScreenWidth + 16, 16));

            Program.Engine.AddEntity(HeadsUpDisplay.Create());

            Program.Referee.ResetTimer(Program.TPS * 180);

            Program.Referee.Start();

            Program.WavProvider.AddMixerInput((ISampleProvider)sound);
            Program.WavPlayer.Play();
        }
    }
}
