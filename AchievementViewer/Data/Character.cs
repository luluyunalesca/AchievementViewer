using System;
using System.Threading;

namespace AchievementViewer;

public class Character
{

    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Server { get; set; }
    public string? Data_Center { get; set; }
    public Achievements? achievements { get; set; }
    public Mounts? mounts { get; set; }
    public Minions? minions { get; set; }
    public Rankings? rankings { get; set; }

    public Character()
	{
	}

    public Character(int id)
    {
        this.Id = id;
        this.Name = "";
        this.Server = "";
    }

    public void FetchData()
    {
        
    }
}



