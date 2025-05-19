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
    /// Логика взаимодействия для TasksPage.xaml
    /// </summary>
    public partial class TasksPage : Page
    {
        private  DataBase db = new DataBase();
        private Administrators _currentAdmin;
        private ObservableCollection<TaskModel> _allTasks;
        public TasksPage(Administrators administrators)
        {
            InitializeComponent();
            _currentAdmin = administrators;
            LoadTasks();
        }


        private void LoadTasks()
        {
            // Генерируем задачи перед загрузкой
            db.GenerateTasksForAdmin(_currentAdmin.Id);

            _allTasks = new ObservableCollection<TaskModel>(db.GetTasksForAdminMain(_currentAdmin.Id));
            TasksList.ItemsSource = _allTasks;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var addTaskWindow = new Windows.AddTaskWindow(_currentAdmin);
            if (addTaskWindow.ShowDialog() == true)
            {
                LoadTasks();
            }
        }

        private void TaskCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is TaskModel task)
            {
                bool isCompleted = checkBox.IsChecked == true;
                db.UpdateTaskCompletion(task.TaskId, isCompleted);

                // Обновляем базовое свойство, от которого зависят вычисляемые свойства
                task.IsCompleted = isCompleted;

                // Вызываем обновление привязки (если используется INotifyPropertyChanged)
                // Если TaskModel реализует INotifyPropertyChanged, это вызовет обновление UI
                // Если нет, можно просто перезагрузить список задач
                LoadTasks();
            }
        }

        private void EditTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TaskModel task)
            {
                var editWindow = new Windows.EditTaskWindow(task);
                if (editWindow.ShowDialog() == true)
                {
                    LoadTasks();
                }
            }
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilterPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            FilterComboBox.SelectedIndex = 0;
            DateFilterPicker.SelectedDate = null;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allTasks == null || DateFilterPicker == null) // ← Проверка на null
                return;

            var filteredTasks = _allTasks.AsEnumerable();

            // Фильтр по статусу
            switch ((FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString())
            {
                case "Только активные":
                    filteredTasks = filteredTasks.Where(t => !t.IsCompleted);
                    break;
                case "Только выполненные":
                    filteredTasks = filteredTasks.Where(t => t.IsCompleted);
                    break;
                case "Просроченные":
                    filteredTasks = filteredTasks.Where(t => !t.IsCompleted && t.Deadline < DateTime.Today);
                    break;
            }

            // Фильтр по дате (с проверкой на null)
            if (DateFilterPicker.SelectedDate.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.Deadline.Date == DateFilterPicker.SelectedDate.Value.Date);
            }

            TasksList.ItemsSource = new ObservableCollection<TaskModel>(filteredTasks);
        }

    }
}
