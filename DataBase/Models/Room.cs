using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public int RoomNumber { get; set; }

    public string RoomType { get; set; } = null!;

    public byte Capacity { get; set; }

    public decimal Price { get; set; }

    public string Description { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    [NotMapped]
    public string TypeAndNumber => $"{RoomNumber} {RoomType}";
}
