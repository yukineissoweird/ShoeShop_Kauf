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

namespace labaBashun
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var userobj = labaEntities1.GetContext().Employee.FirstOrDefault(x => x.Login == Login.Text && x.Password == Password.Password);
                if (userobj == null)
                {
                    MessageBox.Show("Такого пользователя нет!", "Ошибка Авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {

                    switch (userobj.Id_Role)
                    {
                        case 1:
                            MessageBox.Show("Здравствуйте Администратор" + userobj.Employee_Name + "!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                            labaEntities1.GetContext();
                            this.Hide();
                            AdminWindow adminPage = new AdminWindow();
                            adminPage.ShowDialog();
                            this.Show();
                            break;
                        case 2:
                            MessageBox.Show("Здравствуйте Менеджер" + userobj.Employee_Name + "!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                            labaEntities1.GetContext();
                            this.Hide();
                            ManagerWindow managerPage = new ManagerWindow();
                            managerPage.ShowDialog();
                            this.Show();
                            break;
                        case 3:
                            MessageBox.Show("Здравствуйте Авторизированный клиент " + userobj.Employee_Name + "!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                            labaEntities1.GetContext();
                            this.Hide();
                            ClientWindow clientPage = new ClientWindow(userobj.Employee_Name);
                            clientPage.ShowDialog();
                            this.Show();
                            break;
                        default:
                            MessageBox.Show("Данные не обнаруженны!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                    }
                }

            }

            catch (Exception Ex)
            {
                MessageBox.Show("Ошибка" + Ex.Message.ToString() + "Критическая работа приложения!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Здравствуйте гость " + "!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            labaEntities1.GetContext();
            PageGuest pageGuest = new PageGuest();
            pageGuest.ShowDialog();
        }
    }
}
