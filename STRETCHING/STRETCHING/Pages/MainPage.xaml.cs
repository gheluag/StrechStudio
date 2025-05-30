﻿using STRETCHING.Entities;
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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        Administrators _admin;
        DataBase db;
        public ObservableCollection<TaskModel> Tasks { get; set; }
        public MainPage(Administrators admin)
        {
            InitializeComponent();
            _admin = admin;
            db = new();
            Tasks = new();
            LoadTasks();
            LoadTodaySchedule();
        }


        private void LoadTasks()
        {
            Tasks.Clear();
            var taskList = db.GetTasksForAdminMain(_admin.Id);

            foreach (var task in taskList)
            {
                Tasks.Add(task);
            }
            TasksList.ItemsSource = Tasks;
        }

        private void LoadTodaySchedule()
        {
            var todaySchedule = db.GetTodaySchedule();
            ScheduleList.ItemsSource = todaySchedule;
        }

    }
}
