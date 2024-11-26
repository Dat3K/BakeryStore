using System;
using System.Collections.Generic;
using Web.Models.Enums;

namespace Web.Models;

public partial class User
{
    public Guid Sid { get; set; }
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Nickname { get; set; }

    public string? Name { get; set; }

    public string? Picture { get; set; }

    public string Email { get; set; } = null!;

    public UserRole? Role { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
