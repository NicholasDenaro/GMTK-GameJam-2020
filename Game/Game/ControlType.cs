using GameEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class ControlType
    {
        public ControlType(Action<Location> action)
        {
            this.Action = action;
        }

        public Action<Location> Action { get; private set; }
    }
}
