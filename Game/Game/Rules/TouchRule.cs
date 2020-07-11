using GameEngine;
using GameEngine._2D;
using System;

namespace Game.Rules
{
    public class TouchRule<T1, T2> : Rule 
        where T1 : Description2D 
        where T2 : Description2D
    {
        public override Func<Location, object, double> Action { get => (location, obj) => TouchAction(location); }

        public TouchRule(string name, Rule.RuleType type, Action<Location, object> action) : base(name, type, action)
        {
        }

        private double TouchAction(Location location)
        {
            foreach (T1 t1 in location.GetEntities<T1>())
            {
                foreach (T2 t2 in location.GetEntities<T2>())
                {
                    if (t1 != t2 && t1.GetType() != t2.GetType() && t1.Distance(t2) < 8)
                    {
                        base.Action(location, t2);
                    }
                }
            }

            return 0;
        }
    }
}
