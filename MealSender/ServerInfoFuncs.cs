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
                        //TODO ���� ������������ ���� �����, ���� ����� �������� ���-�� ��� �����:
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
                        //List<string> infoAll = msg.Info.Split(':').ToList();
                        //List<string> way = infoAll[0].Split('_').ToList();

                        //int i = 0;

                        ////���� ������ ���� � ������-����
                        //while (!way[i].Equals(serverInfo.Name))
                        //    i++;

                        //if (way.Count - i != 1)
                        //{
                        //    //��������� ���������
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

        //info - ���-��, ��� �� ����� �������� �������
        //message - ������ ���������
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
