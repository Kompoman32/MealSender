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
                case (CodeType.waveCheck):
                    {
                        //TODO либо переработать этот метод, либо здесь написать что-то для волны:
                        //Запускаем волну/отправляем волну дальше
                        serverInfo.ProcessWavesMessages(msg);
                    }
                    break;

                /// sendMsgTo - отправляем сообщение по адресу из Info
                /// В Info - путь через '_', время занятия "столика" и id работы
                /// 
                ///         пример A_B_C_:100_id
                case (CodeType.sendMsgTo):
                    {
                        //List<string> infoAll = msg.Info.Split(':').ToList();
                        //List<string> way = infoAll[0].Split('_').ToList();

                        //int i = 0;

                        ////Ищем данный сайт в списке-пути
                        //while (!way[i].Equals(serverInfo.Name))
                        //    i++;

                        //if (way.Count - i != 1)
                        //{
                        //    //Сохранили следующий
                        //    string targetServer = way[i + 1];

                        //    serverInfo.sendMessage(msg.ToString(), targetServer);
                        //}
                        //else
                        //{
                        //    AddJob(infoAll[1].Split('_')[0], infoAll[1].Split('_')[1]);
                        //}


                    }
                    break;

            }
        }

        //info - что-то, что мы хотим сообщить серверу
        //message - старое сообщение
        public Message MakeMessageToMain(string info, Message msg)
        {
            List<string> infoAll = msg.Info.Split(':').ToList();
            List<string> way = infoAll[0].Split('_').ToList();

            way.Reverse();
            string newWay = string.Concat(way);

            Message message = new Message(serverInfo.Name, "sendMsgTo", newWay + info);

            return message;
        }
       
        public string AddJob(string time, string id)
        {
            int t = int.Parse(time);
            throw new NotImplementedException();
            //return "job {id} is addded"
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
            //serverInfo.messageToFather = new Message(serverInfo.Name, "waveCheck", "");

            /// В Info - путь c разделителем '_'
            //msg.Info += serverInfo + "_";
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
            var cafeInfos = serverInfo.waveInfo;

            string str = "";
            foreach(var c in cafeInfos)
            {
                str += c.ToString() + "\n";
            }


            return str;
        }

        /// <summary>
        /// Обновление инфы текущего узла.
        /// </summary>
        /// <param name="msg">полученное соощение</param>
        public void UpdateInfo(Message msg)
        {
            //Обновляем сообщение для инициатора
            serverInfo.waveInfo.Add(Cafe.Convert(msg.Info));
        }
    }
}
