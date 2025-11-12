using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapGenerator.MapGenerator;

namespace MapGenerator
{
    public static class MovementHelper
    {
        /// Проверяет, может ли юнит переместиться в направлении
        public static bool CanMove(Unit unit, Direction direction, char[,] map)
        {
            Vector2 newPosition = PositionHelper.CalculateNewPosition(unit.Position, direction);
            return PositionHelper.IsWalkable(newPosition, map);
        }

        /// Получает все возможные направления для движения
        public static List<Direction> GetPossibleDirections(Unit unit, char[,] map)
        {
            var directions = new List<Direction>();
            var allDirections = new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

            foreach (var direction in allDirections)
            {
                if (CanMove(unit, direction, map))
                {
                    directions.Add(direction);
                }
            }

            return directions;
        }

        /// Получает лучшее направление для движения к цели
        public static Direction GetBestDirectionTowardsTarget(Unit unit, Vector2 target, char[,] map, Random random = null)
        {
            random ??= new Random();
            var possibleDirections = GetPossibleDirections(unit, map);

            if (possibleDirections.Count == 0)
                throw new InvalidOperationException("Нет возможных направлений для движения");

            // Иногда добавляем случайность чтобы избежать зацикливания
            if (random.Next(100) < 10)
            {
                return possibleDirections[random.Next(possibleDirections.Count)];
            }

            Direction bestDir = possibleDirections[0];
            int bestDistance = int.MaxValue;

            foreach (var dir in possibleDirections)
            {
                Vector2 newPosition = PositionHelper.CalculateNewPosition(unit.Position, dir);
                int distance = PositionHelper.GetManhattanDistance(newPosition, target);

                if (IsDirectionTowardsTarget(dir, unit.Position, target))
                {
                    distance -= 2; // Бонус за движение в правильном направлении
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDir = dir;
                }
            }

            return bestDir;
        }

        /// Проверяет, является ли направление движением к цели
        private static bool IsDirectionTowardsTarget(Direction direction, Vector2 currentPosition, Vector2 target)
        {
            Vector2 directionVector = PositionHelper.DirectionToVector(direction);

            bool movingTowardsX = (directionVector.X > 0 && currentPosition.X < target.X) ||
                                 (directionVector.X < 0 && currentPosition.X > target.X);
            bool movingTowardsY = (directionVector.Y > 0 && currentPosition.Y < target.Y) ||
                                 (directionVector.Y < 0 && currentPosition.Y > target.Y);

            return movingTowardsX || movingTowardsY;
        }
    }
}
