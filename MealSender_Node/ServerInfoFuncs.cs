using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using MealSender_Node;

namespace MealSender
{
    public class ServerInfoFuncs
    {
        // MESSAGE TEMPLATE: FROM_KEY||CODE_KEY||INFO
        // INFO TEMPLATE: 
        //       waveCheck: pointA_pointB_...pointZ_
        //       sendMsgTo: =msgInfoFromA=msgInfoFromB=msgInfoFromC...

        ServerInfo serverInfo;

        int capacity;
        //List<Thread> jobsThreads;
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
                        
                        //Запускаем волну/отправляем волну дальше
                        serverInfo.SendingMessages(msg);
                    }
                    break;

                /// sendMsgTo - отправляем сообщение по адресу из Info
                /// В Info - путь через '_', время занятия "столика" и id работы
                /// 
                ///         пример A_B_C_:id_время
                case (CodeType.sendMsgTo):
                    {
                        var customer = Customer.Convert(msg.Info);

                        serverInfo.cafeInfo.Add(customer);

                        serverInfo.sendMessage(msg.From, new Message(serverInfo.Name, CodeType.sendMsgTo.ToString(), "").ToString());
                        //List<string> way = infoAll[0].Split('_').ToList();

                        //int i = 0;

                        ////Ищем данный сайт в списке-пути
                        //while (!way[i].Equals(serverInfo.Name))
                        //    i++;`

                        //if (way.Count - i != 1)
                        //{
                        //    //Сохранили следующий
                        //    string targetServer = way[i + 1];

                        //    serverInfo.sendMessage(msg.ToString(), targetServer);
                        //}
                        //else
                        //{
                        //    /*
                        //    if (capacity - jobsThreads.Count > 0)
                        //    {
                        //        AddJob(infoAll[0], infoAll[1].Split('_')[0], infoAll[1].Split('_')[1]);
                        //    }*/
                        //    if (capacity - jobsQueue.Count > 0)
                        //    {
                        //        AddJob(infoAll[0], infoAll[1].Split('_')[0], infoAll[1].Split('_')[1]);
                        //    }
                        //    else
                        //    {
                        //        ReturnJob(infoAll[0], infoAll[1].Split('_')[0]);
                        //    }
                        //}
                    }
                    break;


                case (CodeType.getJobFrom):
                    {

                        var customer = serverInfo.cafeInfo.GetMaxCustomer();

                        var info = "";

                        if (customer != null)
                        {
                            info = customer.ToString();
                        }

                        serverInfo.sendMessage(msg.From, new Message(serverInfo.Name, CodeType.getJobFrom.ToString(), info).ToString());
                        serverInfo.cafeInfo.Remove(customer);
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
                        //    //string jobInfo = infoAll[0] + "→" + infoAll[1].Split('_')[0] + "→" + infoAll[1].Split('_')[1];
                        //    if (meal) 
                        //    {
                        //        jobsQueue.Remove(jobInfo);
                        //        ReturnJob(infoAll[0], infoAll[1].Split('_')[0]);
                        //    }
                        //    else
                        //        ;//nothing
                        //}
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
            /// В Info - путь c разделителем '_'
            return new Message(serverInfo.Name, CodeType.waveCheck.ToString(), "").ToString();
        }

        /// <summary>
        /// Получение информации для родительского узла.
        /// </summary>
        /// <param name="msg">полученное сообщение</param>
        /// <returns>Сообщение для родительского узлов</returns>
        public Message GetInfoForFather()
        {
            /// В Info - пути до вершин (сообщения от потомков) + их нагрузка

            var info = serverInfo.messageToFather.Info;

            while (info.Contains(";;"))
            {
                info = info.Replace(";;", ";");
            }

            info = info.Trim().Trim(';');

            serverInfo.messageToFather.Info = $"({info})";

            return serverInfo.messageToFather;
        }

        /// <summary>
        /// Обновление инфы текущего узла.
        /// </summary>
        /// <param name="msg">полученное соощение</param>
        public void UpdateInfo(Message msg)
        {
            //Обновляем сообщение для инициатора

            var info = serverInfo.messageToFather.Info;

            info += $";{msg.Info}";

            serverInfo.messageToFather.Info = info;
        }
    }
}
