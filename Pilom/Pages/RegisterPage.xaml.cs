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
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        private PilomEntities _context = new PilomEntities();

        public RegisterPage()
        {
            InitializeComponent();
        }

        // Обработчики для TextBox
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var placeholder = FindName(textBox.Name + "Placeholder") as TextBlock;
                placeholder?.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var placeholder = FindName(textBox.Name + "Placeholder") as TextBlock;
                if (placeholder != null)
                {
                    placeholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var placeholder = FindName(textBox.Name + "Placeholder") as TextBlock;
                placeholder?.SetCurrentValue(VisibilityProperty,
                    string.IsNullOrEmpty(textBox.Text) ? Visibility.Visible : Visibility.Collapsed);
            }
            ErrorMessage.Visibility = Visibility.Collapsed;
        }

        // Обработчики для PasswordBox
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                var placeholder = FindName(passwordBox.Name + "Placeholder") as TextBlock;
                placeholder?.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            }
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                var placeholder = FindName(passwordBox.Name + "Placeholder") as TextBlock;
                if (placeholder != null)
                {
                    placeholder.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                var placeholder = FindName(passwordBox.Name + "Placeholder") as TextBlock;
                placeholder?.SetCurrentValue(VisibilityProperty,
                    string.IsNullOrEmpty(passwordBox.Password) ? Visibility.Visible : Visibility.Collapsed);
            }
            ErrorMessage.Visibility = Visibility.Collapsed;
        }
        private void UserNameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только буквы, пробелы и дефисы (для двойных фамилий)
            if (!Regex.IsMatch(e.Text, @"^[\p{L}\s-]+$"))
            {
                e.Handled = true;
            }
        }

        // Ограничение ввода только цифр для телефона
        private void PhoneBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры и знак + (в начале номера)
            if (!Regex.IsMatch(e.Text, @"^[0-9\+]+$"))
            {
                e.Handled = true;
            }

            // Если ввод начинается с +, разрешаем только если это начало текста
            var textBox = sender as TextBox;
            if (e.Text == "+" && textBox != null && textBox.Text.Length > 0)
            {
                e.Handled = true;
            }
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = UserNameBox.Text.Trim();
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string confirmPassword = ConfirmPasswordBox.Password.Trim();
            string phone = PhoneBox.Text.Trim();

            // 1. Проверка на пустые поля
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) ||
                string.IsNullOrEmpty(phone))
            {
                ShowErrorMessage("Заполните все поля.");
                return;
            }

            if (password.Length < 8)
            {
                ShowErrorMessage("Пароль должен содержать не менее 8 символов.");
                return;
            }

            // 2. Проверка на совпадение паролей
            if (password != confirmPassword)
            {
                ShowErrorMessage("Пароли не совпадают.");
                return;
            }

            // 3. Проверка формата телефона (простой шаблон)
            if (!Regex.IsMatch(phone, @"^\+?\d{10,15}$"))
            {
                ShowErrorMessage("Введите корректный номер телефона.");
                return;
            }

            // 4. Проверка уникальности логина
            if (_context.Users.Any(u => u.Login == login))
            {
                ShowErrorMessage("Пользователь с таким логином уже существует.");
                return;
            }

            // 5. Добавление нового пользователя
            var newUser = new Users
            {
                Name = fullName,
                Login = login,
                Password = password, // В проде — использовать хеширование!
                Phone = phone,
                RoleID = 2 // Предположим, 2 — обычный пользователь. Уточни при необходимости.
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            MessageBox.Show("Регистрация прошла успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            // Переход на LoginPage
            NavigationService.Navigate(new LoginPage());
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ShowErrorMessage(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}

