using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public abstract class Unit
    {

        public Vector2 Position { get; protected set; }

        private IRenderer _renderer;

        bool _alive = true;
      
        public string View { get; protected set; } // Добавлено публичное свойство

        public event Action Death;

        public Unit(Vector2 startPosition, string view, IRenderer renderer)
        {
            Position = startPosition;
            View = view;
            _renderer = renderer;
        }
        public virtual bool TryMoveLeft()
        {
            return TryChangePosition(new Vector2(Position.X - 1, Position.Y));
        }
        public virtual bool TryMoveRight()
        {
            return TryChangePosition(new Vector2(Position.X + 1, Position.Y));
        }
        public virtual bool TryMoveUp()
        {
            return TryChangePosition(new Vector2(Position.X, Position.Y - 1));

        }
        public virtual bool TryMoveDown()
        {

            return TryChangePosition(new Vector2(Position.X, Position.Y + 1));

        }
        
        public virtual bool IsAlive()
        {
            return _alive;
        }
        protected virtual void OnDeath()
        {
            Death?.Invoke();
            _alive = false;
        }


        protected virtual bool TryChangePosition(Vector2 newPosition)
        {
            char[,] currentMap = LevelModel.GetInstance().GetMap();
            if (currentMap == null) return false;

            // Проверяем границы
            if (newPosition.X < 0 || newPosition.X >= currentMap.GetLength(1) ||
                newPosition.Y < 0 || newPosition.Y >= currentMap.GetLength(0))
            {
                return false;
            }

            // Проверяем стену
            if (currentMap[newPosition.X, newPosition.Y] == '█')
            {
                return false;
            }

            Position = newPosition;
            return true;
        }
        public abstract void Update();

    }
    public enum UnitType
    {
        Player,
        Enemy,
        Missile,
        None
    }
}
