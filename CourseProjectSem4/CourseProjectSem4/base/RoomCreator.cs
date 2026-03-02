using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourseProjectSem4.Models;

namespace CourseProjectSem4.Base
{
    enum RoomQuality
    {
        Deluxe,
        Normal,
        Poor
    }
    abstract class RoomCreator
    {
        public abstract Room CreateRoom(RoomQuality quality, int id, string owner);
    }

    class RoomCreatorHandler: RoomCreator
    {
        public override Room CreateRoom(RoomQuality quality, int id, string owner)
        {
            return quality switch
            {
                RoomQuality.Poor => new PoorRoom(id, owner, 100),
                RoomQuality.Normal => new NormalRoom(id, owner, 250),
                RoomQuality.Deluxe => new DeluxeRoom(id, owner, 500),
                _ => throw new ArgumentException("Невідомий тип кімнати", nameof(quality))
            };
        }
    }
}

