using Pilom.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Pilom.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserProfilePage.xaml
    /// </summary>
    public partial class UserProfilePage : Page
    {
        private PilomEntities _context = new PilomEntities();
        private Users _currentUser;
        private bool _isPasswordValid = true;
        public UserProfilePage(Users currentUser)
        {
            InitializeComponent();
            _currentUser = _context.Users.FirstOrDefault(u => u.UserID == currentUser.UserID);
            LoadUserData();
        }
        private void LoadUserData()
        {
            if (_currentUser != null)
            {
                UserNameBox.Text = _currentUser.Name;
                LoginBox.Text = _currentUser.Login;
                PasswordBox.Password = _currentUser.Password;
                PhoneBox.Text = _currentUser.Phone;
            }
        }

        private void UserNameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[а-яА-Яa-zA-Z]+$"))
            {
                e.Handled = true;
                MessageBox.Show("Имя может содержать только буквы.");
            }
        }

        private void PhoneBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[0-9]+$"))
            {
                e.Handled = true;
                MessageBox.Show("Телефон может содержать только цифры.");
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidatePassword();
        }

        private bool ValidatePassword()
        {
            if (PasswordBox.Password.Length > 0 && PasswordBox.Password.Length < 8)
            {
                MessageBox.Show("Пароль должен содержать минимум 8 символов.");
                _isPasswordValid = false;
                return false;
            }
            _isPasswordValid = true;
            return true;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserNameBox.Text) ||
                string.IsNullOrWhiteSpace(LoginBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            if (!ValidatePassword())
            {
                return;
            }

            try
            {
                var userToUpdate = _context.Users.FirstOrDefault(u => u.UserID == _currentUser.UserID);

                if (userToUpdate != null)
                {
                    userToUpdate.Name = UserNameBox.Text.Trim();
                    userToUpdate.Login = LoginBox.Text.Trim();
                    userToUpdate.Password = PasswordBox.Password.Trim();
                    userToUpdate.Phone = PhoneBox.Text.Trim();

                    _context.SaveChanges();
                    MessageBox.Show("Данные успешно сохранены!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных: " + ex.Message);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Properties["CurrentUser"] = null;
            NavigationService.Navigate(new LoginPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserPage(_currentUser));
        }
    }
}
