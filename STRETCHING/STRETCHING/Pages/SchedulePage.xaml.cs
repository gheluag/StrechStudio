using MySql.Data.MySqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STRETCHING.Pages
{
    /// <summary>
    /// Логика взаимодействия для SchedulePage.xaml
    /// </summary>
    public partial class SchedulePage : Page
    {
        private readonly DataBase db = new DataBase();

        public SchedulePage()
        {
            InitializeComponent();
            LoadSchedule();
        }

        private void LoadSchedule()
        {
            try
            {
                var schedule = db.GetClassSchedule();
                ScheduleDataGrid.ItemsSource = schedule;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке расписания: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddClassButton_Click(object sender, RoutedEventArgs e)
        {
            var addClassWindow = new Windows.AddClassWindow();
            if (addClassWindow.ShowDialog() == true)
            {
                LoadSchedule();
            }
        }

        private void EditClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleDataGrid.SelectedItem is ClassScheduleItem selectedClass)
            {
                var editWindow = new Windows.EditClassWindow(selectedClass.ClassId);
                if (editWindow.ShowDialog() == true)
                {
                    LoadSchedule();
                }
            }
        }

        private void DeleteClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleDataGrid.SelectedItem is ClassScheduleItem selectedClass)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить занятие от {selectedClass.ClassDate:dd.MM.yyyy}?",
                                          "Подтверждение удаления",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (db.DeleteClass(selectedClass.ClassId))
                    {
                        MessageBox.Show("Занятие успешно удалено", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadSchedule();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось удалить занятие", "Ошибка",
                                      MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSchedule();
        }

    }
}
