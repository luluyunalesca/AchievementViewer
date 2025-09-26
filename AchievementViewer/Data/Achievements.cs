using System;
using System.Threading;

namespace AchievementViewer.Data;

public class Achievements
{

    public short? Count { get; set; }
    public short? Total { get; set; }
    public int? Points { get; set; }
    public int? Points_Total { get; set; }
    public int? Ranked_Points { get; set; }
    public bool Public { get; set; }
    public int? Ranked_Points_Total { get; set; }
    public Achievements()
    {

    }
}
