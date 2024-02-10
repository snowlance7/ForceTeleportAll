using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TerminalApi.Classes;
using static TerminalApi.TerminalApi;
using ForceTeleportAll;
using GameNetcodeStuff;
using System.Threading.Tasks;


namespace ForceTeleportAll
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("atomic.terminalapi")]
    public class ForceTeleportAllBase : BaseUnityPlugin
    {
        private const string modGUID = "Snowlance.ForceTeleportAll";
        private const string modName = "Force Teleport Everyone";
        private const string modVersion = "1.0.0";

        private static string MTResult;

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ConfigEntry<bool> configHostOnly;
        public static ConfigEntry<bool> configHostIncluded;
        public static ConfigEntry<bool> configUserIncluded;
        public static ConfigEntry<bool> configRequireTeleporter;
        public static ConfigEntry<bool> configRequireInverse;
        public static ConfigEntry<bool> configRespectCooldown;
        public static ConfigEntry<bool> configAltMethod;

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

            NetworkManagement.Init();

            configHostOnly = PluginInstance.Config.Bind("Host Settings", "Host only?", false, "If this is left true, only the host will be able to run the command.");
            configHostIncluded = PluginInstance.Config.Bind("Host Settings", "Should Host Teleport?", true, "If this is off, the command will teleport everyone EXCEPT for the host.");
            configUserIncluded = PluginInstance.Config.Bind("Teleporter Settings", "Should Terminal User Teleport?", true, "If set to false, will not teleport whoever used the command.");
            configRequireTeleporter = PluginInstance.Config.Bind("Teleporter Settings", "Require Teleporter?", true, "If this is true, a teleporter needs to be bought first before you can use the command.");
            configRequireInverse = PluginInstance.Config.Bind("Teleporter Settings", "Require Inverse Teleporter?", true, "If this is true, an inverse teleporter needs to be bought first before you can use the command.");
            configRespectCooldown = PluginInstance.Config.Bind("Teleporter Settings", "Respect Cooldown?", false, "If left as false, this will run the command even if the teleporters are on cooldown.");
            configAltMethod = PluginInstance.Config.Bind("Alternate Method", "Use Alt Method?", true, "If this is true, the command will use the alternative method of teleporting players.");

            harmony.PatchAll();

            AddCommand("teleportall", new CommandInfo()
            {
                DisplayTextSupplier = () =>
                {
                    if (StartOfRound.Instance.inShipPhase) { return "Cannot use this command until ship is landed.\n"; }

                    LoggerInstance.LogInfo($"{NetworkManagement.CurrentClient.playerUsername}: Attempt teleporting all players...");

                    if (CheckConfigs()) { NetworkManagement.clientEvent.InvokeServer(); }
                    return MTResult + "\n";
                },
                Category = "Other"
            });
        }

        private bool CheckConfigs()
        {
            ShipTeleporter regular = TeleportHandler.GetTeleporter();
            ShipTeleporter inverse = TeleportHandler.GetTeleporter(selectInverse: true);
            ManualLogSource logger = LoggerInstance;


            logger.LogDebug("Got teleporters");
            if (!StartOfRound.Instance.shipHasLanded)
            {
                logger.LogDebug("Ship isnt landed...");
                MTResult = "Cannot use this command until ship is landed.";
                return false;
            }
            logger.LogDebug("Pass Ship has landed");
            if (regular is null && (configRequireTeleporter.Value || configAltMethod.Value))
            {
                logger.LogDebug("No regular teleporter owned");
                MTResult = "A teleporter is required to use this command...";
                return false;
            }
            logger.LogDebug($"Pass teleporter check");
            if (inverse is null && (configRequireInverse.Value || configAltMethod.Value))
            {
                

                logger.LogDebug("No inverse teleporter owned");
                MTResult = "An inverse teleporter is required to use this command...";
                return false;
            }
            else if (inverse.cooldownAmount != 0 && configRespectCooldown.Value) // CHECK FOR NULL
            {
                logger.LogDebug("Teleporter on cooldown.");
                MTResult = $"The Inverse Teleporter is on cooldown. {inverse.cooldownAmount} seconds until this command can be used...";
                return false;
            }
            logger.LogDebug($"Pass inverse check");
            if (!StartOfRound.Instance.localPlayerController.isHostPlayerObject && configHostOnly.Value)
            {
                logger.LogDebug("The host didnt use the command.");
                MTResult = "Only the server owner is allowed to run this command...";
                return false;
            }

            MTResult = $"Attempting to teleport {StartOfRound.Instance.allPlayerScripts.Length - 2} players...";
            return true;
        }
    }
}