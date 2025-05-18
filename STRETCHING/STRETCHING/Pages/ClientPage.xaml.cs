using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
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
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        private readonly DataBase _database = new DataBase();

        public Clients CurrentClient { get; set; }
        public ObservableCollection<ClientSubscription> Subscriptions { get; set; }
        public ObservableCollection<ClientClass> Classes { get; set; }
        public ClientPage(int clientId)
        {
            InitializeComponent();
            LoadClientData(clientId);
            DataContext = this;
        }

        private void LoadClientData(int clientId)
        {
            try
            {
                _database.OpenConnection();

                // Загрузка данных клиента
                var clientQuery = @"
                    SELECT c.*, rc.name_role as RoleName 
                    FROM clients c
                    JOIN role_clients rc ON c.name_role = rc.id_role
                    WHERE c.id_client = @clientId";

                using (var cmd = new MySqlCommand(clientQuery, _database.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            CurrentClient = new Clients
                            {
                                IdClient = reader.GetInt32("id_client"),
                                LastName = reader.GetString("last_name"),
                                FirstName = reader.GetString("first_name"),
                                MiddleName = reader.GetString("middle_name"),
                                DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime("date_of_birth")),
                                PhoneNumber = reader.GetString("phone_number"),
                                Email = reader.GetString("email"),
                                HealthConditions = reader["health_conditions"] == DBNull.Value ?
                                    string.Empty : reader.GetString("health_conditions"),
                                RoleId = reader.GetInt32("name_role"),
                                RoleName = reader.GetString("RoleName")
                            };
                        }
                    }
                }

                // Загрузка абонементов
                Subscriptions = new ObservableCollection<ClientSubscription>();
                var subscriptionQuery = @"
                    SELECT cs.*, s.subscription_name
                    FROM client_subscriptions cs
                    JOIN subscriptions s ON cs.subscription_id = s.subscription_id
                    WHERE cs.client_id = @clientId
                    ORDER BY cs.subscription_end_date DESC";

                using (var cmd = new MySqlCommand(subscriptionQuery, _database.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Subscriptions.Add(new ClientSubscription
                            {
                                SubscriptionName = reader.GetString("subscription_name"),
                                StartDate = reader.GetDateTime("subscription_start_date"),
                                EndDate = reader.GetDateTime("subscription_end_date"),
                                TotalClasses = reader.GetInt32("total_classes"),
                                RemainingClasses = reader.GetInt32("remaining_classes")
                            });
                        }
                    }
                }

                // Загрузка занятий
                Classes = new ObservableCollection<ClientClass>();
                var classesQuery = @"
                    SELECT cl.class_date, cl.start_time, cl.end_time, 
                           d.name_d as direction_name,
                           CONCAT(t.last_name, ' ', t.first_name) as trainer_name,
                           cs.status_name
                    FROM bookings b
                    JOIN classes cl ON b.class_id = cl.class_id
                    JOIN directions d ON cl.direction_id = d.direction_id
                    JOIN trainers t ON cl.trainer_id = t.trainer_id
                    JOIN class_status cs ON b.status_id = cs.status_id
                    WHERE b.client_id = @clientId
                    ORDER BY cl.class_date DESC, cl.start_time DESC";

                using (var cmd = new MySqlCommand(classesQuery, _database.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@clientId", clientId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Classes.Add(new ClientClass
                            {
                                ClassDate = reader.GetDateTime("class_date"),
                                StartTime = reader.GetTimeSpan("start_time"),
                                EndTime = reader.GetTimeSpan("end_time"),
                                DirectionName = reader.GetString("direction_name"),
                                TrainerName = reader.GetString("trainer_name"),
                                Status = reader.GetString("status_name")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных клиента: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _database.CloseConnection();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            /*var editPage = new EditClientPage(CurrentClient);
            NavigationService.Navigate(editPage);*/
        }

        private void AssignSubscription_Click(object sender, RoutedEventArgs e)
        {
           /* var assignPage = new AssignSubscriptionPage(CurrentClient.IdClient);
            NavigationService.Navigate(assignPage);*/
        }

        private void BookClass_Click(object sender, RoutedEventArgs e)
        {
           /* var bookPage = new BookClassPage(CurrentClient.IdClient);
            NavigationService.Navigate(bookPage);*/
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }

    // Вспомогательные классы для отображения данных
    public class ClientSubscription
    {
        public string SubscriptionName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalClasses { get; set; }
        public int RemainingClasses { get; set; }
    }

    public class ClientClass
    {
        public DateTime ClassDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string DirectionName { get; set; }
        public string TrainerName { get; set; }
        public string Status { get; set; }

        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    }
}

