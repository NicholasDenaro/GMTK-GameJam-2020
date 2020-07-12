using GameEngine._2D;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class BulletNull : Description2D, IIdentifiable
    {
        public Guid Id => Guid.Empty;
    }
}
