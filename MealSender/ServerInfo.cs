using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;

namespace MealSender
{
    public class ServerInfo
    {
        string name;
        public string Name => name;

        string[] allServers;
        string[] childServers;
        public string[] AllServers => allServers;
        public string[] ChildServers => childServers;


        public string father;
        public string Father {
            get { return father; }
            set
            {
                father = value;
                if (value != null)
                {
                    var newList = new List<string>(allServers);
                    newList.Remove(father);
                    childServers = newList.ToArray();
                }
                else
                {
                    childServers = allServers;
                }
            }
        }

        // MESSAGE TEMPLATE: FROM_KEY||CODE_KEY||INFO
        public string MessageCode;
        public int MessageCount = 0;
        List<Message> messagesPool = new List<Message>();

        private static int CurrentHandleMailSlot;       // дескриптор мэйлслота
        private static Thread tReceiving;                       // поток для обслуживания мэйлслота
        private static Thread tSending;                       // поток для обслуживания мэйлслота
        bool _continue;
        object _lock;

        int answerBlocksCount = 3;
        string delimeter = "||";
        Regex delimeterRegex = new Regex(@"[|][|]");

        public ServerInfo(string name, string[] strings, string compName = ".")
        {
            this.name = name;
            childServers = strings;

            var mailSlotname = Name;

            _continue = true;

            // создание мэйлслота
            CurrentHandleMailSlot = DIS.Import.CreateMailslot($"\\\\{compName}\\mailslot\\{mailSlotname}", 0, DIS.Types.MAILSLOT_WAIT_FOREVER, 0);

            if (CurrentHandleMailSlot == -1)
            {
                throw new KeyNotFoundException("Сервер уже существует");
            }

            Start();
        }

        public void Start()
        {
            _continue = true;

            tReceiving = new Thread(ReceivingMessage);
            tReceiving.Start();

            tSending = new Thread(SendingMessages);
            tSending.Start();
        }

        public void Abort()
        {
            _continue = false;
            try
            {
                tReceiving.Join();
                tReceiving.Abort();
                tSending.Join();
                tSending.Abort();
                DIS.Import.CloseHandle(CurrentHandleMailSlot);            // закрываем дескриптор мэйлслота
            }
            catch
            {

            }
        }

        /// <summary>
        /// Обработчик по принятию сообщений.
        /// </summary>
        private void ReceivingMessage()
        {
            string msg;            // прочитанное сообщение
            int MailslotSize = 0;       // максимальный размер сообщения
            int lpNextSize = 0;         // размер следующего сообщения
            int MessageCount = 0;       // количество сообщений в мэйлслоте
            uint realBytesReaded = 0;   // количество реально прочитанных из мэйлслота байтов

            // входим в бесконечный цикл работы с мэйлслотом
            while (_continue)
            {
                // получаем информацию о состоянии мэйлслота
                DIS.Import.GetMailslotInfo(CurrentHandleMailSlot, MailslotSize, ref lpNextSize, ref MessageCount, 0);

                // если есть сообщения в мэйлслоте, то обрабатываем каждое из них
                if (MessageCount > 0)
                    for (int i = 0; i < MessageCount; i++)
                    {
                        byte[] buff = new byte[lpNextSize];                           // буфер прочитанных из мэйлслота байтов
                        DIS.Import.FlushFileBuffers(CurrentHandleMailSlot);      // "принудительная" запись данных, расположенные в буфере операционной системы, в файл мэйлслота
                        DIS.Import.ReadFile(CurrentHandleMailSlot, buff, (uint)lpNextSize, ref realBytesReaded, 0);      // считываем последовательность байтов из мэйлслота в буфер buff
                        msg = Encoding.Unicode.GetString(buff);                 // выполняем преобразование байтов в последовательность символов

                        bool ok = ProcessMessage(msg);

                        Thread.Sleep(50);                                      // приостанавливаем работу потока перед тем, как приcтупить к обслуживанию очередного клиента
                    }
            }
        }

        /// <summary>
        /// Обработчик отдельного сообщения и добавление в пулл.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool ProcessMessage(string text)
        {
            if (delimeterRegex.Matches(text).Count != answerBlocksCount)
            {
                return false;
            }

            string[] strings = new string[3];

            strings[0] = text.Remove(text.IndexOf(delimeter));
            text.Substring(2);
            strings[1] = text.Remove(text.IndexOf(delimeter));
            text.Substring(2);
            strings[2] = text;

            string from = strings[0], code = strings[1], info = strings[2];

            messagesPool.Add(new Message(from, code, info));

            return true;
        }

        /// <summary>
        /// Обработчик по отправке всем сообщений из пулла.
        /// </summary>
        public void SendingMessages()
        {
            lock(_lock)
            {
                while (_continue)
                {
                    Message msg;

                    if (Father == null)
                    {
                        msg = messagesPool.FirstOrDefault();
                        Father = msg.From;
                        var childInfo = GetInfoForChild(msg);

                        MessageCount++;

                        sendMessages(childInfo);
                    }
                    else
                    {
                        if (MessageCount == ChildServers.Length)
                        {
                            sendMessage(GetInfoForFather(), Father);
                            Father = null;
                            MessageCount = 0;
                            
                            return;
                        }

                        msg = messagesPool.FirstOrDefault(x => x.Code == MessageCode);
                        MessageCount++;
                    }

                    UpdateInfo(msg);
                }
            }

        }

        private void sendMessage(string text, string targetServerName)
        {
            uint BytesWritten = 0;  // количество реально записанных в мэйлслот байт
            byte[] buff = Encoding.Unicode.GetBytes(text);    // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт

            var HandleMailSlot = DIS.Import.CreateFile($"\\\\.\\mailslot\\{targetServerName}", DIS.Types.EFileAccess.GenericAll, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0); ;

            DIS.Import.WriteFile(HandleMailSlot, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);     // выполняем запись последовательности байт в мэйлслот
            DIS.Import.CloseHandle(HandleMailSlot);
        }

        public void sendMessages(string msg)
        {
            foreach (var s in ChildServers)
            {
                sendMessage(msg, s);
            }
        }

        /// <summary>
        /// Получение информации для дочерних узлов.
        /// </summary>
        /// <param name="msg">полученное сообщение от отца</param>
        /// <returns>Сообщение для дочерних узлов</returns>
        public string GetInfoForChild(Message msg)
        {

        }

        /// <summary>
        /// Получение информации для дочерних узлов.
        /// </summary>
        /// <param name="msg">полученное сообщение</param>
        /// <returns>Сообщение для родительского узлов</returns>
        public string GetInfoForFather()
        {

        }

        /// <summary>
        /// Обновление инфы текущего узла.
        /// </summary>
        /// <param name="msg">полученное соощение</param>
        public void UpdateInfo(Message msg)
        {

        }
    }
}
