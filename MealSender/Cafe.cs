using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealSender
{
    public class Cafe
    {
        public string Name;
        public int CustomerCount;
        public int Capacity;

        public List<Cafe> Cafes;

        public double fullnessPercent => ((double)CustomerCount) / ((double)Capacity);

        public Cafe(string name, int customerCount, int capacity, List<Cafe> cafes)
        {
            Name = name;
            CustomerCount = customerCount;
            Capacity = capacity;
            Cafes = cafes;
        }

        // ex: (Second_23_45;(Fourth_4_8);(Third_9_9))
        public static Cafe Convert(string str)
        {
            str.Replace("\0", "");
            str.Replace(";;", ";")  ;
            if (str.Contains('(') && str.Contains(')'))
            {
                str = str.Substring(1).Remove(str.Length - 2);
            }

            var strings = str.Split(';').ToList();
            var curCafe = strings[0];
            strings.RemoveAt(0);

            var cafes = new List<Cafe>();

            foreach (var s in strings)
            {
                if (!s.Equals("") && !s.Contains("\0"))
                    cafes.Add(Cafe.Convert(s));
            }

            var cafeStrings = curCafe.Split('_');

            return new Cafe(cafeStrings[0], int.Parse(cafeStrings[1]), int.Parse(cafeStrings[2]), cafes);
        }

        public List<Cafe> GetChilds()
        {
            List<Cafe> list = new List<Cafe>();

            list.AddRange(Cafes);

            foreach(var c in Cafes)
            {
                list.AddRange(c.GetChilds());
            }

            return list;
        }

        public string GetPathToChild(string name)
        {
            var possibleCafe = Cafes.Find(x => x.Name == name);

            string path = null;
            if (possibleCafe != null)
            {
                path = possibleCafe.Name;
            }
            else 
            {
                foreach(var c in Cafes)
                {
                    path = c.GetPathToChild(name);
                }
            }


            return path == null ? null : $"{Name}_{possibleCafe.Name}";
        }

        public override string ToString()
        {
            return $"Кафе {Name}: {CustomerCount} клиентов в очереди из {Capacity}";
        }
    }
}
