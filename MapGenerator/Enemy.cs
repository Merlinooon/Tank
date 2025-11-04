using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapGenerator.Enemy;
using static MapGenerator.MapGenerator;

namespace MapGenerator
{
    public class Enemy:Unit,IAttack
    {
        public event Action Death;
        char[,] _map;
        private Random random = new Random(); // Инициализирован Random

        private IRenderer _renderer;

        public List<Vector2> visited = new List<Vector2>();
        private Vector2 _target;
        LevelModel LevelModel { get; set; }
        private MapGenerator _mapGenerator;

        public event Action<Missile> MissileFired;
        private int _shootCooldown = 0;
        private const int SHOOT_COOLDOWN_TIME = 5;

        public Vector2 ShootDirection { get; private set; }

        public Enemy(Vector2 startPosition, IRenderer renderer, IMoveInput input, LevelModel levelModel)
            : base(startPosition, "▼", renderer)
        {
            _renderer = renderer;
            this.LevelModel = levelModel;
            _target = LevelModel.Player?.Position ?? new Vector2(1, 1);
            ShootDirection = new Vector2(0, 1); // Начальное направление - вниз
        }

        public override void Update()
        {
            _map = LevelModel.GetInstance().GetMap();

            if (LevelModel.Player != null && LevelModel.Player.IsAlive())
            {
                _target = LevelModel.Player.Position;

                if (_shootCooldown > 0)
                    _shootCooldown--;

                // Обновляем направление взгляда в сторону игрока
                UpdateLookDirectionTowardsPlayer();

                bool hasSight = HasLineOfSightToPlayer();

                if (hasSight)
                {
                   
                    if (_shootCooldown == 0)
                    {
                      
                        ShootAtPlayer();
                        _shootCooldown = SHOOT_COOLDOWN_TIME;
                    }
                }

                var directions = GetPossibleDirections();
                if (directions.Count > 0)
                {
                    Direction bestDirection = GetBestDirection(directions);
                    UpdateLookDirection(bestDirection); // Обновляем направление перед движением
                    TryMove(bestDirection);
                }

                foreach (Unit unit in LevelModel.Units)
                {
                    if (unit == this) continue;
                    if (Position.Equals(unit.Position))
                    {
                        Death?.Invoke();
                    }
                }
            }
        }

        private  void UpdateView()
        {
            View = ShootDirection switch
            {
                { X: 1, Y: 0 } => "►",   // Вправо
                { X: -1, Y: 0 } => "◄",  // Влево
                { X: 0, Y: -1 } => "▲",  // Вверх
                { X: 0, Y: 1 } => "▼",   // Вниз
                _ => "▼"                  // По умолчанию - вниз
            };
        }
        private void UpdateLookDirection(Direction direction)
        {
            ShootDirection = direction switch
            {
                Direction.Up => new Vector2(0, -1),
                Direction.Down => new Vector2(0, 1),
                Direction.Left => new Vector2(-1, 0),
                Direction.Right => new Vector2(1, 0),
                _ => ShootDirection
            };
            UpdateView();
        }
        private void UpdateLookDirectionTowardsPlayer()
        {
            if (LevelModel.Player == null) return;

            Vector2 playerPos = LevelModel.Player.Position;
            Vector2 directionToPlayer = new Vector2(
                playerPos.X - Position.X,
                playerPos.Y - Position.Y
            );

            // Определяем основное направление (горизонтальное или вертикальное)
            if (Math.Abs(directionToPlayer.X) > Math.Abs(directionToPlayer.Y))
            {
                // Горизонтальное направление преобладает
                ShootDirection = new Vector2(directionToPlayer.X > 0 ? 1 : -1, 0);
            }
            else
            {
                // Вертикальное направление преобладает
                ShootDirection = new Vector2(0, directionToPlayer.Y > 0 ? 1 : -1);
            }

            UpdateView();
        }

        private bool HasLineOfSightToPlayer()
        {
            if (LevelModel.Player == null || !LevelModel.Player.IsAlive() || _map == null)
                return false;

            Vector2 playerPos = LevelModel.Player.Position;

            if (Position.Y == playerPos.Y && HasHorizontalLineOfSight(playerPos))
                return true;

            if (Position.X == playerPos.X && HasVerticalLineOfSight(playerPos))
                return true;

            return false;
        }

        private bool HasHorizontalLineOfSight(Vector2 playerPos)
        {
            int startX = Math.Min(Position.X, playerPos.X);
            int endX = Math.Max(Position.X, playerPos.X);
            int y = Position.Y;

            for (int x = startX + 1; x < endX; x++)
            {
                if (x < 0 || x >= _map.GetLength(0) || y < 0 || y >= _map.GetLength(1))
                    return false;

                if (_map[x, y] == '█' || _map[x, y] == '▒')
                    return false;
            }

            return true;
        }

        private bool HasVerticalLineOfSight(Vector2 playerPos)
        {
            int startY = Math.Min(Position.Y, playerPos.Y);
            int endY = Math.Max(Position.Y, playerPos.Y);
            int x = Position.X;

            for (int y = startY + 1; y < endY; y++)
            {
                if (x < 0 || x >= _map.GetLength(0) || y < 0 || y >= _map.GetLength(1))
                    return false;

                if (_map[x, y] == '█' || _map[x, y] == '▒')
                    return false;
            }

            return true;
        }
        

        private void ShootAtPlayer()
        {
            if (LevelModel.Player == null) return;

            Vector2 direction = GetShootDirection();
            Shoot(direction);
        }

        private Vector2 GetShootDirection()
        {
            Vector2 playerPos = LevelModel.Player.Position;
            Vector2 direction = new Vector2(0, 0);

            if (Position.X == playerPos.X)
            {
                direction = new Vector2(0, Position.Y < playerPos.Y ? 1 : -1);
            }
            else if (Position.Y == playerPos.Y)
            {
                direction = new Vector2(Position.X < playerPos.X ? 1 : -1, 0);
            }

            return direction;
        }

        public Missile Shoot(Vector2 direction)
        {
            Vector2 bulletStartPos = new Vector2(
                Position.X + direction.X * 2,
                Position.Y + direction.Y * 2
            );

            // Проверяем валидность стартовой позиции
            char[,] map = LevelModel.GetInstance().GetMap();
            if (map != null)
            {
                int startX = (int)bulletStartPos.X;
                int startY = (int)bulletStartPos.Y;

                if (startX >= 0 && startX < map.GetLength(0) &&
                    startY >= 0 && startY < map.GetLength(1) &&
                    map[startX, startY] != ' ')
                {
                    bulletStartPos = new Vector2(
                        Position.X + direction.X,
                        Position.Y + direction.Y
                    );
                }
            }

            var missile = new Missile(bulletStartPos, direction, _renderer, _mapGenerator);

            missile.Death += () => missile.OnMissileDeath(missile);
           

            LevelModel.AddUnit(missile);
            MissileFired?.Invoke(missile);

            
            return missile;
        }
        private Direction GetBestDirection(List<Direction> possibleDirections)
        {
            // Простая эвристика: выбираем направление, которое ближе к игроку
            Direction bestDir = possibleDirections[0];
            int bestDistance = int.MaxValue;

            foreach (var dir in possibleDirections)
            {
                var (dx, dy) = GetDirectionVector(dir);
                int newX = Position.X + dx;
                int newY = Position.Y + dy;

                int distance = Math.Abs(newX - _target.X) + Math.Abs(newY - _target.Y);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDir = dir;
                }
            }

            return bestDir;
        }
        private void TryMove(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    if (TryMoveUp()) UpdateLookDirection(Direction.Up);
                    break;
                case Direction.Down:
                    if (TryMoveDown()) UpdateLookDirection(Direction.Down);
                    break;
                case Direction.Left:
                    if (TryMoveLeft()) UpdateLookDirection(Direction.Left);
                    break;
                case Direction.Right:
                    if (TryMoveRight()) UpdateLookDirection(Direction.Right);
                    break;
            }
        }
        public override bool TryMoveLeft()
        {
            if (base.TryMoveLeft())
            {
                visited.Add(new Vector2(Position.X, Position.Y));
                return true;
            }
            return false;
        }

        public override bool TryMoveRight()
        {
            if (base.TryMoveRight())
            {
                visited.Add(new Vector2(Position.X, Position.Y));
                return true;
            }
            return false;
        }

        public override bool TryMoveUp()
        {
            if (base.TryMoveUp())
            {
                visited.Add(new Vector2(Position.X, Position.Y));
                return true;
            }
            return false;
        }

        public override bool TryMoveDown()
        {
            if (base.TryMoveDown())
            {
                visited.Add(new Vector2(Position.X, Position.Y));
                return true;
            }
            return false;
        }
        private List<Direction> GetPossibleDirections()
        {
            var directions = new List<Direction>();

            if (CanMove( Direction.Up)) directions.Add(Direction.Up);
            if (CanMove( Direction.Down)) directions.Add(Direction.Down);
            if (CanMove( Direction.Left)) directions.Add(Direction.Left);
            if (CanMove( Direction.Right)) directions.Add(Direction.Right);

            return directions;
        }

        private bool CanMove( Direction direction)
        {
            var (dx, dy) = GetDirectionVector(direction);

            
            int checkX = Position.X + dx ;
            int checkY = Position.Y + dy ;

            return IsValidPosition(checkX, checkY) &&
                   !visited.Contains(new Vector2(checkX, checkY)) &&
                   _map[checkX, checkY] == ' '; // Исправлены координаты
        }

        private (int dx, int dy) GetDirectionVector(Direction direction)
        {
            return direction switch
            {
                Direction.Up => (0, -1),
                Direction.Down => (0, 1),
                Direction.Left => (-1, 0),
                Direction.Right => (1, 0),
                _ => (0, 0)
            };
        }

        private bool IsValidPosition(int x, int y)
        {

            int rows = _map.GetLength(0);
            int cols = _map.GetLength(1);
            return x >= 0 && x < rows && y >= 0 && y < cols;
        }
        private void ShuffleDirections(List<Direction> directions)
        {
            for (int i = directions.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (directions[i], directions[j]) = (directions[j], directions[i]);
            }
        }
       
      
       
      
    }
}
