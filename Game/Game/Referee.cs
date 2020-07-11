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

        private List<Rule.RuleType> ruleTypes;
        private int ruleIndex;

        public Dictionary<Rule.RuleType, Stack<Rule>> Piles { get; private set; }

        public Referee()
        {
            Piles = new Dictionary<Rule.RuleType, Stack<Rule>>();

            ruleTypes = new List<Rule.RuleType>();
            foreach (Rule.RuleType type in Enum.GetValues(typeof(Rule.RuleType)))
            {
                ruleTypes.Add(type);
                Piles.Add(type, new Stack<Rule>());
            }

            ResetTimer();
        }

        public void ResetTimer()
        {
            Timer = 60 * Program.TPS;

            ruleIndex = 0;
        }

        public void Tick(object sender, GameState state)
        {
            if (Timer <= 0)
            {
                if (!state.Location.GetEntities<Banner>().Any())
                {
                    state.Location.AddEntity(Banner.Create("time expired"));
                }

                return;
            }

            Timer--;

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

            if (Timer % Program.TPS == 0)
            {
                state.Location.AddEntity(Powerup.Create(Rule.GetNameRandomRule(), Program.Random.Next(16, Program.ScreenWidth - 16), Program.Random.Next(16, Program.ScreenWidth - 16)));
            }

            if (Timer % Program.TPS * 3 == 0)
            {
                // scroll through the list of rule types one by one
                string name = Rule.GetNameRandomRule(ruleTypes[ruleIndex++ % ruleTypes.Count]);
                if (name != null)
                {
                    AddRule(name);
                }
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
