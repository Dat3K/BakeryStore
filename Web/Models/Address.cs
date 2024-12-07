using System;
using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class Address
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    [Required(ErrorMessage = "Street address is required")]
    public string Street { get; set; }
    
    [Required(ErrorMessage = "City is required")]
    public string City { get; set; }
    
    [Required(ErrorMessage = "State/Province is required")]
    public string State { get; set; }
    
    [Required(ErrorMessage = "Postal code is required")]
    public string PostalCode { get; set; }
    
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string Phone { get; set; }
    
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public virtual User User { get; set; }
}
