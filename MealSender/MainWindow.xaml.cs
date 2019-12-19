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

            //currentCafeInfos = new List<Cafe>() { new Cafe("Дубай", 4, 5), new Cafe("Номер 1", 1, 5), new Cafe("Дубай2", 4, 5) };

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
            _info.sendMessagesToChilds($"{_info.Name}{ServerInfo.delimeter}"
                                         + $"{Enum.GetName(typeof(CodeType), CodeType.waveCheck)}{ServerInfo.delimeter}"
                                         + $"Hello");
        }

        private void BalanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentCafeInfos == null)
            {
                MessageBox.Show("Нечего балансировать, сначала соберите информацию");
            }
            else
            {
                switch (currentCafeInfos.Count)
                {
                    case 0:
                        MessageBox.Show("Нечего балансировать, у тебя нет кафе :(");
                        break;
                    case 1:
                        MessageBox.Show("Нечего балансировать, пффф, у тебя одно кафе, ты шо?");
                        break;
                    default:
                        var answer = Balance(currentCafeInfos);

                        var result = MessageBox.Show($"Нужно отправить клиента {answer.Item1.Name} в кафе {answer.Item2.Name}\n" +
                            $"Выполнить?",
                            "Балансировка", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            _info.SendCustomerFromTo(answer.Item1, answer.Item2);
                        }
                        break;
                }
            }



            
            
        }

        private Tuple<Cafe, Cafe> Balance(List<Cafe> cafes)
        {
            var sortedCafes = cafes.SelectMany(x=> x.GetChilds()).OrderBy((x) => x.fullnessPercent);


            var answer = new Tuple<Cafe, Cafe>(cafes.Last(), cafes.First());

            return answer;
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

        List<Cafe> currentCafeInfos;

        public void SendToScreen(List<Cafe> cafes)
        {
            Dispatcher.Invoke(() =>
            {
                currentCafeInfos = cafes;
                foreach (var c in cafes)
                {
                    textBlock.Text += cafes.ToString() + "\n";
                }
            });
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
