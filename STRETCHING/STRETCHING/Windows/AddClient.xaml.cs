using MySqlX.XDevAPI;
using STRETCHING.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STRETCHING.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddClient.xaml
    /// </summary>
    public partial class AddClient : Window
    {
        private readonly ClientRepository _clientRepository;
        private readonly int _currentAdminId;
        public AddClient(Administrators administrators)
        {
            InitializeComponent();
            _currentAdminId = administrators.Id;
            var database = new DataBase();
            _clientRepository = new ClientRepository(database);
            PopulateBirthDateComboBoxes();
            LoadComboBoxData();
        }



        private void LoadComboBoxData()
        {
            // Загрузка ролей клиентов
            var roles = _clientRepository.GetClientRoles();
            RoleComboBox.ItemsSource = roles;
            RoleComboBox.DisplayMemberPath = "Name";
            RoleComboBox.SelectedValuePath = "Id";
            RoleComboBox.SelectedIndex = 0;

            // Загрузка абонементов
            var subscriptions = _clientRepository.GetSubscriptions();
            SubscriptionComboBox.ItemsSource = subscriptions;
            SubscriptionComboBox.DisplayMemberPath = "Name";
            SubscriptionComboBox.SelectedValuePath = "Id";
            SubscriptionComboBox.SelectedIndex = 0;
        }

        private void PopulateBirthDateComboBoxes()
        {
           
            for (int day = 1; day <= 31; day++)
            {
                dayCb.Items.Add(day);
            }

            string[] months = new[]
            {
            "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
            "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
        };

            foreach (var month in months)
            {
                monthCb.Items.Add(month);
            }

            int currentYear = DateTime.Now.Year;
            for (int year = currentYear; year >= 1950; year--)
            {
                yearCb.Items.Add(year);
            }

            dayCb.SelectedIndex = -1;
            monthCb.SelectedIndex = -1;
            yearCb.SelectedIndex = -1;
        }


        private DateOnly GetSelectedDate()
        {
            try
            {
                int day = (int)dayCb.SelectedItem;
                int month = monthCb.SelectedIndex + 1;
                int year = (int)yearCb.SelectedItem;

                return new DateOnly(year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Если день не существует в выбранном месяце (например, 31 февраля)
                return new DateOnly((int)yearCb.SelectedItem, monthCb.SelectedIndex + 1, 1);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (LastNameBox.Text.Trim().Length < 2 ||
        FirstNameBox.Text.Trim().Length < 2 ||
        PhoneBox.Text.Trim().Length < 10)
            {
                MessageBox.Show("Пожалуйста, введите реальные данные клиента",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInput())
                return;

            try
            {
                var client = new STRETCHING.Entities.Clients
                {
                    LastName = LastNameBox.Text.Trim(),
                    FirstName = FirstNameBox.Text.Trim(),
                    MiddleName = MiddleNameBox.Text.Trim(),
                    DateOfBirth = GetSelectedDate(),
                    PhoneNumber = PhoneBox.Text.Trim(),
                    Email = EmailBox.Text.Trim(),
                    HealthConditions = HealthNotesBox.Text.Trim(),
                    RoleId = (int)RoleComboBox.SelectedValue
                };

                var subscriptionId = (int)SubscriptionComboBox.SelectedValue;
                var subscription = _clientRepository.GetSubscriptions().First(s => s.Id == subscriptionId);

                var payment = new Payment
                {
                    TotalAmount = subscription.FullPrice,
                    AmountPaid = subscription.FullPrice,
                    AmountRemaining = 0,
                    LastPaymentDate = DateTime.Today,
                    PromisedPaymentDate = null,
                    Comment = "Первоначальная оплата абонемента"
                };

                _clientRepository.AddClient(client, subscriptionId, _currentAdminId, payment);

                MessageBox.Show("Клиент успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool ValidateInput()
        {
            // Валидация Фамилии
            if (string.IsNullOrWhiteSpace(LastNameBox.Text) || !IsValidName(LastNameBox.Text))
            {
                MessageBox.Show("Фамилия должна содержать только буквы и быть длиной от 2 до 50 символов",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                LastNameBox.Focus();
                return false;
            }

            // Валидация Имени
            if (string.IsNullOrWhiteSpace(FirstNameBox.Text) || !IsValidName(FirstNameBox.Text))
            {
                MessageBox.Show("Имя должно содержать только буквы и быть длиной от 2 до 50 символов",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                FirstNameBox.Focus();
                return false;
            }

            // Валидация Отчества (может быть пустым)
            if (!string.IsNullOrWhiteSpace(MiddleNameBox.Text) && !IsValidName(MiddleNameBox.Text))
            {
                MessageBox.Show("Отчество должно содержать только буквы и быть длиной до 50 символов",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                MiddleNameBox.Focus();
                return false;
            }

            // Валидация телефона
            if (string.IsNullOrWhiteSpace(PhoneBox.Text) || !IsValidPhone(PhoneBox.Text))
            {
                MessageBox.Show("Введите корректный номер телефона (10 или 11 цифр, может начинаться с +7 или 8)",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PhoneBox.Focus();
                return false;
            }

            // Валидация email
            if (!IsValidEmail(EmailBox.Text))
            {
                MessageBox.Show("Введите корректный email", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                EmailBox.Focus();
                return false;
            }

            // Проверка на "1 1 1"
            if (LastNameBox.Text.Trim() == "1" && FirstNameBox.Text.Trim() == "1" && MiddleNameBox.Text.Trim() == "1")
            {
                MessageBox.Show("Введены некорректные данные", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        // Проверка ФИО (только буквы, дефисы и пробелы)
        private bool IsValidName(string name)
        {
            if (name.Length < 2 || name.Length > 50)
                return false;

            return Regex.IsMatch(name, @"^[a-zA-Zа-яА-ЯёЁ\- ]+$");
        }

        // Проверка телефона (российские номера)
        private bool IsValidPhone(string phone)
        {
            // Удаляем все нецифровые символы, кроме +
            string cleaned = Regex.Replace(phone, @"[^\d+]", "");

            // Проверяем форматы: +79991112233, 89991112233, 9991112233
            return Regex.IsMatch(cleaned, @"^(\+7|8)?\d{10}$");
        }

        // Проверка email
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void Name_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только буквы, дефисы и пробелы
            if (!Regex.IsMatch(e.Text, @"^[a-zA-Zа-яА-ЯёЁ\- ]$"))
            {
                e.Handled = true;
            }
        }
        private void PhoneBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры, +, пробел и скобки
            if (!char.IsDigit(e.Text, 0) && e.Text != "+" && e.Text != " " && e.Text != "(" && e.Text != ")")
            {
                e.Handled = true;
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
          
            DialogResult = false;
            Close();
        }
    }
}
