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
        private int _targetUpdateCooldown = 0;
        private const int TARGET_UPDATE_INTERVAL = 30; // Обновлять цель каждые 30 кадров

        LevelModel LevelModel { get; set; }
        private MapGenerator _mapGenerator;

        public event Action<Missile> MissileFired;
        private int _shootCooldown = 0;
        private const int SHOOT_COOLDOWN_TIME = 5;

        private int _moveCooldown = 0;
        private const int MOVE_COOLDOWN_TIME = 2; // Задержка между движениями
        public Vector2 ShootDirection { get; private set; }


        public Enemy(Vector2 startPosition, IRenderer renderer, IMoveInput input, LevelModel levelModel, MapGenerator mapGenerator)
            : base(startPosition, "▼", renderer)
        {
            _renderer = renderer;
            this.LevelModel = levelModel;
            _mapGenerator = mapGenerator; // Сохраняем MapGenerator

            _target = GetRandomTargetPosition();

            ShootDirection = new Vector2(0, 1);// на старте направлен углом вниз
        }

        private EnemyState _currentState = EnemyState.Patrolling;

        public override void Update()
        {
            _map = LevelModel.GetInstance().GetMap();

            if (LevelModel.Player != null && LevelModel.Player.IsAlive())
            {
                UpdateState();

                // Обновляем кулдауны движения
                if (_moveCooldown > 0)
                    _moveCooldown--;

                // Двигаемся только если кулдаун прошел
                if (_moveCooldown == 0)
                {
                    ExecuteStateBehavior();
                    _moveCooldown = MOVE_COOLDOWN_TIME; // Устанавливаем задержку
                }

                UpdateCooldowns();
                CheckCollisions();
            }
            else
            {
                CheckCollisions();
            }
        }

        /// Обновление состояния врага
        private void UpdateState()
        {
            bool canSeePlayer = HasLineOfSightToPlayer();
            double distanceToPlayer = GetDistanceToPlayer();

            if (canSeePlayer && distanceToPlayer <= 8) // Видит игрока и достаточно близко
            {
                _currentState = EnemyState.Attacking;
            }
            else if (canSeePlayer && distanceToPlayer <= 15) // Видит игрока, но далеко - преследует
            {
                _currentState = EnemyState.Chasing;
            }
            else
            {
                _currentState = EnemyState.Patrolling;
            }
        }
        
        /// Выполнение поведения в зависимости от состояния
        private void ExecuteStateBehavior()
        {
            switch (_currentState)
            {
                case EnemyState.Patrolling:
                    PatrolBehavior();
                    break;
                case EnemyState.Chasing:
                    ChaseBehavior();
                    break;
                case EnemyState.Attacking:
                    AttackBehavior();
                    break;
            }
        }
        
        /// Поведение при патрулировании
        private void PatrolBehavior()
        {
            // Обновляем случайную цель каждые N кадров
            _targetUpdateCooldown--;
            if (_targetUpdateCooldown <= 0 || Position.Equals(_target) || !CanReachTarget(_target) || IsStuck())
            {
                _target = GetRandomTargetPosition();
                _targetUpdateCooldown = TARGET_UPDATE_INTERVAL;
                visited.Clear();
                Console.WriteLine($"Враг выбрал новую цель: ({_target.X}, {_target.Y})");
            }

            MoveTowardsTarget(_target);
        }
        private bool IsStuck()
        {
            // Проверяем различные признаки "застревания"
            if (visited.Count < 5) return false;

            // Если долго на одном месте
            bool stayingInSmallArea = visited.Distinct().Count() < 4;

            // Если часто возвращаемся в одни и те же позиции
            var recentPositions = visited.Skip(visited.Count - 6).ToList();
            bool repeatingPath = recentPositions.Distinct().Count() < 3;

            if (stayingInSmallArea || repeatingPath)
            {
                Console.WriteLine($"Враг застрял! Уникальных позиций: {visited.Distinct().Count()}");
                return true;
            }

            return false;
        }
        
        /// Поведение при преследовании
        private void ChaseBehavior()
        {
            _target = LevelModel.Player.Position;
            UpdateLookDirectionTowardsPlayer();
            MoveTowardsTarget(_target);
        }

        /// Поведение при атаке
        private void AttackBehavior()
        {
            _target = LevelModel.Player.Position;
            UpdateLookDirectionTowardsPlayer();

            // Стреляем в направлении взгляда, а не рассчитываем заново
            if (_shootCooldown == 0)
            {
                Shoot(ShootDirection); // ← Стреляем в текущем направлении взгляда
                _shootCooldown = SHOOT_COOLDOWN_TIME;
            }
        }

       

        /// Движение к цели
        private void MoveTowardsTarget(Vector2 target)
        {
            var directions = GetPossibleDirections();
            if (directions.Count == 0)
            {
                Console.WriteLine("Враг не может двигаться - нет возможных направлений");

                // Пытаемся найти любой возможный путь
                visited.Clear();
                directions = GetPossibleDirections();

                if (directions.Count == 0)
                {
                    // Если все еще нет путей, выбираем новую цель
                    _target = GetRandomTargetPosition();
                    Console.WriteLine($"Выбрана новая цель из-за тупика: ({_target.X}, {_target.Y})");
                    return;
                }
            }

            Direction bestDirection = GetBestDirectionTowardsTarget( target);
            UpdateLookDirection(bestDirection);

            bool moved = TryMove(bestDirection);

            if (!moved)
            {
                Console.WriteLine($"Враг не смог двигаться в направлении {bestDirection}");

                // Помечаем текущую позицию как проблемную
                visited.Add(new Vector2(Position.X, Position.Y));

                // Если несколько неудачных попыток движения - выбираем новую цель
                if (visited.Count % 5 == 0)
                {
                    _target = GetRandomTargetPosition();
                    visited.Clear();
                }
            }
        }


        public void HandleHit()
        {
            OnDeath();
        }

        //private void RespawnAtRandomPosition()
        //{
        //    Vector2? randomPosition = LevelModel.FindSafeRespawnPosition(this);
        //    if (randomPosition.HasValue)
        //    {
        //        Position = randomPosition.Value;
        //        visited.Clear();
        //        _target = GetRandomTargetPosition(); // Новая цель после респавна
        //        _currentState = EnemyState.Patrolling; // Возвращаемся к патрулированию
        //        Console.WriteLine($"Враг респавн в позиции ({Position.X}, {Position.Y})");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Не найдена свободная позиция для респавна врага");
        //        OnDeath();
        //    }
        //}
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

            return UnitHelper.HasLineOfSight(Position, LevelModel.Player.Position, _map);
        }

        public Missile Shoot(Vector2 direction)
        {
            // Убедимся, что пуля создается на расстоянии от врага
            Vector2 bulletStartPos = new Vector2(
                Position.X + direction.X,
                Position.Y + direction.Y
            );

            // ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА: убедимся, что стартовая позиция не занята врагом
            if (bulletStartPos.Equals(Position))
            {
                Console.WriteLine("ОШИБКА: Пуля создается внутри врага!");
                // Сдвигаем пулю еще на одну клетку
                bulletStartPos = new Vector2(
                    Position.X + direction.X * 2,
                    Position.Y + direction.Y * 2
                );
            }

            // Проверяем, что стартовая позиция валидна
            char[,] map = LevelModel.GetInstance().GetMap();
            if (map != null &&
                (bulletStartPos.X < 0 || bulletStartPos.X >= map.GetLength(0) ||
                 bulletStartPos.Y < 0 || bulletStartPos.Y >= map.GetLength(1) ||
                 IsWallAtPosition(bulletStartPos, map)))
            {
                Console.WriteLine("Нельзя выстрелить - на пути стена или граница карты");
                return null;
            }

            var missile = new Missile(bulletStartPos, direction, _renderer, _mapGenerator);
            missile.Death += () => missile.OnMissileDeath(missile);
            LevelModel.AddUnit(missile);
            MissileFired?.Invoke(missile);

            Console.WriteLine($"Враг выстрелил в направлении ({direction.X}, {direction.Y}) из позиции ({Position.X}, {Position.Y})");

            return missile;
        }

        // Добавляем вспомогательный метод
        private bool IsWallAtPosition(Vector2 position, char[,] map)
        {
            int x = (int)position.X;
            int y = (int)position.Y;
            return x >= 0 && x < map.GetLength(0) &&
                   y >= 0 && y < map.GetLength(1) &&
                   (map[x, y] == '█' || map[x, y] == '▒');
        }

        /// Получение лучшего направления для движения к цели
        private Direction GetBestDirectionTowardsTarget(Vector2 target)
        {
            return MovementHelper.GetBestDirectionTowardsTarget(this, target, _map, random);
        }

         /// Получение случайной целевой позиции на карте
        private Vector2 GetRandomTargetPosition()
        {
            if (_map == null) return new Vector2(1, 1);

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
                // Предпочитаем цели на некотором расстоянии, но не слишком далекие
                Vector2 randomPos = emptyPositions[random.Next(emptyPositions.Count)];
                double distance = GetDistance(Position, randomPos);

                // Если цель слишком близко, попробуем найти другую (но не более 5 попыток)
                int attempts = 0;
                while (distance < 2 && attempts < 5 && emptyPositions.Count > 1)
                {
                    randomPos = emptyPositions[random.Next(emptyPositions.Count)];
                    distance = GetDistance(Position, randomPos);
                    attempts++;
                }

                Console.WriteLine($"Новая цель врага: ({randomPos.X}, {randomPos.Y}), расстояние: {distance}");
                return randomPos;
            }

            return new Vector2(1, 1);
        }

        /// Проверка, можно ли достичь цели
        private bool CanReachTarget(Vector2 target)
        {
            // Простая проверка - цель должна быть в пределах карты и не слишком далеко
            return GetDistance(Position, target) < 20;
        }

        /// Расчет расстояния между двумя точками
        private double GetDistance(Vector2 a, Vector2 b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }


        /// Расчет расстояния до игрока
        private double GetDistanceToPlayer()
        {
            if (LevelModel.Player == null) return double.MaxValue;
            return PositionHelper.GetDistance(Position, LevelModel.Player.Position);
        }


        /// Обновление кулдаунов
        private void UpdateCooldowns()
        {
            if (_shootCooldown > 0)
                _shootCooldown--;

            if (_targetUpdateCooldown > 0)
                _targetUpdateCooldown--;
        }
      
        /// Проверка столкновений
        private void CheckCollisions()
        {
            foreach (Unit unit in LevelModel.Units)
            {
                if (unit == this || !unit.IsAlive()) continue; // Пропускаем себя и мертвые юниты

                if (Position.Equals(unit.Position))
                {
                    if (unit is Missile missile)
                    {
                        HandleHit();
                        missile.OnMissileDeath(missile);
                    }
                    else
                    {
                        OnDeath();
                    }
                }
            }
        }
       
        private bool TryMove(Direction direction)
        {
            bool moved = false;

            switch (direction)
            {
                case Direction.Up:
                    moved = TryMoveUp();
                    if (moved) UpdateLookDirection(Direction.Up);
                    break;
                case Direction.Down:
                    moved = TryMoveDown();
                    if (moved) UpdateLookDirection(Direction.Down);
                    break;
                case Direction.Left:
                    moved = TryMoveLeft();
                    if (moved) UpdateLookDirection(Direction.Left);
                    break;
                case Direction.Right:
                    moved = TryMoveRight();
                    if (moved) UpdateLookDirection(Direction.Right);
                    break;
            }

            if (moved)
            {
                Console.WriteLine($"Враг двигается {direction} в ({Position.X}, {Position.Y})");
            }

            return moved;
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
            return MovementHelper.GetPossibleDirections(this, _map);
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
       
        // Состояние врага
        private enum EnemyState
        {
            Patrolling,    // Патрулирование - движение к случайной цели
            Chasing,       // Преследование - движение к игроку
            Attacking      // Атака - стрельба по игроку
        }
    }
}
