using BepInEx.Logging;
using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ForceTeleportAll;
using GameNetcodeStuff;
using BepInEx;

namespace ForceTeleportAll
{
    [HarmonyPatch]
    internal static class RoundManagerPatch
    {
        private static readonly ManualLogSource LoggerInstance = ForceTeleportAllBase.LoggerInstance;

        [HarmonyPatch(typeof(RoundManager), "GenerateNewFloor")]
        [HarmonyPostfix]
        private static void GenerateNewFloorPatch(RoundManager __instance)
        {
            LoggerInstance.LogDebug("SERVER: GenerateNewFloorPatch ran!");
            //LoggerInstance.LogDebug($"{NetworkManagement.CurrentClient.playerUsername}: HARMONY POST FIX RUNNING");

            TeleportHandler._random = new System.Random(StartOfRound.Instance.randomMapSeed + 17 + (int)GameNetworkManager.Instance.localPlayerController.playerClientId);
            LoggerInstance.LogDebug($"SERVER: Initialized a new random number generator with seed {StartOfRound.Instance.randomMapSeed}");

            if (GameNetworkManager.Instance.isHostingGame)
            {
                NetworkManagement.configHostOnly.Value = ForceTeleportAllBase.configHostOnly.Value;
                NetworkManagement.configHostIncluded.Value = ForceTeleportAllBase.configHostIncluded.Value;
                NetworkManagement.configUserIncluded.Value = ForceTeleportAllBase.configUserIncluded.Value;
                NetworkManagement.configRequireTeleporter.Value = ForceTeleportAllBase.configRequireTeleporter.Value;
                NetworkManagement.configRequireInverse.Value = ForceTeleportAllBase.configRequireInverse.Value;
                NetworkManagement.configRespectCooldown.Value = ForceTeleportAllBase.configRespectCooldown.Value;
                LoggerInstance.LogDebug($"SERVER: Config values set to {NetworkManagement.configHostOnly.Value}, {NetworkManagement.configHostIncluded.Value}, {NetworkManagement.configUserIncluded.Value}, {NetworkManagement.configRequireTeleporter.Value}, {NetworkManagement.configRequireInverse.Value}, {NetworkManagement.configRespectCooldown.Value}");

                NetworkManagement.GetAudioClips();
            }
        }
    }
}