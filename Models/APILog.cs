using System;
using System.Collections.Generic;

namespace Models;

public partial class Apilog
{
    public int LogId { get; set; }

    public string RequestType { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime? Timestamp { get; set; }

    public string? ApiResponseStatus { get; set; }

    public virtual User User { get; set; } = null!;
}
