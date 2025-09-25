
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Xml.Linq;
using static FFXIVClientStructs.ThisAssembly.Git;
using AchievementViewer.Data;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Text.Json.Serialization;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Serilog;


namespace AchievementViewer;



public class CharacterCache
{
    private List<Character> cachedChars = new List<Character>();
    private Dictionary<(string, string), int> nameToIDMap = new Dictionary<(string, string), int>();

    private short maxCacheSize = 24;
    private short maxRequests = 24;

    public CharacterCache()
    {

    }

    public bool IsAlreadyStored(Character c)
    {
        return cachedChars.Exists(x => x.Id == c.Id);
    }

    public bool IsAlreadyStored(string name, ushort server)
    {
        return IsAlreadyStored(name, Service.GameData.GetServer(server));
    }

    public bool IsAlreadyStored(string name, string server)
    {
        return IsAlreadyStored(new Character(GetID(name, server), false));
        
    }

    public int GetID(string name, string server)
    {
        if (nameToIDMap.TryGetValue((name, server), out var id))
        {
            return id;
        } else
        { 
            return -1;
        }
    }

    public Character GetCharacter(string name, ushort server)
    {
        return GetCharacter(name, Service.GameData.GetServer(server));
    }

    public Character GetCharacter(string name, string server)
    {
        if (!IsAlreadyStored(name,server))
        {
            return new Character(-1, false);
        }
        int id = GetID(name, server);
        return cachedChars.Find(x => x.Id == id);
       
    }

   
    public void AddCharacterToCache(Character c) { cachedChars.Add(c); }

    public void RemoveCharacterFromCache(Character c) { cachedChars.Remove(c); }

    public void AddToMapping(string name, string server, string id)
    {
        AddToMapping(name, server, Service.CharData.ParseID(id));
    }

    public void AddToMapping(string name, string server, int id)
    {
        nameToIDMap.Add((name, server), id);

    }
}

