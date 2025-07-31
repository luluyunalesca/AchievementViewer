using AchievementViewer.Windows;
using Dalamud;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Lua;
using Lumina;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Layer;
using NetStone;
using NetStone.Search;
using NetStone.Search.Character;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.System.Scheduler.Resource.SchedulerResource;

namespace AchievementViewer;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;

    private const string CommandName = "/aviewer";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("AchievementViewer");
    private ConfigWindow ConfigWindow { get; init; }
    private AchievementWindow AchievementWindow { get; init; }

    bool achWindowOpen;


    



    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        AchievementWindow = new AchievementWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(AchievementWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens Achievement Viewer Config"
        });

        achWindowOpen = false;

        //PluginInterface.UiBuilder.Draw += Update;
        PluginInterface.UiBuilder.Draw += DrawUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "CharaCard", AchievementWindow.UpdatePosition);
        AddonLifecycle.RegisterListener(AddonEvent.PreSetup, "CharaCard", OpenAchWindow);
        AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "CharaCard", CloseAchWindow);

    }

    private void OpenAchWindow(AddonEvent type, AddonArgs args)
    {
        if (!achWindowOpen)
        {
            achWindowOpen = true;
            ToggleAchievementUI();
        }
    }

    private void CloseAchWindow(AddonEvent type, AddonArgs args)
    {
        if (achWindowOpen)
        {
            achWindowOpen = false;
            ToggleAchievementUI();
        }
    }


    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        AchievementWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "CharaCard", AchievementWindow.UpdatePosition);
        AddonLifecycle.UnregisterListener(AddonEvent.PreSetup, "CharaCard", OpenAchWindow);
        AddonLifecycle.UnregisterListener(AddonEvent.PreFinalize, "CharaCard", CloseAchWindow);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleConfigUI();
    }


    

    private void DrawUI() => WindowSystem.Draw();

    
    public void ToggleConfigUI() => ConfigWindow.Toggle();

    public void ToggleAchievementUI() => AchievementWindow.Toggle();
}
