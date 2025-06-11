using Pilom.AppData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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
using ZXing;
using System.Drawing;

namespace Pilom.Pages
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        // Контекст базы данных
        private PilomEntities _context = new PilomEntities();
        public static int rol;

        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Управление видимостью плейсхолдера для логина
            LoginPlaceholder.Visibility = string.IsNullOrEmpty(LoginBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Скрываем сообщение об ошибке при изменении текста
            ErrorMessage.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Управление видимостью плейсхолдера для пароля
            PasswordPlaceholder.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Скрываем сообщение об ошибке при изменении пароля
            ErrorMessage.Visibility = Visibility.Collapsed;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowErrorMessage("Пожалуйста, введите логин и пароль");
                return;
            }

            try
            {
                // Поиск пользователя в базе данных
                var user = _context.Users
                        .Include("UserRoles") // Используем строку для указания навигационного свойства
                        .FirstOrDefault(u => u.Login == login && u.Password == password);

                if (user != null)
                {
                    // Сохраняем текущего пользователя в статическом свойстве
                    App.Current.Properties["CurrentUser"] = user;

                    // Перенаправляем в зависимости от роли
                    if (user.RoleID == 1) // Администратор
                    {
                        rol = user.RoleID;

                        NavigationService.Navigate(new AdminPage(user));

                    }
                    else // Обычный пользователь
                    {
                        NavigationService.Navigate(new UserPage(user));
                    }
                }
                else
                {
                    ShowErrorMessage("Неверный логин или пароль");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Ошибка при подключении к базе данных: " + ex.Message);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу регистрации
            NavigationService.Navigate(new RegisterPage());
        }

        private void ShowErrorMessage(string message)
        {
            // Отображение сообщения об ошибке
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
        private void InputFields_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(null, null);
            }
        }


        private void Btn_qrcode_Click(object sender, RoutedEventArgs e)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 300,
                    Height = 300
                }
            };
            var result = writer.Write(@"https://t.me/nn6le9niy");
            var bitmap = new BitmapImage();
            using (var memoryStream = new MemoryStream())
            {
                result.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;
                bitmap.BeginInit();
                bitmap.StreamSource = memoryStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
            }
            imgQr.Source = bitmap;
        }


    }
}

