using System;
using System.Collections.Generic;

namespace Models;

public partial class ReadingQuestion
{
    public int QuestionId { get; set; }

    public int? PresetId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string? Explanation { get; set; }

    public bool IsMultipleChoice { get; set; } = false;

    public virtual ICollection<ReadingOption> ReadingOptions { get; set; } = new List<ReadingOption>();

    public virtual PresetReading? Preset { get; set; } = null!;
}