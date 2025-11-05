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
        private MapGenerator _mapGenerator;
        private bool _isDead = false;

        public Missile(Vector2 startPosition, Vector2 direction, IRenderer renderer, MapGenerator mapGenerator)
            : base(startPosition, "·", renderer)
        {
            _direction = direction;
            _mapGenerator = mapGenerator;

            Console.WriteLine($"Создана пуля в позиции ({startPosition.X}, {startPosition.Y})");

            // Сразу проверяем столкновение при создании
            CheckImmediateCollision();
        }

        public override bool IsAlive()
        {
            return !_isDead;
        }

        private void CheckImmediateCollision()
        {
            if (_isDead) return;

            char[,] map = LevelModel.GetInstance().GetMap();
            if (map == null)
            {
                DestroyMissile();
                return;
            }

            if (IsWallAtPosition(Position, map))
            {
                Console.WriteLine($"Пуля создана ВНУТРИ СТЕНЫ! Позиция ({Position.X}, {Position.Y})");
                HandleWallCollision(Position);
                DestroyMissile(); // НЕМЕДЛЕННО уничтожаем и удаляем
                return;
            }
        }

        public override void Update()
        {
            if (_isDead) return;

            char[,] map = LevelModel.GetInstance().GetMap();
            if (map == null)
            {
                DestroyMissile();
                return;
            }

            // Проверяем столкновение в ТЕКУЩЕЙ позиции (на случай если пуля создана в юните)
            if (CheckUnitCollision(Position))
            {
                Console.WriteLine($"Пуля попала в юнита в текущей позиции ({Position.X}, {Position.Y})");
                DestroyMissile();
                return;
            }

            // Проверяем столкновение со стеной в текущей позиции
            if (IsWallAtPosition(Position, map))
            {
                Console.WriteLine($"Пуля в стене при обновлении! ({Position.X}, {Position.Y})");
                HandleWallCollision(Position);
                DestroyMissile();
                return;
            }

            Vector2 newPosition = new Vector2(
                Position.X + _direction.X,
                Position.Y + _direction.Y
            );

            Console.WriteLine($"Пуля движется: ({Position.X}, {Position.Y}) -> ({newPosition.X}, {newPosition.Y})");

            // Проверяем ВСЕ клетки между текущей и новой позицией
            if (CheckCollisionAlongPath(Position, newPosition, map))
            {
                DestroyMissile();
                return;
            }

            // Проверяем столкновение в НОВОЙ позиции
            if (CheckUnitCollision(newPosition))
            {
                Console.WriteLine($"Пуля попала в юнита в новой позиции ({newPosition.X}, {newPosition.Y})");
                DestroyMissile();
                return;
            }

            // Проверяем стену в новой позиции
            if (IsWallAtPosition(newPosition, map))
            {
                Console.WriteLine($"Пуля столкнулась со стеной в ({newPosition.X}, {newPosition.Y})");
                HandleWallCollision(newPosition);
                DestroyMissile();
                return;
            }

            // Проверяем границы карты
            if (newPosition.X < 0 || newPosition.X >= map.GetLength(0) ||
                newPosition.Y < 0 || newPosition.Y >= map.GetLength(1))
            {
                Console.WriteLine($"Пуля вышла за границы карты");
                DestroyMissile();
                return;
            }

            // Если все проверки пройдены - двигаем пулю
            Position = newPosition;
        }

        private bool CheckCollisionAlongPath(Vector2 from, Vector2 to, char[,] map)
        {
            // Для горизонтального движения
            if (_direction.Y == 0)
            {
                int startX = Math.Min((int)from.X, (int)to.X);
                int endX = Math.Max((int)from.X, (int)to.X);
                int y = (int)from.Y;

                for (int x = startX; x <= endX; x++)
                {
                    Vector2 checkPos = new Vector2(x, y);

                    // Проверяем юнитов в каждой клетке пути
                    if (CheckUnitCollision(checkPos))
                    {
                        Console.WriteLine($"Пуля попала в юнита вдоль пути в ({x}, {y})");
                        return true;
                    }

                    // Проверяем стены в каждой клетке пути
                    if (IsWallAtPosition(checkPos, map))
                    {
                        Console.WriteLine($"Пуля столкнулась со стеной вдоль пути в ({x}, {y})");
                        HandleWallCollision(checkPos);
                        return true;
                    }
                }
            }
            // Для вертикального движения
            else if (_direction.X == 0)
            {
                int startY = Math.Min((int)from.Y, (int)to.Y);
                int endY = Math.Max((int)from.Y, (int)to.Y);
                int x = (int)from.X;

                for (int y = startY; y <= endY; y++)
                {
                    Vector2 checkPos = new Vector2(x, y);

                    // Проверяем юнитов в каждой клетке пути
                    if (CheckUnitCollision(checkPos))
                    {
                        Console.WriteLine($"Пуля попала в юнита вдоль пути в ({x}, {y})");
                        return true;
                    }

                    // Проверяем стены в каждой клетке пути
                    if (IsWallAtPosition(checkPos, map))
                    {
                        Console.WriteLine($"Пуля столкнулась со стеной вдоль пути в ({x}, {y})");
                        HandleWallCollision(checkPos);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool IsWallAtPosition(Vector2 position, char[,] map)
        {
            int x = (int)position.X;
            int y = (int)position.Y;

            if (x < 0 || x >= map.GetLength(0) || y < 0 || y >= map.GetLength(1))
                return true;

            char cell = map[x, y];
            return cell == '█' || cell == '▒';
        }

        private bool CheckUnitCollision(Vector2 position)
        {
            if (LevelModel.Units == null) return false;

            foreach (Unit unit in LevelModel.Units)
            {
                if (unit == this || unit is Missile || !unit.IsAlive())
                    continue;

                if (unit.Position.X == position.X && unit.Position.Y == position.Y)
                {
                    Console.WriteLine($"Пуля попала в {unit.GetType().Name} в позиции ({position.X}, {position.Y})");

                    if (unit is Enemy enemy)
                    {
                        enemy.HandleHit();
                    }
                    else if (unit is Player player)
                    {
                        // Вызываем HandleHit у игрока
                        player.HandleHit();
                    }

                    return true;
                }
            }

            return false;
        }

        private void HandleWallCollision(Vector2 collisionPosition)
        {
            Console.WriteLine($"=== ОБРАБОТКА СТОЛКНОВЕНИЯ СО СТЕНОЙ ===");
            Console.WriteLine($"Позиция: ({collisionPosition.X}, {collisionPosition.Y})");

            _mapGenerator?.DamageWallAt(collisionPosition);
            WallHit?.Invoke(collisionPosition);
        }

        private void DestroyMissile()
        {
            if (_isDead) return;

            _isDead = true;
            Console.WriteLine($"Уничтожение пули в позиции ({Position.X}, {Position.Y})");

            // СРАЗУ удаляем из LevelModel
            LevelModel.RemoveUnit(this);

            // Вызываем события
            OnDeath();
        }

        public void OnMissileDeath(Missile missile)
        {
            LevelModel.RemoveUnit(missile);
        }

        protected override void OnDeath()
        {
            Death?.Invoke();
        }

        // Пули игнорируют обычные проверки движения
        protected override bool TryChangePosition(Vector2 newPosition)
        {
            Position = newPosition;
            return true;
        }
    }
    //public class Missile : Unit
    //{
    //    private Vector2 _direction;
    //    public event Action<Vector2> WallHit;
    //    public event Action Death;
    //    private MapGenerator _mapGenerator;

    //    public Missile(Vector2 startPosition, Vector2 direction, IRenderer renderer, MapGenerator mapGenerator)
    //        : base(startPosition, "·", renderer)
    //    {
    //        _direction = direction;
    //        _mapGenerator = mapGenerator;

    //        Console.WriteLine($"Создана пуля в позиции ({startPosition.X}, {startPosition.Y})");

    //        // Сразу проверяем столкновение при создании
    //        CheckImmediateCollision();
    //    }

    //    private void CheckImmediateCollision()
    //    {
    //        char[,] map = LevelModel.GetInstance().GetMap();
    //        if (map == null) return;

    //        // Проверяем текущую позицию на столкновение
    //        if (IsWallAtPosition(Position, map))
    //        {
    //            Console.WriteLine($"Пуля создана ВНУТРИ СТЕНЫ! Позиция ({Position.X}, {Position.Y})");
    //            HandleWallCollision(Position);
    //            OnDeath();
    //            return;
    //        }

    //        Console.WriteLine($"Пуля создана на свободной клетке ({Position.X}, {Position.Y})");
    //    }

    //    public override void Update()
    //    {
    //        // Если пуля уже должна быть уничтожена, выходим
    //        if (!IsAlive()) return;

    //        char[,] map = LevelModel.GetInstance().GetMap();
    //        if (map == null)
    //        {
    //            OnDeath();
    //            return;
    //        }

    //        // Проверяем столкновение в ТЕКУЩЕЙ позиции
    //        if (IsWallAtPosition(Position, map))
    //        {
    //            Console.WriteLine($"Пуля внутри стены при обновлении! ({Position.X}, {Position.Y})");
    //            HandleWallCollision(Position);
    //            OnDeath();
    //            return;
    //        }

    //        // Вычисляем НОВУЮ позицию
    //        Vector2 newPosition = new Vector2(
    //            Position.X + _direction.X,
    //            Position.Y + _direction.Y
    //        );

    //        Console.WriteLine($"Пуля движется: ({Position.X}, {Position.Y}) -> ({newPosition.X}, {newPosition.Y})");

    //        // Проверяем столкновение в НОВОЙ позиции
    //        if (IsWallAtPosition(newPosition, map))
    //        {
    //            Console.WriteLine($"Пуля столкнулась со стеной в ({newPosition.X}, {newPosition.Y})");
    //            HandleWallCollision(newPosition);
    //            OnDeath();
    //            return;
    //        }

    //        // Проверяем границы карты
    //        if (newPosition.X < 0 || newPosition.X >= map.GetLength(0) ||
    //            newPosition.Y < 0 || newPosition.Y >= map.GetLength(1))
    //        {
    //            Console.WriteLine($"Пуля вышла за границы карты");
    //            OnDeath();
    //            return;
    //        }

    //        // Проверяем столкновение с юнитами в НОВОЙ позиции
    //        if (CheckUnitCollision(newPosition))
    //        {
    //            OnDeath();
    //            return;
    //        }

    //        // Если все проверки пройдены - двигаем пулю
    //        Position = newPosition;
    //    }

    //    private bool IsWallAtPosition(Vector2 position, char[,] map)
    //    {
    //        int x = (int)position.X;
    //        int y = (int)position.Y;

    //        // Проверяем границы
    //        if (x < 0 || x >= map.GetLength(0) || y < 0 || y >= map.GetLength(1))
    //            return true;

    //        char cell = map[x, y];
    //        bool isWall = cell == '█' || cell == '▒';

    //        if (isWall)
    //        {
    //            Console.WriteLine($"Обнаружена стена '{cell}' в позиции ({x}, {y})");
    //        }

    //        return isWall;
    //    }

    //    private bool CheckUnitCollision(Vector2 position)
    //    {
    //        if (LevelModel.Units == null) return false;

    //        foreach (Unit unit in LevelModel.Units)
    //        {
    //            if (unit == this || unit is Missile)
    //                continue;

    //            if (unit.Position.X == position.X && unit.Position.Y == position.Y)
    //            {
    //                Console.WriteLine($"Пуля попала в {unit.GetType().Name} в позиции ({position.X}, {position.Y})");

    //                if (unit is Enemy enemy)
    //                {
    //                    enemy.HandleHit();
    //                }

    //                return true;
    //            }
    //        }

    //        return false;
    //    }

    //    private void HandleWallCollision(Vector2 collisionPosition)
    //    {
    //        Console.WriteLine($"=== ОБРАБОТКА СТОЛКНОВЕНИЯ СО СТЕНОЙ ===");
    //        Console.WriteLine($"Позиция: ({collisionPosition.X}, {collisionPosition.Y})");

    //        _mapGenerator?.DamageWallAt(collisionPosition);
    //        WallHit?.Invoke(collisionPosition);
    //    }

    //    public void OnMissileDeath(Missile missile)
    //    {
    //        Console.WriteLine($"Уничтожение пули");
    //        LevelModel.RemoveUnit(missile);
    //    }

    //    // Пули игнорируют обычные проверки движения
    //    protected override bool TryChangePosition(Vector2 newPosition)
    //    {
    //        Position = newPosition;
    //        return true;
    //    }
    //}



}

