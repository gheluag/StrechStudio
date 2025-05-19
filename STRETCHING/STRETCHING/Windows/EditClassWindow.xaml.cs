using STRETCHING.Entities;
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

namespace STRETCHING.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditClassWindow.xaml
    /// </summary>
    public partial class EditClassWindow : Window
    {
        private readonly DataBase db = new DataBase();
        private readonly int _classId;
        private List<Direction> _directions;
        private List<Trainer> _trainers;
        private List<Hall> _halls;
        private Class _currentClass;
        public EditClassWindow(int classId)
        {
            InitializeComponent();
            _classId = classId;
            LoadData();
            InitializeTimeComboBoxes();
            LoadClassData();
        }

        private void LoadData()
        {
            try
            {
                _directions = db.GetAllDirections();
                _trainers = db.GetAllTrainers();
                _halls = db.GetAllHalls();

                DirectionComboBox.ItemsSource = _directions;
                TrainerComboBox.ItemsSource = _trainers;
                HallComboBox.ItemsSource = _halls;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeTimeComboBoxes()
        {
            // Заполняем комбобоксы временем с шагом 30 минут
            for (int hours = 8; hours < 22; hours++)
            {
                for (int minutes = 0; minutes < 60; minutes += 30)
                {
                    var time = new TimeSpan(hours, minutes, 0);
                    StartTimeComboBox.Items.Add(time.ToString(@"hh\:mm"));
                    EndTimeComboBox.Items.Add(time.ToString(@"hh\:mm"));
                }
            }
        }

        private void LoadClassData()
        {
            try
            {
                _currentClass = db.GetClassById(_classId);
                if (_currentClass == null)
                {
                    MessageBox.Show("Занятие не найдено", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                ClassDatePicker.SelectedDate = _currentClass.ClassDate;
                StartTimeComboBox.SelectedItem = _currentClass.StartTime.ToString(@"hh\:mm");
                EndTimeComboBox.SelectedItem = _currentClass.EndTime.ToString(@"hh\:mm");

                // Устанавливаем выбранные элементы в комбобоксы
                foreach (var direction in _directions)
                {
                    if (direction.DirectionId == _currentClass.DirectionId)
                    {
                        DirectionComboBox.SelectedItem = direction;
                        break;
                    }
                }

                foreach (var trainer in _trainers)
                {
                    if (trainer.TrainerId == _currentClass.TrainerId)
                    {
                        TrainerComboBox.SelectedItem = trainer;
                        break;
                    }
                }

                foreach (var hall in _halls)
                {
                    if (hall.HallId == _currentClass.HallId)
                    {
                        HallComboBox.SelectedItem = hall;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных занятия: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                _currentClass.ClassDate = ClassDatePicker.SelectedDate.Value;
                _currentClass.StartTime = TimeSpan.Parse(StartTimeComboBox.SelectedItem.ToString());
                _currentClass.EndTime = TimeSpan.Parse(EndTimeComboBox.SelectedItem.ToString());
                _currentClass.DirectionId = ((Direction)DirectionComboBox.SelectedItem).DirectionId;
                _currentClass.TrainerId = ((Trainer)TrainerComboBox.SelectedItem).TrainerId;
                _currentClass.HallId = ((Hall)HallComboBox.SelectedItem).HallId;

                if (db.UpdateClass(_currentClass))
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось обновить занятие", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении занятия: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (ClassDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату занятия", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (StartTimeComboBox.SelectedItem == null || EndTimeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Укажите время начала и окончания", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            var startTime = TimeSpan.Parse(StartTimeComboBox.SelectedItem.ToString());
            var endTime = TimeSpan.Parse(EndTimeComboBox.SelectedItem.ToString());

            if (endTime <= startTime)
            {
                MessageBox.Show("Время окончания должно быть позже времени начала", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (DirectionComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите направление", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (TrainerComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите тренера", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (HallComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите зал", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
