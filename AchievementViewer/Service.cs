using AchievementViewer.Windows;
using Dalamud;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace AchievementViewer;

internal class Service
{
    internal static Configuration Configuration { get; set; } = null!;
    internal static ConfigWindow ConfigWindow { get; set; } = null!;
    internal static AchievementWindow AchievementWindow { get; set; } = null!;
    internal static CharaCardWindow CharaCardWindow { get; set; } = null!;
    internal static CharacterCache CharacterCache { get; set; } = null!;
    internal static CharData CharData { get; set; } = null!;
    internal static GameData GameData { get; set; } = null!;


    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
    [PluginService] internal static IContextMenu ContextMenu { get; private set; } = null!;
}
