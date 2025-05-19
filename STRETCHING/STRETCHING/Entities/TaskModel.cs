using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace STRETCHING.Entities
{
    public class TaskModel : INotifyPropertyChanged
    {
        private bool _isCompleted;

        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime TaskDate { get; set; }
        public DateTime Deadline { get; set; }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged(nameof(IsCompleted));
                    OnPropertyChanged(nameof(StatusText));
                    OnPropertyChanged(nameof(StatusColor));
                    OnPropertyChanged(nameof(DeadlineStatus));
                }
            }
        }

        public int AdminId { get; set; }
        public int? ClientId { get; set; }

        public string StatusText => IsCompleted ? "Выполнено" : Deadline < DateTime.Today ? "Просрочено" : "Активна";

        public Brush StatusColor => IsCompleted ?
            Brushes.Gray :
            Deadline < DateTime.Today ?
                Brushes.Red :
                new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A503D"));

        public string DeadlineStatus => Deadline < DateTime.Today ?
            $"Просрочено на {(DateTime.Today - Deadline).Days} дн." :
            $"Осталось {(Deadline - DateTime.Today).Days} дн.";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
