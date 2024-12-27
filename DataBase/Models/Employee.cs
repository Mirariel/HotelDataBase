using System;
using System.Collections.Generic;

namespace DataBase.Models;

public partial class Employee
{
    public int EmployeesId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Position { get; set; } = null!;

    public DateTime Birthday { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime HireDate { get; set; }

    public string ResidencePlace { get; set; } = null!;

    public string Education { get; set; } = null!;

    public decimal Salary { get; set; }

    public virtual ICollection<ServiceUsage> ServiceUsages { get; set; } = new List<ServiceUsage>();
}
