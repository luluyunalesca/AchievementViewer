
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AchievementViewer;

public class CharData
{

    public Dictionary<string, string> worlds = new Dictionary<string, string>();

    public string lastSeenName = "";
    public string lastSeenServer = "";

    public short offsetY = 5;

    public Player lastSeenPlate = new Player(0);

    public bool alreadyRequested = false;

    public CharData()
    {

    }

    //Parses Server ID to World Mapping from World.csv file into a dictionary
    private void parseCSV(String path)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (Stream? stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + path))
        {
            if (stream == null)
            {
                return;
            }
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var lsplit = line?.Split(',');
                    if (lsplit?.Length > 1)
                    {
                        var newkey = lsplit[0];
                        var newval = lsplit[2];
                        worlds[newkey] = newval;
                    }
                }


            }
        }

    }

    //Read Adventurer Plate player data that is displayed onscreen
    private unsafe List<string> getCardData()
    {

        List<string> data = new List<string>();

        if (AgentCharaCard.Instance() == null || AgentCharaCard.Instance()->Data == null)
        {
            return data;
        }


        var card = AgentCharaCard.Instance()->Data;
        data.Add(card->Name.ToString());
        data.Add(worlds[card->WorldId.ToString()].Replace("\"", ""));

        return data;
    }


    private async Task<string> requestData(string name, string surname, string server)
    {
        string lodestoneID = "";

        lodestoneID = await requestLodestoneID(name, surname, server);

        var response = await requestAchievements(lodestoneID);

        return response;
    }

    //parse Achievement json file to deduplicate strings and deserialize it
    private Player parseData(string data)
    {
        data = change2ndOccurence(data, "achievements", "achievement_rank");
        data = change2ndOccurence(data, "mounts", "mount_rank");
        data = change2ndOccurence(data, "minions", "minion_rank");
        Player player = JsonConvert.DeserializeObject<Player>(data);
        return player;
    }


    private string change2ndOccurence(string s, string oldString, string newString)
    {
        int i = s.IndexOf(oldString, s.IndexOf(oldString) + 1);
        return s.Substring(0, i) + s.Substring(i, s.Length - i).Replace(oldString, newString);
    }

    //Request LodestoneID via Netstone using Players name and server
    private async Task<string> requestLodestoneID(string name, string surname, string server)
    {
        try
        {
            var lodestoneClient = await LodestoneClient.GetClientAsync();

            try
            {
                //Get Lodestone Id if not known
                var searchResponse = await lodestoneClient.SearchCharacter(new CharacterSearchQuery()
                {
                    CharacterName = name + " " + surname,
                    World = server
                });
                var lodestoneCharacter =
                    searchResponse?.Results
                    .FirstOrDefault(entry => entry.Name == name + " " + surname);
                string lodestoneId = lodestoneCharacter.Id;

                //If Lodestone id is known
                await lodestoneClient.GetCharacter(lodestoneId);

                return lodestoneId;
            }
            catch (HttpRequestException e)
            {
                //Handle potential errors in web request

            }
        }
        catch (HttpRequestException ex)
        {

        }
        return null;
    }

    //Request Achievements as a json from ffxivcollect pertaining to the LodestonedID id
    private async Task<string> requestAchievements(string id)
    {

        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "https://ffxivcollect.com/api/characters/" + id);
        var response = await client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            return "";
        }


    }
}
