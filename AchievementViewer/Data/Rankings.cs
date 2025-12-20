using System;
using System.Threading;

namespace AchievementViewer.Data;

public class Rankings
{
    public Achievement_Rank? Achievement_Rank { get; set; }
    public Mount_Rank? Mount_Rank { get; set; }
    public Minion_Rank? Minion_Rank { get; set; }
    public Rankings() { }
}
