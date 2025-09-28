using System;
using System.Threading;

namespace AchievementViewer.Data;

public class Rankings
{
    public Achievement_Rank? achievement_Rank { get; set; }
    public Mount_Rank? mount_Rank { get; set; }
    public Minion_Rank? minion_Rank { get; set; }
    public Rankings() { }
}
