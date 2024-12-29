using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public int CustomerId { get; set; }

    public int RoomId { get; set; }

    public DateTime CheckInDate { get; set; }

    public DateTime CheckOutDate { get; set; }

    public decimal TotalPrice { get; set; }
    [ForeignKey("CustomerId")]
    public virtual Customer? Customer { get; set; }

    public virtual ICollection<ServiceUsage> ServiceUsages { get; set; } = new List<ServiceUsage>();

    public virtual Room? Room { get; set; }
    [NotMapped]
    public string DisplayText
    {
        get
        {
            return $"{Customer?.FirstName} {Customer?.LastName}, {Room?.RoomNumber}, {CheckInDate:yyyy.MM.dd}-{CheckOutDate:yyyy.MM.dd}";
        }
    }
}


