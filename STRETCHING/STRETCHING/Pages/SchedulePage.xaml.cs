using MySql.Data.MySqlClient;
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
        private readonly List<string> daysOfWeek = new() { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

        public SchedulePage()
        {
            InitializeComponent();
            BuildScheduleGrid();
            LoadScheduleData();
        }


        private void BuildScheduleGrid()
        {
            ScheduleGrid.Children.Clear();
            ScheduleGrid.ColumnDefinitions.Clear();
            ScheduleGrid.RowDefinitions.Clear();

            // Добавим строку заголовков + 6 строк под занятия
            ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < 6; i++)
                ScheduleGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Создаем 7 колонок — по числу дней
            for (int col = 0; col < 7; col++)
                ScheduleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Отображаем заголовки дней недели
            for (int col = 0; col < 7; col++)
            {
                DateTime date = DateTime.Today.AddDays(col);
                var header = new TextBlock
                {
                    Text = $"{daysOfWeek[(int)(date.DayOfWeek + 6) % 7]} ({date:dd.MM})",
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBA276")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };
                Grid.SetRow(header, 0);
                Grid.SetColumn(header, col);
                ScheduleGrid.Children.Add(header);
            }

            // Горизонтальные линии между строками
            for (int row = 1; row < ScheduleGrid.RowDefinitions.Count; row++)
            {
                var hLine = new Border
                {
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(0, 1, 0, 0)
                };
                Grid.SetRow(hLine, row);
                Grid.SetColumnSpan(hLine, 7);
                Panel.SetZIndex(hLine, 0);
                ScheduleGrid.Children.Add(hLine);
            }

            // Вертикальные линии между колонками
            for (int col = 1; col < ScheduleGrid.ColumnDefinitions.Count; col++)
            {
                var vLine = new Border
                {
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1, 0, 0, 0)
                };
                Grid.SetColumn(vLine, col);
                Grid.SetRowSpan(vLine, ScheduleGrid.RowDefinitions.Count);
                Panel.SetZIndex(vLine, 0);
                ScheduleGrid.Children.Add(vLine);
            }

            // Сюда можно вставлять элементы расписания в каждую ячейку:
            // Пример:
            // AddClassToCell("Йога", 1, 0); // строка 1 (вторая), колонка 0 (понедельник)
        }



        private void LoadScheduleData()
        {
            string query = @"
                SELECT 
                    cl.class_id,
                    cl.class_date,
                    cl.start_time,
                    cl.end_time,
                    d.name_d,
                    t.last_name,
                    t.first_name
                FROM classes cl
                JOIN directions d ON cl.direction_id = d.direction_id
                JOIN trainers t ON cl.trainer_id = t.trainer_id
                WHERE cl.class_date BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL 6 DAY)
                ORDER BY cl.class_date, cl.start_time;";

            db.OpenConnection();
            var cmd = new MySqlCommand(query, db.GetConnection());
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var date = Convert.ToDateTime(reader["class_date"]);
                var column = (int)(date.DayOfWeek + 6) % 7; // Пн=0
                var startTime = TimeSpan.Parse(reader["start_time"].ToString());
                var row = GetRowIndex(startTime);

                if (row < 1 || row > 6) continue;

                var direction = reader["name_d"].ToString();
                var trainer = $"{reader["last_name"]} {reader["first_name"].ToString()[0]}.";
                var time = $"{startTime:hh\\:mm} - {TimeSpan.Parse(reader["end_time"].ToString()):hh\\:mm}";

                var block = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A503D")),
                    CornerRadius = new CornerRadius(6),
                    Margin = new Thickness(5),
                    Padding = new Thickness(6),
                    Child = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock { Text = direction, Foreground = Brushes.White, FontWeight = FontWeights.SemiBold },
                            new TextBlock { Text = trainer, Foreground = Brushes.White },
                            new TextBlock { Text = time, Foreground = Brushes.White }
                        }
                    }
                };

                Grid.SetRow(block, row);
                Grid.SetColumn(block, column);
                ScheduleGrid.Children.Add(block);
            }

            reader.Close();
            db.CloseConnection();
        }

        private int GetRowIndex(TimeSpan startTime)
        {
            // Пример: от 8:00 до 20:00 — шесть временных слотов
            if (startTime.Hours < 8 || startTime.Hours > 20) return -1;
            return (startTime.Hours - 8) / 2 + 1;
        }

        private void AddClassButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
