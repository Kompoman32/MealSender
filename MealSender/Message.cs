using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealSender
{
    public struct Message
    {
        public string From;
        public string Code;
        public string Info;

        public Message(string from, string code, string info)
        {
            From = from;
            Code = code;
            Info = info;
        }
    }
}
