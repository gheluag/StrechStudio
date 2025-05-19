using STRETCHING.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace STRETCHING.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminsPage.xaml
    /// </summary>
    public partial class AdminsPage : Page
    {
        private readonly DataBase _db = new DataBase();
        private ObservableCollection<AdminModel> _admins;
        public AdminsPage()
        {
            InitializeComponent();
            LoadAdmins();
        }

        private void LoadAdmins()
        {
            
        }

        private void AddAdminButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAdmins();
        }

        private void ViewAdminButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DeleteAdminButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void AdminsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

    }
}
