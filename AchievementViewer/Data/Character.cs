
namespace AchievementViewer.Data;

public class Character
{

    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Server { get; set; }
    public string? Data_Center { get; set; }
    public bool foundOnCollect { get; set; }
    public bool foundOnLodestone { get; set; }
    public Achievements? achievements { get; set; }
    public Mounts? mounts { get; set; }
    public Minions? minions { get; set; }
    public Rankings? rankings { get; set; }

    public Character()
	{
	}

    public Character(int id, bool foundOnCollect, bool foundOnLodestone)
    {
        this.Id = id;
        this.Name = "";
        this.Server = "";
        this.foundOnCollect = foundOnCollect;
        this.foundOnLodestone = foundOnLodestone;
    }

}



