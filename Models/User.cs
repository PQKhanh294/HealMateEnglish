using System;
using System.Collections.Generic;

namespace Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Level { get; set; }

    public bool? IsAdmin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Apilog> Apilogs { get; set; } = new List<Apilog>();

    public virtual ICollection<PresetReading> PresetReadings { get; set; } = new List<PresetReading>();

    public virtual ICollection<PresetWritingTopic> PresetWritingTopics { get; set; } = new List<PresetWritingTopic>();

    public virtual ICollection<ReadingSession> ReadingSessions { get; set; } = new List<ReadingSession>();

    public virtual ICollection<WritingSession> WritingSessions { get; set; } = new List<WritingSession>();
}
