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
                        //TODO: вывести на экран сообщение о завершении работы
                        serverInfo.sendToDisplayAction.Invoke(CodeType.sendMsgTo);
                        serverInfo.SendCustomerTo = null;
                        serverInfo.WaitForCompleteBalance = false;
                    }
                    break;

                /// getJobFrom - по этому коду получаем работу от некоего кафе
                /// В Info - путь через '_' и id работы
                case (CodeType.getJobFrom):
                    {
                        //TODO: отправить тому, у кого наименьшая нагрузка

                        if (msg.Info == "")
                        {
                            serverInfo.sendToDisplayAction.Invoke(CodeType.getJobFrom);
                            serverInfo.SendCustomerTo = null;
                            serverInfo.WaitForCompleteBalance = false;
                        }
                        else
                        {
                            var customer = Customer.Convert(msg.Info);

                            var to = serverInfo.SendCustomerTo;

                            //var tempCafe = new Cafe("", 0, 0, serverInfo.currentCafeInfos);
                            //string pathToCafe = tempCafe.GetPathToChild(to.Name);

                            //var strings = pathToCafe.Split('_').ToList();

                            //var nameToSend = strings[0];
                            //strings.RemoveAt(0);

                            //pathToCafe = "";

                            //foreach (var s in strings)
                            //{
                            //    pathToCafe += s;
                            //}

                            Message msgGetJobFrom = new Message(serverInfo.Name, CodeType.sendMsgTo.ToString(), customer.ToString());
                            serverInfo.sendMessage(msgGetJobFrom.ToString(), to.Name);
                        }

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

            Message message = new Message(serverInfo.Name, CodeType.sendMsgTo.ToString(), newWay + info);

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
