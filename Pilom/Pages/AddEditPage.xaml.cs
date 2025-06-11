using Pilom.AppData;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Pilom.Pages
{
    public partial class AddEditPage : Page
    {
        private Products _currentProduct;
        private bool _isEditMode;
        private string _imagePath; // полный путь к файлу изображения

        private PilomEntities _context = new PilomEntities();

        public AddEditPage(Products product = null)
        {
            InitializeComponent();

            _isEditMode = product != null;
            _currentProduct = product ?? new Products();

            PageTitleTextBlock.Text = _isEditMode ? "Редактирование продукта" : "Добавление продукта";

            CategoryComboBox.ItemsSource = _context.Categories.ToList();
            WoodTypeComboBox.ItemsSource = _context.WoodTypes.ToList();
            UnitComboBox.ItemsSource = _context.Units.ToList();

            if (_isEditMode)
                LoadProductData();
        }

        private void LoadProductData()
        {
            NameBox.Text = _currentProduct.Name;
            DescriptionBox.Text = _currentProduct.Description;
            CategoryComboBox.SelectedValue = _currentProduct.CategoryID;
            WoodTypeComboBox.SelectedValue = _currentProduct.WoodTypeID;
            UnitComboBox.SelectedValue = _currentProduct.UnitID;
            LengthBox.Text = _currentProduct.Length?.ToString();
            WidthBox.Text = _currentProduct.Width?.ToString();
            ThicknessBox.Text = _currentProduct.Thickness?.ToString();
            PriceBox.Text = _currentProduct.Price.ToString();
            StockQBox.Text = _currentProduct.StockQ?.ToString();

            // Если есть имя файла изображения, пробуем сформировать полный путь и загрузить картинку
            if (!string.IsNullOrEmpty(_currentProduct.Image))
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                _imagePath = Path.Combine(baseDir, "Images", _currentProduct.Image);

                if (File.Exists(_imagePath))
                {
                    try
                    {
                        imgPreview.Source = new BitmapImage(new Uri(_imagePath));
                        ImagePathTextBox.Text = _imagePath;
                    }
                    catch
                    {
                        SetPlaceholderImage();
                    }
                }
                else
                {
                    SetPlaceholderImage();
                }
            }
            else
            {
                SetPlaceholderImage();
            }
        }

        private void SetPlaceholderImage()
        {
            string placeholderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "picture.jpg");
            if (File.Exists(placeholderPath))
                imgPreview.Source = new BitmapImage(new Uri(placeholderPath));
            else
                imgPreview.Source = null;

            ImagePathTextBox.Text = "";
            _imagePath = null;
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg)|*.png;*.jpg"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _imagePath = dialog.FileName;
                    ImagePathTextBox.Text = _imagePath;

                    imgPreview.Source = new BitmapImage(new Uri(_imagePath));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка загрузки изображения: " + ex.Message);
                    SetPlaceholderImage();
                }
            }
        }

        //private string ValidateImagePath(string imagePath)
        //{
        //    string defaultImage = "picture.jpg";

        //    if (string.IsNullOrWhiteSpace(imagePath))
        //        return defaultImage;

        //    string fileName = Path.GetFileName(imagePath);
        //    string imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
        //    string fullPath = Path.Combine(imagesDirectory, fileName);

        //    if (!File.Exists(fullPath))
        //        return defaultImage;

        //    return fileName;
        //}

        private string ValidateImagePath(string imagePath)
        {
            // Заглушка по умолчанию
            string defaultImage = "picture.jpg";

            // Проверка на null или пустую строку
            if (string.IsNullOrWhiteSpace(imagePath))
                return defaultImage;

            // Получаем только имя файла
            string fileName = System.IO.Path.GetFileName(imagePath);

            // Путь до папки с изображениями (зависит от твоего проекта — укажи свой путь)
            string imagesDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

            // Полный путь до файла
            string fullPath = System.IO.Path.Combine(imagesDirectory, fileName);

            // Проверка на существование файла
            if (!System.IO.File.Exists(fullPath))
                return defaultImage;

            return fileName;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _currentProduct.Name = NameBox.Text.Trim();
            _currentProduct.Description = DescriptionBox.Text.Trim();
            _currentProduct.CategoryID = (int?)CategoryComboBox.SelectedValue;
            _currentProduct.WoodTypeID = (int?)WoodTypeComboBox.SelectedValue;
            _currentProduct.UnitID = (int?)UnitComboBox.SelectedValue;

            if (int.TryParse(LengthBox.Text, out int length))
                _currentProduct.Length = length;
            else
                _currentProduct.Length = null;

            if (int.TryParse(WidthBox.Text, out int width))
                _currentProduct.Width = width;
            else
                _currentProduct.Width = null;

            if (int.TryParse(ThicknessBox.Text, out int thickness))
                _currentProduct.Thickness = thickness;
            else
                _currentProduct.Thickness = null;

            if (decimal.TryParse(PriceBox.Text, out decimal price))
                _currentProduct.Price = price;
            else
            {
                MessageBox.Show("Некорректное значение цены.");
                return;
            }

            if (int.TryParse(StockQBox.Text, out int stockQ))
                _currentProduct.StockQ = stockQ;
            else
                _currentProduct.StockQ = null;

            // Копируем файл в папку Images (если выбрали новый)
            if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
            {
                string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                string destPath = Path.Combine(imagesDir, Path.GetFileName(_imagePath));
                try
                {
                    File.Copy(_imagePath, destPath, true);
                }
                catch
                {
                    // Можно обработать ошибку копирования, если нужно
                }
            }

            // Сохраняем в БД только имя файла, либо заглушку, если файл невалидный
            _currentProduct.Image = ValidateImagePath(_imagePath);
            _currentProduct.Image = ValidateImagePath(ImagePathTextBox.Text);
            try
            {
                if (!_isEditMode)
                    _context.Products.Add(_currentProduct);

                _context.SaveChanges();

                MessageBox.Show("Продукт успешно сохранён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePlaceholders();
        }

        private void UpdatePlaceholders()
        {
            NamePlaceholder.Visibility = string.IsNullOrWhiteSpace(NameBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            DescriptionPlaceholder.Visibility = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            LengthPlaceholder.Visibility = string.IsNullOrWhiteSpace(LengthBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            WidthPlaceholder.Visibility = string.IsNullOrWhiteSpace(WidthBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            ThicknessPlaceholder.Visibility = string.IsNullOrWhiteSpace(ThicknessBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            PricePlaceholder.Visibility = string.IsNullOrWhiteSpace(PriceBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            StockQPlaceholder.Visibility = string.IsNullOrWhiteSpace(StockQBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            ImagePlaceholder.Visibility = string.IsNullOrWhiteSpace(ImagePathTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;

            CategoryPlaceholder.Visibility = CategoryComboBox.SelectedItem == null ? Visibility.Visible : Visibility.Collapsed;
            WoodTypePlaceholder.Visibility = WoodTypeComboBox.SelectedItem == null ? Visibility.Visible : Visibility.Collapsed;
            UnitPlaceholder.Visibility = UnitComboBox.SelectedItem == null ? Visibility.Visible : Visibility.Collapsed;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
