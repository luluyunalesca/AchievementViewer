using System;
using System.Threading;

namespace AchievementViewer;

public class Player
{

    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Server { get; set; }
    public string? Data_Center { get; set; }
    public Achievements? achievements { get; set; }
    public Mounts? mounts { get; set; }
    public Minions? minions { get; set; }
    public Rankings? rankings { get; set; }

    public Player()
	{
	}

    public Player (int id)
    {
        this.Id = id;
    }

}

public class Achievements
{

    public int? Points { get; set; }
    public int? Ranked_Points { get; set; }
    public bool Public { get; set; }
    public Achievements()
    {

    }
}

public class Mounts
{
    public short? Count { get; set; }
    public short? Ranked_Count { get; set; }
    public bool Public { get; set; }
    public Mounts() { }
}

public class Minions
{
    public short? Count { get; set; }
    public short? Ranked_Count { get; set; }
    public bool Public { get; set; }
    public Minions() { }
}

public class Rankings
{
    public Achievement_Rank achievement_Rank { get; set; }
    public Mount_Rank? mount_Rank { get; set; }
    public Minion_Rank?  minion_Rank { get; set; }
    public Rankings() { }
}

public class Achievement_Rank
{
    public int? Server { get; set; }
    public int? Data_Center { get; set; }
    public int? Global { get; set; }
    public Achievement_Rank() { }
}

public class Mount_Rank
{
    public int? Server { get; set; }
    public int? Data_Center { get; set; }
    public int? Global { get; set; }

    public Mount_Rank() { }
}

public class Minion_Rank
{
    public int? Server { get; set; }
    public int? Data_Center { get; set; }
    public int? Global { get; set; }

    public Minion_Rank() { }
}
