using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public class Units : IEnumerable
    {
        private List<Unit> _units = new();

        public void Add(Unit unit)
        {
            _units.Add(unit);
            Console.WriteLine($"Добавлен юнит {unit.GetType().Name}. Всего: {_units.Count}");
        }

        public void Remove(Unit unit)
        {
            bool removed = _units.Remove(unit);
            Console.WriteLine($"Попытка удалить юнит {unit.GetType().Name}: {removed}. Осталось: {_units.Count}");
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < _units.Count; i++)
            {
                yield return _units[i];
            }
        }
        public int Count()
        {
            return _units.Count;
        }

        public int Count(Func<Unit, bool> predicate)
        {
            return _units.Count(predicate);
        }
    }
}
