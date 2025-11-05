using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public class LevelModel
    {
        private static LevelModel _instance;
        private static char[,] _map; // Наша собственная копия карты

        public static Player _player;
        private static Units _units;

        public static Player Player => _player;
        public static Units Units => _units;

        private LevelModel() { }

        public static LevelModel GetInstance()
        {
            if (_instance == null)
            {
                _instance = new LevelModel();
            }
            return _instance;
        }

        /// <summary>
        /// Устанавливает карту (вызывается из MapGenerator)
        /// </summary>
        public static void SetMap(char[,] map)
        {
            if (map == null) return;

            // Создаем полную копию карты
            _map = new char[map.GetLength(0), map.GetLength(1)];
            Array.Copy(map, _map, map.Length);

            Console.WriteLine($"LevelModel: карта установлена {_map.GetLength(0)}x{_map.GetLength(1)}");
        }

        /// <summary>
        /// Обновляет конкретную клетку карты (для повреждения стен)
        /// </summary>
        public static void UpdateCell(int x, int y, char value)
        {
            if (_map != null && x >= 0 && x < _map.GetLength(0) && y >= 0 && y < _map.GetLength(1))
            {
                _map[x, y] = value;
            }
        }

        public char[,] GetMap()
        {
            return _map;
        }

        public static void RemoveUnit(Unit unit)
        {
            if (_units != null)
            {
                int countBefore = _units.Count();
                _units.Remove(unit);
                int countAfter = _units.Count();

                Console.WriteLine($"Удален юнит {unit.GetType().Name}. Было: {countBefore}, стало: {countAfter}");
            }
        }
        public static void SetPlayer(Player player)
        {
            _player?.Dispose();
            _player = player;
        }

        public static void SetUnits(Units units)
        {
            _units = units;
        }

        public static void AddUnit(Unit unit)
        {
            _units?.Add(unit);
        }
        /// <summary>
        /// Находит случайную свободную позицию на карте
        /// </summary>
        public static Vector2? FindRandomEmptyPosition()
        {
            if (_map == null) return null;

            Random random = new Random();
            List<Vector2> emptyPositions = new List<Vector2>();
            int width = _map.GetLength(0);
            int height = _map.GetLength(1);

            // Собираем все свободные позиции
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (_map[x, y] == ' ')
                    {
                        emptyPositions.Add(new Vector2(x, y));
                    }
                }
            }

            if (emptyPositions.Count > 0)
            {
                return emptyPositions[random.Next(emptyPositions.Count)];
            }

            return null;
        }

        /// <summary>
        /// Находит безопасную случайную позицию для респавна
        /// </summary>
        public static Vector2? FindSafeRespawnPosition(Unit excludingUnit = null)
        {
            if (_map == null) return null;

            Random random = new Random();
            List<Vector2> safePositions = new List<Vector2>();
            int width = _map.GetLength(0);
            int height = _map.GetLength(1);

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (_map[x, y] == ' ' && IsPositionSafeForRespawn(x, y, excludingUnit))
                    {
                        safePositions.Add(new Vector2(x, y));
                    }
                }
            }

            if (safePositions.Count > 0)
            {
                return safePositions[random.Next(safePositions.Count)];
            }

            // Если безопасных позиций нет, возвращаем любую свободную
            return FindRandomEmptyPosition();
        }

        /// <summary>
        /// Проверяет, безопасна ли позиция для респавна
        /// </summary>
        private static bool IsPositionSafeForRespawn(int x, int y, Unit excludingUnit)
        {
            // Проверяем расстояние до игрока
            if (_player != null && _player.IsAlive())
            {
                double distanceToPlayer = Math.Sqrt(
                    Math.Pow(x - _player.Position.X, 2) +
                    Math.Pow(y - _player.Position.Y, 2)
                );

                if (distanceToPlayer < 3) // Минимальное расстояние до игрока
                    return false;
            }

            // Проверяем расстояние до врагов и что позиция не занята
            if (_units != null)
            {
                foreach (Unit unit in _units)
                {
                    if (unit == excludingUnit || !unit.IsAlive())
                        continue;

                    // Проверяем занятость позиции
                    if (unit.Position.X == x && unit.Position.Y == y)
                        return false;

                    // Для врагов проверяем минимальное расстояние
                    if (unit is Enemy)
                    {
                        double distanceToEnemy = Math.Sqrt(
                            Math.Pow(x - unit.Position.X, 2) +
                            Math.Pow(y - unit.Position.Y, 2)
                        );

                        if (distanceToEnemy < 2) // Минимальное расстояние до врага
                            return false;
                    }
                }
            }

            return true;
        }
    }
}
