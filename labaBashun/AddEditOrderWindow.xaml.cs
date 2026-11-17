using System;
using System.Linq;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Windows;
using System.Runtime.InteropServices;

namespace labaBashun
{
    public partial class AddEditOrderWindow : Window
    {
        private readonly int? _orderId;

        public AddEditOrderWindow(int? orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            HeaderText.Text = _orderId.HasValue ? "Редактирование заказа" : "Добавление заказа";
            LoadFormData();
        }

        private void LoadFormData()
        {
            try
            {
                using (var db = new labaEntities1())
                {
                    Order_List current = _orderId.HasValue
                        ? db.Order_List.FirstOrDefault(x => x.Id_Order == _orderId.Value)
                        : null;
                    if (_orderId.HasValue && current == null)
                    {
                        MessageBox.Show("Заказ не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        IsEnabled = false;
                        return;
                    }

                    var products = db.Product.ToList()
                        .Where(p => !string.IsNullOrWhiteSpace(p.Article) &&
                                    (!db.Order_List.Any(o => o.Id_Order == p.Id) ||
                                    (current != null && p.Id == current.Id_Order)))
                        .OrderBy(p => p.Article).ToList();
                    ArticleCombo.ItemsSource = products;
                    StatusCombo.ItemsSource = db.Order_Status.OrderBy(s => s.Id_Order_Status).ToList();

                    var pickups = db.Pickup_Point.ToList().Select(point => new
                    {
                        ID = point.ID,
                        DisplayAddress = BuildPickupAddress(point)
                    }).ToList();
                    PickupCombo.ItemsSource = pickups;

                    if (current != null)
                    {
                        ArticleCombo.SelectedValue = current.Id_Order;
                        StatusCombo.SelectedValue = current.Order_Status;
                        PickupCombo.SelectedValue = current.Address_point_of_issue;
                        OrderDatePicker.SelectedDate = current.Order_date;
                        DeliveryDatePicker.SelectedDate = current.Delivery_date;
                    }
                    else
                    {
                        OrderDatePicker.SelectedDate = DateTime.Today;
                        DeliveryDatePicker.SelectedDate = DateTime.Today.AddDays(7);
                        if (StatusCombo.Items.Count > 0) StatusCombo.SelectedIndex = 0;
                        if (PickupCombo.Items.Count > 0) PickupCombo.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось подготовить форму заказа.\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                IsEnabled = false;
            }
        }

        private static string BuildPickupAddress(Pickup_Point point)
        {
            var parts = new[] { point.Mail_Index, point.City, point.Address, point.Number_Address }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            return string.Join(", ", parts);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ArticleCombo.SelectedValue == null || StatusCombo.SelectedValue == null ||
                PickupCombo.SelectedValue == null || !OrderDatePicker.SelectedDate.HasValue ||
                !DeliveryDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Заполните все поля заказа.", "Проверка данных",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var orderDate = OrderDatePicker.SelectedDate.Value.Date;
            var deliveryDate = DeliveryDatePicker.SelectedDate.Value.Date;
            if (deliveryDate < orderDate)
            {
                MessageBox.Show("Дата выдачи не может быть раньше даты заказа.", "Проверка данных",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int productId = Convert.ToInt32(ArticleCombo.SelectedValue);
            int statusId = Convert.ToInt32(StatusCombo.SelectedValue);
            int pickupId = Convert.ToInt32(PickupCombo.SelectedValue);

            try
            {
                using (var db = new labaEntities1())
                {
                    var connection = ((IObjectContextAdapter)db).ObjectContext.Connection;
                    if (connection.State != ConnectionState.Open) connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                    var product = db.Product.FirstOrDefault(p => p.Id == productId);
                    if (product == null)
                    {
                        MessageBox.Show("Выбранный товар не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var duplicate = db.Order_List.Any(o => o.Id_Order == productId &&
                        (!_orderId.HasValue || o.Id_Order != _orderId.Value));
                    if (duplicate)
                    {
                        MessageBox.Show("Для этого товара уже существует заказ. В текущей структуре базы один товар может быть связан только с одним заказом.",
                            "Невозможно сохранить", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (_orderId.HasValue)
                    {
                        var order = db.Order_List.FirstOrDefault(o => o.Id_Order == _orderId.Value);
                        if (order == null)
                        {
                            MessageBox.Show("Редактируемый заказ больше не существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (order.Id_Order != productId)
                        {
                            // Id_Order является одновременно PK и FK на Product, поэтому меняем ключ
                            // безопасно через удаление и повторное создание записи в одной транзакции.
                            int? itemId = order.ID_Order_list;
                            int? clientId = order.Client_Name;
                            int? code = order.Code;
                            int? quantity = order.Quantity;
                            var item = itemId.HasValue
                                ? db.Order_Items.FirstOrDefault(i => i.Id_Order_list == itemId.Value)
                                : null;
                            db.Order_List.Remove(order);
                            db.SaveChanges();

                            if (item != null)
                            {
                                item.ID_Order = productId;
                                item.Article_Order = product.Article;
                                item.Quantity = quantity ?? 1;
                            }
                            var replacement = new Order_List
                            {
                                Id_Order = productId,
                                ID_Order_list = itemId,
                                Article_Order = product.Article,
                                Quantity = quantity,
                                Order_date = orderDate,
                                Delivery_date = deliveryDate,
                                Address_point_of_issue = pickupId,
                                Client_Name = clientId,
                                Code = code,
                                Order_Status = statusId
                            };
                            db.Order_List.Add(replacement);
                        }
                        else
                        {
                            order.Article_Order = product.Article;
                            order.Order_date = orderDate;
                            order.Delivery_date = deliveryDate;
                            order.Address_point_of_issue = pickupId;
                            order.Order_Status = statusId;
                            if (order.ID_Order_list.HasValue)
                            {
                                var item = db.Order_Items.FirstOrDefault(i => i.Id_Order_list == order.ID_Order_list.Value);
                                if (item != null)
                                {
                                    item.Article_Order = product.Article;
                                    item.ID_Order = productId;
                                }
                            }
                        }
                    }
                    else
                    {
                        int newItemId = db.Order_Items.Any()
                            ? db.Order_Items.Max(i => i.Id_Order_list) + 1 : 1;
                        var item = new Order_Items
                        {
                            Id_Order_list = newItemId,
                            ID_Order = productId,
                            Article_Order = product.Article,
                            Quantity = 1
                        };
                        db.Order_Items.Add(item);
                        db.SaveChanges();

                        var order = new Order_List
                        {
                            Id_Order = productId,
                            ID_Order_list = newItemId,
                            Article_Order = product.Article,
                            Quantity = 1,
                            Order_date = orderDate,
                            Delivery_date = deliveryDate,
                            Address_point_of_issue = pickupId,
                            Order_Status = statusId
                        };
                        db.Order_List.Add(order);
                    }

                    db.SaveChanges();
                    transaction.Commit();
                    }
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить заказ. Проверьте связи и ограничения базы данных.\n" + ex.Message,
                    "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
