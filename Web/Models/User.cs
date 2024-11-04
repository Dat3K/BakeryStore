using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class User
{
    public Guid Sid { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? Nickname { get; set; }

    public string? Name { get; set; }

    public string? Picture { get; set; }
}
