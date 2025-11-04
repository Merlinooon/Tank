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



            // Проверяем, что стартовая позиция не в стене
            char[,] map = LevelModel.GetInstance().GetMap();
            if (map != null)
            {
                int startX = (int)bulletStartPos.X;
                int startY = (int)bulletStartPos.Y;

                // Если стартовая позиция в стене - не создаем пулю
                if (startX < 0 || startX >= map.GetLength(0) ||
                    startY < 0 || startY >= map.GetLength(1) ||
                    map[startX, startY] == '█' || map[startX, startY] == '▒')
                {
                    Console.WriteLine("Нельзя стрелять в стену!");
                    return null;
                }
            }

            var missile = new Missile(bulletStartPos, direction, _renderer, _mapGenerator);

            missile.Death += () => missile.OnMissileDeath(missile);
           
            LevelModel.AddUnit(missile);

            Console.WriteLine($"Игрок выстрелил с позиции ({bulletStartPos.X}, {bulletStartPos.Y})");

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
