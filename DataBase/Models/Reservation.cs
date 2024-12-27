using System;
using System.Collections.Generic;

namespace DataBase.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public int CustomerId { get; set; }

    public int RoomId { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public decimal TotalPrice { get; set; }

    public virtual Customer ?Customer { get; set; }

    public virtual Room ?Room { get; set; }
}
