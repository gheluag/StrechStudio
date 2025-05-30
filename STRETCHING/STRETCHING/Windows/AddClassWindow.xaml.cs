﻿using STRETCHING.Entities;
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
    /// Логика взаимодействия для AddClassWindow.xaml
    /// </summary>
    public partial class AddClassWindow : Window
    {
        private readonly DataBase db = new DataBase();
        private List<Direction> _directions;
        private List<Trainer> _trainers;
        private List<Hall> _halls;
        public AddClassWindow()
        {
            InitializeComponent();
            LoadData();
            InitializeTimeComboBoxes();
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

                ClassDatePicker.SelectedDate = DateTime.Today;
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

            // Устанавливаем время по умолчанию
            StartTimeComboBox.SelectedIndex = 0;
            EndTimeComboBox.SelectedIndex = 2; // 1.5 часа после начала
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                var newClass = new Class
                {
                    ClassDate = ClassDatePicker.SelectedDate.Value,
                    StartTime = TimeSpan.Parse(StartTimeComboBox.SelectedItem.ToString()),
                    EndTime = TimeSpan.Parse(EndTimeComboBox.SelectedItem.ToString()),
                    DirectionId = ((Direction)DirectionComboBox.SelectedItem).DirectionId,
                    TrainerId = ((Trainer)TrainerComboBox.SelectedItem).TrainerId,
                    HallId = ((Hall)HallComboBox.SelectedItem).HallId
                };

                if (db.AddClass(newClass))
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить занятие", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении занятия: {ex.Message}", "Ошибка",
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
