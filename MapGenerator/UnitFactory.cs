using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public class UnitFactory
    {
        private MapGenerator _mapGenerator;
        private IRenderer _renderer;
        private IMoveInput _moveInput;

        public UnitFactory(IRenderer renderer, IMoveInput moveInput, MapGenerator mapGenerator)
        {
            _renderer = renderer;
            _moveInput = moveInput;
            _mapGenerator = mapGenerator; // Устанавливаем MapGenerator
        }
        public void CreateUnit(UnitConfig config)
        {
            switch (config.Type)
            {
                case UnitType.Player:
                    Player player = new Player(config.Position, _renderer, _moveInput,_mapGenerator);
                    LevelModel.AddUnit(player);
                    LevelModel.SetPlayer(player);
                    break;
                //case UnitType.Missile:
                //    LevelModel.AddUnit( new Missile(config.Position, _renderer));
                //    break;
                case UnitType.Enemy:
                    LevelModel.AddUnit(new Enemy(config.Position, _renderer, _moveInput,LevelModel.GetInstance()));
                    break;
            }
        }
    }
}
