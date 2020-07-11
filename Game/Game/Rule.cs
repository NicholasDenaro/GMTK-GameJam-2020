using GameEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class Rule
    {
        public static Dictionary<string, Rule> Rules = new Dictionary<string, Rule>();

        public static string GetNameRandomRule()
        {
            int r = Program.Random.Next(0, Rules.Count);
            return Rules.Keys.Skip(r).First();
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

        public enum RuleType { ATTACK, VICTORY, DEATH, STYLE, SPEED, POWERUP, SPAWN, POP }
    }
}
