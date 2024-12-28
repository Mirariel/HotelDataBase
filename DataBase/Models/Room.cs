using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public int RoomNumber { get; set; }

    public int RoomTypeId { get; set; } // Зовнішній ключ для RoomType

    public bool IsAvailable { get; set; }

    // Навігаційна властивість для RoomType
    public virtual RoomType ?RoomType { get; set; } = null!;

    // Навігаційна властивість для Reservation
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    // Властивість для відображення типу та номера кімнати
    [NotMapped]
    public string TypeAndNumber => $"{RoomNumber} - {RoomType?.TypeName}";
}