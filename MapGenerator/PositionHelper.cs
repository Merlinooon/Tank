using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapGenerator.MapGenerator;

namespace MapGenerator
{
    public static class PositionHelper
    {
        /// Проверяет, находится ли позиция в пределах карты
        public static bool IsWithinMapBounds(Vector2 position, char[,] map)
        {
            return position.X >= 0 && position.X < map.GetLength(0) &&
                   position.Y >= 0 && position.Y < map.GetLength(1);
        }

        /// Проверяет, является ли позиция стеной
        public static bool IsWall(Vector2 position, char[,] map)
        {
            if (!IsWithinMapBounds(position, map)) return false;
            char cell = map[position.X, position.Y];
            return cell == '█' || cell == '▒';
        }

        /// Проверяет, является ли позиция водой
        public static bool IsWater(Vector2 position, char[,] map)
        {
            if (!IsWithinMapBounds(position, map)) return false;
            return map[position.X, position.Y] == '▓';
        }

        /// Проверяет, является ли позиция проходимой
        public static bool IsWalkable(Vector2 position, char[,] map)
        {
            if (!IsWithinMapBounds(position, map)) return false;
            char cell = map[position.X, position.Y];
            return cell == ' '; // Только пустое пространство проходимо
        }

        /// Проверяет, безопасна ли позиция для появления юнита
        public static bool IsSafeForSpawn(Vector2 position, char[,] map, Units units = null, Unit excludingUnit = null)
        {
            if (!IsWithinMapBounds(position, map)) return false;
            if (IsWall(position, map)) return false;
            if (IsWater(position, map)) return false;

            // Проверяем, не занята ли позиция другими юнитами
            if (units != null)
            {
                foreach (Unit unit in units)
                {
                    if (unit == excludingUnit || !unit.IsAlive()) continue;
                    if (unit.Position.Equals(position)) return false;
                }
            }

            return true;
        }

        /// Вычисляет расстояние между двумя точками
        public static double GetDistance(Vector2 a, Vector2 b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// Вычисляет манхэттенское расстояние
        public static int GetManhattanDistance(Vector2 a, Vector2 b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        /// Вычисляет новую позицию после перемещения в направлении
        public static Vector2 CalculateNewPosition(Vector2 currentPosition, Direction direction)
        {
            var directionVector = DirectionToVector(direction);
            return new Vector2(
                currentPosition.X + directionVector.X,
                currentPosition.Y + directionVector.Y
            );
        }

        /// Преобразует направление в вектор перемещения
        public static Vector2 DirectionToVector(Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Vector2(0, -1),
                Direction.Down => new Vector2(0, 1),
                Direction.Left => new Vector2(-1, 0),
                Direction.Right => new Vector2(1, 0),
                _ => new Vector2(0, 0)
            };
        }
    }
}
