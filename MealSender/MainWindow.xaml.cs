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

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO:
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

            return new ServerInfo(serverName, list.ToArray());
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
    }
}
