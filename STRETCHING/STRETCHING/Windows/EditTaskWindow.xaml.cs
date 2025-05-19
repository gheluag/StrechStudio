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
    /// Логика взаимодействия для EditTaskWindow.xaml
    /// </summary>
    public partial class EditTaskWindow : Window
    {
        private readonly TaskModel _task;
        private readonly DataBase _db = new DataBase();

        public EditTaskWindow(TaskModel task)
        {
            InitializeComponent();
            _task = task;
            DataContext = _task;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите заголовок задачи", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!DeadlineDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите дату выполнения", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _task.Title = TitleTextBox.Text.Trim();
            _task.Description = DescriptionTextBox.Text;
            _task.Deadline = DeadlineDatePicker.SelectedDate.Value;

            if (_db.UpdateTask(_task))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Не удалось обновить задачу", "Ошибка",
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
