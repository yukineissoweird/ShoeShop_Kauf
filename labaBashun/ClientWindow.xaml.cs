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
    /// Логика взаимодействия для ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        public string ClientName { get; set; }
        public ClientWindow(string userName)
        {
            InitializeComponent();
            var curr = labaEntities1.GetContext().Product.ToList();
            ListProduct.ItemsSource = curr;
            Txt_Name.Text = labaEntities1.GetContext().Order_List.ToString();
            ClientName = userName;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
