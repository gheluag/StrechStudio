using STRETCHING.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для AddTrainerWindow.xaml
    /// </summary>
    public partial class AddTrainerWindow : Window
    {
        private readonly DataBase db = new DataBase();

        private List<TrainerSpecialization> _allSpecializations;
        public AddTrainerWindow()
        {
            InitializeComponent();
            InitializeDateControls();

            LoadSpecializations();
        }


        private void LoadSpecializations()
        {
            _allSpecializations = db.GetSpecializations();
            MainSpecializationComboBox.ItemsSource = _allSpecializations;
        }
        private void InitializeDateControls()
        {
            for (int i = 1; i <= 31; i++)
            {
                DayComboBox.Items.Add(i);
            }

            var months = new List<string>
            {
                "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
            };
            MonthComboBox.ItemsSource = months;

            // Заполнение годов (1900-текущий год)
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear; year >= 1900; year--)
            {
                YearComboBox.Items.Add(year);
            }

            // Установка текущей даты по умолчанию
            SetSelectedDate(DateTime.Today);
        }

        private void SetSelectedDate(DateTime date)
        {
            DayComboBox.SelectedItem = date.Day;
            MonthComboBox.SelectedIndex = date.Month - 1;
            YearComboBox.SelectedItem = date.Year;
        }


        private DateTime? GetSelectedDate()
        {
            if (DayComboBox.SelectedItem == null ||
                MonthComboBox.SelectedItem == null ||
                YearComboBox.SelectedItem == null)
                return null;

            int day = (int)DayComboBox.SelectedItem;
            int month = MonthComboBox.SelectedIndex + 1;
            int year = (int)YearComboBox.SelectedItem;

            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
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

        private bool IsValidName(string name)
        {
            // Проверяем, что имя не состоит только из цифр или повторяющихся символов
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Проверка на содержание только цифр
            if (Regex.IsMatch(name, @"^[\d\s]+$"))
                return false;

            // Проверка на повторяющиеся символы (например, "111" или "aaa")
            if (name.Length > 1 && name.Distinct().Count() == 1)
                return false;

            // Проверка минимальной длины имени (минимум 2 символа)
            if (name.Trim().Length < 2)
                return false;

            return true;
        }

        private bool IsValidBirthDate(DateTime date)
        {
            // Тренер должен быть не младше 18 лет и не старше 100 лет
            int minAge = 18;
            int maxAge = 100;
            int age = DateTime.Today.Year - date.Year;

            if (date > DateTime.Today.AddYears(-age)) age--;

            return age >= minAge && age <= maxAge;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация ФИО
            if (!IsValidName(LastNameTextBox.Text) ||
                !IsValidName(FirstNameTextBox.Text) ||
                !IsValidName(MiddleNameTextBox.Text))
            {
                MessageBox.Show("ФИО тренера должно содержать минимум 2 символа и не может состоять только из цифр или повторяющихся символов.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка даты рождения
            DateTime? birthDate = GetSelectedDate();
            if (!birthDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите корректную дату рождения",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidBirthDate(birthDate.Value))
            {
                MessageBox.Show($"Тренер должен быть в возрасте от 18 до 100 лет.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка специализации
            if (MainSpecializationComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите специализацию тренера",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Создание объекта тренера
            var trainer = new Trainer
            {
                LastName = LastNameTextBox.Text.Trim(),
                FirstName = FirstNameTextBox.Text.Trim(),
                MiddleName = MiddleNameTextBox.Text.Trim(),
                DateOfBirth = birthDate.Value,
                SpecializationIds = new List<int> { ((TrainerSpecialization)MainSpecializationComboBox.SelectedItem).SpecializationId }
            };

         
          

            // Добавление в базу данных
            int trainerId = db.AddTrainer(trainer);
            if (trainerId > 0)
            {
                MessageBox.Show("Тренер успешно добавлен",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Не удалось добавить тренера",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    
}

