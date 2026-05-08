using System.Collections.Generic;
using CourseProjectSem4.Models;

namespace CourseProjectSem4.Base
{
    public enum RoomFeature { Wifi, AllInclusive }

    abstract class RoomCreator
    {
        public abstract Room CreateRoom(RoomQuality quality, int id, string owner, IEnumerable<RoomFeature> features);
    }

    class RoomCreatorHandler : RoomCreator
    {
        public override Room CreateRoom(RoomQuality quality, int id, string owner, IEnumerable<RoomFeature> features)
        {
            var builder = new RoomBuilder();
            builder.CreateRoom(quality, id, owner);

            if (features != null)
                foreach (var feature in features)
                    switch (feature)
                    {
                        case RoomFeature.Wifi:         builder.AddWiFi();         break;
                        case RoomFeature.AllInclusive: builder.AddAllInclusive(); break;
                    }

            return builder.Build();
        }
    }
}
