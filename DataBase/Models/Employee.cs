using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Models;

public partial class Employee
{
    [Key]
    public int EmployeesId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Position { get; set; } = null!;

    public DateTime Birthday { get; set; }

    public string Phone { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Неправильний формат пошти!")]
    public string Email { get; set; } = null!;
    public string ?PasswordHash { get; set; } = null!;
    [NotMapped]
    public string ?Password { get; set; } = null!;

    public DateTime HireDate { get; set; }

    public string ResidencePlace { get; set; } = null!;

    public string Education { get; set; } = null!;

    public decimal Salary { get; set; }

    public virtual ICollection<ServiceUsage> ServiceUsages { get; set; } = new List<ServiceUsage>();

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
