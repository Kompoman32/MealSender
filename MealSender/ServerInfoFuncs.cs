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
        /// ����� ��� �������� ������� �� ������ ���������
        /// </summary>
        public void UnderstandingWhatShouldIDo(Message msg)
        {
            switch (msg.Code)
            {
                /// waveCheck - ��������� �����, ��� ����� ������ � ��������
                /// � Info - 
                case (CodeType.waveCheck):
                    {
                        
                        //��������� �����/���������� ����� ������
                        serverInfo.ProcessWavesMessages(msg);
                    }
                    break;

                /// sendMsgTo - ���������� ��������� �� ������ �� Info
                /// � Info - ���� ����� '_', ����� ������� "�������" � id ������
                /// 
                ///         ������ A_B_C_:100_id
                case (CodeType.sendMsgTo):
                    {
                        //TODO: ������� �� ����� ��������� � ���������� ������
                        serverInfo.sendToDisplayAction.Invoke(CodeType.sendMsgTo);
                        serverInfo.SendCustomerTo = null;
                        serverInfo.WaitForCompleteBalance = false;
                    }
                    break;

                /// getJobFrom - �� ����� ���� �������� ������ �� ������� ����
                /// � Info - ���� ����� '_' � id ������
                case (CodeType.getJobFrom):
                    {
                        //TODO: ��������� ����, � ���� ���������� ��������

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

        //info - ���-��, ��� �� ����� �������� �������
        //message - ������ ���������
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
        /// ��������� ���������� ��� �������� �����.
        /// </summary>
        /// <param name="msg">���������� ��������� �� ����</param>
        /// <returns>��������� ��� �������� �����</returns>
        public string GetInfoForChild(Message msg)
        {
            //����� ��������� ��������� �� ���������� (��������), 
            //������� ����� ��������� ��� ������ ����������
            //serverInfo.messageToFather = new Message(serverInfo.Name, "waveCheck", "");

            /// � Info - ���� c ������������ '_'
            //msg.Info += serverInfo + "_";
            return msg.Info;
        }

        /// <summary>
        /// ��������� ���������� ��� ������������� ����.
        /// </summary>
        /// <param name="msg">���������� ���������</param>
        /// <returns>��������� ��� ������������� �����</returns>
        public string GetInfoForFather()
        {
            /// � Info - ���� �� ������ (��������� �� ��������) + �� ��������
            var cafeInfos = serverInfo.waveInfo;

            string str = "";
            foreach(var c in cafeInfos)
            {
                str += c.ToString() + "\n";
            }


            return str;
        }

        /// <summary>
        /// ���������� ���� �������� ����.
        /// </summary>
        /// <param name="msg">���������� ��������</param>
        public void UpdateInfo(Message msg)
        {
            //��������� ��������� ��� ����������
            serverInfo.waveInfo.Add(Cafe.Convert(msg.Info));
        }
    }
}
