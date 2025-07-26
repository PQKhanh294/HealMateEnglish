using System;
using System.Collections.Generic;

namespace Models;

public partial class ReadingOption
{
    public int OptionId { get; set; }

    public int QuestionId { get; set; }

    public string OptionLabel { get; set; } = null!;

    public string? OptionText { get; set; }

    public bool? IsCorrect { get; set; }

    // UserSelected property moved to ReadingOption.Partial.cs for PropertyChanged support

    public virtual ReadingQuestion Question { get; set; } = null!;
}
