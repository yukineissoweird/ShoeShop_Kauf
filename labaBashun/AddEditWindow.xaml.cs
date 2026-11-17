
using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace labaBashun
{
    public partial class AddEditWindow : Window
    {
        private Product _currentProduct = new Product();

        public AddEditWindow(Product prod)
        {
            InitializeComponent();

            CmbCategory.ItemsSource = labaEntities1.GetContext().Category.ToList();
            CmbSupplier.ItemsSource = labaEntities1.GetContext().Supplier.ToList();
            CmbMeasure.ItemsSource = labaEntities1.GetContext().Measure.ToList();
            CmbManufacturer.ItemsSource = labaEntities1.GetContext().Manufacturer.ToList();

            if (prod != null && prod.Id != 0)
            {
                _currentProduct = prod;
            }
            else
            {
                _currentProduct = new Product();
            }

            DataContext = _currentProduct;

            if (prod != null && prod.Id != 0)
            {
                TxtDescription.Text = _currentProduct.Description.ToString();
                TxtPrice.Text = _currentProduct.Price?.ToString();
                TxtQuantity.Text = _currentProduct.Quantity?.ToString();
                TxtDiscount.Text = _currentProduct.Discount?.ToString();
                TxtArticle.Text = _currentProduct.Article?.ToString();

                if (_currentProduct.Product_Name != null)
                {
                    var nameRow = labaEntities1.GetContext().Product_Name
                        .FirstOrDefault(x => x.id == _currentProduct.Product_Name);

                    if (nameRow != null)
                    {
                        TxtName.Text = nameRow.Product_Name1.ToString();
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _currentProduct.Article = TxtArticle.Text.Trim();
                _currentProduct.Description = TxtDescription.Text.Trim();

                if (decimal.TryParse(TxtPrice.Text, out decimal price))
                    _currentProduct.Price = price;

                if (int.TryParse(TxtQuantity.Text, out int quantity))
                    _currentProduct.Quantity = quantity;

                if (int.TryParse(TxtDiscount.Text, out int discount))
                    _currentProduct.Discount = discount;

                if (CmbCategory.SelectedValue != null)
                    _currentProduct.Category = Convert.ToInt32(CmbCategory.SelectedValue);

                if (CmbSupplier.SelectedValue != null)
                    _currentProduct.Supplier = Convert.ToInt32(CmbSupplier.SelectedValue);

                if (CmbMeasure.SelectedValue != null)
                    _currentProduct.Measure = Convert.ToInt32(CmbMeasure.SelectedValue);

                if (CmbManufacturer.SelectedValue != null)
                    _currentProduct.Manufacturer = Convert.ToInt32(CmbManufacturer.SelectedValue);

                string enteredName = TxtName.Text.Trim();

                if (!string.IsNullOrWhiteSpace(enteredName))
                {
                    var existingName = labaEntities1.GetContext().Product_Name
                        .FirstOrDefault(x =>
                            x.Product_Name1.ToLower() == enteredName.ToLower());

                    if (existingName != null)
                    {
                        _currentProduct.Product_Name = existingName.id;
                    }
                    else
                    {
                        var newNameRow = new Product_Name();

                        int maxNameId = labaEntities1.GetContext().Product_Name.Any()
                            ? labaEntities1.GetContext().Product_Name.Max(x => x.id)
                            : 0;

                        newNameRow.id = maxNameId + 1;
                        newNameRow.Product_Name1 = enteredName;

                        labaEntities1.GetContext().Product_Name.Add(newNameRow);
                        labaEntities1.GetContext().SaveChanges();

                        _currentProduct.Product_Name = newNameRow.id;
                    }
                }

                if (_currentProduct.Id == 0)
                {
                    int maxId = labaEntities1.GetContext().Product.Any()
                        ? labaEntities1.GetContext().Product.Max(x => x.Id)
                        : 0;

                    _currentProduct.Id = maxId + 1;

                    labaEntities1.GetContext().Product.Add(_currentProduct);
                }

                labaEntities1.GetContext().SaveChanges();

                MessageBox.Show(
                    "Данные успешно сохранены!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                string innerError = ex.InnerException != null
                    ? ex.InnerException.InnerException?.Message
                        ?? ex.InnerException.Message
                    : ex.Message;

                MessageBox.Show(
                    $"Ошибка базы данных: {innerError}",
                    "Критическая ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                string targetDirectory = @"C:\Users\kkauf\OneDrive\Desktop\W2\";

                if (!System.IO.Directory.Exists(targetDirectory))
                {
                    System.IO.Directory.CreateDirectory(targetDirectory);
                }

                string oldPath = null;

                if (!string.IsNullOrEmpty(_currentProduct.Photo))
                {
                    oldPath = _currentProduct.Photo.Contains(@":\")
                        ? _currentProduct.Photo
                        : System.IO.Path.Combine(
                            targetDirectory,
                            _currentProduct.Photo);
                }

                OpenFileDialog dialog = new OpenFileDialog
                {
                    DefaultExt = ".png",
                    Filter = "Image Filters |*.jpg;*.jpeg;*.png;*.bmp;*.gif;",
                    InitialDirectory = targetDirectory
                };

                if (dialog.ShowDialog() == true)
                {
                    string sourseFilePath = dialog.FileName;

                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = new Uri(sourseFilePath);
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();

                    if (img.PixelWidth > 300 || img.PixelHeight > 200)
                    {
                        MessageBox.Show(
                            "Размер фото превышает лимит 300Х200 пикселей!",
                            "Ошибка валидации",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    PhotoProduct.Source = null;

                    if (oldPath != null && System.IO.File.Exists(oldPath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldPath);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"Не удалось удалить старый файл: {ex.Message}");
                        }
                    }

                    string _photoPath = dialog.SafeFileName;
                    TxtPhotoName.Text = _photoPath;

                    string newPath = System.IO.Path.Combine(
                        targetDirectory,
                        _photoPath);

                    if (sourseFilePath.ToLower() != newPath.ToLower())
                    {
                        System.IO.File.Copy(
                            sourseFilePath,
                            newPath,
                            overwrite: true);
                    }

                    _currentProduct.Photo = newPath;
                    PhotoProduct.Source = img;

                    MessageBox.Show(
                        "Изображение успешно обновлено!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка работы программы с изображением: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
