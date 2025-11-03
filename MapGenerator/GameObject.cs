using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public class GameObject
    {
        public Vector2 Position;
        public char View;

        StateOfTheObject condition;

      
        //public List<TheWall> wall;
        public GameObject(Vector2 _position,char _view,StateOfTheObject _condition)
        {
            Position = _position;
            View = _view;
            condition = _condition;
        }

        public virtual void Update() { }


        //public bool CheckCollision(GameObject other)
        //{
        //    return CollisionChecker.CheckRectangleCollision(this.Bounds, other.Bounds);
        //}
    }
    public enum StateOfTheObject
    {
        Damaged,
        Untouched,
        None
    }
}
