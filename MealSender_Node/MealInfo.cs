using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealSender_Node
{
    public class Customer
    {
        public int ID;

        static Random rnd = new Random();

        public readonly int DurationInSeconds;

        public Customer(int duration)
        {
            ID = rnd.Next();
            DurationInSeconds = duration;
        }
    }

    public class MealInfo
    {
        public Queue<Customer> customers;
        int capacity;

        DateTime lastTime;

        public MealInfo(int capacity)
        {
            this.capacity = capacity;
            customers = new Queue<Customer>(capacity);
            lastTime = DateTime.Now;
        }

        public bool Add(Customer customer)
        {
            if (customers.Count >= capacity )
            {
                return false;
            }

            customers.Enqueue(customer);

            return true;
        }

        public Customer Remove(Customer customer)
        {
            var count = customers.Count;

            Customer ret = null;

            for(var i = 0; i < count; i++)
            {
                var dequeued = customers.Dequeue();
                if (customer.ID == dequeued.ID)
                {
                    ret = dequeued;
                }
                customers.Enqueue(dequeued);
            }


            return ret;
        }

        public Customer GetMaxCustomer()
        {
            var maxDuration = customers.Where(x => x != customers.Peek()).Max(x => x.DurationInSeconds);

            var ret = customers.Where(x => x != customers.Peek()).FirstOrDefault(x => x.DurationInSeconds == maxDuration);

            return ret;
        }

        public int GetDuration ()
        {
            var duration = 0;

            foreach(var c in customers)
            {
                duration += c.DurationInSeconds;
            }


            return duration;
        }
    }
}
