
namespace AchievementViewer.Data;

public class Character
{

    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Server { get; set; }
    public string? Data_Center { get; set; }
    public bool FoundOnCollect { get; set; }
    public bool FoundOnLodestone { get; set; }
    public Achievements? Achievements { get; set; }
    public Mounts? Mounts { get; set; }
    public Minions? Minions { get; set; }
    public Rankings? Rankings { get; set; }

    public Character()
	{
	}

    public Character(int id, bool foundOnCollect, bool foundOnLodestone)
    {
        this.Id = id;
        this.Name = "";
        this.Server = "";
        this.FoundOnCollect = foundOnCollect;
        this.FoundOnLodestone = foundOnLodestone;
    }

}



