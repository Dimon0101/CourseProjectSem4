using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseProjectSem4.Models
{
    abstract class Room
    {
        static int roomNumber = 0;
        int id;
        public int Id
        {
            get;
            init;
        }
        string owner;
        public string Owner
        {
            get
            {
                return owner;
            }
            set
            {
                if(value != owner && owner.Length > 2)
                    owner = value;
            }
        }
        bool isFree;
        public bool IsFree
        {
            get;
            set;
        }
        float price;
        public float Price
        {
            get
            {
                return price;
            }
            set
            {
                if(value > 0)
                {
                    price = value;
                }
            }
        }

        public Room(int id, string owner, float price)
        {
            if (roomNumber < 8)
            {
                Id = roomNumber;
                Owner = owner;
                Price = price;
                isFree = true;
                roomNumber++;
            }
        }
    }

    class PoorRoom : Room
    {
        public PoorRoom(int id, string owner,float price) : base(id,owner, price)
        {
        }
    }

    class NormalRoom : Room
    {
        public NormalRoom(int id, string owner, float price) : base(id, owner, price)
        {
        }
    }

    class DeluxeRoom : Room
    {
        public DeluxeRoom(int id, string owner, float price) : base(id, owner, price)
        {
        }
    }
}
