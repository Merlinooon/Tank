using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public class TheWall:GameObject
    {
       

        public StateOfTheObject condition;

        public TheWall(Vector2 _position,char _view, StateOfTheObject _condition) :base(_position, _view,_condition)
        { 
            
        }
        public override void Update()
        {
            if (condition == StateOfTheObject.Damaged)
            {
                View = '▒'; // Полуразрушенная стена
            }
            else if (condition == StateOfTheObject.Untouched)
            {
                View = '█'; // Целая стена
            }
            else
            {
                View = ' '; // Пустое пространство
            }
        }

        // Метод для нанесения урона стене
        public void TakeDamage()
        {
            if (condition == StateOfTheObject.Untouched)
            {
                condition = StateOfTheObject.Damaged;
                Update(); // Обновляем View
            }
        }
    }
}
