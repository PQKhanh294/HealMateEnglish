using System;
using System.Collections.Generic;

namespace Models;

public partial class PresetReading
{
    public int PresetId { get; set; }

    public string Title { get; set; } = null!;

    public string? Part { get; set; }

    public string Passage { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool IsAiCreated { get; set; } // New: mark AI-generated presets

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<ReadingSession> ReadingSessions { get; set; } = new List<ReadingSession>();

    // Navigation for reading questions under this preset
    public virtual ICollection<ReadingQuestion> ReadingQuestions { get; set; } = new List<ReadingQuestion>();
}