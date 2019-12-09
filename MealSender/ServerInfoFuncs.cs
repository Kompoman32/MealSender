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
                case ("waveCheck"):
                    {
                        //TODO ���� ������������ ���� �����, ���� ����� �������� ���-�� ��� �����:
                        //��������� �����/���������� ����� ������
                        serverInfo.SendingMessages();
                    }
                    break;

                /// sendMsgTo - ���������� ��������� �� ������ �� Info
                /// � Info - ���� ����� '_', ����� ������� "�������"
                ///         ������ A_B_C_100
                case ("sendMsgTo"):
                    {
                        List<string> way = msg.Info.Split('_').ToList();
                        int i = 0;

                        if (way.Count != 2)
                        {
                            //���� ������ ���� � ������-����
                            while (!way[i].Equals(serverInfo.Name))
                                i++;

                            //��������� ���������
                            string targetServer = way[i + 1];

                            //�������� ����� ����                            
                            way.RemoveAt(0);
                            msg.Info = string.Concat(way);

                            serverInfo.sendMessage(msg.ToString(), targetServer);
                        }
                        else
                        {
                            //TODO: �������� job (������ �����, ������ ����� �� �) �� ������������ ������� �� �������� �����
                        }
                    }
                    break;

            }
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
            serverInfo.messageToFather = new Message(serverInfo.Name, "waveCheck", "");

            /// � Info - ���� c ������������ '_'
            msg.Info += serverInfo + "_";
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
            return serverInfo.messageToFather.Info;
        }

        /// <summary>
        /// ���������� ���� �������� ����.
        /// </summary>
        /// <param name="msg">���������� ��������</param>
        public void UpdateInfo(Message msg)
        {
            //��������� ��������� ��� ����������
            serverInfo.messageToFather.Info += "=" + msg.Info;
        }
    }
}
