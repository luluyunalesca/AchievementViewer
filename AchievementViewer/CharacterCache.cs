
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Xml.Linq;
using static FFXIVClientStructs.ThisAssembly.Git;
using AchievementViewer.Data;

namespace AchievementViewer;

public class CharacterCache
{

    private List<string[]> alreadyRequested = new List<string[]>();
    private List<Character> cachedChars = new List<Character>();

    private short maxCacheSize = 24;
    private short maxRequests = 24;

    public CharacterCache()
    {

    }

    public bool IsAlreadyStored(Character c)
    {
        return cachedChars.Exists(x => x.Name == c.Name && x.Server == c.Server);
    }

    public bool IsAlreadyStored(string name, ushort server)
    {
        return IsAlreadyStored(name, Service.GameData.GetServer(server));
    }

    public bool IsAlreadyStored(string name, string server)
    {
        return cachedChars.Exists(x => x.Name == name && x.Server == server);
    }

    public Character GetCharacter(string name, ushort server)
    {
        return GetCharacter(name, Service.GameData.GetServer(server));
    }

    public Character GetCharacter(string name, string server)
    {
        if (!IsAlreadyStored(name,server))
        {
            return new Character(-1);
        }
        return cachedChars.Find(x => x.Name == name && x.Server == server);
       
    }

    public bool IsAlreadyRequested(string name, string server)
    {
        return alreadyRequested.Exists(x => x[0] == name && x[1] == server);
    }

    public void AddCharacterToCache(Character c) { cachedChars.Add(c); }

    public void RemoveCharacterFromCache(Character c) { cachedChars.Remove(c); }

    public void AddCharacterToRequested(string name, ushort id)
    {
        AddCharacterToRequested(name, Service.GameData.GetServer(id));
    }

    public void AddCharacterToRequested(string name, string server)
    {
        alreadyRequested.Add(new[] { name, server});
    }

    public void RemoveCharacterFromRequested(string name, ushort id) 
    {
        RemoveCharacterFromRequested(name, Service.GameData.GetServer(id));
    }

    public void RemoveCharacterFromRequested(string name, string server)
    {
        alreadyRequested.Remove(alreadyRequested.Find(x => x[0] == name && x[1] == server));
    }
}

