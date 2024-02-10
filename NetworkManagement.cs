using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BepInEx.Logging;
using GameNetcodeStuff;
using JetBrains.Annotations;
using LethalNetworkAPI;
using UnityEngine;
using ForceTeleportAll;
using System.Diagnostics;
using BepInEx.Configuration;

namespace ForceTeleportAll
{
    internal static class NetworkManagement
    {
        private static ManualLogSource LoggerInstance = ForceTeleportAllBase.LoggerInstance;

        public static PlayerControllerB CurrentClient
        {
            get
            {
                return GameNetworkManager.Instance.localPlayerController;
            }
        }

        public static LethalServerEvent serverEvent = new LethalServerEvent(identifier: "FTEevent");
        public static LethalClientEvent clientEvent = new LethalClientEvent(identifier: "FTEevent");

        [PublicNetworkVariable]
        public static LethalNetworkVariable<AudioClip> ShipTeleporterSpinInverseSFX = new LethalNetworkVariable<AudioClip>(identifier: "ShipTeleporterSpinInverseSFX");
        [PublicNetworkVariable]
        public static LethalNetworkVariable<AudioClip> ShipTeleporterBeamSFX = new LethalNetworkVariable<AudioClip>(identifier: "ShipTeleporterBeamSFX");
        //[PublicNetworkVariable]
        //public static LethalNetworkVariable<string> MTResult = new LethalNetworkVariable<string>(identifier: "MTResult");

        public static LethalNetworkVariable<bool> configHostOnly = new LethalNetworkVariable<bool>(identifier: "configHostOnly");
        public static LethalNetworkVariable<bool> configHostIncluded = new LethalNetworkVariable<bool>(identifier: "configHostIncluded");
        public static LethalNetworkVariable<bool> configUserIncluded = new LethalNetworkVariable<bool>(identifier: "configUserIncluded");
        public static LethalNetworkVariable<bool> configRequireTeleporter = new LethalNetworkVariable<bool>(identifier: "configRequireTeleporter");
        public static LethalNetworkVariable<bool> configRequireInverse = new LethalNetworkVariable<bool>(identifier: "configRequireInverse");
        public static LethalNetworkVariable<bool> configRespectCooldown = new LethalNetworkVariable<bool>(identifier: "configRespectCooldown");
        public static LethalNetworkVariable<bool> configAltMethod = new LethalNetworkVariable<bool>(identifier: "configAltMethod");

        public static TeleportHandler teleportHandler;

        public static void Init()
        {
            serverEvent.OnReceived += RecieveFromClient;
            clientEvent.OnReceived += RecieveFromServer;
        }

        private static void RecieveFromClient(ulong clientId) // Get message from client
        {
            PlayerControllerB player = clientId.GetPlayerController();
            PlayerControllerB playerServer = GameNetworkManager.Instance.localPlayerController;
            LoggerInstance.LogDebug($"{playerServer.playerUsername}: Recieved teleportall command from client: {player.playerUsername}, running MassTeleport...");

            MassTeleport(clientId);
        }

        private static void RecieveFromServer() // Get server message
        {
            ulong playerId = GameNetworkManager.Instance.localPlayerController.actualClientId; // FIX HERE gets host object and not current player
            LoggerInstance.LogDebug($"{playerId} recieved teleportall command from server");
            //GameObject gameObject = new GameObject();
            teleportHandler = new TeleportHandler();// gameObject.AddComponent<TeleportHandler>();
            teleportHandler.SetTeleportData(playerId);
            teleportHandler.StartTeleport();
        }

        public static void MassTeleport(ulong clientId) // NETWORK send teleport instructions to each player
        {
            if (configAltMethod.Value)
            {
                ShipTeleporter regular = TeleportHandler.GetTeleporter();
                ShipTeleporter inverse = TeleportHandler.GetTeleporter(selectInverse: true);

                int count = StartOfRound.Instance.allPlayerScripts.Length;
                Terminal terminal = new Terminal();

                for (int i = 0; i < count; i++)
                {
                    //StartOfRound.Instance.mapScreen.radarTargets[0]; // TODO: Get radar targets and teleport them
                }

                regular.PressTeleportButtonOnLocalClient();
            }

            int _teleportCount = 0;
            LoggerInstance.LogDebug("MassTeleport Start");
            LoggerInstance.LogDebug($"{GameNetworkManager.Instance.localPlayerController} should only get this");
            PlayerControllerB terminalUser = clientId.GetPlayerController();
            LoggerInstance.LogDebug($"{terminalUser.playerUsername} used the teleportall command");

            LoggerInstance.LogDebug("Starting loop");

            for (int j = 0; j < StartOfRound.Instance.allPlayerObjects.Length; j++)
            {
                PlayerControllerB _player = StartOfRound.Instance.allPlayerScripts[j];
                if (!_player.isPlayerControlled) { continue; }
                LoggerInstance.LogDebug($"Attempting teleport {_player.playerUsername}, steamId: {_player.playerSteamId}");
                if (RoundManager.Instance.insideAINodes.Length != 0)
                {
                    LoggerInstance.LogDebug($"insideAINodes = {RoundManager.Instance.insideAINodes.Length}");
                    if (_player.isHostPlayerObject && !configHostIncluded.Value) { continue; }
                    LoggerInstance.LogDebug("Pass host check in for loop");
                    if (_player.inTerminalMenu && !configUserIncluded.Value) { continue; }
                    LoggerInstance.LogDebug("Pass userIncluded check in for loop");
                    serverEvent.InvokeClient(_player.actualClientId); // Invoke RecieveFromServer
                    ++_teleportCount;
                    LoggerInstance.LogDebug($"_teleportCount is {_teleportCount}");
                }
            }

            LoggerInstance.LogDebug($"Teleported {_teleportCount} players...");
        }

        public static void GetAudioClips() // Possible solution to errors: get spot in array after resources are loaded?
        {
            LoggerInstance.LogDebug("Inside GetAudioClips");

            AudioClip spin = ShipTeleporterSpinInverseSFX.Value;
            AudioClip beam = ShipTeleporterBeamSFX.Value;

            LoggerInstance.LogDebug($"Values for audio files are {spin} and {beam}");
            if (spin == null || beam == null)
            {
                LoggerInstance.LogDebug("Getting audio files from game");
                AudioClip[] resources = Resources.FindObjectsOfTypeAll<AudioClip>();
                LoggerInstance.LogDebug($"Found {resources.Length} audio files");

                LoggerInstance.LogDebug("Setting audio files in foreach loop");
                foreach (AudioClip resource in resources)
                {
                    
                    if (resource.name == "ShipTeleporterSpinInverse")
                    {
                        LoggerInstance.LogDebug($"Found AudioClip 'ShipTeleporterSpinInverse'");
                        spin = resource;
                    }
                    if (resource.name == "ShipTeleporterBeam")
                    {
                        LoggerInstance.LogDebug($"Found AudioClip 'ShipTeleporterBeam'");
                        beam = resource;
                    }
                }

                if (spin != null && beam != null)
                {
                    LoggerInstance.LogDebug("Setting audio files to network variables");
                    ShipTeleporterSpinInverseSFX.Value = spin;
                    ShipTeleporterBeamSFX.Value = beam;
                }
                else
                {
                    LoggerInstance.LogError("Failed to get audio files");
                }
            }
            else
            {
                LoggerInstance.LogDebug("Audio files already loaded, skipping GetAudioClips");
            }
        }
    }
}