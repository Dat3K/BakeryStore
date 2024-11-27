using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Web.Models.Enums;

namespace Web.Models;

public class User
{
    [Key]
    public Guid Sid { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    public string? NickName { get; set; }

    public string? Picture { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.customer;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Order>? Orders { get; set; }
}
