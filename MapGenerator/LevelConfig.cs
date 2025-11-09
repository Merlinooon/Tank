using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public class LevelConfig
    {
        public int Width { get; }
        public int Height { get; }
        public int BaseEnemyCount { get; }
        public int MaxEnemies { get; }

        public int TargetEnemyCount { get; } // Количество врагов для победы
        public string Description { get; }

        public LevelConfig(int width, int height, int baseEnemyCount, int maxEnemies, int targetEnemyCount, string description)
        {
            Width = width;
            Height = height;
            BaseEnemyCount = baseEnemyCount;
            MaxEnemies = maxEnemies;
            TargetEnemyCount = targetEnemyCount;
            Description = description;
        }
    }

    // Сложность
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard,
        Expert
    }

    // Статистика уровня
    public class LevelStats
    {
        public int LevelNumber { get; set; }
        public string LevelName { get; set; }
        public Difficulty Difficulty { get; set; }
        public string MapSize { get; set; }
        public int EnemyCount { get; set; }
       
    }
}
