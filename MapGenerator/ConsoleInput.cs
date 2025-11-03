using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public class ConsoleInput : IMoveInput
    {
        public event Action MoveUp;
        public event Action MoveDown;
        public event Action MoveLeft;
        public event Action MoveRight;
        public event Action Esc;
        public event Action Space;

        public void Update()
        {
            ConsoleKeyInfo keyInfo;
            while (Console.KeyAvailable)
            {
                keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        MoveUp?.Invoke();
                        break;
                    case ConsoleKey.DownArrow:
                        MoveDown?.Invoke();
                        break;
                    case ConsoleKey.RightArrow:
                        MoveRight?.Invoke();
                        break;
                    case ConsoleKey.LeftArrow:
                        MoveLeft?.Invoke();
                        break;
                    case ConsoleKey.Spacebar:
                        Space?.Invoke();
                        break;
                    case ConsoleKey.Escape:
                        Esc?.Invoke();
                        break;
                }
            }
        }
    }
}
