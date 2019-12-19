using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealSender
{
    public class Customer
    {
        private static Random rnd = new Random();

        public readonly string Id;
        public readonly int Time;

        private static string alph = "abcdefghijklmnopqrstuvwxyz1234567890".ToUpper();

        private Customer()
        {
            Time = rnd.Next(30);
            for (var i = 0;i < 20; i ++)
            {
                Id += alph[rnd.Next(alph.Length)];
            }
        }

        private Customer(string id, int time)
        {
            Time = time;
            Id = id;
        }

        public static Customer Generate()
        {
            return new Customer();
        }

        public static Customer Convert(string str)
        {
            var strings = str.Split('_');
            return new Customer(strings[0], int.Parse(strings[1]));
        }

        public override string ToString()
        {
            return $"{Id}_{Time}";
        }
    }
}
