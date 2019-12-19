using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MealSender
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _info = ReadServerInfo("servers.txt");
            Title = _info.Name;
            ShowInfo();
        }

        ServerInfo _info;

        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Файлы списка серверов (*.txt)|*.txt";
            dialog.Multiselect = false;
            dialog.ShowDialog();
        }

        private void GetInfoButton_Click(object sender, RoutedEventArgs e)
        {
            _info.sendMessages($"{_info.Name}{ServerInfo.delimeter}"
                             + $"{Enum.GetName(typeof(CodeType), CodeType.waveCheck)}{ServerInfo.delimeter}"
                             + $"Hello");
        }

        private void RefreshTextBlockButton_Click(object sender, RoutedEventArgs e)
        {
            textBlock.Text = "";
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(_info.messageToFather.Info);
        }

        private ServerInfo ReadServerInfo(string fileName)
        {
            var strings = File.ReadAllLines(fileName);
            if (strings.Length < 1)
            {
                MessageBox.Show("Ничего не найдено :(");
                return null;
            }

            var list  = strings.ToList();

            var serverName = list[0];

            list.RemoveAt(0);

            return new ServerInfo(serverName, list.ToArray(), SendToScreen);
        }

        public void SendToScreen(string msg)
        {
            lock (_info)
            {
                Dispatcher.Invoke(() =>
                {
                    textBlock.Text += $"{new DateTime().ToString("HH:MM:ss")}: Отправлено сообщение\n";
                    textBlock.Text += "---------------\n";
                    foreach (var l in msg.Split('\n'))
                    {
                        textBlock.Text += l + "\n";
                    }
                    textBlock.Text += "---------------\n";
                });
            }
        }

        private void ShowInfo()
        {
            if (_info == null) return;

            Info.Text = "";

            Info.Text += $"Name: {_info.Name}\n\n";
            Info.Text += $"Servers:\n";

            foreach (var o in _info.AllServers)
            {
                Info.Text += $"{o}\n";
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _info.Abort();
        }
    }
}
