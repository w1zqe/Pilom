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
    /// Логика взаимодействия для ProductDetailsAdminPage.xaml
    /// </summary>
    public partial class ProductDetailsAdminPage : Page
    {
        private PilomEntities _context = new PilomEntities();
        private Products _product;
        private Users _currentUser;
        public ProductDetailsAdminPage(Products product, Users currentUser)
        {
            InitializeComponent();
            _product = product;

            LoadProductData();
        }
        private void LoadProductData()
        {
            NameText.Text = _product.Name;
            DescriptionText.Text = _product.Description;
            PriceText.Text = $"Цена: {_product.Price:C}";
            StockText.Text = $"В наличии: {_product.StockQ} шт.";
            BrandText.Text = $"Длина: {_product.Length} мм";
            CategoryText.Text = $"Ширина: {_product.Width} мм";
            CountryText.Text = $"Толщина: {_product.Thickness} мм";

            try
            {
                ProductImage.Source = new BitmapImage(new Uri(_product.CurrentPhoto, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                ProductImage.Source = new BitmapImage(new Uri("/Images/picture.jpg", UriKind.Relative));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

            NavigationService.Navigate(new AdminPage(_currentUser));
        }
    }
}

