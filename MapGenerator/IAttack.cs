using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public interface IAttack
    {
        public Missile Shoot(Vector2 direction);
       
    }
}
