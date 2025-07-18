using System;
using System.Collections.Generic;

namespace Models;

public partial class ReadingQuestion
{
    public int QuestionId { get; set; }

    public int SessionId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string? Explanation { get; set; }

    public virtual ICollection<ReadingOption> ReadingOptions { get; set; } = new List<ReadingOption>();

    public virtual ReadingSession Session { get; set; } = null!;
}
