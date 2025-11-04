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
            _units?.Remove(unit);
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
    }
}
