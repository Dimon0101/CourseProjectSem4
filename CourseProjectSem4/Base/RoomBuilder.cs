using System;
using CourseProjectSem4.Models;

namespace CourseProjectSem4.Base
{
    enum RoomQuality { Deluxe, Normal, Poor }

    class RoomBuilder
    {
        private Room _room;

        public RoomBuilder CreateRoom(RoomQuality quality, int id, string owner)
        {
            var cfg = PriceConfig.Instance;
            _room = quality switch
            {
                RoomQuality.Poor   => new PoorRoom(id, owner,   cfg.PoorRoomPrice),
                RoomQuality.Normal => new NormalRoom(id, owner, cfg.NormalRoomPrice),
                RoomQuality.Deluxe => new DeluxeRoom(id, owner, cfg.DeluxeRoomPrice),
                _ => throw new ArgumentException("Невідомий тип кімнати", nameof(quality))
            };
            return this;
        }

        public RoomBuilder AddWiFi()
        {
            _room.HasWiFi = true;
            _room.Price  += PriceConfig.Instance.WiFiPrice;
            return this;
        }

        public RoomBuilder AddAllInclusive()
        {
            _room.AllInclusive = true;
            _room.Price       += PriceConfig.Instance.AllInclusivePrice;
            return this;
        }

        public Room Build() => _room;
    }
}
