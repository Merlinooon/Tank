using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MapGenerator.Enemy;
using static MapGenerator.MapGenerator;

namespace MapGenerator
{
    public class Enemy:Unit
    {
        public event Action Death;
        char[,] _map;
        private Random random = new Random(); // Инициализирован Random

        //public Vector2 Position { get; private set; }
        private IRenderer _renderer;

        public List<Vector2> visited = new List<Vector2>();
        private Vector2 _target;
        LevelModel LevelModel { get; set; }
        public Enemy(Vector2 startPosition, IRenderer renderer, IMoveInput input,LevelModel levelModel) : base(startPosition, "▼", renderer)
        {
            _renderer = renderer;
            //Position = startPosition;
            this.LevelModel=levelModel;
            _map=LevelModel.GetMap();
           
            _target = LevelModel.Player?.Position ?? new Vector2(1, 1);
        }
        public override void Update()
        {

            if(LevelModel.Player.IsAlive())
            {
                _target = LevelModel.Player.Position;

                var directions = GetPossibleDirections();
                if (directions.Count > 0)
                {
                    // Выбираем наилучшее направление к игроку
                    Direction bestDirection = GetBestDirection(directions);
                    TryMove(bestDirection);
                }
                foreach (Unit unit in LevelModel.Units)
                {
                    if (unit == this)
                        continue;
                    if (Position.Equals(unit.Position))
                    {
                        Death?.Invoke();
                    }

                }
            }
               
            
        }
        private Direction GetBestDirection(List<Direction> possibleDirections)
        {
            // Простая эвристика: выбираем направление, которое ближе к игроку
            Direction bestDir = possibleDirections[0];
            int bestDistance = int.MaxValue;

            foreach (var dir in possibleDirections)
            {
                var (dx, dy) = GetDirectionVector(dir);
                int newX = Position.X + dx;
                int newY = Position.Y + dy;

                int distance = Math.Abs(newX - _target.X) + Math.Abs(newY - _target.Y);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDir = dir;
                }
            }

            return bestDir;
        }
        private void TryMove(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    TryMoveUp();
                    break;
                case Direction.Down:
                    TryMoveDown();
                    break;
                case Direction.Left:
                    TryMoveLeft();
                    break;
                case Direction.Right:
                    TryMoveRight();
                    break;
            }
        }
        private List<Direction> GetPossibleDirections()
        {
            var directions = new List<Direction>();

            if (CanMove( Direction.Up)) directions.Add(Direction.Up);
            if (CanMove( Direction.Down)) directions.Add(Direction.Down);
            if (CanMove( Direction.Left)) directions.Add(Direction.Left);
            if (CanMove( Direction.Right)) directions.Add(Direction.Right);

            return directions;
        }

        private bool CanMove( Direction direction)
        {
            var (dx, dy) = GetDirectionVector(direction);

            
            int checkX = Position.X + dx ;
            int checkY = Position.Y + dy ;

            return IsValidPosition(checkX, checkY) &&
                   !visited.Contains(new Vector2(checkX, checkY)) &&
                   _map[checkX, checkY] == ' '; // Исправлены координаты
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

            int rows = _map.GetLength(0);
            int cols = _map.GetLength(1);
            return x >= 0 && x < rows && y >= 0 && y < cols;
        }
        private void ShuffleDirections(List<Direction> directions)
        {
            for (int i = directions.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (directions[i], directions[j]) = (directions[j], directions[i]);
            }
        }
       
      
       
      
    }
}
