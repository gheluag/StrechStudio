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
using System.Windows.Shapes;

namespace STRETCHING.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddTrainerWindow.xaml
    /// </summary>
    public partial class AddTrainerWindow : Window
    {
        private readonly DataBase db = new DataBase();
        public Trainer Trainer { get; private set; }
        private List<TrainerSpecialization> _allSpecializations;
        private ObservableCollection<SpecializationViewModel> _addedSpecializations;
        public AddTrainerWindow()
        {
            InitializeComponent();
            Trainer = new Trainer();
            _addedSpecializations = new ObservableCollection<SpecializationViewModel>();
            InitializeDateControls();
            LoadSpecializations();
        }

        public AddTrainerWindow(Trainer trainer) : this()
        {
            Trainer = trainer;
            LastNameTextBox.Text = trainer.LastName;
            FirstNameTextBox.Text = trainer.FirstName;
            MiddleNameTextBox.Text = trainer.MiddleName;

            // Установка даты рождения
            if (trainer.DateOfBirth != default)
            {
                SetSelectedDate(trainer.DateOfBirth);
            }

            // Добавление существующих специализаций
            if (trainer.SpecializationIds != null && trainer.SpecializationIds.Any())
            {
                foreach (var specId in trainer.SpecializationIds)
                {
                    var spec = _allSpecializations.FirstOrDefault(s => s.SpecializationId == specId);
                    if (spec != null)
                    {
                        AddSpecialization(spec);
                    }
                }
            }
        }

        private void InitializeDateControls()
        {
            // Заполнение дней (1-31)
            for (int i = 1; i <= 31; i++)
            {
                DayComboBox.Items.Add(i);
            }

            // Заполнение месяцев
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
            {
                return null;
            }

            try
            {
                int day = (int)DayComboBox.SelectedItem;
                int month = MonthComboBox.SelectedIndex + 1;
                int year = (int)YearComboBox.SelectedItem;

                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }

        private void LoadSpecializations()
        {
            _allSpecializations = db.GetAllSpecializations();
            MainSpecializationComboBox.ItemsSource = _allSpecializations;

            if (_allSpecializations.Any())
            {
                MainSpecializationComboBox.SelectedIndex = 0;
            }
        }

        private void AddSpecialization(TrainerSpecialization specialization)
        {
            var availableSpecs = _allSpecializations
                .Where(s => !_addedSpecializations.Any(aspec => aspec.SelectedSpecialization?.SpecializationId == s.SpecializationId))
                .ToList();

            var vm = new SpecializationViewModel
            {
                AvailableSpecializations = availableSpecs,
                SelectedSpecialization = specialization
            };

            _addedSpecializations.Add(vm);
            UpdateMainComboBoxItems();
        }

        private void UpdateMainComboBoxItems()
        {
            var availableSpecs = _allSpecializations
                .Where(s => !_addedSpecializations.Any(aspec => aspec.SelectedSpecialization?.SpecializationId == s.SpecializationId))
                .ToList();

            MainSpecializationComboBox.ItemsSource = availableSpecs;

            
            
                MainSpecializationComboBox.SelectedIndex = 0;
            
           
        }

        private void AddSpecializationButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainSpecializationComboBox.SelectedItem is TrainerSpecialization selectedSpec)
            {
                AddSpecialization(selectedSpec);
            }
        }

        private void RemoveSpecializationButton_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is SpecializationViewModel vm)
            {
                _addedSpecializations.Remove(vm);
                UpdateMainComboBoxItems();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните обязательные поля (Фамилия, Имя).",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                return;
            }

            var selectedDate = GetSelectedDate();
            if (!selectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, укажите корректную дату рождения.",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                return;
            }

            Trainer.LastName = LastNameTextBox.Text.Trim();
            Trainer.FirstName = FirstNameTextBox.Text.Trim();
            Trainer.MiddleName = MiddleNameTextBox.Text.Trim();
            Trainer.DateOfBirth = selectedDate.Value;

            // Получаем выбранные специализации
            Trainer.SpecializationIds = _addedSpecializations
                .Where(vm => vm.SelectedSpecialization != null)
                .Select(vm => vm.SelectedSpecialization.SpecializationId)
                .ToList();

            Trainer.SpecializationNames = string.Join(", ",
                _addedSpecializations
                    .Where(vm => vm.SelectedSpecialization != null)
                    .Select(vm => vm.SelectedSpecialization.Name));

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class SpecializationViewModel
    {
        public List<TrainerSpecialization> AvailableSpecializations { get; set; }
        public TrainerSpecialization SelectedSpecialization { get; set; }
    }
}

