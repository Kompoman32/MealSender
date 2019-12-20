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
                                         + $"{CodeType.waveCheck.ToString()}{ServerInfo.delimeter}"
                                         + $"Hello");
        }

        private void BalanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_info.WaitForCompleteBalance)
            {
                MessageBox.Show("Извини, я жду конца предыдущей балансировки");
                return;
            }

            if (_info.currentCafeInfos == null)
            {
                MessageBox.Show("Нечего балансировать, сначала соберите информацию");
                return;
            }

            switch (_info.currentCafeInfos.Count)
            {
                case 0:
                    MessageBox.Show("Нечего балансировать, у тебя нет кафе :(");
                    break;
                case 1:
                    MessageBox.Show("Нечего балансировать, пффф, у тебя одно кафе, ты шо?");
                    break;
                default:
                    var answer = Balance(_info.currentCafeInfos);

                    var result = MessageBox.Show($"Нужно отправить клиента {answer.Item1.Name} в кафе {answer.Item2.Name}\n" +
                        $"Выполнить?",
                        "Балансировка", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        _info.WaitForCompleteBalance = true;
                        _info.GetCustomerFrom(answer.Item1);
                    }
                    break;
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


        public void SendToScreen(CodeType info)
        {
            Dispatcher.Invoke(() =>
            {

                if (info == CodeType.sendMsgTo)
                {
                    MessageBox.Show("Балансировка закончена");
                }

                if (info == CodeType.getJobFrom)
                {
                    MessageBox.Show("с подопытного нечего брать( попробуйте ещё");
                }

                if (info == CodeType.waveCheck)
                {
                    //MessageBox.Show("Дерево собрано");
                    UpdateCafeInfo();
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

        private void JobsCountUp_Click(object sender, RoutedEventArgs e)
        {
            JobsCount.Text = (int.Parse(JobsCount.Text) + 1).ToString();
        }

        List<Customer> currentCustomers = new List<Customer>();


        private void JobsCountDown_Click(object sender, RoutedEventArgs e)
        {
            var val = int.Parse(JobsCount.Text);
            if (val != 0)
            {
                val--;
                JobsCount.Text = val.ToString();
            }
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var val = int.Parse(JobsCount.Text);
            for(var i = 0; i < val; i ++)
            {
                currentCustomers.Add(Customer.Generate());
            }

            UpdateCustomerInfo();
        }

        private void UpdateCustomerInfo()
        {
            var str = "";

            foreach(var c in currentCustomers)
            {
                str += $"{c.Id} : {c.Time}\n";
            }

            customersTextBlock.Text = str;
        }

        private void UpdateCafeInfo()
        {
            var str = "";

            foreach (var c in _info.currentCafeInfos.SelectMany(x => {
                var childs = x.GetChilds();
                childs.Add(x);
                return childs;
            }))
            {
                str += $"{c.Name} : {c.CustomerCount}/{c.Capacity}\n";
            }

            CafeTextBlock.Text = str;
        }

        private void SendCustomersButton_Click(object sender, RoutedEventArgs e)
        {
            if (_info.currentCafeInfos == null)
            {
                MessageBox.Show("Сначала собери инфу, лол");
                return;
            }

            var currentCafeInfos = new Cafe[_info.currentCafeInfos.Count];
            _info.currentCafeInfos.CopyTo(currentCafeInfos);

            var maxCustomers = currentCafeInfos.Sum(x => x.Capacity - x.CustomerCount);
            if (maxCustomers == 0)
            {
                MessageBox.Show("Всё заполнено :( собери инфу, вдруг что-то освободилось");
                return;
            }

            var curCustomers = currentCustomers.GetRange(0, Math.Min(maxCustomers, currentCustomers.Count));

            if (curCustomers.Count == 0)
            {
                MessageBox.Show("Некого отправлять :( добавь клиентов");
                return;
            }

            foreach(var c in curCustomers)
            {
                currentCustomers.Remove(c);
                var sortedCafes = currentCafeInfos.SelectMany(x => {
                    var childs = x.GetChilds();
                    childs.Add(x);
                    return childs;
                    }).OrderBy((x) => x.fullnessPercent);
                var target = sortedCafes.First();

                target.CustomerCount++;

                _info.sendMessage(new Message(_info.Name, CodeType.sendMsgTo.ToString(), c.ToString()).ToString(), target.Name);
            }

            UpdateCustomerInfo();
            UpdateCafeInfo();
        }
    }
}
