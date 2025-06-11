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
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        private PilomEntities _context = new PilomEntities();
        private Users _currentUser;
        public OrdersPage(Users currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadOrders();
        }
        private void LoadOrders()
        {
            var userOrders = _context.Orders
                .Where(o => o.UserID == _currentUser.UserID)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var displayOrders = userOrders.Select(order => new OrderDisplay
            {
                OrderHeader = $"Заказ от {order.OrderDate.ToShortDateString()}",
                Items = order.OrderDetails.Select(podr => new OrderItemDisplay
                {
                    ProductName = podr.Products.Name,
                    Quantity = podr.Quantity,
                    TotalPrice = podr.Products.Price * podr.Quantity
                }).ToList(),
                TotalOrderPrice = order.OrderDetails.Sum(p => p.Products.Price * p.Quantity)
            }).ToList();

            OrdersList.ItemsSource = displayOrders;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserPage(_currentUser));
        }

        // Классы для отображения
        private class OrderDisplay
        {
            public string OrderHeader { get; set; }
            public List<OrderItemDisplay> Items { get; set; }
            public decimal TotalOrderPrice { get; set; }
        }

        private class OrderItemDisplay
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
}
