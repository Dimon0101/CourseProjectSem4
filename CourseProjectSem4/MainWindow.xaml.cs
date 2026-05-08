using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using CourseProjectSem4.Base;
using CourseProjectSem4.Commands;
using CourseProjectSem4.Database;
using CourseProjectSem4.Models;
using CourseProjectSem4.ViewModels;
using CourseProjectSem4.Observers;
using CourseProjectSem4.States;
using CourseProjectSem4.Strategics;
using CourseProjectSem4.Templates;

namespace CourseProjectSem4
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<RoomViewModel> _rooms  = new();
        private readonly ObservableCollection<Order>         _orders = new();
        private readonly ListCollectionView                  _freeRoomsView;
        private readonly Dictionary<int, RoomViewModel>      _orderToRoom = new();
        private readonly Dictionary<int, int>                   _orderDbId   = new();
        private readonly Dictionary<int, List<(DateTime from, DateTime to)>> _roomBookings = new();

        private readonly CommandInvoker _invoker = new();

        private readonly ObservableCollection<string> _eventLog = new();
        public ObservableCollection<string> EventLog     => _eventLog;
        public ObservableCollection<string> CommandHistory => _invoker.History;
        private readonly EventLogObserver             _eventLogObserver;
        private readonly BillingObserver              _billingObserver = new();

        private readonly StandardCheckOutProcessor _standardProcessor = new();
        private readonly DeluxeCheckOutProcessor   _deluxeProcessor   = new();

        private readonly RoomCreatorHandler _creator = new();

        public MainWindow()
        {
            InitializeComponent();
            OrderCheckInPicker.DisplayDateStart = DateTime.Today;
            OrderCheckInPicker.SelectedDate    = DateTime.Today;

            _eventLogObserver = new EventLogObserver(_eventLog);
            _freeRoomsView = new ListCollectionView(_rooms);
            OrderRoomBox.ItemsSource      = _freeRoomsView;
            RoomsItemsControl.ItemsSource = _rooms;
            OrdersListView.ItemsSource    = _orders;
            try
            {
                HotelDb.Initialize();
                HotelDb.LoadPriceConfig();
                AdminSession.Instance.LoadPassword(HotelDb.LoadPassword("admin123"));
                LoadRoomsFromDb();
                LoadOrdersFromDb();
                LoadLogsFromDb();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Не вдалось підключитись до бази даних:\n{ex.Message}\n\n" +
                    "Перевірте рядок підключення в Database/HotelDb.cs",
                    "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadPriceFields();
            UpdateStats();
            ApplyAdminMode();
            AdminSession.Instance.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(AdminSession.IsAdmin))
                    ApplyAdminMode();
            };
        }


        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ChangePasswordDialog { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    HotelDb.SavePassword(dlg.NewPassword);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Пароль змінено, але не вдалось зберегти в БД:\n{ex.Message}",
                        "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                MessageBox.Show("Пароль успішно змінено.", "Готово",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AdminToggle_Click(object sender, RoutedEventArgs e)
        {
            if (AdminSession.Instance.IsAdmin)
            {
                AdminSession.Instance.Logout();
            }
            else
            {
                var dlg = new AdminLoginDialog { Owner = this };
                if (dlg.ShowDialog() == true)
                {
                    if (!AdminSession.Instance.TryLogin(dlg.Password))
                        MessageBox.Show("Невірний пароль.", "Доступ заборонено",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ApplyAdminMode()
        {
            bool admin = AdminSession.Instance.IsAdmin;
            AdminModeLabel.Text    = AdminSession.Instance.ModeLabel;
            AdminToggleBtn.Content = AdminSession.Instance.ToggleLabel;
            AdminBadgeBrush.Color  = AdminSession.Instance.BadgeColor;
            AddRoomBtn.IsEnabled     = admin;
            SavePricesBtn.IsEnabled  = admin;
            ChangePasswordBtn.Visibility = admin ? Visibility.Visible : Visibility.Collapsed;
            ResetPricesBtn.IsEnabled = admin;
            PricePoor.IsReadOnly         = !admin;
            PriceNormal.IsReadOnly       = !admin;
            PriceDeluxe.IsReadOnly       = !admin;
            PriceWifi.IsReadOnly         = !admin;
            PriceAllInclusive.IsReadOnly = !admin;
            RefreshDeleteButtons(admin);
        }
        private void RefreshDeleteButtons(bool show)
        {
            foreach (var item in RoomsItemsControl.Items)
            {
                var container = RoomsItemsControl.ItemContainerGenerator
                    .ContainerFromItem(item) as FrameworkElement;
                if (container == null) continue;
                var btn = FindChild<Button>(container, "DeleteRoomBtn");
                if (btn != null)
                    btn.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void LoadRoomsFromDb()
        {
            _rooms.Clear();
            foreach (var dbRoom in HotelDb.LoadRooms())
            {
                var quality = dbRoom.Type switch
                {
                    "Normal" => RoomQuality.Normal,
                    "Deluxe" => RoomQuality.Deluxe,
                    _        => RoomQuality.Poor
                };

                var builder = new RoomBuilder();
                builder.CreateRoom(quality, 0, "");
                if (dbRoom.HasWiFi)      builder.AddWiFi();
                if (dbRoom.AllInclusive) builder.AddAllInclusive();
                var room = builder.Build();

                _rooms.Add(new RoomViewModel(room, quality, dbId: dbRoom.Id));
            }
            UpdateStats();
        }

        private void NavRooms_Click(object sender, RoutedEventArgs e)   => ShowPanel("rooms");
        private void NavOrders_Click(object sender, RoutedEventArgs e)  => ShowPanel("orders");
        private void NavLog_Click(object sender, RoutedEventArgs e)     => ShowPanel("log");
        private void NavPrices_Click(object sender, RoutedEventArgs e)  => ShowPanel("prices");

        private bool _logBound = false;

        private void ShowPanel(string panel)
        {
            RoomsPanel.Visibility  = panel == "rooms"  ? Visibility.Visible : Visibility.Collapsed;
            OrdersPanel.Visibility = panel == "orders" ? Visibility.Visible : Visibility.Collapsed;
            LogPanel.Visibility    = panel == "log"    ? Visibility.Visible : Visibility.Collapsed;
            PricesPanel.Visibility = panel == "prices" ? Visibility.Visible : Visibility.Collapsed;

            SetNavActive(NavRoomsBtn,  panel == "rooms");
            SetNavActive(NavOrdersBtn, panel == "orders");
            SetNavActive(NavLogBtn,    panel == "log");
            SetNavActive(NavPricesBtn, panel == "prices");

            if (panel == "orders") _freeRoomsView.Refresh();
            if (panel == "prices") LoadPriceFields();
            if (panel == "rooms")  RefreshDeleteButtons(AdminSession.Instance.IsAdmin);

            if (panel == "log" && !_logBound)
            {
                EventLogList.ItemsSource   = _eventLog;
                CommandLogList.ItemsSource = _invoker.History;
                _logBound = true;
            }
        }

        private static void SetNavActive(Button btn, bool active)
        {
            btn.Background = active
                ? new SolidColorBrush(Color.FromRgb(37, 61, 104))
                : Brushes.Transparent;
            btn.Foreground = active
                ? Brushes.White
                : new SolidColorBrush(Color.FromRgb(168, 192, 220));
        }

        private void ToggleAddRoom_Click(object sender, RoutedEventArgs e)
        {
            if (!AdminSession.Instance.IsAdmin) return;
            AddRoomForm.Visibility = AddRoomForm.Visibility == Visibility.Visible
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            if (!AdminSession.Instance.IsAdmin) return;

            var quality = AddRoomTypeBox.SelectedIndex switch
            {
                0 => RoomQuality.Poor,
                1 => RoomQuality.Normal,
                _ => RoomQuality.Deluxe
            };

            bool hasWifi = AddRoomWifi.IsChecked         == true;
            bool allIncl = AddRoomAllInclusive.IsChecked == true;

            var features = new List<RoomFeature>();
            if (hasWifi) features.Add(RoomFeature.Wifi);
            if (allIncl) features.Add(RoomFeature.AllInclusive);

            var room = _creator.CreateRoom(quality, 0, "", features);
            string typeStr = quality switch
            {
                RoomQuality.Normal => "Normal",
                RoomQuality.Deluxe => "Deluxe",
                _                  => "Poor"
            };

            try
            {
                int dbId = HotelDb.AddRoom(typeStr, hasWifi, allIncl, (double)room.Price);
                var roomVm = new RoomViewModel(room, quality, dbId: dbId);
                _rooms.Add(roomVm);
                AddRoomForm.Visibility = Visibility.Collapsed;
                UpdateStats();
                AddActionLog($"✔  Додано кімнату {roomVm.DisplayNumber} ({roomVm.TypeName}, {roomVm.BasePrice} грн/ніч)");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження в БД:\n{ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            if (!AdminSession.Instance.IsAdmin) return;
            if (sender is not Button { Tag: RoomViewModel roomVm }) return;

            if (roomVm.IsOccupied)
            {
                MessageBox.Show("Не можна видалити зайняту кімнату.",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var res = MessageBox.Show(
                $"Видалити {roomVm.DisplayNumber} ({roomVm.TypeName})?",
                "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;

            try
            {
                HotelDb.DeleteRoom(roomVm.DbId);
                _rooms.Remove(roomVm);
                _freeRoomsView.Refresh();
                UpdateStats();
                AddActionLog($"✔  Видалено кімнату {roomVm.DisplayNumber} ({roomVm.TypeName})");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення:\n{ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BookRoom_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: RoomViewModel roomVm })
            {
                ShowPanel("orders");
                OrderRoomBox.SelectedItem = roomVm;
            }
        }

        private void UpdateTotalPreview()
        {
            if (OrderRoomBox?.SelectedItem is not RoomViewModel selectedRoom
                || OrderDaysBox == null || OrderStrategyBox == null
                || OrderTotalPreview == null)
                return;



            if (!int.TryParse(OrderDaysBox.Text, out int days) || days < 1)
            { OrderTotalPreview.Text = "—"; return; }

            IPriceStrategy strategy = OrderStrategyBox.SelectedIndex switch
            {
                1 => new HolidayStrategy(0.20f),
                2 => new SalesStrategy(-0.15f),
                3 => new TouristFullPeriodStrategy(0.10f),
                _ => new NormalStrategy()
            };

            float ppd = strategy.PriceCalculate(selectedRoom.BasePrice);
            OrderTotalPreview.Text = $"{ppd * days} грн";

            if (OrderCheckOutLabel != null && OrderCheckInPicker?.SelectedDate.HasValue == true)
                OrderCheckOutLabel.Text = OrderCheckInPicker.SelectedDate.Value.AddDays(days).ToString("dd.MM.yyyy");
        }

        private void OrderRoomBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => UpdateTotalPreview();
        private void OrderDaysBox_TextChanged(object sender, TextChangedEventArgs e)
            => UpdateTotalPreview();
        private void OrderStrategyBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => UpdateTotalPreview();
        private void OrderCheckInPicker_Changed(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
            => UpdateTotalPreview();

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            ClearError();

            if (OrderRoomBox.SelectedItem is not RoomViewModel selectedRoom)
            { ShowError("Оберіть вільну кімнату."); return; }

            string guest = OrderGuestBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(guest))
            { ShowError("Введіть ім'я гостя."); return; }

            if (!int.TryParse(OrderDaysBox.Text, out int days) || days < 1)
            { ShowError("Введіть кількість ночей (мінімум 1)."); return; }

            DateTime checkIn  = OrderCheckInPicker?.SelectedDate ?? DateTime.Today;
            DateTime checkOut = checkIn.AddDays(days);

            if (HasDateConflict(selectedRoom, checkIn, checkOut))
            {
                ShowError($"Кімната вже заброньована на частину вказаного періоду. Оберіть інші дати.");
                return;
            }

            IPriceStrategy strategy = OrderStrategyBox.SelectedIndex switch
            {
                1 => new HolidayStrategy(0.20f),
                2 => new SalesStrategy(-0.15f),
                3 => new TouristFullPeriodStrategy(0.10f),
                _ => new NormalStrategy()
            };

            float ppd = strategy.PriceCalculate(selectedRoom.BasePrice);
            var order = new Order(selectedRoom.Room, guest, ppd, days, checkIn);

            order.Subscribe(_eventLogObserver);
            order.Subscribe(_billingObserver);

            int dbId = HotelDb.SaveOrder(order, selectedRoom.DbId, checkIn, checkIn.AddDays(days));
            order.DbId = dbId;
            _orderDbId[order.Id] = dbId;

            _orderToRoom[order.Id] = selectedRoom;
            RegisterBooking(selectedRoom, checkIn, checkOut);
            selectedRoom.Status = RoomStatus.Reserved;
            _orders.Add(order);
            _freeRoomsView.Refresh();
            UpdateStats();

            string checkOutStr = checkIn.AddDays(days).ToString("dd.MM.yyyy");
            AddActionLog($"✔  Створено замовлення #{order.Id} — {guest}, {selectedRoom.DisplayNumber}, {checkIn:dd.MM.yyyy}–{checkOutStr}, {order.TotalPrice} грн");
            AddEventLog($"Замовлення #{order.Id} ({guest}) створено  →  Нове");

            OrderGuestBox.Text             = "";
            OrderDaysBox.Text              = "1";
            OrderRoomBox.SelectedItem      = null;
            OrderTotalPreview.Text         = "—";
            OrderCheckOutLabel.Text        = "—";
            OrderCheckInPicker.SelectedDate = DateTime.Today;
        }

        private Order? SelectedOrder()
        {
            if (OrdersListView.SelectedItem is Order o) return o;
            ShowError("Оберіть замовлення зі списку.");
            return null;
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            ClearError();
            var order = SelectedOrder(); if (order == null) return;
            var err = _invoker.Execute(new PayOrderCommand(order));
            if (err != null) ShowError(err); else { UpdateOrderInDb(order); UpdateRoomStatus(order); RefreshOrders(); }
        }

        private void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            ClearError();
            var order = SelectedOrder(); if (order == null) return;
            var err = _invoker.Execute(new CheckInCommand(order));
            if (err != null) ShowError(err); else { UpdateOrderInDb(order); UpdateRoomStatus(order); RefreshOrders(); }
        }

        private void CheckOut_Click(object sender, RoutedEventArgs e)
        {
            ClearError();
            var order = SelectedOrder(); if (order == null) return;

            if (!float.TryParse(MiniBarBox.Text, out float miniBar) || miniBar < 0)
            { ShowError("Введіть коректну суму за міні-бар."); return; }

            CheckOutProcessor processor = order.Room is DeluxeRoom
                ? _deluxeProcessor : _standardProcessor;

            var err = _invoker.Execute(new CheckOutCommand(order, miniBar, processor));
            if (err != null) { ShowError(err); return; }

            string receipt = order.Room is DeluxeRoom
                ? _deluxeProcessor.LastReceipt
                : _standardProcessor.LastReceipt;

            MessageBox.Show(receipt, "Чек виселення",
                MessageBoxButton.OK, MessageBoxImage.Information);

            if (order.RefundAmount > 0)
                MessageBox.Show(
                    $"Дострокове виселення.\n" +
                    $"Заброньовано: {order.Days} діб\n" +
                    $"Фактично:     {order.ActualDays} діб\n\n" +
                    $"Сума до повернення: {order.RefundAmount} грн",
                    "Повернення коштів", MessageBoxButton.OK, MessageBoxImage.Information);

            UpdateOrderInDb(order);
            FreeUpRoom(order);
            RefreshOrders();
            UpdateStats();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ClearError();
            var order = SelectedOrder(); if (order == null) return;

            float prevRefund = order.RefundAmount;
            var err = _invoker.Execute(new CancelOrderCommand(order));
            if (err != null) { ShowError(err); return; }

            if (order.RefundAmount > prevRefund)
                MessageBox.Show(
                    $"Замовлення скасовано.\nСума повернення: {order.RefundAmount} грн",
                    "Скасування", MessageBoxButton.OK, MessageBoxImage.Information);

            UpdateOrderInDb(order);
            FreeUpRoom(order);
            RefreshOrders();
            UpdateStats();
        }

        private void OrdersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ClearError();

        private void LoadPriceFields()
        {
            var cfg = PriceConfig.Instance;
            PricePoor.Text         = cfg.PoorRoomPrice.ToString();
            PriceNormal.Text       = cfg.NormalRoomPrice.ToString();
            PriceDeluxe.Text       = cfg.DeluxeRoomPrice.ToString();
            PriceWifi.Text         = cfg.WiFiPrice.ToString();
            PriceAllInclusive.Text = cfg.AllInclusivePrice.ToString();
            PricesStatusText.Text  = "";
        }

        private void SavePrices_Click(object sender, RoutedEventArgs e)
        {
            if (!AdminSession.Instance.IsAdmin) return;

            if (!TryParsePositive(PricePoor.Text,         "Бюджетний",    out float poor))   return;
            if (!TryParsePositive(PriceNormal.Text,       "Стандарт",     out float normal)) return;
            if (!TryParsePositive(PriceDeluxe.Text,       "Делюкс",       out float deluxe)) return;
            if (!TryParseNonNeg  (PriceWifi.Text,         "Wi-Fi",        out float wifi))   return;
            if (!TryParseNonNeg  (PriceAllInclusive.Text, "All Inclusive", out float allInc)) return;

            var cfg = PriceConfig.Instance;
            cfg.PoorRoomPrice     = poor;
            cfg.NormalRoomPrice   = normal;
            cfg.DeluxeRoomPrice   = deluxe;
            cfg.WiFiPrice         = wifi;
            cfg.AllInclusivePrice = allInc;

            try
            {
                HotelDb.SavePriceConfig();
                PricesStatusText.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                PricesStatusText.Text = "✔  Ціни збережено в базі даних.";
            }
            catch (Exception ex)
            {
                ShowPriceError($"Помилка збереження в БД: {ex.Message}");
            }
        }

        private void ResetPrices_Click(object sender, RoutedEventArgs e)
        {
            if (!AdminSession.Instance.IsAdmin) return;

            var cfg = PriceConfig.Instance;
            cfg.PoorRoomPrice     = 100f;
            cfg.NormalRoomPrice   = 250f;
            cfg.DeluxeRoomPrice   = 500f;
            cfg.WiFiPrice         = 30f;
            cfg.AllInclusivePrice = 150f;

            try { HotelDb.SavePriceConfig(); } catch {  }

            LoadPriceFields();
            PricesStatusText.Foreground = new SolidColorBrush(Color.FromRgb(96, 125, 139));
            PricesStatusText.Text = "↺  Відновлено стандартні значення.";
        }

        private bool TryParsePositive(string text, string field, out float value)
        {
            if (float.TryParse(text, out value) && value > 0) return true;
            ShowPriceError($"Поле «{field}»: введіть число більше 0.");
            return false;
        }

        private bool TryParseNonNeg(string text, string field, out float value)
        {
            if (float.TryParse(text, out value) && value >= 0) return true;
            ShowPriceError($"Поле «{field}»: введіть невід'ємне число.");
            return false;
        }

        private void ShowPriceError(string msg)
        {
            PricesStatusText.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
            PricesStatusText.Text = "✘  " + msg;
        }

        private void FreeUpRoom(Order order)
        {
            if (!_orderToRoom.TryGetValue(order.Id, out var roomVm)) return;
            if (order.StatusName is "Завершено" or "Скасовано")
            {
                UnregisterBooking(roomVm, order.BookedCheckIn, order.BookedCheckOut);
                roomVm.Status = RoomStatus.Free;
                _freeRoomsView.Refresh();
            }
        }

        private void RefreshOrders()
        {
            var sel = OrdersListView.SelectedItem;
            OrdersListView.Items.Refresh();
            OrdersListView.SelectedItem = sel;
        }

        private void UpdateStats()
        {
            int total    = _rooms.Count;
            int occupied = _rooms.Count(r => r.IsOccupied);
            int free     = total - occupied;

            StatTotal.Text     = total.ToString();
            StatFree.Text      = free.ToString();
            StatOccupied.Text  = occupied.ToString();
            StatCompleted.Text = _billingObserver.CompletedCount.ToString();
            StatRevenue.Text   = $"{_billingObserver.TotalRevenue} грн";

            RoomsSubtitle.Text = total == 0
                ? "Кімнат немає. Адміністратор може додати нові."
                : $"Всього: {total}  •  Вільних: {free}  •  Зайнятих: {occupied}";
        }


        private void LoadLogsFromDb()
        {
            try
            {
                foreach (var log in HotelDb.LoadLogs())
                {
                    if (log.Kind == "event")
                        _eventLog.Add(log.Message);
                    else if (log.Kind == "action")
                        _invoker.History.Add(log.Message);
                }
            }
            catch { }
        }


        private void UpdateOrderInDb(Order order)
        {
            if (!_orderDbId.TryGetValue(order.Id, out int dbId)) return;
            try
            {
                HotelDb.UpdateOrder(dbId, order);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оновлення замовлення в БД:\n{ex.Message}",
                    "Помилка БД", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadOrdersFromDb()
        {
            try
            {
                var dbOrders = HotelDb.LoadOrders();
                foreach (var dbO in dbOrders)
                {
                    var roomVm = _rooms.FirstOrDefault(r => r.DbId == dbO.RoomDbId);
                    if (roomVm == null) continue;

                    var checkIn = dbO.CheckInDate;
                    var order   = new Order(roomVm.Room, dbO.GuestName, (float)dbO.PricePerDay, dbO.Days, checkIn);
                    order.DbId = dbO.Id;
                    _orderDbId[order.Id] = dbO.Id;

                    // Відновлюємо фінансові дані
                    order.TotalPrice    = (float)dbO.TotalPrice;
                    order.MiniBarCharge = (float)dbO.MiniBarCharge;
                    order.RefundAmount  = (float)dbO.RefundAmount;
                    if (dbO.ActualCheckInDate.HasValue)
                        order.CheckInDate = dbO.ActualCheckInDate;

                    // Відновлюємо стан без нотифікацій спостерігачів
                    order.RestoreState(dbO.Status);

                    order.Subscribe(_eventLogObserver);
                    order.Subscribe(_billingObserver);

                    roomVm.Status = dbO.Status switch
                    {
                        "Нове"     => RoomStatus.Reserved,
                        "Оплачено" => RoomStatus.Reserved,
                        "Активне"  => RoomStatus.Occupied,
                        _          => RoomStatus.Free
                    };

                    _orderToRoom[order.Id] = roomVm;
                    if (dbO.Status is "Нове" or "Оплачено" or "Активне")
                        RegisterBooking(roomVm, dbO.CheckInDate, dbO.CheckOutDate);
                    _orders.Add(order);
                }
            }
            catch { }
        }


        private void AddActionLog(string message)
        {
            _invoker.History.Insert(0, message);
            HotelDb.AddLog("action", message);
        }

        private void AddEventLog(string message)
        {
            _eventLog.Insert(0, message);
            HotelDb.AddLog("event", message);
        }



        private bool HasDateConflict(RoomViewModel roomVm, DateTime from, DateTime to)
        {
            if (!_roomBookings.TryGetValue(roomVm.DbId, out var bookings)) return false;
            foreach (var (bFrom, bTo) in bookings)
                if (from < bTo && bFrom < to) return true;
            return false;
        }

        private void RegisterBooking(RoomViewModel roomVm, DateTime from, DateTime to)
        {
            if (!_roomBookings.ContainsKey(roomVm.DbId))
                _roomBookings[roomVm.DbId] = new List<(DateTime, DateTime)>();
            _roomBookings[roomVm.DbId].Add((from, to));
        }

        private void UnregisterBooking(RoomViewModel roomVm, DateTime from, DateTime to)
        {
            if (!_roomBookings.TryGetValue(roomVm.DbId, out var bookings)) return;
            bookings.Remove((from, to));
        }

        private void UpdateRoomStatus(Order order)
        {
            if (!_orderToRoom.TryGetValue(order.Id, out var roomVm)) return;
            roomVm.Status = order.StatusName switch
            {
                "Нове"     => RoomStatus.Reserved,
                "Оплачено" => RoomStatus.Reserved,
                "Активне"  => RoomStatus.Occupied,
                _          => RoomStatus.Free
            };
        }


        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var order = SelectedOrder();
            if (order == null) return;

            if (order.StatusName is not ("Завершено" or "Скасовано"))
            {
                ShowError("Видаляти можна лише завершені або скасовані замовлення.");
                return;
            }

            try
            {
                if (_orderDbId.TryGetValue(order.Id, out int dbId))
                    HotelDb.DeleteOrder(dbId);

                if (_orderToRoom.TryGetValue(order.Id, out var rv))
                    UnregisterBooking(rv, order.BookedCheckIn, order.BookedCheckOut);

                _orders.Remove(order);
                _orderToRoom.Remove(order.Id);
                _orderDbId.Remove(order.Id);
                AddActionLog($"✔  Видалено замовлення #{order.Id} ({order.GuestName})");
                UpdateStats();
            }
            catch (Exception ex)
            {
                ShowError($"Помилка видалення: {ex.Message}");
            }
        }

        private void ShowError(string msg) => ErrorText.Text = msg;
        private void ClearError()          => ErrorText.Text = "";
        private static T? FindChild<T>(DependencyObject parent, string childName)
            where T : FrameworkElement
        {
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T fe && fe.Name == childName) return fe;
                var result = FindChild<T>(child, childName);
                if (result != null) return result;
            }
            return null;
        }
    }
}
