using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public static class UnitHelper
    {
        /// Подсчитывает количество живых врагов
        public static int CountAliveEnemies(Units units)
        {
            if (units == null) return 0;

            int count = 0;
            foreach (Unit unit in units)
            {
                if (unit is Enemy && unit.IsAlive())
                {
                    count++;
                }
            }
            return count;
        }

        ///// Проверяет столкновение юнита с другими юнитами
        //public static Unit GetCollidingUnit(Unit checkingUnit, Units allUnits)
        //{
        //    if (allUnits == null) return null;

        //    foreach (Unit otherUnit in allUnits)
        //    {
        //        if (otherUnit == checkingUnit || !otherUnit.IsAlive()) continue;
        //        if (checkingUnit.Position.Equals(otherUnit.Position))
        //        {
        //            return otherUnit;
        //        }
        //    }
        //    return null;
        //}

        ///// Получает ближайшего врага к указанной позиции
        //public static Enemy GetNearestEnemy(Vector2 position, Units units)
        //{
        //    if (units == null) return null;

        //    Enemy nearest = null;
        //    double minDistance = double.MaxValue;

        //    foreach (Unit unit in units)
        //    {
        //        if (unit is Enemy enemy && enemy.IsAlive())
        //        {
        //            double distance = PositionHelper.GetDistance(position, enemy.Position);
        //            if (distance < minDistance)
        //            {
        //                minDistance = distance;
        //                nearest = enemy;
        //            }
        //        }
        //    }

        //    return nearest;
        //}

        /// Проверяет, есть ли прямая видимость между двумя позициями
        public static bool HasLineOfSight(Vector2 from, Vector2 to, char[,] map)
        {
            if (from.Y == to.Y) return HasHorizontalLineOfSight(from, to, map);
            if (from.X == to.X) return HasVerticalLineOfSight(from, to, map);
            return false;
        }

        private static bool HasHorizontalLineOfSight(Vector2 from, Vector2 to, char[,] map)
        {
            int startX = Math.Min(from.X, to.X);
            int endX = Math.Max(from.X, to.X);
            int y = from.Y;

            for (int x = startX + 1; x < endX; x++)
            {
                if (PositionHelper.IsWall(new Vector2(x, y), map))
                    return false;
            }
            return true;
        }

        private static bool HasVerticalLineOfSight(Vector2 from, Vector2 to, char[,] map)
        {
            int startY = Math.Min(from.Y, to.Y);
            int endY = Math.Max(from.Y, to.Y);
            int x = from.X;

            for (int y = startY + 1; y < endY; y++)
            {
                if (PositionHelper.IsWall(new Vector2(x, y), map))
                    return false;
            }
            return true;
        }
    }
}
