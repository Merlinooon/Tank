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
                { '█', ConsoleColor.Red },    // Полная стена
                { '▒', ConsoleColor.Yellow },      // Полуразрушенная стена
            
                // Игрок и враги
                { '▲', ConsoleColor.Green },       // Игрок (вверх)
                { '▼', ConsoleColor.Red },         // Враг (вниз)
                { '►', ConsoleColor.Green },       // Игрок (вправо)
                { '◄', ConsoleColor.Green },       // Игрок (влево)
            
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

        public void Renderer(char[,] map, Units units = null)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            Console.Clear();

            // Создаем временный буфер для отрисовки
            char[,] renderBuffer = new char[width, height];

            // Копируем карту в буфер
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    renderBuffer[x, y] = map[x, y];
                }
            }

            // Отрисовываем юниты поверх карты
            if (units != null)
            {
                foreach (Unit unit in units)
                {
                    if (unit.Position.X >= 0 && unit.Position.X < width &&
                        unit.Position.Y >= 0 && unit.Position.Y < height)
                    {
                        // Не перезаписываем важные элементы карты
                        char currentCell = renderBuffer[unit.Position.X, unit.Position.Y];
                        if (currentCell == ' ' || currentCell == '·' || currentCell == '●')
                        {
                            renderBuffer[unit.Position.X, unit.Position.Y] = unit.View[0];
                        }
                    }
                }
            }

            // Выводим буфер с цветами
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char cell = renderBuffer[x, y];
                    ConsoleColor color = GetColorForChar(cell);

                    // Особые случаи для юнитов
                    if (units != null)
                    {
                        foreach (Unit unit in units)
                        {
                            if (unit.Position.X == x && unit.Position.Y == y)
                            {
                                color = GetColorForUnit(unit);
                                break;
                            }
                        }
                    }

                    Console.ForegroundColor = color;
                    Console.Write(cell);
                }
                Console.WriteLine();
            }

            
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
