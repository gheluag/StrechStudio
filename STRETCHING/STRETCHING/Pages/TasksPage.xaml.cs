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
        Administrators _currentAdmin;
        private ObservableCollection<TaskItem> Tasks;
        DataBase db;
        public TasksPage(Administrators administrators)
        {
            InitializeComponent();
            _currentAdmin = administrators;
            db = new();
            db.GenerateTasksForAdmin(_currentAdmin.Id);
            LoadTasks();
        }


        private void LoadTasks()
        {
            Tasks = db.GetTasksForAdmin(_currentAdmin.Id);
            TasksList.ItemsSource = Tasks;
        }

        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is TaskItem task)
            {
                db.UpdateTaskCompletion(task.Id, checkBox.IsChecked == true);
            }
        }

    }
}
