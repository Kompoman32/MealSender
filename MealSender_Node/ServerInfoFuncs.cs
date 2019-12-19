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
        List<String> jobsQueue;
        //List<Thread> jobsThreads;
        public ServerInfoFuncs(ServerInfo serverInfo)
        {
            this.serverInfo = serverInfo;
            jobsQueue = new List<string>(5);
            ServerInfo.tWork = new Thread(DoJob);
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

        //info - что-то, что мы хотим сообщить серверу
        //message - старое сообщение
        public Message MakeMessageToMain(string info, Message msg)
        {
            List<string> infoAll = msg.Info.Split(':').ToList();

            string way = infoAll[0];
            List<string> reverseWay = way.Split('_').Reverse().ToList();
            string wayBack = "";
            foreach (string site in reverseWay)
                wayBack += "_" + site;
            wayBack = wayBack.Remove(0, 1);

            


            Message message = new Message(serverInfo.Name, "sendMsgTo", wayBack + info);

            return message;
        }
       
        public void AddJob(string way, string id, string time)
        {

            List<string> reverseWay = way.Split('_').Reverse().ToList();
            string wayBack = "";
            foreach (string site in reverseWay)
                wayBack += "_" + site;
            wayBack = wayBack.Remove(0, 1);

            jobsQueue.Add(wayBack + "→" + id + "→" + time);
            
            //Thread jobThread = new Thread(serverInfo.DoJob);
            //jobThread.Start(new { wayBack, id, t });

        }
        public void ReturnJob(string to, Customer customer)
        {
            //List<string> reverseWay = way.Split('_').Reverse().ToList();
            //string wayBack = "";
            //foreach (string site in reverseWay)
            //    wayBack += "_" + site;
            //wayBack = wayBack.Remove(0, 1);
            //Thread jobThread = new Thread(serverInfo.DoJob);
            //jobThread.Start(new { wayBack, id, t });
            serverInfo.sendMessage(to, new Message(serverInfo.Name, CodeType.getJobFrom.ToString(), customer.ToString()).ToString());
        }
        public void DoJob()
        {
            string way = "";
            string jobId = "";
            int time = 0;
            while (true)
            {
                if (jobsQueue.Count != 0)
                {
                    string[] info = jobsQueue[0].Split('→');
                    jobsQueue.RemoveAt(0);

                    int t = int.Parse(info[2]);

                    Thread.Sleep(time);

                    List<string> reverseWay = way.Split('_').Reverse().ToList();
                    string wayBack = "";
                    foreach (string site in reverseWay)
                        wayBack += "_" + site;
                    wayBack = wayBack.Remove(0, 1);

                    serverInfo.sendMessage(new Message(serverInfo.Name, CodeType.sendMsgTo.ToString(), wayBack + ":" + jobId).ToString(), serverInfo.Name);
                }
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

            info = info.Trim(';');

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

            var info = serverInfo.messageToFather.Info +
                "_" + jobsQueue.Count.ToString() +
                "_" + capacity.ToString();

            info += $";{msg.Info}";

            serverInfo.messageToFather.Info = info;
        }
    }
}
