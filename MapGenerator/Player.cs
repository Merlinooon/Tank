using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapGenerator.MapGenerator;

namespace MapGenerator
{
    
    public class Player : Unit, IDisposable, IAttack
    {
        public event Action Death;
        private IMoveInput _input;
        private MapGenerator _mapGenerator;
        IRenderer _renderer;
        public Vector2 ShootDirection { get; private set; }

        public Player(Vector2 startPosition, IRenderer renderer, IMoveInput input, MapGenerator mapGenerator)
            : base(startPosition, "►", renderer)
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
                if (unit == this || !unit.IsAlive()) // Пропускаем себя и мертвые юниты
                    continue;

                if (Position.Equals(unit.Position))
                {
                    if (unit is Missile missile)
                    {
                        HandleHit();
                        missile.OnMissileDeath(missile);
                    }
                    else if (unit is Enemy)
                    {
                        Console.WriteLine("Игрок столкнулся с врагом!");
                        OnDeath();
                    }
                }
            }
        }

        public void HandleHit()
        {
           
            RespawnAtRandomPosition();
        }

        private void RespawnAtRandomPosition()
        {
            Vector2? randomPosition = LevelModel.FindSafeRespawnPosition(this);
            if (randomPosition.HasValue)
            {
                Position = randomPosition.Value;
                
            }
            else
            {
               
                OnDeath(); // Используем метод из базового класса
            }
        }
        public Missile Shoot(Vector2 direction)
        {
            // Стартовая позиция пули - ПЕРЕД игроком (следующая клетка)
            Vector2 bulletStartPos = new Vector2(
                Position.X + direction.X,
                Position.Y + direction.Y
            );

            //Console.WriteLine($"=== ВЫСТРЕЛ ИГРОКА ===");
            //Console.WriteLine($"Позиция игрока: ({Position.X}, {Position.Y})");
            //Console.WriteLine($"Направление: ({direction.X}, {direction.Y})");
            //Console.WriteLine($"Старт пули: ({bulletStartPos.X}, {bulletStartPos.Y})");

            // Создаем пулю ВСЕГДА, даже если в стене
            var missile = new Missile(bulletStartPos, direction, _renderer, _mapGenerator);
            missile.Death += () => missile.OnMissileDeath(missile);
            LevelModel.AddUnit(missile);

            return missile;
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
