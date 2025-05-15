using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using STRETCHING.Entities;

namespace STRETCHING.Windows
{
    /// <summary>
    /// Логика взаимодействия для AuthenticateWindow.xaml
    /// </summary>
    public partial class AuthenticateWindow : Window
    {
        public AuthenticateWindow()
        {
            InitializeComponent();
        }


        private void Login()
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            DataBase db = new();
            Administrators admin = db.Authorize(login, password);

            if (admin != null)
            {

                MainWindow mainWindow = new MainWindow(admin);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void Input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                if (sender == LoginTextBox)
                    PasswordBox.Focus();
                else if (sender == PasswordBox)
                    LoginTextBox.Focus();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (sender == PasswordBox)
                    LoginTextBox.Focus();
                else if (sender == LoginTextBox)
                    PasswordBox.Focus();
                e.Handled = true;
            }
        }

    }
}
