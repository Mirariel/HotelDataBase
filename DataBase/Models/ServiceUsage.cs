using System;
using System.Collections.Generic;

namespace DataBase.Models;

public partial class ServiceUsage
{
    public int UsageId { get; set; }

    public int CustomerId { get; set; }

    public int ServicesId { get; set; }

    public int? EmployeeId { get; set; }

    public virtual Customer ?Customer { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Service ?Services { get; set; }
}
