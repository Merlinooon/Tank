



namespace MapGenerator
{

   
    class Program
    {
        static void Main()
        {
            IRenderer renderer = new ConsoleRenderer();
            ConsoleInput input = new ConsoleInput();

            // Создаем генератор карты ПЕРВЫМ
            MapGenerator generator = new MapGenerator(15, 15, renderer);

            // Инициализируем LevelModel тем же массивом
            LevelModel.SetMap(generator.Map); // Используем generator.Map
            LevelModel.SetUnits(new Units());

            // Создаем фабрику с передачей MapGenerator
            UnitFactory unitFactory = new UnitFactory(renderer, input, generator);

            // Создаем игрока
            UnitConfig playerConfig = new UnitConfig(new Vector2(1, 1), "▲", UnitType.Player);
            unitFactory.CreateUnit(playerConfig);

            UnitConfig enemyConfig = new UnitConfig(new Vector2(13, 13), "▼", UnitType.Enemy);
            unitFactory.CreateUnit(enemyConfig);

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

                // Отрисовываем актуальную карту из генератора
                renderer.Renderer(generator.Map, LevelModel.Units);

                Thread.Sleep(200);
            }
        }
    }
}