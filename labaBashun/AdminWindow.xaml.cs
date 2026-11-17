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
using System.Windows.Shapes;

namespace labaBashun
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private int _currentSort = 0;
        public AdminWindow()
        {
            InitializeComponent();
            InitializeComponent();
            var curr = labaEntities1.GetContext().Product.ToList();
            ListProduct.ItemsSource = curr;

            var suppliers = labaEntities1.GetContext().Supplier.ToList();
            suppliers.Insert(0, new Supplier { Supplier_Name = "Все поставщики", ID_provider = 0 });

            Cmb_Supplier.ItemsSource = suppliers;
            Cmb_Supplier.SelectedIndex = 0;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Cmb_Supplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var searchList = labaEntities1.GetContext().Product.ToList();

            if (Cmb_Supplier.SelectedValue != null)
            {
                int selectedId = (int)Cmb_Supplier.SelectedValue;

                if (selectedId != 0)
                {
                    searchList = searchList.Where(x => x.Supplier == selectedId).ToList();
                }
            }

            ListProduct.ItemsSource = searchList;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchList = labaEntities1.GetContext().Product.ToList();

            string text = TxtSearch.Text.ToLower();
            if (!string.IsNullOrEmpty(text))
            {
                searchList = searchList.Where(x => x.Product_Name1 != null && x.Product_Name1.Product_Name1.ToLower().Contains(text)).ToList();
            }

            if (Cmb_Supplier.SelectedValue != null)
            {
                int selectedId = (int)Cmb_Supplier.SelectedValue;
                if (selectedId != 0) searchList = searchList.Where(x => x.Supplier == selectedId).ToList();
            }

            if (_currentSort == 1) searchList = searchList.OrderBy(x => x.Quantity).ToList();
            if (_currentSort == 2) searchList = searchList.OrderByDescending(x => x.Quantity).ToList();

            ListProduct.ItemsSource = searchList;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _currentSort = 1;
            var searchList = labaEntities1.GetContext().Product.ToList();

            if (!string.IsNullOrEmpty(TxtSearch.Text))
            {
                string text = TxtSearch.Text.ToLower();
                searchList = searchList.Where(x => x.Product_Name1 != null && x.Product_Name1.Product_Name1.ToLower().Contains(text)).ToList();
            }

            if (Cmb_Supplier.SelectedValue != null)
            {
                int selectedId = (int)Cmb_Supplier.SelectedValue;
                if (selectedId != 0) searchList = searchList.Where(x => x.Supplier == selectedId).ToList();
            }

            searchList = searchList.OrderBy(x => x.Quantity).ToList();
            ListProduct.ItemsSource = searchList;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _currentSort = 2;
            var searchList = labaEntities1.GetContext().Product.ToList();

            if (!string.IsNullOrEmpty(TxtSearch.Text))
            {
                string text = TxtSearch.Text.ToLower();
                searchList = searchList.Where(x => x.Product_Name1 != null && x.Product_Name1.Product_Name1.ToLower().Contains(text)).ToList();
            }

            if (Cmb_Supplier.SelectedValue != null)
            {
                int selectedId = (int)Cmb_Supplier.SelectedValue;
                if (selectedId != 0) searchList = searchList.Where(x => x.Supplier == selectedId).ToList();
            }

            searchList = searchList.OrderByDescending(x => x.Quantity).ToList();
            ListProduct.ItemsSource = searchList;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            _currentSort = 0;
            TxtSearch.Text = "";
            Cmb_Supplier.SelectedIndex = 0;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            AddEditWindow addEditWindow = new AddEditWindow(new Product());
            addEditWindow.ShowDialog();
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;

            if (item != null)
            {
                var selectedProduct = item.DataContext as Product;

                if (selectedProduct != null)
                {
                    AddEditWindow editWindow = new AddEditWindow(selectedProduct);

                    if (editWindow.ShowDialog() == true)
                    {
                        ListProduct.ItemsSource = labaEntities1.GetContext().Product.ToList();
                    }
                }
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var del = ListProduct.SelectedItems.Cast<Product>().ToList();
            if (MessageBox.Show($"Вы точно хотите удалить следующие элементы? ({del.Count()})", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    foreach (Product p in del)
                    {
                        labaEntities1.GetContext().Product.Remove(p);
                    }
                    labaEntities1.GetContext().SaveChanges();
                    MessageBox.Show("Данные удалены!");
                    ListProduct.ItemsSource = labaEntities1.GetContext().Product.ToList();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            labaEntities1.GetContext();
            this.Hide();
            OrderListWindow window = new OrderListWindow(true);
            window.Owner = this;
            window.ShowDialog();
        }
    }
}
