using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;

namespace MealSender
{
    public class ServerInfoFuncs
    {
        // MESSAGE TEMPLATE: FROM_KEY||CODE_KEY||INFO
        // INFO TEMPLATE: 
        //       waveCheck: pointA_pointB_...pointZ_
        //       sendMsgTo: =msgInfoFromA=msgInfoFromB=msgInfoFromC...

        ServerInfo serverInfo;

        public ServerInfoFuncs(ServerInfo serverInfo)
        {
            this.serverInfo = serverInfo;
        }

        /// <summary>
        /// Метод для принятия решения на основе сообщения
        /// </summary>
        public void UnderstandingWhatShouldIDo(Message msg)
        {
            switch (msg.Code)
            {
                /// waveCheck - запускаем волну, для сбора данных о нагрузке
                /// В Info - 
                case ("waveCheck"):
                    {
                        //TODO либо переработать этот метод, либо здесь написать что-то для волны:
                        //Запускаем волну/отправляем волну дальше
                        serverInfo.SendingMessages();
                    }
                    break;

                /// sendMsgTo - отправляем сообщение по адресу из Info
                /// В Info - путь через '_', время занятия "столика"
                ///         пример A_B_C_100
                case ("sendMsgTo"):
                    {
                        List<string> way = msg.Info.Split('_').ToList();
                        int i = 0;

                        if (way.Count != 2)
                        {
                            //Ищем данный сайт в списке-пути
                            while (!way[i].Equals(serverInfo.Name))
                                i++;

                            //Сохранили следующий
                            string targetServer = way[i + 1];

                            //Собираем новую инфу                            
                            way.RemoveAt(0);
                            msg.Info = string.Concat(way);

                            serverInfo.sendMessage(msg.ToString(), targetServer);
                        }
                        else
                        {
                            //TODO: добавить job (занять поток, занять место хз я) по обслуживанию клиента на заданное время
                        }
                    }
                    break;

            }
        }

       

        /// <summary>
        /// Получение информации для дочерних узлов.
        /// </summary>
        /// <param name="msg">полученное сообщение от отца</param>
        /// <returns>Сообщение для дочерних узлов</returns>
        public string GetInfoForChild(Message msg)
        {
            //После получения сообщения от инициатора (родителя), 
            //создаем новое сообщение для ответа инициатору
            serverInfo.messageToFather = new Message(serverInfo.Name, "waveCheck", "");

            /// В Info - путь c разделителем '_'
            msg.Info += serverInfo + "_";
            return msg.Info;
        }

        /// <summary>
        /// Получение информации для родительского узла.
        /// </summary>
        /// <param name="msg">полученное сообщение</param>
        /// <returns>Сообщение для родительского узлов</returns>
        public string GetInfoForFather()
        {
            /// В Info - пути до вершин (сообщения от потомков) + их нагрузка
            return serverInfo.messageToFather.Info;
        }

        /// <summary>
        /// Обновление инфы текущего узла.
        /// </summary>
        /// <param name="msg">полученное соощение</param>
        public void UpdateInfo(Message msg)
        {
            //Обновляем сообщение для инициатора
            serverInfo.messageToFather.Info += "=" + msg.Info;
        }
    }
}
