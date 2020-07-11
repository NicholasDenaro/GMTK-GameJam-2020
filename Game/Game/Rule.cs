using GameEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class Rule
    {
        public static Dictionary<string, Rule> Rules = new Dictionary<string, Rule>();

        public static string GetNameRandomRule(RuleType? type = null)
        {
            List<string> filteredRules = Rules.Keys.ToList();
            if (type != null)
            {
                filteredRules = filteredRules.Where(name => Rules[name].Type == type).ToList();
            }

            if (!filteredRules.Any())
            {
                return null;
            }

            int r = Program.Random.Next(0, filteredRules.Count);
            return filteredRules.Skip(r).First();
        }

        public RuleType Type { get; private set; }

        public virtual Func<Location, object, double> Action { get; protected set; }

        public string Name { get; private set; }

        protected Rule(string name, RuleType type) : this(name, type, null)
        {

        }

        public Rule(string name, RuleType type, Action<Location, object> action) : this(name, type, (location, obj) => { action(location, obj); return 0; })
        {
        }

        public Rule(string name, RuleType type, Func<Location, object, double> action)
        {
            this.Type = type;
            this.Action = action;
            this.Name = name;

            Rules.Add(name, this);
        }

        public enum RuleType {
            ATTACK,
            VICTORY,
            DEATH,
            STYLE,
            SPEED,
            POWERUP,
            SPAWN,
            POP,
            PERSPECTIVE,
            OVERLAY,
            CONTROL
        }
    }
}
