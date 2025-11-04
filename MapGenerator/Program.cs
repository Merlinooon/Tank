



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

            Console.WriteLine("Игра запущена! Проверьте файл map.txt для просмотра карты.");

            while (true)
            {
                input.Update();

                if (LevelModel.Units != null)
                {
                    List<Unit> unitsToUpdate = new List<Unit>();
                    foreach (Unit unit in LevelModel.Units)
                    {
                        unitsToUpdate.Add(unit);
                    }

                    foreach (Unit unit in unitsToUpdate)
                    {
                        unit.Update();
                    }
                }

                // Отрисовываем карту из LevelModel
                renderer.Renderer(LevelModel.GetInstance().GetMap(), LevelModel.Units);
                Thread.Sleep(200);
            }
        }
    }
}