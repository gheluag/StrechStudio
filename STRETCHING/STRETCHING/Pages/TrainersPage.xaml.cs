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
            // Очищаем текущий источник данных
            TrainersListBox.ItemsSource = null;

            // Загружаем свежие данные
            var trainers = db.GetAllTrainers();
            TrainersListBox.ItemsSource = trainers;

            // Принудительное обновление UI
            TrainersListBox.Items.Refresh();
        }

        private void AddTrainerButton_Click(object sender, RoutedEventArgs e)
        {
            var addTrainerWindow = new Windows.AddTrainerWindow();
            if (addTrainerWindow.ShowDialog() == true)
            {
                if (db.AddTrainer(addTrainerWindow.Trainer) > 0)
                {
                    MessageBox.Show("Тренер успешно добавлен", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTrainers();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить тренера", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        

        private void EditTrainerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Trainer trainer)
            {
                EditTrainer(trainer);
            }
        }

     

        private void EditTrainer(Trainer trainer)
        {
            var editTrainerWindow = new Windows.AddTrainerWindow(trainer);
            if (editTrainerWindow.ShowDialog() == true)
            {
                if (db.UpdateTrainer(editTrainerWindow.Trainer))
                {
                    MessageBox.Show("Данные тренера успешно обновлены", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTrainers();
                }
                else
                {
                    MessageBox.Show("Не удалось обновить данные тренера", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
    }
}
