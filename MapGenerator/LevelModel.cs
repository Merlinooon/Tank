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
        private static char[,] _map;

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

        public static void SetMap(char[,] map)
        {
            _map = map; // Теперь используем тот же массив что и в MapGenerator
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
