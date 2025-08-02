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

public sealed class AchievementViewer : IDalamudPlugin
{

    private const string CommandName = "/aviewer";

    public readonly WindowSystem WindowSystem = new WindowSystem("AchievementViewer");

    public AchievementViewer(IDalamudPluginInterface PluginInterface)
    {
        PluginInterface.Create<Service>();

        Service.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        Service.ConfigWindow = new ConfigWindow();
        Service.CharaCardWindow = new CharaCardWindow();
        Service.AchievementWindow = new AchievementWindow();
        Service.CharacterCache = new CharacterCache();
        Service.CharData = new CharData();
        Service.GameData = new GameData();

        WindowSystem.AddWindow(Service.ConfigWindow);
        WindowSystem.AddWindow(Service.CharaCardWindow);
        WindowSystem.AddWindow(Service.AchievementWindow);

        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens Achievement Viewer Config"
        });

        ContextMenu.Enable();

        //PluginInterface.UiBuilder.Draw += Update;
        PluginInterface.UiBuilder.Draw += DrawUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        Service.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "CharaCard", Service.CharaCardWindow.UpdatePosition);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreSetup, "CharaCard", OpenCharaCardUI);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "CharaCard", CloseCharaCardUI);

    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        Service.ConfigWindow.Dispose();
        Service.AchievementWindow.Dispose();
        Service.CharaCardWindow.Dispose();

        Service.CommandManager.RemoveHandler(CommandName);

        Service.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "CharaCard", Service.CharaCardWindow.UpdatePosition);
        Service.AddonLifecycle.UnregisterListener(AddonEvent.PreSetup, "CharaCard", OpenCharaCardUI);
        Service.AddonLifecycle.UnregisterListener(AddonEvent.PreFinalize, "CharaCard", CloseCharaCardUI);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        
        ToggleConfigUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    private void OpenCharaCardUI(AddonEvent type, AddonArgs args)
    {
        Service.CharaCardWindow.IsOpen = true;
    }

    private void CloseCharaCardUI(AddonEvent type, AddonArgs args)
    {
        Service.CharaCardWindow.IsOpen = false;
    }

    public void ToggleConfigUI() => Service.ConfigWindow.Toggle();

    public void ToggleCharaCardUI() => Service.CharaCardWindow.Toggle();
}
