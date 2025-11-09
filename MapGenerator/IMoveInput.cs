using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public interface IMoveInput
    {
        public event Action MoveUp;
        public event Action MoveDown;
        public event Action MoveLeft;
        public event Action MoveRight;
        public event Action Esc;
        public event Action Space;

        public void Update();
      
    }
}
