using GameEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public static class EntityExtensions
    {
        public static Entity AddTickAction(this Entity entity, Action<Location,Entity> action)
        {
            entity.TickAction = action;

            return entity;
        }
    }
}
