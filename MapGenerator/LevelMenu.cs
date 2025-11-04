using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public class LevelsMenu
    {
        //private Dictionary<string, char[,]> LevelMaps;
        private MapGenerator _gameData;
        private ConsoleInput _input;
        private IRenderer _renderer;
        private UnitFactory _unitFactory;
        public LevelsMenu(MapGenerator gameData, ConsoleInput input, IRenderer renderer, UnitFactory unitFactory)
        {
            _gameData = gameData;
            _input = input;
            _renderer = renderer;

            //_input.Esc += SetMenu;
            _unitFactory = unitFactory;
        }

       
        public void SetMapPixels(char[,] map)
        {
            // x - горизонталь(строка)
            // y - вертикаль(столбец)
            for (int y = 0; y < map.GetLength(0); y++)
            {
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    _renderer.SetCell(x, y, map[y, x].ToString());
                }
            }
        }
        public void SetUnits(List<UnitConfig> units)
        {
            foreach (var unitConfig in units)
            {
                _unitFactory.CreateUnit(unitConfig);
            }
        }
    }
}
