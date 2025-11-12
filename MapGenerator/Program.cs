



namespace MapGenerator
{


    class Program
    {
        static void Main()
        {
            Console.Title = "Танки";
            Console.CursorVisible = false;

            // Показываем заставку
            ShowSplashScreen();

            // Создаем ОДИН экземпляр input для всей игры
            ConsoleInput input = new ConsoleInput();
            LevelManager levelManager = LevelManager.Instance;

            // Показываем главное меню после заставки
            ShowMainMenu(levelManager, input);

            Console.WriteLine("Спасибо за игру!");
        }

        private static void ShowSplashScreen()
        {
            Console.Clear();
            Console.WriteLine("================================");
            Console.WriteLine("           ТАНКИ");
            Console.WriteLine("================================");
            Console.WriteLine();
            Console.WriteLine("   Классическая аркадная игра");
            Console.WriteLine();
            Console.WriteLine("================================");
            Console.WriteLine("   Нажмите любую клавишу...");
            Console.WriteLine("================================");
            Console.ReadKey(true);
        }

        private static void ShowMainMenu(LevelManager levelManager, IMoveInput input)
        {
            bool inMenu = true;

            while (inMenu)
            {
                Console.Clear();
                Console.WriteLine("=== ГЛАВНОЕ МЕНЮ ===");
                Console.WriteLine("1 - Начать игру");
                Console.WriteLine("2 - Выбор сложности");
                Console.WriteLine("3 - Об игре");
                Console.WriteLine("ESC - Выход");
                Console.WriteLine("====================");
                Console.WriteLine($"Текущая сложность: {levelManager.Difficulty}");

                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.D1:
                        // ТОЛЬКО когда пользователь нажал "1" - начинаем игру
                        StartGame(levelManager, input);
                        break;
                    case ConsoleKey.D2:
                        ShowDifficultyMenu(levelManager);
                        break;
                    case ConsoleKey.D3:
                        ShowAbout();
                        break;
                    case ConsoleKey.Escape:
                        inMenu = false;
                        break;
                }
            }
        }

        private static void StartGame(LevelManager levelManager, IMoveInput input)
        {
            Console.Clear();
            Console.WriteLine("Загружаем уровень 1...");
            Thread.Sleep(1000);

            // ЗАГРУЖАЕМ уровень только когда пользователь выбрал "Начать игру"
            levelManager.LoadLevel(1, input);

            // Основной игровой цикл
            RunGameLoop(levelManager, input);
        }

        private static void RunGameLoop(LevelManager levelManager, IMoveInput input)
        {
            bool gameRunning = true;
            input.Esc += () => gameRunning = false;

            while (gameRunning)
            {
                input.Update();

                // Игровая логика
                if (LevelModel.Units != null)
                {
                    List<Unit> unitsToUpdate = new List<Unit>();
                    foreach (Unit unit in LevelModel.Units)
                    {
                        unitsToUpdate.Add(unit);
                    }

                    foreach (Unit unit in unitsToUpdate)
                    {
                        if (unit.IsAlive())
                        {
                            unit.Update();
                        }
                    }
                }

                // Отрисовка с передачей статистики
                if (levelManager.CurrentMapGenerator != null)
                {
                    var renderer = new ConsoleRenderer();

                    // Получаем статистику врагов
                    int aliveEnemies = GetAliveEnemiesCount();
                    int totalEnemies = levelManager.CurrentLevelConfig?.TargetEnemyCount ?? 0;

                    renderer.Renderer(
                        LevelModel.GetInstance().GetMap(),
                        LevelModel.Units,
                        levelManager.CurrentLevel,
                        aliveEnemies,
                        totalEnemies
                    );
                }

                // Проверка условий игры
                CheckGameConditions(levelManager, input);

                Thread.Sleep(150);
            }
        }

        /// Подсчитывает количество живых врагов
        private static int GetAliveEnemiesCount()
        {
            int aliveEnemies = 0;
            if (LevelModel.Units != null)
            {
                foreach (Unit unit in LevelModel.Units)
                {
                    if (unit is Enemy && unit.IsAlive())
                    {
                        aliveEnemies++;
                    }
                }
            }
            return aliveEnemies;
        }
       
        private static void ShowDifficultyMenu(LevelManager levelManager)
        {
            Console.Clear();
            Console.WriteLine("=== ВЫБОР СЛОЖНОСТИ ===");
            Console.WriteLine("1 - Легкая");
            Console.WriteLine("2 - Нормальная");
            Console.WriteLine("3 - Сложная");
            Console.WriteLine("4 - Эксперт");
            Console.WriteLine("=======================");

            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.D1:
                    levelManager.SetDifficulty(Difficulty.Easy);
                    break;
                case ConsoleKey.D2:
                    levelManager.SetDifficulty(Difficulty.Normal);
                    break;
                case ConsoleKey.D3:
                    levelManager.SetDifficulty(Difficulty.Hard);
                    break;
                case ConsoleKey.D4:
                    levelManager.SetDifficulty(Difficulty.Expert);
                    break;
            }

            Console.WriteLine($"Сложность установлена: {levelManager.Difficulty}");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey(true);
        }

        private static void ShowAbout()
        {
            Console.Clear();
            Console.WriteLine("=== ОБ ИГРЕ ===");
            Console.WriteLine("Танки - классическая аркадная игра");
            Console.WriteLine();
            Console.WriteLine("Управление:");
            Console.WriteLine("• Стрелки - движение");
            Console.WriteLine("• Пробел - стрельба");
            Console.WriteLine("• ESC - выход в меню");
            Console.WriteLine();
            Console.WriteLine("Цель: уничтожить всех врагов");
            Console.WriteLine("=================");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey(true);
        }

        private static void CheckGameConditions(LevelManager levelManager, IMoveInput input)
        {
            // Проверка победы на уровне
            if (levelManager.IsLevelCompleted())
            {
                Console.WriteLine("🎉 Уровень пройден! Переходим на следующий...");
                Thread.Sleep(1500);
                levelManager.NextLevel(input);
                return;
            }

            // Проверка поражения (игрок умер)
            if (LevelModel.Player != null && !LevelModel.Player.IsAlive())
            {
                Console.WriteLine("Поражение! Рестарт уровня...");
                Thread.Sleep(2000);
                levelManager.RestartLevel(input);
                return;
            }

            // Проверка выхода в меню
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
            {
                ShowPauseMenu(levelManager, input);
            }
        }
        private static void ShowPauseMenu(LevelManager levelManager, IMoveInput input)
        {
            Console.Clear();
            Console.WriteLine("=== ПАУЗА ===");
            Console.WriteLine("1 - Продолжить");
            Console.WriteLine("2 - Рестарт уровня");
            Console.WriteLine("3 - В главное меню");
            Console.WriteLine("==============");

            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.D1:
                    // Продолжить игру
                    break;
                case ConsoleKey.D2:
                    levelManager.RestartLevel(input);
                    break;
                case ConsoleKey.D3:
                    throw new OperationCanceledException("Возврат в меню"); // Простой способ выйти из игрового цикла
            }
        }
    }
}