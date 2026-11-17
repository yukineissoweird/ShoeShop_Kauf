using System;
using System.Linq;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Windows;
using System.Windows.Input;

namespace labaBashun
{
    public partial class OrderListWindow : Window
    {
        private readonly bool _isAdmin;

        public OrderListWindow(bool isAdmin)
        {
            InitializeComponent();

            _isAdmin = isAdmin;

            AddButton.Visibility = _isAdmin
                ? Visibility.Visible
                : Visibility.Collapsed;

            DeleteButton.Visibility = _isAdmin
                ? Visibility.Visible
                : Visibility.Collapsed;

            RoleText.Text = _isAdmin
                ? "Администратор: добавление, редактирование и удаление заказов"
                : "Режим просмотра: изменение заказов доступно только администратору";

            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                using (var db = new labaEntities1())
                {
                    var orders = db.Order_List.ToList();

                    var rows = orders.Select(o => new OrderRow
                    {
                        OrderId = o.Id_Order,
                        OrderItemId = o.ID_Order_list,
                        Article = o.Article_Order,

                        Status = o.Order_Status1 == null
                            ? "Не указан"
                            : o.Order_Status1.Order_Status1,

                        PickupAddress = FormatPickup(o.Pickup_Point),

                        OrderDate = o.Order_date,
                        DeliveryDate = o.Delivery_date

                    }).OrderBy(o => o.OrderDate).ToList();

                    OrdersGrid.ItemsSource = rows;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить заказы.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static string FormatPickup(Pickup_Point point)
        {
            if (point == null)
                return "Не указан";

            var parts = new[]
            {
                point.Mail_Index,
                point.City,
                point.Address,
                point.Number_Address
            }
            .Where(x => !string.IsNullOrWhiteSpace(x));

            return string.Join(", ", parts);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin)
                return;

            var window = new AddEditOrderWindow(null)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
                LoadOrders();
        }

        private void OrdersGrid_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!_isAdmin)
                return;

            var row = OrdersGrid.SelectedItem as OrderRow;

            if (row == null)
                return;

            var window = new AddEditOrderWindow(row.OrderId)
            {
                Owner = this
            };

            if (window.ShowDialog() == true)
                LoadOrders();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin)
                return;

            var selected = OrdersGrid.SelectedItems
                .Cast<OrderRow>()
                .ToList();

            if (selected.Count == 0)
            {
                MessageBox.Show(
                    "Выберите заказ для удаления.",
                    "Удаление",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            if (MessageBox.Show(
                "Удалить выбранные заказы (" + selected.Count + ")?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using (var db = new labaEntities1())
                {
                    var connection =
                        ((IObjectContextAdapter)db).ObjectContext.Connection;

                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        var ids = selected.Select(x => x.OrderId).ToList();

                        var orders = db.Order_List
                            .Where(x => ids.Contains(x.Id_Order))
                            .ToList();

                        var itemIds = orders
                            .Where(x => x.ID_Order_list.HasValue)
                            .Select(x => x.ID_Order_list.Value)
                            .Distinct()
                            .ToList();

                        foreach (var order in orders)
                            db.Order_List.Remove(order);

                        db.SaveChanges();

                        var usedItemIds = db.Order_List
                            .Where(o => o.ID_Order_list.HasValue)
                            .Select(o => o.ID_Order_list.Value)
                            .ToList();

                        var unusedItems = db.Order_Items
                            .Where(i => itemIds.Contains(i.Id_Order_list))
                            .ToList()
                            .Where(i => !usedItemIds.Contains(i.Id_Order_list))
                            .ToList();

                        foreach (var item in unusedItems)
                            db.Order_Items.Remove(item);

                        db.SaveChanges();

                        foreach (var item in unusedItems)
                            db.Order_Items.Remove(item);

                        db.SaveChanges();

                        transaction.Commit();
                    }
                }

                LoadOrders();

                MessageBox.Show(
                    "Заказы удалены.",
                    "Готово",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить заказы.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private class OrderRow
        {
            public int OrderId { get; set; }
            public int? OrderItemId { get; set; }
            public string Article { get; set; }
            public string Status { get; set; }
            public string PickupAddress { get; set; }
            public DateTime? OrderDate { get; set; }
            public DateTime? DeliveryDate { get; set; }
        }
    }
}