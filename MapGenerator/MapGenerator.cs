using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MapGenerator
{
    public class MapGenerator
    {
        private int width;
        private int height;
        public List<Vector2> visited = new List<Vector2>();
        public char[,] Map { get; private set; } // Делаем public с getter
        private Random random = new Random();
        IRenderer renderer;
        public Vector2 start = new Vector2(1, 1);
        private Vector2 target;
        public Units units = new Units();

        // Система здоровья стен
        private Dictionary<Vector2, int> wallHealth = new Dictionary<Vector2, int>();

        public MapGenerator(int width, int height, IRenderer _renderer)
        {
            this.width = width;
            this.height = height;
            this.renderer = _renderer;
            this.Map = new char[width, height]; // Инициализируем свойство Map
            this.target = new Vector2(width - 2, height - 2);

            DrawTheWallAllMap();
            Termite(start, target, this.Map);
            renderer.Renderer(Map, units);
        }

       

        private void DrawTheWallAllMap()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Map[x, y] = '█';
                    wallHealth[new Vector2(x, y)] = 2;
                }
            }
        }

        public void DamageWallAt(Vector2 position)
        {
            int x = (int)position.X;
            int y = (int)position.Y;

          

            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                Vector2 posKey = new Vector2(x, y);

                if (wallHealth.ContainsKey(posKey))
                {
                    wallHealth[posKey]--;
                   

                    if (wallHealth[posKey] <= 0)
                    {
                        Map[x, y] = ' ';
                        wallHealth.Remove(posKey);
                        
                    }
                    else if (wallHealth[posKey] == 1)
                    {
                        Map[x, y] = '▒';
                       
                    }

                    // СРАЗУ обновляем LevelModel
                    LevelModel.SetMap(this.Map);
                }
                else
                {
                  
                }
            }
        }

        // Остальные методы без изменений...
        private List<Direction> GetPossibleDirections(int x, int y)
        {
            var directions = new List<Direction>();

            if (CanMove(x, y, Direction.Up)) directions.Add(Direction.Up);
            if (CanMove(x, y, Direction.Down)) directions.Add(Direction.Down);
            if (CanMove(x, y, Direction.Left)) directions.Add(Direction.Left);
            if (CanMove(x, y, Direction.Right)) directions.Add(Direction.Right);

            return directions;
        }

       
        private bool CanMove(int x, int y, Direction direction)
        {
            var (dx, dy) = GetDirectionVector(direction);

            // Проверяем позицию через 2 шага (пропуская стену)
            int checkX = x + dx * 2;
            int checkY = y + dy * 2;

            return IsValidPosition(checkX, checkY) &&
                   !visited.Contains(new Vector2(checkX, checkY)) &&
                   Map[checkX, checkY] == '█';
        }

       
        private (int dx, int dy) GetDirectionVector(Direction direction)
        {
            return direction switch
            {
                Direction.Up => (0, -1),    // Вверх: y уменьшается
                Direction.Down => (0, 1),   // Вниз: y увеличивается  
                Direction.Left => (-1, 0),  // Влево: x уменьшается
                Direction.Right => (1, 0),  // Вправо: x увеличивается
                _ => (0, 0)
            };
        }

      
        private bool IsValidPosition(int x, int y)
        {
            return x > 0 && x < width - 1 && y > 0 && y < height - 1;
        }
        private void ShuffleDirections(List<Direction> directions)
        {
            for (int i = directions.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (directions[i], directions[j]) = (directions[j], directions[i]);
            }
        }

       
        private int GetMaxStepLength(int x, int y, Direction direction)
        {
            var (dx, dy) = GetDirectionVector(direction);
            int maxSteps = 0;

            for (int steps = 1; steps <= Math.Max(width, height); steps++)
            {
                // Позиция стены
                int wallX = x + dx * (steps * 2 - 1);
                int wallY = y + dy * (steps * 2 - 1);

                // Конечная позиция
                int finalX = x + dx * steps * 2;
                int finalY = y + dy * steps * 2;

                if (!IsValidPosition(wallX, wallY) || !IsValidPosition(finalX, finalY) ||
                    visited.Contains(new Vector2(finalX, finalY)) ||
                    Map[finalX, finalY] != '█')
                {
                    break;
                }
                maxSteps = steps;
            }

            int maxAllowedSteps = Math.Min(width, height) / 3 + 1;
            return Math.Min(maxSteps, maxAllowedSteps);
        }
       
        private bool TryCreatePath(int startX, int startY, Direction direction, int steps)
        {
            var (dx, dy) = GetDirectionVector(direction);

            // Сначала проверяем, можно ли пройти весь путь
            for (int step = 1; step <= steps; step++)
            {
                // Промежуточная позиция (стена)
                int wallX = startX + dx * (step * 2 - 1);
                int wallY = startY + dy * (step * 2 - 1);

                // Конечная позиция (пустое пространство)
                int finalX = startX + dx * step * 2;
                int finalY = startY + dy * step * 2;

                if (!IsValidPosition(wallX, wallY) || !IsValidPosition(finalX, finalY) ||
                    visited.Contains(new Vector2(finalX, finalY)))
                {
                    return false;
                }
            }

            // Если можно пройти, то пробиваем путь
            for (int step = 1; step <= steps; step++)
            {
                // Пробиваем стену
                int wallX = startX + dx * (step * 2 - 1);
                int wallY = startY + dy * (step * 2 - 1);
                Map[wallX, wallY] = ' ';

                // Пробиваем конечную позицию
                int finalX = startX + dx * step * 2;
                int finalY = startY + dy * step * 2;
                Map[finalX, finalY] = ' ';
                visited.Add(new Vector2(finalX, finalY));
            }

            return true;
        }
       
        private (int x, int y) MoveInDirection(int startX, int startY, Direction direction, int steps)
        {
            var (dx, dy) = GetDirectionVector(direction);
            return (startX + dx * steps * 2, startY + dy * steps * 2);
        }

       
        private void Termite(Vector2 startPosition, Vector2 targetPosition, char[,] map)
        {
            visited.Add(startPosition);
            Map[(int)startPosition.X, (int)startPosition.Y] = ' '; // Используем приведение типов

            List<Direction> possibleDirections = GetPossibleDirections((int)startPosition.X, (int)startPosition.Y);
            ShuffleDirections(possibleDirections);

            foreach (var direction in possibleDirections)
            {
                int maxStep = GetMaxStepLength((int)startPosition.X, (int)startPosition.Y, direction);

                if (maxStep > 0)
                {
                    int stepLength = random.Next(1, maxStep + 1);

                    if (TryCreatePath((int)startPosition.X, (int)startPosition.Y, direction, stepLength))
                    {
                        var newPos = MoveInDirection((int)startPosition.X, (int)startPosition.Y, direction, stepLength);
                        Termite(new Vector2(newPos.x, newPos.y), targetPosition, Map);
                    }
                }
            }
        }


        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
    }

   

}


