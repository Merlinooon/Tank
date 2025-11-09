using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public interface IRenderer
    {
        public void SetCell(int x, int y, string val);
        public void Renderer(char[,] map, Units units, int currentLevel = 1, int enemiesRemaining = 0, int totalEnemies = 0);
        public void Clear();
    }
}
