using System;
using System.Collections.Generic;
using System.Linq;
using CourseProjectSem4.Base;
using CourseProjectSem4.States;
using Microsoft.EntityFrameworkCore;

namespace CourseProjectSem4.Database
{
    static class HotelDb
    {
        public static void Initialize()
        {
            using var ctx = new HotelContext();
            ctx.Database.EnsureCreated();

            ctx.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Logs' AND xtype='U')
                CREATE TABLE Logs (
                    Id        INT IDENTITY(1,1) PRIMARY KEY,
                    Kind      NVARCHAR(10)  NOT NULL,
                    Message   NVARCHAR(500) NOT NULL,
                    CreatedAt DATETIME2     NOT NULL DEFAULT GETDATE()
                );

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Settings' AND xtype='U')
                CREATE TABLE Settings (
                    [Key]   NVARCHAR(50)  NOT NULL PRIMARY KEY,
                    [Value] NVARCHAR(200) NOT NULL
                );

                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
                CREATE TABLE Orders (
                    Id                INT IDENTITY(1,1) PRIMARY KEY,
                    RoomDbId          INT            NOT NULL,
                    GuestName         NVARCHAR(200)  NOT NULL,
                    PricePerDay       FLOAT          NOT NULL,
                    Days              INT            NOT NULL,
                    TotalPrice        FLOAT          NOT NULL,
                    MiniBarCharge     FLOAT          NOT NULL DEFAULT 0,
                    FinalAmount       FLOAT          NOT NULL DEFAULT 0,
                    RefundAmount      FLOAT          NOT NULL DEFAULT 0,
                    CheckInDate       DATETIME2      NOT NULL,
                    CheckOutDate      DATETIME2      NOT NULL,
                    ActualCheckInDate DATETIME2      NULL,
                    Status            NVARCHAR(20)   NOT NULL DEFAULT 'Нове'
                );
            ");
        }

        public static List<RoomEntity> LoadRooms()
        {
            using var ctx = new HotelContext();
            return ctx.Rooms.OrderBy(r => r.Id).ToList();
        }

        public static int AddRoom(string type, bool hasWifi, bool allInclusive, double price)
        {
            using var ctx = new HotelContext();
            var entity = new RoomEntity
            {
                Type         = type,
                HasWiFi      = hasWifi,
                AllInclusive = allInclusive,
                Price        = price
            };
            ctx.Rooms.Add(entity);
            ctx.SaveChanges();
            return entity.Id;
        }

        public static void DeleteRoom(int id)
        {
            using var ctx = new HotelContext();
            var entity = ctx.Rooms.Find(id)
                ?? throw new Exception($"Кімнату з Id={id} не знайдено.");
            ctx.Rooms.Remove(entity);
            ctx.SaveChanges();
        }

        public static void LoadPriceConfig()
        {
            using var ctx = new HotelContext();
            var cfg = PriceConfig.Instance;
            foreach (var row in ctx.PriceConfig.ToList())
                switch (row.Key)
                {
                    case "PoorRoomPrice":     cfg.PoorRoomPrice     = (float)row.Value; break;
                    case "NormalRoomPrice":   cfg.NormalRoomPrice   = (float)row.Value; break;
                    case "DeluxeRoomPrice":   cfg.DeluxeRoomPrice   = (float)row.Value; break;
                    case "WiFiPrice":         cfg.WiFiPrice         = (float)row.Value; break;
                    case "AllInclusivePrice": cfg.AllInclusivePrice = (float)row.Value; break;
                }
        }

        public static void SavePriceConfig()
        {
            using var ctx = new HotelContext();
            var cfg = PriceConfig.Instance;
            var updates = new Dictionary<string, double>
            {
                ["PoorRoomPrice"]     = cfg.PoorRoomPrice,
                ["NormalRoomPrice"]   = cfg.NormalRoomPrice,
                ["DeluxeRoomPrice"]   = cfg.DeluxeRoomPrice,
                ["WiFiPrice"]         = cfg.WiFiPrice,
                ["AllInclusivePrice"] = cfg.AllInclusivePrice,
            };
            foreach (var row in ctx.PriceConfig.ToList())
                if (updates.TryGetValue(row.Key, out double val))
                    row.Value = val;
            ctx.SaveChanges();
        }

        public static void AddLog(string kind, string message)
        {
            try
            {
                using var ctx = new HotelContext();
                ctx.Logs.Add(new LogEntity
                {
                    Kind      = kind,
                    Message   = message,
                    CreatedAt = DateTime.Now
                });
                ctx.SaveChanges();
            }
            catch { }
        }

        public static List<LogEntity> LoadLogs()
        {
            using var ctx = new HotelContext();
            return ctx.Logs.OrderByDescending(l => l.CreatedAt).ToList();
        }

        public static int SaveOrder(Order order, int roomDbId,
                                    DateTime checkInDate, DateTime checkOutDate)
        {
            using var ctx = new HotelContext();
            var entity = new OrderEntity
            {
                RoomDbId          = roomDbId,
                GuestName         = order.GuestName,
                PricePerDay       = order.PricePerDay,
                Days              = order.Days,
                TotalPrice        = order.TotalPrice,
                MiniBarCharge     = order.MiniBarCharge,
                FinalAmount       = order.FinalAmount,
                RefundAmount      = order.RefundAmount,
                CheckInDate       = checkInDate,
                CheckOutDate      = checkOutDate,
                ActualCheckInDate = order.CheckInDate,
                Status            = order.StatusName
            };
            ctx.Orders.Add(entity);
            ctx.SaveChanges();
            return entity.Id;
        }

        public static void UpdateOrder(int dbId, Order order)
        {
            using var ctx = new HotelContext();
            var entity = ctx.Orders.Find(dbId);
            if (entity == null) return;
            entity.Status            = order.StatusName;
            entity.TotalPrice        = order.TotalPrice;
            entity.MiniBarCharge     = order.MiniBarCharge;
            entity.FinalAmount       = order.FinalAmount;
            entity.RefundAmount      = order.RefundAmount;
            entity.ActualCheckInDate = order.CheckInDate;
            ctx.SaveChanges();
        }

        public static void DeleteOrder(int id)
        {
            using var ctx = new HotelContext();
            var entity = ctx.Orders.Find(id);
            if (entity == null) return;
            ctx.Orders.Remove(entity);
            ctx.SaveChanges();
        }

        public static string LoadPassword(string defaultPassword)
        {
            using var ctx = new HotelContext();
            var row = ctx.Settings.Find("AdminPassword");
            return row?.Value ?? defaultPassword;
        }

        public static void SavePassword(string password)
        {
            using var ctx = new HotelContext();
            var row = ctx.Settings.Find("AdminPassword");
            if (row != null)
                row.Value = password;
            else
                ctx.Settings.Add(new SettingEntity { Key = "AdminPassword", Value = password });
            ctx.SaveChanges();
        }

        public static List<OrderEntity> LoadOrders()
        {
            using var ctx = new HotelContext();
            return ctx.Orders.OrderByDescending(o => o.Id).ToList();
        }
    }
}
