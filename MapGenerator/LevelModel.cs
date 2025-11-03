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
        //private static LevelModel _instance;
        //private List<Units> _unitList = new List<Units>();

        //private static char[,] _map;

        //public static Player _player;
        //private static Units _units;


        //public static Player Player => _player;
        //public static Units Units => _units;

        //private LevelModel()
        //{

        //}

        //public static LevelModel GetInstance()
        //{
        //    if (_instance == null)
        //    {
        //        _instance = new LevelModel();
        //    }
        //    return _instance;
        //}
        //public static void SetMap(char[,] map)
        //{
        //    _map = map;
        //}

        //public char[,] GetMap()
        //{
        //    if (_map == null)
        //    {
        //        Console.WriteLine("Карта не установлена");
        //        return null;
        //    }

        //    return _map;
        //}
        //public static void RemoveUnit(Unit unit)
        //{
        //    if (_units != null)
        //    {
        //        // Нужно добавить метод Remove в класс Units
        //        _units.Remove(unit);
        //    }
        //}
        ////public char[,] GetMap
        ////{
        ////    get => _map;
        ////}
        ////public char[,] TransposeMap()
        ////{
        ////    //char[,] map = GameData.GetInstance().GetMap();
        ////    int rows = _map.GetLength(0);
        ////    int cols = _map.GetLength(1);
        ////    char[,] transposedMap = new char[cols, rows]; // Размеры меняются местами

        ////    for (int y = 0; y < rows; y++)
        ////    {
        ////        for (int x = 0; x < cols; x++)
        ////        {
        ////            transposedMap[x, y] = _map[y, x]; // Меняем координаты местами
        ////        }
        ////    }

        ////    return transposedMap;
        ////}
        //public List<Units> GetAllUnits(Units units)
        //{

        //    foreach (Units unit in units)
        //    {
        //        _unitList.Add(unit);
        //    }
        //    return _unitList;
        //}
        //public static void SetPlayer(Player player)
        //{
        //    _player?.Dispose();
        //    _player = player;
        //}
        //public static void SetUnits(Units units)
        //{
        //    _units = units;
        //}
        //public static void AddUnit(Unit unit)
        //{
        //    if (_units != null)
        //    {
        //        _units.Add(unit);
        //    }
        //}

    }
}
