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
    /// Логика взаимодействия для AddTaskWindow.xaml
    /// </summary>
    public partial class AddTaskWindow : Window
    {
        private readonly Administrators _currentAdmin;
        private readonly DataBase _db = new DataBase();
        public AddTaskWindow(Administrators admin)
        {
            InitializeComponent();
            _currentAdmin = admin;
            DeadlineDatePicker.SelectedDate = DateTime.Today.AddDays(1);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите заголовок задачи", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!DeadlineDatePicker.SelectedDate.HasValue ||
                DeadlineDatePicker.SelectedDate.Value < DateTime.Today)
            {
                MessageBox.Show("Укажите корректную дату выполнения (не ранее сегодняшнего дня)",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newTask = new TaskModel
            {
                Title = TitleTextBox.Text.Trim(),
                Description = DescriptionTextBox.Text,
                Deadline = DeadlineDatePicker.SelectedDate.Value,
                AdminId = _currentAdmin.Id,
                IsCompleted = false
            };

            if (_db.AddTask(newTask))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Не удалось добавить задачу", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
