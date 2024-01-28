using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TerminalApi.Classes;
using static TerminalApi.TerminalApi;
using ForceTeleportAll;
using GameNetcodeStuff;


namespace ForceTeleportAll
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("atomic.terminalapi")]
    public class ForceTeleportAllBase : BaseUnityPlugin
    {
        private const string modGUID = "Snowlance.ForceTeleportAll";
        private const string modName = "Force Teleport Everyone";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ConfigEntry<bool> configHostOnly;
        public static ConfigEntry<bool> configHostIncluded;
        public static ConfigEntry<bool> configUserIncluded;
        public static ConfigEntry<bool> configRequireTeleporter;
        public static ConfigEntry<bool> configRequireInverse;
        public static ConfigEntry<bool> configRespectCooldown;
        
        public static ManualLogSource LoggerInstance {  get; private set; }

        public static ForceTeleportAllBase PluginInstance;

        private void Awake()
        {
            if (PluginInstance == null)
            {
                PluginInstance = this;
            }

            LoggerInstance = PluginInstance.Logger;
            LoggerInstance.LogDebug($"Plugin {modName} loaded successfully.");
            LoggerInstance.LogDebug($"IS THIS SHOWING?");

            configHostOnly = PluginInstance.Config.Bind("Host Settings", "Host only?", false, "If this is left true, only the host will be able to run the command.");
            configHostIncluded = PluginInstance.Config.Bind("Host Settings", "Should Host Teleport?", true, "If this is off, the command will teleport everyone EXCEPT for the host.");
            configUserIncluded = PluginInstance.Config.Bind("Teleporter Settings", "Should Terminal User Teleport?", true, "If set to false, will not teleport whoever used the command.");
            configRequireTeleporter = PluginInstance.Config.Bind("Teleporter Settings", "Require Teleporter?", false, "If this is true, a teleporter needs to be bought first before you can use the command.");
            configRequireInverse = PluginInstance.Config.Bind("Teleporter Settings", "Require Inverse Teleporter?", false, "If this is true, an inverse teleporter needs to be bought first before you can use the command.");
            configRespectCooldown = PluginInstance.Config.Bind("Teleporter Settings", "Respect Cooldown?", false, "If left as false, this will run the command even if the teleporters are on cooldown.");

            harmony.PatchAll();

            AddCommand("teleportall", new CommandInfo()
            {
                DisplayTextSupplier = () =>
                {
                    try
                    {
                        ulong? _clientId = NetworkManagement.CurrentClient.actualClientId;
                        if ( _clientId == null ) { return "Cannot use this command until ship is landed.\n"; }
                    }
                    catch (System.Exception ex)
                    {
                        LoggerInstance.LogError($"Round Instance is null. {ex}");
                        return "Cannot use this command until ship is landed\n";
                    }
                    
                    LoggerInstance.LogInfo($"{NetworkManagement.CurrentClient.playerUsername}: Attempt teleporting all players...");

                    NetworkManagement.clientEvent.InvokeServer();
                    return NetworkManagement.MTResult.Value + "\n";
                },
                Category = "Other"
            });
        }
    }
}