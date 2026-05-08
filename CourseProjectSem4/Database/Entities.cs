using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseProjectSem4.Database
{
    [Table("Rooms")]
    class RoomEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(10)]
        public string Type { get; set; } = "";

        public bool   HasWiFi      { get; set; }
        public bool   AllInclusive { get; set; }
        public double Price        { get; set; }
    }

    [Table("PriceConfig")]
    class PriceConfigEntity
    {
        [Key, MaxLength(50)]
        public string Key   { get; set; } = "";
        public double Value { get; set; }
    }

    [Table("Logs")]
    class LogEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(10)]
        public string Kind { get; set; } = "";

        [Required, MaxLength(500)]
        public string Message { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("Orders")]
    class OrderEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int RoomDbId { get; set; }

        [Required, MaxLength(200)]
        public string GuestName { get; set; } = "";

        public double PricePerDay { get; set; }
        public int    Days        { get; set; }
        public double TotalPrice  { get; set; }
        public double MiniBarCharge { get; set; }
        public double FinalAmount { get; set; }
        public double RefundAmount { get; set; }

        public DateTime  CheckInDate  { get; set; }
        public DateTime  CheckOutDate { get; set; }
        public DateTime? ActualCheckInDate { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Нове";
    }
}

    [Table("Settings")]
    class SettingEntity
    {
        [Key, MaxLength(50)]
        public string Key   { get; set; } = "";

        [Required, MaxLength(200)]
        public string Value { get; set; } = "";
    }
