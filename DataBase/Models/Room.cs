using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public int RoomNumber { get; set; }

    public int RoomTypeId { get; set; } 

    public bool IsAvailable { get; set; } = true;

    public virtual RoomType ?RoomType { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    [NotMapped]
    public string TypeAndNumber => $"{RoomNumber} - {RoomType?.TypeName}";
}