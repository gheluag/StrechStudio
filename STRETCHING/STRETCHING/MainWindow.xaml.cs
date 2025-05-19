using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using STRETCHING.Entities;

namespace STRETCHING
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Administrators _currentAdmin;
        public MainWindow(Administrators administrators)
        {
            InitializeComponent();
            _currentAdmin = administrators;
            if (_currentAdmin.Role == 2) adminBtn.Visibility = Visibility.Visible;
            mainFrame.Navigate(new Pages.MainPage(_currentAdmin));
        }

        private void clientsBtn_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Pages.ClientsPage(_currentAdmin));
        }

        private void mainBtn_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Pages.MainPage(_currentAdmin));
        }

        private void tasksBtn_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Pages.TasksPage(_currentAdmin));
        }

        private void sheduleBtn_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Pages.SchedulePage());
        }

        private void trainersBtn_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Pages.TrainersPage());
        }

        private void adminBtn_Click(object sender, RoutedEventArgs e)
        {
            mainFrame.Navigate(new Pages.AdminsPage());
        }
    }
}