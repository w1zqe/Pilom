using Pilom.AppData;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Логика взаимодействия для UserPage.xaml
    /// </summary>
    public partial class UserPage : Page
    {
        private PilomEntities _context = new PilomEntities();
        private List<Products> _products;

        private Users _currentUser; // ← задаётся извне, можно через конструктор

        public UserPage(Users CurrentUser)
        {
            InitializeComponent();
            _currentUser = CurrentUser;
            LoadData();
        }
        public class StockToVisibilityConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is int stockQ)
                {
                    bool isAvailable = stockQ > 0;

                    // Если передан параметр "Inverse", инвертируем результат
                    if (parameter is string param && param == "Inverse")
                        isAvailable = !isAvailable;

                    return isAvailable ? Visibility.Visible : Visibility.Collapsed;
                }
                return Visibility.Collapsed;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Products product)
            {
                //// Добавляем проверку наличия товара
                //if (product.StockQ <= 0)
                //{
                //    MessageBox.Show("Этот товар закончился на складе и недоступен для заказа.");
                //    return;
                //}

                var existingItem = _context.Korzina.FirstOrDefault(k => k.UserID == _currentUser.UserID && k.ProductID == product.ProductID);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    _context.Korzina.Add(new Korzina
                    {
                        UserID = _currentUser.UserID,
                        ProductID = product.ProductID,
                        Quantity = 1
                    });
                }

                _context.SaveChanges();

                MessageBox.Show($"Товар \"{product.Name}\" добавлен в корзину.");
            }
        }

        private void LoadData()
        {
            _products = _context.Products.ToList();

            var categories = _context.Categories.ToList();
            categories.Insert(0, new Categories { CategoryID = -1, Name = "Все категории" });

            CategoryBox.ItemsSource = categories;
            CategoryBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            IEnumerable<Products> filtered = _products;

            string search = SearchBox.Text.ToLower();
            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(p => p.Name.ToLower().Contains(search));

            if (CategoryBox.SelectedItem is Categories category && category.CategoryID != -1)
                filtered = filtered.Where(p => p.CategoryID == category.CategoryID);

            switch ((SortBox.SelectedItem as ComboBoxItem)?.Content.ToString())
            {
                case "По названию":
                    filtered = filtered.OrderBy(p => p.Name);
                    break;
                case "По цене (возр)":
                    filtered = filtered.OrderBy(p => p.Price);
                    break;
                case "По цене (убыв)":
                    filtered = filtered.OrderByDescending(p => p.Price);
                    break;
            }

            ProductList.ItemsSource = filtered.ToList();
            ProductCountText.Text = $"Найдено товаров: {filtered.Count()}";
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

        private void ProductList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProductList.SelectedItem is Products selectedProduct)
            {
                NavigationService.Navigate(new ProductDetailsPage(selectedProduct, _currentUser));
            }
        }
        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CartPage(_currentUser));
        }
        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new UserProfilePage(_currentUser));
        }



        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OrdersPage(_currentUser));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
        private void ProductList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно оставить пустым, если не используешь одиночный выбор
        }

    }
}
