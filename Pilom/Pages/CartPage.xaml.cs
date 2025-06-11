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
    /// Логика взаимодействия для CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        private PilomEntities _context = new PilomEntities();
        private Users _currentUser;
        private List<Korzina> _cartItems;
        public CartPage(Users currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadCart();
        }
        private void LoadCart()
        {
            _cartItems = _context.Korzina
                .Where(k => k.UserID == _currentUser.UserID)
                .ToList();

            CartItemsList.ItemsSource = _cartItems;

            decimal total = _cartItems.Sum(i => i.Products.Price * i.Quantity);
            TotalPriceText.Text = $"Итого: ${total:F2}";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Korzina item)
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductID == item.ProductID);
                if (product != null && item.Quantity < product.StockQ)
                {
                    item.Quantity++;
                    _context.SaveChanges();
                    LoadCart();
                }
                else
                {
                    MessageBox.Show($"Достигнуто максимальное доступное количество. На складе: {product.StockQ} шт.");
                }
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Korzina item)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    _context.Korzina.Remove(item);
                }
                _context.SaveChanges();
                LoadCart();
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Korzina item)
            {
                _context.Korzina.Remove(item);
                _context.SaveChanges();
                LoadCart();
            }
        }

        private void PlaceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("Ваша корзина пуста.");
                return;
            }

            try
            {
                // Создание нового заказа
                var newOrder = new Orders
                {
                    UserID = _currentUser.UserID,
                    OrderDate = DateTime.Now
                };

                _context.Orders.Add(newOrder);
                _context.SaveChanges(); // Сохраняем заказ, чтобы получить ID

                foreach (var item in _cartItems)
                {
                    var product = item.Products;

                    if (product.StockQ < item.Quantity)
                    {
                        MessageBox.Show($"Недостаточно товара \"{product.Name}\" на складе. Доступно: {product.StockQ}");
                        return;
                    }

                    // Добавляем детали заказа
                    var orderDetail = new OrderDetails
                    {
                        OrderID = newOrder.OrderID,
                        ProductID = product.ProductID,
                        Quantity = item.Quantity
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // Уменьшаем количество на складе
                    product.StockQ -= item.Quantity;
                }

                // Очищаем корзину
                _context.Korzina.RemoveRange(_cartItems);

                // Сохраняем все изменения
                _context.SaveChanges();

                MessageBox.Show("Заказ успешно оформлен!");
                LoadCart();
                NavigationService.Navigate(new OrdersPage(_currentUser));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при оформлении заказа: " + ex.Message);
            }
        }
    }
}
