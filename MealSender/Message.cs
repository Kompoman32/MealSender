using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealSender
{
    /// <summary>
    /// Формат сообщений между сайтами
    /// </summary>
    public struct Message
    {
        /// <summary>
        /// Кто шлёт сообщение
        /// </summary>
        public string From;

        /// <summary>
        /// Цель сообщения
        /// </summary>
        public CodeType Code;

        /// <summary>
        /// Данные сообщения
        /// waveCheck - ???
        /// sendMsgTo - путь через '_', время занятия "столика"
        ///         пример A_B_C_100
        /// </summary>
        public string Info;

        public Message(string from, string code, string info)
        {
            From = from;
            Code = (CodeType)Enum.Parse(typeof(CodeType), code);
            Info = info;
        }

        public override string ToString()
        {
            return From + ServerInfo.delimeter +
                   Enum.GetName(typeof(CodeType), Code) + ServerInfo.delimeter +
                   Info;
        }
    }
}
