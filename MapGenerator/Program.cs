



namespace MapGenerator
{


    class Program
    {
        static void Main()
        {
            IRenderer renderer = new ConsoleRenderer();
            ConsoleInput input = new ConsoleInput();

            // Создаем генератор карты - он сам сохранит карту в LevelModel
            MapGenerator generator = new MapGenerator(15, 15, renderer);

            LevelModel.SetUnits(new Units());
            UnitFactory unitFactory = new UnitFactory(renderer, input, generator);

            UnitConfig playerConfig = new UnitConfig(new Vector2(1, 1), "▲", UnitType.Player);
            unitFactory.CreateUnit(playerConfig);

            UnitConfig enemyConfig = new UnitConfig(new Vector2(5, 5), "▼", UnitType.Enemy);
            unitFactory.CreateUnit(enemyConfig);

            Console.WriteLine("Игра запущена! Управление: Стрелки - движение, Пробел - стрельба");

            int frameCount = 0; // Объявляем frameCount здесь

            while (true)
            {
                frameCount++;
                Console.WriteLine($"\n=== Кадр {frameCount} ===");

                input.Update();

                if (LevelModel.Units != null)
                {
                    // Собираем всех юнитов для обновления (только живые будут обновляться)
                    List<Unit> unitsToUpdate = new List<Unit>();
                    foreach (Unit unit in LevelModel.Units)
                    {
                        unitsToUpdate.Add(unit);
                    }

                    Console.WriteLine($"Юнитов для обновления: {unitsToUpdate.Count}");

                    foreach (Unit unit in unitsToUpdate)
                    {
                        // IsAlive() вернет false для уже удаленных пуль
                        if (unit.IsAlive())
                        {
                            unit.Update();
                        }
                    }
                }

                // Отрисовываем карту
                renderer.Renderer(LevelModel.GetInstance().GetMap(), LevelModel.Units);
                Thread.Sleep(200);
            }
        }
    }
}