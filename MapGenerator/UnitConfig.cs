using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public class UnitConfig
    {

        public Vector2 Position;
        public string View;
        public UnitType Type;
        public UnitConfig(Vector2 pos, string view, UnitType unitType)
        {
            Position = pos;

            View = view;
            Type = unitType;
        }
    }
}
