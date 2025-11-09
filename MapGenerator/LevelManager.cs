using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public class LevelManager
    {
        private static LevelManager _instance;
        public static LevelManager Instance => _instance ??= new LevelManager();

        // Словарь уровней: ключ - номер уровня, значение - конфигурация уровня
        private Dictionary<int, LevelConfig> _levelsConfig;

        // Текущий уровень
        public int CurrentLevel { get; private set; }
        public LevelConfig CurrentLevelConfig { get; private set; }

        // Сложность
        public Difficulty Difficulty { get; private set; }

        // Генератор карты
        public MapGenerator CurrentMapGenerator { get; private set; }

        private LevelManager()
        {
            InitializeLevels();
            CurrentLevel = 1;
            Difficulty = Difficulty.Normal;
           
        }

        /// <summary>
        /// Инициализация конфигураций уровней
        /// </summary>
        private void InitializeLevels()
        {
            _levelsConfig = new Dictionary<int, LevelConfig>
            {
                { 1, new LevelConfig(15, 15, 2, 3, 2, "Уровень 1 - Начало пути") },
                { 2, new LevelConfig(20, 20, 3, 5, 3, "Уровень 2 - Наращивая темп") },
                { 3, new LevelConfig(25, 25, 4, 7, 4, "Уровень 3 - Сложные решения") },
                { 4, new LevelConfig(30, 30, 5, 10, 5, "Уровень 4 - Битва титанов") },
                { 5, new LevelConfig(35, 35, 6, 12, 6, "Уровень 5 - Финальное испытание") }
            };
        }
        public bool IsLevelCompleted()
        {
            if (LevelModel.Units == null || LevelModel.Player == null || !LevelModel.Player.IsAlive())
                return false;

            int aliveEnemies = 0;
            foreach (Unit unit in LevelModel.Units)
            {
                if (unit is Enemy && unit.IsAlive())
                {
                    aliveEnemies++;
                }
            }

            int targetEnemies = CurrentLevelConfig?.TargetEnemyCount ?? 0;
            bool levelCompleted = aliveEnemies == 0 && targetEnemies > 0;

            if (levelCompleted)
            {
                Console.WriteLine($"Уровень {CurrentLevel} завершен! Уничтожено врагов: {targetEnemies}");
            }

            return levelCompleted;
        }
        public void ShowLevelCompletionMessage()
        {
            Console.Clear();
            Console.WriteLine("==========================================");
            Console.WriteLine("            🎉 ПОЗДРАВЛЯЕМ! 🎉");
            Console.WriteLine("==========================================");
            Console.WriteLine($"    Уровень {CurrentLevel} пройден!");
            Console.WriteLine($"    Уничтожено врагов: {CurrentLevelConfig.TargetEnemyCount}");
            Console.WriteLine();

            if (_levelsConfig.ContainsKey(CurrentLevel + 1))
            {
                LevelConfig nextLevel = _levelsConfig[CurrentLevel + 1];
                Console.WriteLine($"    Следующий уровень: {nextLevel.Description}");
                Console.WriteLine($"    Врагов для уничтожения: {nextLevel.TargetEnemyCount}");
                Console.WriteLine($"    Размер карты: {nextLevel.Width}x{nextLevel.Height}");
            }
            else
            {
                Console.WriteLine("    Вы прошли все уровни! 🏆");
            }

            Console.WriteLine();
            Console.WriteLine("==========================================");
            Console.WriteLine("    Нажмите любую клавишу для продолжения...");
            Console.WriteLine("==========================================");
            Console.ReadKey(true);
        }
        /// <summary>
        /// Загрузка уровня
        /// </summary>
        public void LoadLevel(int levelNumber, IMoveInput input) // Добавляем input как параметр
        {
            if (!_levelsConfig.ContainsKey(levelNumber))
            {
                Console.WriteLine($"Уровень {levelNumber} не найден!");
                return;
            }

            CurrentLevel = levelNumber;
            CurrentLevelConfig = _levelsConfig[levelNumber];

            Console.WriteLine($"Загружаем уровень {levelNumber}: {CurrentLevelConfig.Description}");

            // Создаем новый генератор карты
            IRenderer renderer = new ConsoleRenderer();
            CurrentMapGenerator = new MapGenerator(
                CurrentLevelConfig.Width,
                CurrentLevelConfig.Height,
                renderer
                
            );

            // Устанавливаем карту в LevelModel
            LevelModel.SetMap(CurrentMapGenerator.Map);
            LevelModel.SetUnits(new Units());

            CreateLevelUnits(input);

            Console.WriteLine($"Уровень {levelNumber} загружен!");
        }

        /// <summary>
        /// Создание юнитов для уровня
        /// </summary>
        private void CreateLevelUnits(IMoveInput input)
        {
            UnitFactory unitFactory = new UnitFactory(
                new ConsoleRenderer(),
                input, // Используем переданный input
                CurrentMapGenerator
            );

            // Создаем игрока
            Vector2 playerStart = FindSafeStartPosition();
            UnitConfig playerConfig = new UnitConfig(playerStart, "▲", UnitType.Player);
            unitFactory.CreateUnit(playerConfig);

            // Создаем врагов
            int enemyCount = CalculateEnemyCount();
            for (int i = 0; i < enemyCount; i++)
            {
                Vector2 enemyStart = FindSafeEnemyPosition();
                UnitConfig enemyConfig = new UnitConfig(enemyStart, "▼", UnitType.Enemy);
                unitFactory.CreateUnit(enemyConfig);
            }

            Console.WriteLine($"Создано врагов: {enemyCount}");
        }

        /// <summary>
        /// Расчет количества врагов
        /// </summary>
        private int CalculateEnemyCount()
        {
            int baseCount = CurrentLevelConfig.BaseEnemyCount;

            // Модификатор сложности
            float difficultyMultiplier = Difficulty switch
            {
                Difficulty.Easy => 0.7f,
                Difficulty.Normal => 1.0f,
                Difficulty.Hard => 1.3f,
                Difficulty.Expert => 1.6f,
                _ => 1.0f
            };

            return (int)(baseCount * difficultyMultiplier);
        }

        /// <summary>
        /// Поиск безопасной стартовой позиции для игрока
        /// </summary>
        private Vector2 FindSafeStartPosition()
        {
            // Предпочтительные стартовые позиции
            Vector2[] preferredPositions = {
            new Vector2(1, 1),
            new Vector2(1, 2),
            new Vector2(2, 1)
        };

            foreach (var pos in preferredPositions)
            {
                if (IsPositionSafeForStart(pos))
                    return pos;
            }

            // Если предпочтительные заняты, ищем любую безопасную
            return FindAnySafePosition();
        }

        /// <summary>
        /// Поиск безопасной позиции для врага
        /// </summary>
        private Vector2 FindSafeEnemyPosition()
        {
            // Враги появляются дальше от игрока
            char[,] map = LevelModel.GetInstance().GetMap();
            Vector2 playerPos = LevelModel.Player?.Position ?? new Vector2(1, 1);

            for (int attempts = 0; attempts < 50; attempts++)
            {
                int x = new Random().Next(3, map.GetLength(0) - 3);
                int y = new Random().Next(3, map.GetLength(1) - 3);

                Vector2 candidate = new Vector2(x, y);

                // Проверяем расстояние до игрока и безопасность позиции
                double distanceToPlayer = Math.Sqrt(
                    Math.Pow(x - playerPos.X, 2) + Math.Pow(y - playerPos.Y, 2)
                );

                if (distanceToPlayer > 5 && IsPositionSafeForStart(candidate))
                    return candidate;
            }

            return FindAnySafePosition();
        }

        /// <summary>
        /// Поиск любой безопасной позиции
        /// </summary>
        private Vector2 FindAnySafePosition()
        {
            char[,] map = LevelModel.GetInstance().GetMap();

            for (int y = 1; y < map.GetLength(1) - 1; y++)
            {
                for (int x = 1; x < map.GetLength(0) - 1; x++)
                {
                    if (IsPositionSafeForStart(new Vector2(x, y)))
                        return new Vector2(x, y);
                }
            }

            return new Vector2(1, 1); // Fallback
        }

        /// <summary>
        /// Проверка безопасности позиции для старта
        /// </summary>
        private bool IsPositionSafeForStart(Vector2 position)
        {
            char[,] map = LevelModel.GetInstance().GetMap();
            int x = (int)position.X;
            int y = (int)position.Y;

            if (x < 0 || x >= map.GetLength(0) || y < 0 || y >= map.GetLength(1))
                return false;

            // Позиция безопасна если это пустое пространство
            return map[x, y] == ' ';
        }

        /// <summary>
        /// Переход на следующий уровень
        /// </summary>

        public void NextLevel(IMoveInput input)
        {
            int nextLevel = CurrentLevel + 1;

            if (_levelsConfig.ContainsKey(nextLevel))
            {
                ShowLevelCompletionMessage();
                LoadLevel(nextLevel, input);
            }
            else
            {
                ShowGameCompletionMessage();
            }
        }
        private void ShowGameCompletionMessage()
        {
            Console.Clear();
            Console.WriteLine("==========================================");
            Console.WriteLine("        🏆 ПОБЕДА! 🏆");
            Console.WriteLine("==========================================");
            Console.WriteLine("  Вы прошли все уровни игры Танки!");
            Console.WriteLine();
            Console.WriteLine("  Спасибо за игру!");
            Console.WriteLine("==========================================");
            Console.WriteLine("  Нажмите любую клавишу для выхода...");
            Console.WriteLine("==========================================");
            Console.ReadKey(true);
        }
        /// <summary>
        /// Перезапуск текущего уровня
        /// </summary>
        public void RestartLevel(IMoveInput input) // Добавляем input как параметр
        {
            LoadLevel(CurrentLevel, input); // Передаем input
        }

        /// <summary>
        /// Установка сложности
        /// </summary>
        public void SetDifficulty(Difficulty difficulty)
        {
            Difficulty = difficulty;
            Console.WriteLine($"Установлена сложность: {difficulty}");
        }

        /// <summary>
        /// Получение статистики уровня
        /// </summary>
        public LevelStats GetLevelStats()
        {
            return new LevelStats
            {
                LevelNumber = CurrentLevel,
                LevelName = CurrentLevelConfig.Description,
                //Difficulty = Difficulty,
                MapSize = $"{CurrentLevelConfig.Width}x{CurrentLevelConfig.Height}",
                EnemyCount = CalculateEnemyCount(),
              
            };
        }
    }

}