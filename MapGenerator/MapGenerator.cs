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
        //private int width;
        //private int height;
        //public List<Vector2> visited = new List<Vector2>();
        //char[,] map;

        //private Random random = new Random(); // Инициализирован Random
        //IRenderer renderer;
        //public Vector2 start = new Vector2(1, 1);
        //private Vector2 target;
        //public Units units= new Units();
        ////public MapGenerator(int width, int height, IRenderer _renderer)
        ////{
        ////    this.width = width;
        ////    this.height = height;
        ////    this.renderer = _renderer;
        ////    this.map = new char[width, height];
        ////    this.target = new Vector2(width - 2, height - 2);

        ////    DrawTheWallAllMap();
        ////    Termite(start, target, this.map);
        ////    renderer.Renderer(map, units);
        ////}

        //// Добавляем список стен для управления состоянием
        //public List<TheWall> walls = new List<TheWall>();

        //public MapGenerator(int width, int height, IRenderer _renderer)
        //{
        //    this.width = width;
        //    this.height = height;
        //    this.renderer = _renderer;
        //    this.map = new char[width, height];
        //    this.target = new Vector2(width - 2, height - 2);

        //    DrawTheWallAllMap();
        //    Termite(start, target, this.map);
        //    renderer.Renderer(map, units);
        //}

        //public char[,] Map {  get { return map; } }
        //private List<Direction> GetPossibleDirections(int x, int y)
        //{
        //    var directions = new List<Direction>();

        //    if (CanMove(x, y, Direction.Up)) directions.Add(Direction.Up);
        //    if (CanMove(x, y, Direction.Down)) directions.Add(Direction.Down);
        //    if (CanMove(x, y, Direction.Left)) directions.Add(Direction.Left);
        //    if (CanMove(x, y, Direction.Right)) directions.Add(Direction.Right);

        //    return directions;
        //}

        //private bool CanMove(int x, int y, Direction direction)
        //{
        //    var (dx, dy) = GetDirectionVector(direction);

        //    // Проверяем клетку на расстоянии 2 шага (прыжок через стену)
        //    int checkX = x + dx * 2;
        //    int checkY = y + dy * 2;

        //    return IsValidPosition(checkX, checkY) &&
        //           !visited.Contains(new Vector2(checkX, checkY)) &&
        //           map[checkX, checkY] == '█'; // Исправлены координаты
        //}

        //private (int dx, int dy) GetDirectionVector(Direction direction)
        //{
        //    return direction switch
        //    {
        //        Direction.Up => (0, -1),
        //        Direction.Down => (0, 1),
        //        Direction.Left => (-1, 0),
        //        Direction.Right => (1, 0),
        //        _ => (0, 0)
        //    };
        //}

        //private bool IsValidPosition(int x, int y)
        //{
        //    return x > 0 && x < width - 1 && y > 0 && y < height - 1;
        //}

        ////private void DrawTheWallAllMap()
        ////{
        ////    for (int y = 0; y < height; y++)
        ////    {
        ////        for (int x = 0; x < width; x++)
        ////        {
        ////            wall = new TheWall(new Vector2(x,y),'█',StateOfTheObject.Untouched); // Исправлены координаты
        ////        }
        ////    }
        ////}

        //// ИСПРАВЛЕННЫЙ метод - теперь заполняет массив map
        //private void DrawTheWallAllMap()
        //{
        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            // Заполняем массив символов
        //            map[x, y] = '█';

        //            // Создаем объект стены (если нужен для логики)
        //            var wall = new TheWall(new Vector2(x, y), '█', StateOfTheObject.Untouched);
        //            walls.Add(wall);
        //        }
        //    }
        //}
        //// Добавляем метод для обновления карты на основе состояния стен
        //public void UpdateMapFromWalls()
        //{
        //    foreach (var wall in walls)
        //    {
        //        if (wall.Position.X >= 0 && wall.Position.X < width &&
        //            wall.Position.Y >= 0 && wall.Position.Y < height)
        //        {
        //            map[(int)wall.Position.X, (int)wall.Position.Y] = wall.View;
        //        }
        //    }
        //}

        //private void ShuffleDirections(List<Direction> directions)
        //{
        //    for (int i = directions.Count - 1; i > 0; i--)
        //    {
        //        int j = random.Next(i + 1);
        //        (directions[i], directions[j]) = (directions[j], directions[i]);
        //    }
        //}

        //private int GetMaxStepLength(int x, int y, Direction direction)
        //{
        //    var (dx, dy) = GetDirectionVector(direction);
        //    int maxSteps = 0;

        //    for (int steps = 1; steps <= Math.Max(width, height); steps++)
        //    {
        //        int newX = x + dx * steps * 2; // Учитываем прыжок через стену
        //        int newY = y + dy * steps * 2;

        //        if (!IsValidPosition(newX, newY) ||
        //            visited.Contains(new Vector2(newX, newY)) ||
        //            map[newX, newY] != '█') // Исправлены координаты
        //        {
        //            break;
        //        }
        //        maxSteps = steps;
        //    }

        //    int maxAllowedSteps = Math.Min(width, height) / 3 + 1;
        //    return Math.Min(maxSteps, maxAllowedSteps);
        //}

        //private bool TryCreatePath(int startX, int startY, Direction direction, int steps)
        //{
        //    var (dx, dy) = GetDirectionVector(direction);

        //    // Проверяем весь путь
        //    for (int step = 1; step <= steps; step++)
        //    {
        //        // Промежуточная позиция (стенка)
        //        int intermediateX = startX + dx;
        //        int intermediateY = startY + dy;

        //        // Конечная позиция (новая точка)
        //        int finalX = startX + dx;
        //        int finalY = startY + dy;

        //        if (!IsValidPosition(intermediateX, intermediateY) ||
        //            !IsValidPosition(finalX, finalY) ||
        //            visited.Contains(new Vector2(finalX, finalY)))
        //        {
        //            return false;
        //        }
        //    }

        //    // Прокладываем путь
        //    for (int step = 1; step <= steps; step++)
        //    {
        //        // Убираем промежуточную стенку
        //        int intermediateX = startX + dx;
        //        int intermediateY = startY + dy;
        //        map[intermediateX, intermediateY] = ' '; // Исправлены координаты

        //        // Отмечаем конечную позицию
        //        int finalX = startX + dx;
        //        int finalY = startY + dy;
        //        map[finalX, finalY] = ' '; // Исправлены координаты
        //        visited.Add(new Vector2(finalX, finalY));
        //    }

        //    return true;
        //}

        //private (int x, int y) MoveInDirection(int startX, int startY, Direction direction, int steps)
        //{
        //    var (dx, dy) = GetDirectionVector(direction);
        //    return (startX + dx * steps * 2, startY + dy * steps * 2);
        //}

        //private void Termite(Vector2 startPosition, Vector2 targetPosition, char[,] map)
        //{
        //    visited.Add(startPosition);
        //    map[startPosition.X, startPosition.Y] = ' '; // Исправлены координаты

        //    List<Direction> possibleDirections = GetPossibleDirections(startPosition.X, startPosition.Y);
        //    ShuffleDirections(possibleDirections);

        //    foreach (var direction in possibleDirections)
        //    {
        //        int maxStep = GetMaxStepLength(startPosition.X, startPosition.Y, direction);

        //        if (maxStep > 0)
        //        {
        //            int stepLength = random.Next(1, maxStep + 1);

        //            if (TryCreatePath(startPosition.X, startPosition.Y, direction, stepLength))
        //            {
        //                var newPos = MoveInDirection(startPosition.X, startPosition.Y, direction, stepLength);
        //                Termite(new Vector2(newPos.x, newPos.y), targetPosition, map);
        //            }
        //        }
        //    }
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

        // УБИРАЕМ дублирующее свойство
        // public char[,] Map { get { return map; } }

        private void DrawTheWallAllMap()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Map[y, x] = '█';
                    wallHealth[new Vector2(x, y)] = 2;
                }
            }
        }

        public void DamageWallAt(Vector2 position)
        {
            int x = (int)position.X;
            int y = (int)position.Y;

            Console.WriteLine($"DamageWallAt: позиция ({x}, {y})");

            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                Vector2 posKey = new Vector2(x, y);

                if (wallHealth.ContainsKey(posKey))
                {
                    wallHealth[posKey]--;
                    Console.WriteLine($"Стена в ({x}, {y}) - здоровье: {wallHealth[posKey]}");

                    if (wallHealth[posKey] <= 0)
                    {
                        Map[y, x] = ' ';
                        wallHealth.Remove(posKey);
                        Console.WriteLine($"Стена в ({x}, {y}) УНИЧТОЖЕНА");
                    }
                    else if (wallHealth[posKey] == 1)
                    {
                        Map[y, x] = '▒';
                        Console.WriteLine($"Стена в ({x}, {y}) полуразрушена");
                    }

                    // СРАЗУ обновляем LevelModel
                    LevelModel.SetMap(this.Map);
                }
                else
                {
                    Console.WriteLine($"Нет стены в ({x}, {y})");
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

            int checkX = x + dx * 2;
            int checkY = y + dy * 2;

            return IsValidPosition(checkX, checkY) &&
                   !visited.Contains(new Vector2(checkX, checkY)) &&
                   Map[checkY, checkX] == '█';
        }

        private (int dx, int dy) GetDirectionVector(Direction direction)
        {
            return direction switch
            {
                Direction.Up => (0, -1),
                Direction.Down => (0, 1),
                Direction.Left => (-1, 0),
                Direction.Right => (1, 0),
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
                int newX = x + dx * steps * 2;
                int newY = y + dy * steps * 2;

                if (!IsValidPosition(newX, newY) ||
                    visited.Contains(new Vector2(newX, newY)) ||
                    Map[newY, newX] != '█')
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

            for (int step = 1; step <= steps; step++)
            {
                int intermediateX = startX + dx;
                int intermediateY = startY + dy;

                int finalX = startX + dx;
                int finalY = startY + dy;

                if (!IsValidPosition(intermediateX, intermediateY) ||
                    !IsValidPosition(finalX, finalY) ||
                    visited.Contains(new Vector2(finalX, finalY)))
                {
                    return false;
                }
            }

            for (int step = 1; step <= steps; step++)
            {
                int intermediateX = startX + dx;
                int intermediateY = startY + dy;
                Map[intermediateY, intermediateX] = ' ';

                int finalX = startX + dx;
                int finalY = startY + dy;
                Map[finalY, finalX] = ' ';
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
            Map[startPosition.Y, startPosition.X] = ' ';

            List<Direction> possibleDirections = GetPossibleDirections(startPosition.X, startPosition.Y);
            ShuffleDirections(possibleDirections);

            foreach (var direction in possibleDirections)
            {
                int maxStep = GetMaxStepLength(startPosition.X, startPosition.Y, direction);

                if (maxStep > 0)
                {
                    int stepLength = random.Next(1, maxStep + 1);

                    if (TryCreatePath(startPosition.X, startPosition.Y, direction, stepLength))
                    {
                        var newPos = MoveInDirection(startPosition.X, startPosition.Y, direction, stepLength);
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


