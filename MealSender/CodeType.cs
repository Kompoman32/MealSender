using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealSender
{
    /// <summary>
    /// Цель сообщения
    /// </summary>
    public enum CodeType
    {
        /// <summary>
        /// запускаем волну, для сбора данных о нагрузке
        /// </summary>
        waveCheck,

        /// <summary>
        /// отправляем сообщение по адресу из Info
        /// Формат всего сообщения
        /// отКого||код||путь:idРаботы_времяРаботы
        /// Формат пути
        /// узелА_узелБ_узелВ...
        /// </summary>
        sendMsgTo,

        /// <summary>
        /// в info адрес и id 
        /// адрес - у кого забрать работу
        /// id - какую работу
        /// </summary>
        getJobFrom,
    }
}
