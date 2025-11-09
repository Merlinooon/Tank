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
            _mapGenerator = mapGenerator;
        }

        public void CreateUnit(UnitConfig config)
        {
            Vector2 safePosition = GetSafeStartPosition(config.Position);

            switch (config.Type)
            {
                case UnitType.Player:
                    Player player = new Player(safePosition, _renderer, _moveInput, _mapGenerator);
                    LevelModel.AddUnit(player);
                    LevelModel.SetPlayer(player);
                    break;
                case UnitType.Enemy:
                    // Передаем MapGenerator врагу
                    Enemy enemy = new Enemy(safePosition, _renderer, _moveInput, LevelModel.GetInstance(), _mapGenerator);
                    LevelModel.AddUnit(enemy);
                    break;
            }
        }

        /// <summary>
        /// Находит безопасную стартовую позицию (не в воде)
        /// </summary>
        private Vector2 GetSafeStartPosition(Vector2 desiredPosition)
        {
            char[,] map = LevelModel.GetInstance().GetMap();
            if (map == null) return desiredPosition;

            // Проверяем желаемую позицию
            if (IsPositionSafeForStart(desiredPosition, map))
            {
                return desiredPosition;
            }

            // Ищем ближайшую безопасную позицию
            return FindNearestSafePosition(desiredPosition, map);
        }

        /// <summary>
        /// Проверяет, безопасна ли позиция для старта
        /// </summary>
        private bool IsPositionSafeForStart(Vector2 position, char[,] map)
        {
            int x = (int)position.X;
            int y = (int)position.Y;

            if (x < 0 || x >= map.GetLength(0) || y < 0 || y >= map.GetLength(1))
                return false;

            // Позиция безопасна если это пустое пространство
            return map[x, y] == ' ';
        }

        /// <summary>
        /// Находит ближайшую безопасную позицию
        /// </summary>
        private Vector2 FindNearestSafePosition(Vector2 startPosition, char[,] map)
        {
            // Ищем в радиусе 5 клеток
            for (int radius = 1; radius <= 5; radius++)
            {
                for (int y = (int)startPosition.Y - radius; y <= (int)startPosition.Y + radius; y++)
                {
                    for (int x = (int)startPosition.X - radius; x <= (int)startPosition.X + radius; x++)
                    {
                        if (x >= 1 && x < map.GetLength(0) - 1 &&
                            y >= 1 && y < map.GetLength(1) - 1)
                        {
                            Vector2 candidate = new Vector2(x, y);
                            if (IsPositionSafeForStart(candidate, map))
                            {
                                Console.WriteLine($"Найдена безопасная стартовая позиция: ({x}, {y}) вместо ({startPosition.X}, {startPosition.Y})");
                                return candidate;
                            }
                        }
                    }
                }
            }

            // Если не нашли, возвращаем исходную (будет ошибка, но лучше чем ничего)
            Console.WriteLine($"ВНИМАНИЕ: Не найдена безопасная стартовая позиция рядом с ({startPosition.X}, {startPosition.Y})");
            return startPosition;
        }
    }
}
