using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapGenerator.MapGenerator;

namespace MapGenerator
{
    public class Missile : Unit
    {
         private Vector2 _direction;
        public event Action<Vector2> WallHit;
        public event Action Death;
        Vector2 _startPosition;
        IRenderer _renderer;
        private int _speed = 1;
        private MapGenerator _mapGenerator;

        public Missile(Vector2 startPosition, Vector2 direction, IRenderer renderer, MapGenerator mapGenerator)
            : base(startPosition, "·", renderer)
        {
            _direction = direction;
            _startPosition = startPosition;
            _renderer = renderer;
            _mapGenerator = mapGenerator;
        }

        public override void Update()
        {
            if (_mapGenerator == null || _mapGenerator.Map == null)
            {
                Death?.Invoke();
                return;
            }

            Vector2 newPosition = new Vector2(
                Position.X + _direction.X * _speed,
                Position.Y + _direction.Y * _speed
            );

            // Проверяем столкновение со стенами
            if (CheckWallCollision(newPosition))
            {
                HandleWallCollision(newPosition);
                Death?.Invoke();
                return;
            }

            Position = newPosition;

            // Проверяем столкновения с юнитами
            if (LevelModel.Units != null)
            {
                foreach (Unit unit in LevelModel.Units)
                {
                    if (unit == this || unit is Missile)
                        continue;

                    if (unit.Position.X == Position.X && unit.Position.Y == Position.Y)
                    {
                        Death?.Invoke();
                        break;
                    }
                }
            }

            // Проверяем выход за границы
            if (Position.X < 0 || Position.X >= _mapGenerator.Map.GetLength(1) ||
                Position.Y < 0 || Position.Y >= _mapGenerator.Map.GetLength(0))
            {
                Death?.Invoke();
            }
        }

        private bool CheckWallCollision(Vector2 position)
        {
            if (_mapGenerator == null || _mapGenerator.Map == null)
                return true;

            int x = (int)position.X;
            int y = (int)position.Y;

            if (x < 0 || x >= _mapGenerator.Map.GetLength(1) ||
                y < 0 || y >= _mapGenerator.Map.GetLength(0))
            {
                return true;
            }

            char cell = _mapGenerator.Map[y, x];
            return cell == '█' || cell == '▒';
        }

        private void HandleWallCollision(Vector2 collisionPosition)
        {
            if (_mapGenerator == null) return;

            _mapGenerator.DamageWallAt(collisionPosition);
            WallHit?.Invoke(collisionPosition);
        }
    }
        //private Vector2 _direction;
        //public event Action Death;
        //Vector2 _startPosition;
        //IRenderer _renderer;
        //private int _speed = 1;

        //public Missile(Vector2 startPosition, Vector2 direction, IRenderer renderer) : base(startPosition, "·", renderer)
        //{
        //    _direction = direction;
        //    _startPosition = startPosition;
        //    _renderer = renderer;
        //}

        //public override void Update()
        //{
        //    // Двигаем пулю вперед
        //    //if()
        //    Position = new Vector2(Position.X + _direction.X, Position.Y +_direction.Y);


        //    // Проверяем столкновения
        //    foreach (Unit unit in LevelModel.Units)
        //    {
        //        if (unit == this || unit is Missile)
        //            continue;

        //        if (Position.Equals(unit.Position))
        //        {
        //            Death?.Invoke();
        //            break;
        //        }
        //    }

        //    // Уничтожаем пулю если она вышла за пределы экрана
        //    if (Position.X > 50) // Замените на реальные границы уровня
        //    {
        //        Death?.Invoke();
        //    }
        //}

       
}

