using System;
using System.Collections.Generic;

namespace DataBase.Models;

public partial class Service
{
    public int ServicesId { get; set; }

    public string ServicesName { get; set; } = null!;

    public decimal Price { get; set; }

    public string Description { get; set; } = null!;
    public string ServiceImg {  get; set; }

    public virtual ICollection<ServiceUsage> ServiceUsages { get; set; } = new List<ServiceUsage>();
}
