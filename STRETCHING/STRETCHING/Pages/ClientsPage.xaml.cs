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
    /// Логика взаимодействия для ClientsPage.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {
        Administrators _admin;
        DataBase db;
        ObservableCollection<Clients> clientsColl { get; set; }
        public ClientsPage(Administrators admin)
        {
            try
            {
                InitializeComponent();
                _admin = admin;
                db = new();
                clientsColl = new();
                LoadClients();
                ClientsListBox.ItemsSource = clientsColl;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
           
        }


        private void LoadClients()
        {
            try
            {
                clientsColl.Clear();
                string searchtxt = SearchBox.Text?.ToLower() ?? "";

                List<Clients> clients;
                if (string.IsNullOrWhiteSpace(searchtxt))
                {
                    clients = db.GetClientsByAdmin(_admin.Id);
                }
                else
                {
                    clients = db.SearchClients(searchtxt, _admin.Id);
                }

                foreach (var client in clients)
                {
                    clientsColl.Add(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadClients();
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
