using System;
using System.Collections.Generic;

namespace Models;

public partial class PresetWritingTopic
{
    public int TopicId { get; set; }

    public string? Title { get; set; }  // Make nullable to match database schema

    public string? Band { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }  // Make nullable

    public virtual ICollection<WritingSession> WritingSessions { get; set; } = new List<WritingSession>();
}
