using Microsoft.EntityFrameworkCore;

namespace CourseProjectSem4.Database
{
    class HotelContext : DbContext
    {
        public static string ConnectionString { get; set; } =
            @"Server=(localdb)\MSSQLLocalDB;Database=HotelDb;Integrated Security=True;";

        public DbSet<RoomEntity>        Rooms       { get; set; }
        public DbSet<PriceConfigEntity> PriceConfig { get; set; }
        public DbSet<LogEntity>         Logs        { get; set; }
        public DbSet<OrderEntity>       Orders      { get; set; }
        public DbSet<SettingEntity>      Settings    { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PriceConfigEntity>().HasData(
                new PriceConfigEntity { Key = "PoorRoomPrice",     Value = 100 },
                new PriceConfigEntity { Key = "NormalRoomPrice",   Value = 250 },
                new PriceConfigEntity { Key = "DeluxeRoomPrice",   Value = 500 },
                new PriceConfigEntity { Key = "WiFiPrice",         Value =  30 },
                new PriceConfigEntity { Key = "AllInclusivePrice", Value = 150 }
            );
        }
    }
}
