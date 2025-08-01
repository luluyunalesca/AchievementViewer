using System;
using System.Threading;

namespace AchievementViewer.Data;

public class Mounts
{
    public short? Count { get; set; }
    public short? Ranked_Count { get; set; }
    public bool Public { get; set; }
    public Mounts() { }
}