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

        public bool IsStarted { get; private set; }

        public bool OutofControl { get; set; }

        public Dictionary<Rule.RuleType, Stack<Rule>> Piles { get; private set; }

        public Referee()
        {
            Piles = new Dictionary<Rule.RuleType, Stack<Rule>>();

            ClearRules();

            ResetTimer();
        }

        public void ResetTimer(int time = 60 * Program.TPS)
        {
            Timer = time;

            ruleIndex = 0;
        }

        public void Tick(object sender, GameState state)
        {
            if (!IsStarted)
            {
                return;
            }

            if (Timer <= 0)
            {
                if (!state.Location.GetEntities<Banner>().Any())
                {
                    state.Location.AddEntity(Banner.Create("time expired"));
                }

                return;
            }

            if (state.Location.GetEntities<Banner>().Any())
            {
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

                        if (stack.Any())
                        {
                            stack.Pop();
                        }
                    }
                    else if (stack.Peek().Type != Rule.RuleType.SPAWN || Timer % Program.TPS == 0)
                    {
                        stack.Peek().Action(state.Location, null);
                    }
                }
            }

            if (OutofControl)
            {
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
        }

        public void AddRule(string name)
        {
            Rule rule = Rule.Rules[name];
            Piles[rule.Type].Push(rule);

            if(rule.Type == Rule.RuleType.PERSPECTIVE)
            {
                ControlSchemas.Reset();
            }
        }

        public void AddRule(Rule rule)
        {
            Piles[rule.Type].Push(rule);
        }

        internal void Start()
        {
            IsStarted = true;
        }

        internal void ClearRules()
        {
            Piles.Clear();
            ruleTypes = new List<Rule.RuleType>();
            foreach (Rule.RuleType type in Enum.GetValues(typeof(Rule.RuleType)))
            {
                ruleTypes.Add(type);
                Piles.Add(type, new Stack<Rule>());
            }
        }

        internal void Stop()
        {
            IsStarted = false;
        }
    }
}
