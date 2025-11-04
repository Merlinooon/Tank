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
        private int _framesAlive = 0;

        private int _speed = 1;
        private MapGenerator _mapGenerator;

        public Missile(Vector2 startPosition, Vector2 direction, IRenderer renderer, MapGenerator mapGenerator)
            : base(startPosition, "·", renderer)
        {
            _direction = direction;
            _startPosition = startPosition;
            _renderer = renderer;
            _mapGenerator = mapGenerator;
            _framesAlive = 0;
        }

        public override void Update()
        {
            char[,] map = LevelModel.GetInstance().GetMap();
            if (map == null)
            {
                Death?.Invoke();
                return;
            }

            Vector2 newPosition = new Vector2(
                Position.X + _direction.X * _speed,
                Position.Y + _direction.Y * _speed
            );

            if (CheckWallCollision(newPosition, map))
            {
                HandleWallCollision(newPosition);
                Death?.Invoke();
                return;
            }

            Position = newPosition;

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

            if (Position.X < 0 || Position.X >= map.GetLength(0) ||
                Position.Y < 0 || Position.Y >= map.GetLength(1))
            {
                Death?.Invoke();
            }
        }
        public void OnMissileDeath(Missile missile)
        {
            LevelModel.RemoveUnit(missile);
          
        }
        private bool CheckWallCollision(Vector2 position, char[,] map)
        {
            if (map == null) return true;

            int x = (int)position.X;
            int y = (int)position.Y;

            if (x < 0 || x >= map.GetLength(0) || y < 0 || y >= map.GetLength(1))
                return true;

            char cell = map[x, y];
            return cell == '█' || cell == '▒';
        }

        private void HandleWallCollision(Vector2 collisionPosition)
        {
            int x = (int)collisionPosition.X;
            int y = (int)collisionPosition.Y;

            Console.WriteLine($"Пуля столкнулась со стеной в ({x}, {y})");

            // Используем переданный MapGenerator
            _mapGenerator?.DamageWallAt(collisionPosition);

            WallHit?.Invoke(collisionPosition);
        }
    }



}

