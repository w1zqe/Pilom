using Pilom.AppData;
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

namespace Pilom.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrdersAdminPage.xaml
    /// </summary>
    public partial class OrdersAdminPage : Page
    {
        private readonly PilomEntities _context = new PilomEntities();

        public OrdersAdminPage()
        {
            InitializeComponent();
            LoadOrders();
        }
        // Загрузка списка заказов
        private void LoadOrders()
        {
            OrdersGrid.ItemsSource = _context.Orders
                .Include("Users") // Подгружаем связанного пользователя 
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        // Просмотр состава заказа
        private void ShowOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                var orderDetails = _context.OrderDetails
                    .Where(op => op.OrderID == orderId)
                    .Select(op => new
                    {
                        Товар = op.Products.Name,
                        Количество = op.Quantity,
                        Цена = op.Products.Price
                    })
                    .ToList();

                string details = string.Join("\n", orderDetails.Select(d => $"{d.Товар} x{d.Количество} ({d.Цена} руб.)"));
                MessageBox.Show(details, $"Состав заказа #{orderId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
