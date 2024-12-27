using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime Birthday { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PassportNumber { get; set; } = null!;

    public string Address { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ICollection<ServiceUsage> ServiceUsages { get; set; } = new List<ServiceUsage>();

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
