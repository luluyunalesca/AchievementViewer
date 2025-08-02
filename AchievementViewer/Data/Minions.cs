using System;
using System.Threading;

namespace AchievementViewer.Data;


public class Minions
{
    public short? Count { get; set; }
    public short? Ranked_Count { get; set; }
    public bool Public { get; set; }
    public Minions() { }
}
