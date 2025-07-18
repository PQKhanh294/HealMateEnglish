using System;
using System.Collections.Generic;

namespace Models;

public partial class ReadingSession
{
    public int SessionId { get; set; }

    public int UserId { get; set; }

    public string SourceType { get; set; } = null!;

    public int? PresetId { get; set; }

    public string Passage { get; set; } = null!;

    public string? Band { get; set; }

    public double? Score { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual PresetReading? Preset { get; set; }

    public virtual ICollection<ReadingQuestion> ReadingQuestions { get; set; } = new List<ReadingQuestion>();

    public virtual User User { get; set; } = null!;
}
