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

        public Vector2 ShootDirection { get; private set; }


        public Enemy(Vector2 startPosition, IRenderer renderer, IMoveInput input, LevelModel levelModel)
            : base(startPosition, "▼", renderer)
        {
            _renderer = renderer;
            this.LevelModel = levelModel;
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
                ExecuteStateBehavior();
                UpdateCooldowns();
                CheckCollisions(); // Эта строка должна быть здесь
            }
            else
            {
                // Если игрок мертв, все равно проверяем столкновения с пулями
                CheckCollisions();
            }
        }
        /// <summary>
        /// Обновление состояния врага
        /// </summary>
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
        /// <summary>
        /// Выполнение поведения в зависимости от состояния
        /// </summary>
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
        /// <summary>
        /// Поведение при патрулировании
        /// </summary>
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
        /// <summary>
        /// Поведение при преследовании
        /// </summary>
        private void ChaseBehavior()
        {
            _target = LevelModel.Player.Position;
            UpdateLookDirectionTowardsPlayer();
            MoveTowardsTarget(_target);
        }
        /// <summary>
        /// Поведение при атаке
        /// </summary>
        private void AttackBehavior()
        {
            _target = LevelModel.Player.Position;
            UpdateLookDirectionTowardsPlayer();

            // Стреляем, если готовы
            if (_shootCooldown == 0)
            {
                ShootAtPlayer();
                _shootCooldown = SHOOT_COOLDOWN_TIME;
            }

            // Двигаемся к игроку, но не слишком близко
            double distanceToPlayer = GetDistanceToPlayer();
            if (distanceToPlayer > 3) // Поддерживаем дистанцию для стрельбы
            {
                MoveTowardsTarget(_target);
            }
            else if (distanceToPlayer < 2) // Слишком близко - отходим
            {
                MoveAwayFromPlayer();
            }
        }
        /// <summary>
        /// Движение к цели
        /// </summary>
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

            Direction bestDirection = GetBestDirectionTowardsTarget(directions, target);
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
        /// <summary>
        /// Отход от игрока
        /// </summary>
        private void MoveAwayFromPlayer()
        {
            if (LevelModel.Player == null) return;

            var directions = GetPossibleDirections();
            if (directions.Count == 0) return;

            Direction bestDirection = GetBestDirectionAwayFromPlayer(directions);
            UpdateLookDirection(bestDirection);
            TryMove(bestDirection);
        }

        public void HandleHit()
        {
            Console.WriteLine($"Враг получил попадание в позиции ({Position.X}, {Position.Y})");
            RespawnAtRandomPosition();
        }

        private void RespawnAtRandomPosition()
        {
            Vector2? randomPosition = LevelModel.FindSafeRespawnPosition(this);
            if (randomPosition.HasValue)
            {
                Position = randomPosition.Value;
                visited.Clear();
                _target = GetRandomTargetPosition(); // Новая цель после респавна
                _currentState = EnemyState.Patrolling; // Возвращаемся к патрулированию
                Console.WriteLine($"Враг респавн в позиции ({Position.X}, {Position.Y})");
            }
            else
            {
                Console.WriteLine("Не найдена свободная позиция для респавна врага");
                OnDeath();
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
        /// <summary>
        /// Получение лучшего направления для движения к цели
        /// </summary>
        /// 

        private Direction GetBestDirectionTowardsTarget(List<Direction> possibleDirections, Vector2 target)
        {
            if (possibleDirections.Count == 0)
                throw new InvalidOperationException("Нет возможных направлений");

            // Иногда добавляем случайность чтобы избежать зацикливания
            if (random.Next(100) < 10) // 10% chance
            {
                Direction randomDir = possibleDirections[random.Next(possibleDirections.Count)];
                Console.WriteLine($"Случайный выбор направления: {randomDir}");
                return randomDir;
            }

            Direction bestDir = possibleDirections[0];
            int bestDistance = int.MaxValue;

            foreach (var dir in possibleDirections)
            {
                var (dx, dy) = GetDirectionVector(dir);
                int newX = Position.X + dx;
                int newY = Position.Y + dy;

                int distance = Math.Abs(newX - target.X) + Math.Abs(newY - target.Y);

                if (IsDirectionTowardsTarget(dir, target))
                {
                    distance -= 2;
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDir = dir;
                }
            }

            Console.WriteLine($"Лучшее направление: {bestDir}, расстояние: {bestDistance}");
            return bestDir;
        }
        private bool IsDirectionTowardsTarget(Direction direction, Vector2 target)
        {
            var (dx, dy) = GetDirectionVector(direction);

            // Проверяем, движемся ли мы в общем направлении к цели
            bool movingTowardsX = (dx > 0 && Position.X < target.X) || (dx < 0 && Position.X > target.X);
            bool movingTowardsY = (dy > 0 && Position.Y < target.Y) || (dy < 0 && Position.Y > target.Y);

            return movingTowardsX || movingTowardsY;
        }

        //private Direction GetBestDirectionTowardsTarget(List<Direction> possibleDirections, Vector2 target)
        //{
        //    Direction bestDir = possibleDirections[0];
        //    int bestDistance = int.MaxValue;

        //    foreach (var dir in possibleDirections)
        //    {
        //        var (dx, dy) = GetDirectionVector(dir);
        //        int newX = Position.X + dx;
        //        int newY = Position.Y + dy;

        //        int distance = Math.Abs(newX - target.X) + Math.Abs(newY - target.Y);

        //        if (distance < bestDistance)
        //        {
        //            bestDistance = distance;
        //            bestDir = dir;
        //        }
        //    }

        //    return bestDir;
        //}



        /// <summary>
        /// Получение лучшего направления для отхода от игрока
        /// </summary>
        private Direction GetBestDirectionAwayFromPlayer(List<Direction> possibleDirections)
        {
            if (LevelModel.Player == null) return possibleDirections[0];

            Direction bestDir = possibleDirections[0];
            int bestDistance = 0;

            foreach (var dir in possibleDirections)
            {
                var (dx, dy) = GetDirectionVector(dir);
                int newX = Position.X + dx;
                int newY = Position.Y + dy;

                int distance = Math.Abs(newX - LevelModel.Player.Position.X) +
                              Math.Abs(newY - LevelModel.Player.Position.Y);

                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    bestDir = dir;
                }
            }

            return bestDir;
        }
        /// <summary>
        /// Получение случайной целевой позиции на карте
        /// </summary>
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
        /// <summary>
        /// Проверка, можно ли достичь цели
        /// </summary>
        private bool CanReachTarget(Vector2 target)
        {
            // Простая проверка - цель должна быть в пределах карты и не слишком далеко
            return GetDistance(Position, target) < 20;
        }
        /// <summary>
        /// Расчет расстояния между двумя точками
        /// </summary>
        private double GetDistance(Vector2 a, Vector2 b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        /// <summary>
        /// Расчет расстояния до игрока
        /// </summary>
        private double GetDistanceToPlayer()
        {
            if (LevelModel.Player == null) return double.MaxValue;
            return GetDistance(Position, LevelModel.Player.Position);
        }

        /// <summary>
        /// Обновление кулдаунов
        /// </summary>
        private void UpdateCooldowns()
        {
            if (_shootCooldown > 0)
                _shootCooldown--;

            if (_targetUpdateCooldown > 0)
                _targetUpdateCooldown--;
        }
        /// <summary>
        /// Проверка столкновений
        /// </summary>
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
        //private Direction GetBestDirection(List<Direction> possibleDirections)
        //{
        //    // Простая эвристика: выбираем направление, которое ближе к игроку
        //    Direction bestDir = possibleDirections[0];
        //    int bestDistance = int.MaxValue;

        //    foreach (var dir in possibleDirections)
        //    {
        //        var (dx, dy) = GetDirectionVector(dir);
        //        int newX = Position.X + dx;
        //        int newY = Position.Y + dy;

        //        int distance = Math.Abs(newX - _target.X) + Math.Abs(newY - _target.Y);

        //        if (distance < bestDistance)
        //        {
        //            bestDistance = distance;
        //            bestDir = dir;
        //        }
        //    }

        //    return bestDir;
        //}
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
            var directions = new List<Direction>();

            // Проверяем все направления
            if (CanMove(Direction.Up)) directions.Add(Direction.Up);
            if (CanMove(Direction.Down)) directions.Add(Direction.Down);
            if (CanMove(Direction.Left)) directions.Add(Direction.Left);
            if (CanMove(Direction.Right)) directions.Add(Direction.Right);

            // Если нет возможных направлений, пробуем очистить историю и попробовать снова
            if (directions.Count == 0 && visited.Count > 0)
            {
                Console.WriteLine("Враг застрял, очищаем историю посещений");
                visited.Clear();

                // Повторно проверяем направления после очистки истории
                if (CanMove(Direction.Up)) directions.Add(Direction.Up);
                if (CanMove(Direction.Down)) directions.Add(Direction.Down);
                if (CanMove(Direction.Left)) directions.Add(Direction.Left);
                if (CanMove(Direction.Right)) directions.Add(Direction.Right);
            }

            Console.WriteLine($"Возможные направления: {directions.Count}");
            return directions;
        }

        private bool CanMove(Direction direction)
        {
            var (dx, dy) = GetDirectionVector(direction);

            int checkX = Position.X + dx;
            int checkY = Position.Y + dy;

            // Проверяем, что позиция валидна и не является стеной
            bool isValid = IsValidPosition(checkX, checkY) &&
                          _map[checkX, checkY] != '█'; // Только целая стена блокирует

            // Проверяем, не посещали ли недавно эту позицию (но не блокируем полностью)
            bool notRecentlyVisited = !visited.Contains(new Vector2(checkX, checkY)) ||
                                     visited.Count > 8; // Разрешаем повторное посещение после 8 ходов

            return isValid && notRecentlyVisited;
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
        // Состояние врага
        private enum EnemyState
        {
            Patrolling,    // Патрулирование - движение к случайной цели
            Chasing,       // Преследование - движение к игроку
            Attacking      // Атака - стрельба по игроку
        }
    }
}
