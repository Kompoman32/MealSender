﻿using System;
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

        public List<string> AllServers { get; }
        public List<string> ChildServers
        {
            get
            {
                if (Father == null) return AllServers;

                List<string> list = new List<string>(AllServers.ToArray());
                list.Remove(Father);
                return list;
            }
        }


        public string father;
        public string Father
        {
            get { return father; }
            set
            {
                father = value;
            }
        }

        // MESSAGE TEMPLATE: FROM_KEY||CODE_KEY||INFO
        public CodeType MessageCode;
        public int MessageCount = 0;
        List<Message> messagesPool = new List<Message>();

        ServerInfoFuncs serverInfoFuncs;
        public Action<CodeType> sendToDisplayAction;

        private static int CurrentHandleMailSlot;       // дескриптор мэйлслота
        private static Thread tReceiving;                       // поток для обслуживания мэйлслота
        private static Thread tMain;                       // поток для обслуживания мэйлслота
        bool _continue;

        public List<Cafe> currentCafeInfos;
        public bool WaitForCompleteBalance;
        public Cafe SendCustomerTo;

        const int answerBlocksCount = 3;
        static public string delimeter = "||";
        Regex delimeterRegex = new Regex(@"\|\|");


        public List<Cafe> waveInfo;

        public ServerInfo(string name, string[] strings, Action<CodeType> sendToDisplayAction, string compName = ".")
        {
            this.name = name;

            AllServers = new List<string>(strings);

            this.sendToDisplayAction = sendToDisplayAction;

            waveInfo = new List<Cafe>();

            var mailSlotname = Name;

            _continue = true;

            // создание мэйлслота
            CurrentHandleMailSlot = DIS.Import.CreateMailslot($"\\\\{compName}\\mailslot\\{mailSlotname}", 0, DIS.Types.MAILSLOT_WAIT_FOREVER, 0);

            if (CurrentHandleMailSlot == -1)
            {
                throw new KeyNotFoundException("Сервер уже существует");
            }

            this.serverInfoFuncs = new ServerInfoFuncs(this);

            Start();
        }

        public void Start()
        {
            _continue = true;

            tReceiving = new Thread(ReceivingMessage);
            tReceiving.Start();

            tMain = new Thread(DoingThings);
            tMain.Start();
        }

        public void Abort()
        {
            _continue = false;
            try
            {
                tReceiving.Join();
                tReceiving.Abort();
                tMain.Join();
                tMain.Abort();
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
            if (delimeterRegex.Matches(text).Count != answerBlocksCount - 1)
            {
                return false;
            }

            string[] strings = new string[3];

            strings[0] = text.Remove(text.IndexOf(delimeter));
            text = text.Substring(text.IndexOf(delimeter) + 2);
            strings[1] = text.Remove(text.IndexOf(delimeter));
            text = text.Substring(text.IndexOf(delimeter) + 2);
            strings[2] = text;

            string from = strings[0], code = strings[1], info = strings[2];

            messagesPool.Add(new Message(from, code, info));

            return true;
        }

        public void DoingThings()
        {
            while (_continue)
            {
                if (messagesPool.Count == 0)
                    continue;

                Message msg = messagesPool.FirstOrDefault();

                messagesPool.Remove(msg);

                serverInfoFuncs.UnderstandingWhatShouldIDo(msg);
            }
        }

        /// <summary>
        /// Обработчик по отправке всем сообщений о волне из пулла.
        /// </summary>
        public void ProcessWavesMessages(Message msg)
        {
            MessageCount++;

            serverInfoFuncs.UpdateInfo(msg);

            if (MessageCount == AllServers.Count)
            {
                MessageCount = 0;

                currentCafeInfos = waveInfo;

                sendToDisplayAction.Invoke(CodeType.waveCheck);
                waveInfo = new List<Cafe>();
                return;
            }
        }

        public void sendMessage(string text, string targetServerName)
        {
            uint BytesWritten = 0;  // количество реально записанных в мэйлслот байт
            byte[] buff = Encoding.Unicode.GetBytes(text);    // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт

            var HandleMailSlot = DIS.Import.CreateFile($"\\\\.\\mailslot\\{targetServerName}", DIS.Types.EFileAccess.GenericAll, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0); ;

            DIS.Import.WriteFile(HandleMailSlot, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);     // выполняем запись последовательности байт в мэйлслот
            DIS.Import.CloseHandle(HandleMailSlot);
        }

        public void sendMessagesToChilds(string msg)
        {
            foreach (var s in ChildServers)
            {
                sendMessage(msg, s);
            }
        }

        public void GetCustomerFrom(Cafe from)
        {
            //List<Cafe> cafes = currentCafeInfos;
            //var tempCafe = new Cafe("", 0, 0, cafes);
            //string pathToCafe = tempCafe.GetPathToChild(from.Name);

            //var strings = pathToCafe.Split('_').ToList();

            //var nameToSend = strings.Last();


            Message msgGetJobFrom = new Message(this.name, CodeType.getJobFrom.ToString(), "");
            sendMessage(msgGetJobFrom.ToString(), from.Name);
        }
    }
}