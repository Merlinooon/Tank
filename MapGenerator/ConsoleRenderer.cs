using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public class ConsoleRenderer : IRenderer
    {
        private char[,] _pixels;
        private char[,] _previousPixels;
        private int _width;
        private int _height;

        // Цветовая схема
        private Dictionary<char, ConsoleColor> _colorScheme;

        public ConsoleRenderer()
        {
            _width = Console.WindowWidth;
            _height = Console.WindowHeight;
            _pixels = new char[_width, _height];
            _previousPixels = new char[_width, _height];
            Console.CursorVisible = false;

            InitializeColorScheme();
            ApplyColorTheme();
        }

        /// <summary>
        /// Инициализация цветовой схемы
        /// </summary>
        private void InitializeColorScheme()
        {
            _colorScheme = new Dictionary<char, ConsoleColor>
            {
                // Стены
                { '█', ConsoleColor.Red },         // Полная стена
                { '▒', ConsoleColor.Yellow },      // Полуразрушенная стена
            
                // Игрок и враги
                { '▲', ConsoleColor.Green },       // Игрок (вверх)
                { '▼', ConsoleColor.Red },         // Враг (вниз)
                { '►', ConsoleColor.Green },       // Игрок (вправо)
                { '◄', ConsoleColor.Green },       // Игрок (влево)

                { '▓', ConsoleColor.Blue },

            };
        }

       
        /// Применяет цветовую тему к консоли
    
        private void ApplyColorTheme()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
        }

       
        /// Получает цвет для символа

        private ConsoleColor GetColorForChar(char ch)
        {
            if (_colorScheme.ContainsKey(ch))
                return _colorScheme[ch];

            return ConsoleColor.White; // Цвет по умолчанию
        }

      
        /// Получает цвет для юнита на основе его типа
       
        private ConsoleColor GetColorForUnit(Unit unit)
        {
            if (unit is Player)
                return ConsoleColor.Green;
            else if (unit is Enemy)
                return ConsoleColor.Red;
            else if (unit is Missile)
            {
                // Определяем цвет пули в зависимости от направления или типа стрелка
                if (unit.View == "·") return ConsoleColor.Cyan;    // Пуля игрока
                else return ConsoleColor.Magenta;                  // Пуля врага
            }

            return ConsoleColor.White;
        }

        private void SetPixel(int w, int h, char val)
        {
            _pixels[w, h] = val;
        }

        public void SetCell(int w, int h, string val)
        {
            SetPixel(w, h, val[0]);
        }
        public void Renderer(char[,] map, Units units, int currentLevel = 1, int enemiesRemaining = 0, int totalEnemies = 0)
        {
            Console.Clear();

            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    char cell = map[x, y];
                    ConsoleColor color = GetColorForChar(cell);
                    char finalChar = cell;

                    // Проверяем юниты в этой позиции
                    if (units != null)
                    {
                        Unit unitInCell = null;
                        foreach (Unit unit in units)
                        {
                            if (unit.IsAlive() && unit.Position.X == x && unit.Position.Y == y)
                            {
                                unitInCell = unit;
                                break;
                            }
                        }

                        if (unitInCell != null)
                        {
                            // Если это пуля - отрисовываем поверх всего
                            if (unitInCell is Missile)
                            {
                                finalChar = unitInCell.View[0];
                                color = GetColorForUnit(unitInCell);
                            }
                            // Если это игрок/враг и он на пустой клетке
                            else if (cell == ' ')
                            {
                                finalChar = unitInCell.View[0];
                                color = GetColorForUnit(unitInCell);
                            }
                            // Если игрок/враг на воде - не отрисовываем (вода имеет приоритет)
                        }
                    }

                    Console.ForegroundColor = color;
                    Console.Write(finalChar);
                }

                Console.WriteLine();
            }
            RenderLevelStats(currentLevel, enemiesRemaining, totalEnemies);
        }

        /// Отрисовывает статистику уровня
        /// </summary>
        private void RenderLevelStats(int currentLevel, int enemiesRemaining, int totalEnemies)
        {
            Console.WriteLine("══════════════════════════════════════════");
            Console.WriteLine($"🎯 Уровень: {currentLevel} | 🎯 Врагов: {enemiesRemaining}/{totalEnemies}");

            // Прогресс-бар для визуализации
            if (totalEnemies > 0)
            {
                double progress = (double)(totalEnemies - enemiesRemaining) / totalEnemies;
                RenderProgressBar(progress);
            }

            Console.WriteLine("⚡ Управление: Стрелки - Движение | Пробел - Огонь | ESC - Меню");
            Console.WriteLine("══════════════════════════════════════════");
        }

        // Отрисовывает прогресс-бар уничтожения врагов
        /// </summary>
        private void RenderProgressBar(double progress)
        {
            int barLength = 20;
            int filledLength = (int)(barLength * progress);

            Console.Write("Прогресс: [");
            Console.ForegroundColor = ConsoleColor.Green;
            for (int i = 0; i < filledLength; i++)
            {
                Console.Write("█");
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            for (int i = filledLength; i < barLength; i++)
            {
                Console.Write("░");
            }
            Console.ResetColor();
            Console.WriteLine($"] {(int)(progress * 100)}%");
        }

        public void Clear()
        {
            for (int w = 0; w < _width; w++)
            {
                for (int h = 0; h < _height; h++)
                {
                    _previousPixels[w, h] = ' ';
                    _pixels[w, h] = ' ';
                }
            }
            Console.Clear();
            ApplyColorTheme();
        }
    }
}
