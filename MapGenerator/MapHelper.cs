using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public static class MapHelper
    {
        /// Находит все свободные позиции на карте
        public static List<Vector2> FindAllEmptyPositions(char[,] map)
        {
            var emptyPositions = new List<Vector2>();
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    if (map[x, y] == ' ')
                    {
                        emptyPositions.Add(new Vector2(x, y));
                    }
                }
            }

            return emptyPositions;
        }

        /// Находит случайную свободную позицию
        public static Vector2? FindRandomEmptyPosition(char[,] map, Random random = null)
        {
            random ??= new Random();
            var emptyPositions = FindAllEmptyPositions(map);

            if (emptyPositions.Count == 0) return null;
            return emptyPositions[random.Next(emptyPositions.Count)];
        }

        /// Находит позицию на заданном расстоянии от указанной точки
        public static Vector2? FindPositionAwayFrom(Vector2 fromPosition, char[,] map, int minDistance, Random random = null)
        {
            random ??= new Random();
            var emptyPositions = FindAllEmptyPositions(map);
            var validPositions = emptyPositions.Where(pos =>
                PositionHelper.GetDistance(pos, fromPosition) >= minDistance).ToList();

            if (validPositions.Count == 0) return null;
            return validPositions[random.Next(validPositions.Count)];
        }
    }
}
