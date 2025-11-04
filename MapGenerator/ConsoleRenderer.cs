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

        public ConsoleRenderer()
        {
            _width = Console.WindowWidth;
            _height = Console.WindowHeight;
            _pixels = new char[_width, _height];
            _previousPixels = new char[_width, _height];
            Console.CursorVisible = false;
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
            int height = map.GetLength(0);
            int width = map.GetLength(1);

            Console.Clear();

            var sb = new System.Text.StringBuilder();

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
                        renderBuffer[unit.Position.X, unit.Position.Y] = unit.View[0];
                    }
                }
            }

            // Выводим буфер
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    sb.Append(renderBuffer[x, y]);
                }
                if (y < height - 1)
                    sb.AppendLine();
            }

            Console.Write(sb.ToString());
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
        }
    }
}
