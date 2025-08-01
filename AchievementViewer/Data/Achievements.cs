using System;
using System.Threading;

namespace AchievementViewer.Data;

public class Achievements
{

    public int? Points { get; set; }
    public int? Ranked_Points { get; set; }
    public bool Public { get; set; }
    public Achievements()
    {

    }
}
