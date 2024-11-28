using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class User
{
    public Guid Sid { get; set; }

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Nickname { get; set; }

    public string? Picture { get; set; }

    public string? Name { get; set; }

    public string Email { get; set; } = null!;

    public string? NameIdentifier { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
