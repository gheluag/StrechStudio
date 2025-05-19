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
    /// Логика взаимодействия для TrainersPage.xaml
    /// </summary>
    public partial class TrainersPage : Page
    {
        private readonly DataBase db = new DataBase();
        public TrainersPage()
        {
            InitializeComponent();
            LoadTrainers();
        }

        private void LoadTrainers()
        {
            try
            {
                string searchText = SearchBox.Text?.ToLower() ?? "";
                List<Trainer> trainers;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    trainers = db.GetAllTrainers();
                }
                else
                {
                    trainers = db.GetSearchTrainers(searchText);
                }

                TrainersListBox.ItemsSource = trainers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке тренеров: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddTrainerButton_Click(object sender, RoutedEventArgs e)
        {
            var addTrainerWindow = new Windows.AddTrainerWindow();
            if (addTrainerWindow.ShowDialog() == true)
            {
                LoadTrainers();
                
            }
        }




        private void DeleteTrainerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Trainer trainer)
            {
                DeleteTrainer(trainer);
            }
        }

        private void DeleteTrainer(Trainer trainer)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить тренера {trainer.FullName}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (db.DeleteTrainer(trainer.TrainerId))
                {
                    MessageBox.Show("Тренер успешно удален", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTrainers();
                }
                else
                {
                    MessageBox.Show("Не удалось удалить тренера", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTrainers();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadTrainers();
        }
    }
}
