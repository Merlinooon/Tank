using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapGenerator.MapGenerator;

namespace MapGenerator
{
    //public class Player : Unit, IDisposable, IAttack
    //{
    //    public event Action Death;
    //    private IMoveInput _input;
    //    private MapGenerator _mapGenerator;
    //    IRenderer _renderer;
    //    public Vector2 ShootDirection { get; private set; }
    //    public Player(Vector2 startPosition, IRenderer renderer, IMoveInput input) : base(startPosition, "▲", renderer)
    //    {
    //        _input = input;
    //        _renderer = renderer;

    //        SetupInput();
    //    }
    //    private void UpdateView()
    //    {
    //        // Обновляем символ игрока в зависимости от направления
    //        View = ShootDirection switch
    //        {
    //            { X: 1, Y: 0 } => "►",  // Вправо
    //            { X: -1, Y: 0 } => "◄", // Влево
    //            { X: 0, Y: -1 } => "▲", // Вверх
    //            { X: 0, Y: 1 } => "▼",  // Вниз
    //            _ => "►"
    //        };
    //    }
    //    private void SetupInput()
    //    {
    //        _input.MoveUp += () => { ShootDirection = new Vector2(0, -1); UpdateView(); TryMoveUp(); };
    //        _input.MoveDown += () => { ShootDirection = new Vector2(0, 1); UpdateView(); TryMoveDown(); };
    //        _input.MoveLeft += () => { ShootDirection = new Vector2(-1, 0); UpdateView(); TryMoveLeft(); };
    //        _input.MoveRight += () => { ShootDirection = new Vector2(1, 0); UpdateView(); TryMoveRight(); };
    //        _input.Space += () => Shoot(ShootDirection);
    //    }
    //    public override void Update()
    //    {
    //        foreach (Unit unit in LevelModel.Units)
    //        {
    //            if (unit == this)
    //                continue;
    //            if (Position.Equals(unit.Position))
    //            {
    //                Death?.Invoke();
    //            }
    //        }
    //    }

    //    //public void Shoot()
    //    //{
    //    //    // Создаем пулю в позиции перед игроком (в зависимости от направления)
    //    //    Vector2 bulletPosition = GetBulletStartPosition();
    //    //    Missile missile = new Missile(bulletPosition, ShootDirection, _renderer,_mapGenerator);
    //    //    LevelModel.AddUnit(missile);

    //    //    // Подписываемся на событие смерти пули для очистки
    //    //    missile.Death += () => OnMissileDeath(missile);
    //    //}
    //    public Missile Shoot(Vector2 direction)
    //    {
    //        // Создаем пулю с передачей MapGenerator
    //        var missile = new Missile(Position, direction, _renderer, _mapGenerator);

    //        // Подписываемся на события пули
    //        missile.Death += () => OnMissileDeath(missile);
    //        missile.WallHit += (position) => OnMissileWallHit(position);

    //        return missile;
    //    }


    //    private void OnMissileWallHit(Vector2 position)
    //    {
    //        // Дополнительная логика при попадании в стену
    //        Console.WriteLine($"Танк попал в стену на позиции {position}");
    //    }

    //    private Vector2 GetBulletStartPosition()
    //    {

    //        Vector2 bulletPos = new Vector2(
    //        Position.X + ShootDirection.X,
    //        Position.Y + ShootDirection.Y);

    //        return bulletPos;
    //    }

    //    private void OnMissileDeath(Missile missile)
    //    {
    //        LevelModel.RemoveUnit(missile);
    //    }

    //    public void Dispose()
    //    {
    //        _input.MoveUp -= () => TryMoveUp();
    //        _input.MoveDown -= () => TryMoveDown();
    //        _input.MoveLeft -= () => TryMoveLeft();
    //        _input.MoveRight -= () => TryMoveRight();
    //        _input.Space -= () => Shoot(ShootDirection);

    //    }
    //}
    public class Player : Unit, IDisposable, IAttack
    {
        public event Action Death;
        private IMoveInput _input;
        private MapGenerator _mapGenerator;
        IRenderer _renderer;
        public Vector2 ShootDirection { get; private set; }

        public Player(Vector2 startPosition, IRenderer renderer, IMoveInput input, MapGenerator mapGenerator)
            : base(startPosition, "▲", renderer)
        {
            _input = input;
            _renderer = renderer;
            _mapGenerator = mapGenerator;
            ShootDirection = new Vector2(1, 0); // Начальное направление - вправо

            SetupInput();
        }

        private void UpdateView()
        {
            View = ShootDirection switch
            {
                { X: 1, Y: 0 } => "►",
                { X: -1, Y: 0 } => "◄",
                { X: 0, Y: -1 } => "▲",
                { X: 0, Y: 1 } => "▼",
                _ => "►"
            };
        }

        private void SetupInput()
        {
            _input.MoveUp += () => { ShootDirection = new Vector2(0, -1); UpdateView(); TryMoveUp(); };
            _input.MoveDown += () => { ShootDirection = new Vector2(0, 1); UpdateView(); TryMoveDown(); };
            _input.MoveLeft += () => { ShootDirection = new Vector2(-1, 0); UpdateView(); TryMoveLeft(); };
            _input.MoveRight += () => { ShootDirection = new Vector2(1, 0); UpdateView(); TryMoveRight(); };

            // ИСПРАВЛЕНИЕ: запоминаем направление в момент выстрела
            _input.Space += () =>
            {
                Vector2 shootDir = ShootDirection; // Фиксируем текущее направление
                Shoot(shootDir);
            };
        }

        public override void Update()
        {
            foreach (Unit unit in LevelModel.Units)
            {
                if (unit == this)
                    continue;
                if (Position.Equals(unit.Position))
                {
                    Death?.Invoke();
                }
            }
        }

        public Missile Shoot(Vector2 direction)
        {
            Vector2 bulletStartPos = new Vector2(
            Position.X + direction.X,  // Абсолютная X
            Position.Y + direction.Y); // Абсолютная Y

           

            // Создаем пулю с передачей MapGenerator
            var missile = new Missile(bulletStartPos, direction, _renderer, _mapGenerator);

            // Подписываемся на события пули
            missile.Death += () => OnMissileDeath(missile);
            missile.WallHit += (position) => OnMissileWallHit(position);

            LevelModel.AddUnit(missile);

            return missile;
        }

        private void OnMissileWallHit(Vector2 position)
        {
            Console.WriteLine($"Игрок попал в стену на позиции {position}");
        }

        private void OnMissileDeath(Missile missile)
        {
            LevelModel.RemoveUnit(missile);
            Console.WriteLine("Пуля уничтожена");
        }

        public void Dispose()
        {
            _input.MoveUp -= () => TryMoveUp();
            _input.MoveDown -= () => TryMoveDown();
            _input.MoveLeft -= () => TryMoveLeft();
            _input.MoveRight -= () => TryMoveRight();
            _input.Space -= () =>
            {
                Vector2 shootDir = ShootDirection;
                Shoot(shootDir);
            };
        }
    }
}
