using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Reflection;

namespace AchievementViewer;

public class GameData
{

    private Dictionary<string, string> servers = new Dictionary<string, string>();

    public GameData()
	{
        ParseCSV("World.csv");
    }

    //Parses Server ID to World Mapping from World.csv file into a dictionary
    private void ParseCSV(string path)
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
                        servers[newkey] = newval;
                    }
                }


            }
        }

    }

    public string GetServer(int id)
    {
        return servers[id.ToString()].Replace("\"", "");
    }

    //Read Adventurer Plate player data that is displayed onscreen
    public unsafe List<string> GetCardData()
    {

        List<string> data = new List<string>();

        if (AgentCharaCard.Instance() == null || AgentCharaCard.Instance()->Data == null)
        {
            return data;
        }


        var card = AgentCharaCard.Instance()->Data;
        data.Add(card->Name.ToString());
        data.Add(GetServer(card->WorldId));

        return data;
    }
}
