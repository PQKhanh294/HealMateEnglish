using System;
using System.Collections.Generic;

namespace Models;

public partial class WritingSession
{
    public int SessionId { get; set; }

    public int UserId { get; set; }

    public string SourceType { get; set; } = null!;

    public int? TopicId { get; set; }

    public string? CustomTopic { get; set; }

    public string UserText { get; set; } = null!;

    public string? AiFeedback { get; set; }

    public double? Score { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual PresetWritingTopic? Topic { get; set; }

    public virtual User User { get; set; } = null!;
}
