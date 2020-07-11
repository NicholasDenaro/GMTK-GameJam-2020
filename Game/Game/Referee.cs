using GameEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    public class Referee
    {
        public int Timer { get; private set; }

        public Dictionary<Rule.RuleType, Stack<Rule>> Piles { get; private set; }

        public Referee()
        {
            Piles = new Dictionary<Rule.RuleType, Stack<Rule>>();

            foreach (Rule.RuleType type in Enum.GetValues(typeof(Rule.RuleType)))
            {
                Piles.Add(type, new Stack<Rule>());
            }

            ResetTimer();
        }

        public void ResetTimer()
        {
            Timer = 15 * Program.TPS;
        }

        public void Tick(object sender, GameState state)
        {
            foreach (Stack<Rule> stack in Piles.Values)
            {
                if (stack.Any())
                {
                    if (stack.Peek().Type == Rule.RuleType.POP)
                    {
                        Rule.RuleType popType = (Rule.RuleType)Enum.Parse(typeof(Rule.RuleType), stack.Peek().Name.Split(' ')[1]);
                        if (Piles[popType].Any())
                        {
                            Piles[popType].Pop();
                        }

                        stack.Pop();
                    }
                    else if (stack.Peek().Type != Rule.RuleType.SPAWN || Timer % Program.TPS == 0)
                    {
                        stack.Peek().Action(state.Location, null);
                    }
                }
            }

            Timer--;

            if (Timer <= 0)
            {
                state.Location.AddEntity(Banner.Create("time expired"));
            }

            if (Timer % Program.TPS == 0)
            {
                state.Location.AddEntity(Powerup.Create(Rule.GetNameRandomRule(), Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenWidth - 16)));
            }
        }

        public void AddRule(string name)
        {
            Rule rule = Rule.Rules[name];
            Piles[rule.Type].Push(rule);
        }

        public void AddRule(Rule rule)
        {
            Piles[rule.Type].Push(rule);
        }
    }
}
