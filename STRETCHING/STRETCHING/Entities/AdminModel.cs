using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace STRETCHING.Entities
{
    public class AdminModel : INotifyPropertyChanged
    {
        public int AdminId { get; set; }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
                OnPropertyChanged(nameof(FullName));
            }
        }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(FullName));
            }
        }

        private string _middleName;
        public string MiddleName
        {
            get => _middleName;
            set
            {
                _middleName = value;
                OnPropertyChanged(nameof(MiddleName));
                OnPropertyChanged(nameof(FullName));
            }
        }

        public string FullName => $"{LastName} {FirstName} {MiddleName}";
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Login { get; set; }

        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(StatusColor));
            }
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                _createdAt = value;
                OnPropertyChanged(nameof(CreatedAt));
            }
        }

        private DateTime? _lastLogin;
        public DateTime? LastLogin
        {
            get => _lastLogin;
            set
            {
                _lastLogin = value;
                OnPropertyChanged(nameof(LastLogin));
            }
        }


        private IEnumerable<RoleAdmin> _roles;
        public IEnumerable<RoleAdmin> Roles
        {
            get => _roles;
            set
            {
                _roles = value;
                OnPropertyChanged(nameof(Roles));
            }
        }

        public string StatusText => IsActive ? "Активен" : "Неактивен";
        public Brush StatusColor => IsActive ?
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A503D")) :
            Brushes.Gray;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
