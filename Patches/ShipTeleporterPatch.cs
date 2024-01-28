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
using static ForceTeleportAll.ForceTeleportAllBase;
using System.Runtime.CompilerServices;

namespace ForceTeleportAll
{
    [HarmonyPatch]
    internal static class ShipTeleporterPatch
    {
        [HarmonyPatch(typeof(ShipTeleporter), "SetRandomSeed")]
        [HarmonyPrefix]
        private static void SetRandomSeedPreFix(ref bool ___isInverseTeleporter, ref bool ___hasBeenSpawnedThisSession, ref bool ___hasBeenSpawnedThisSessionInverse)
        {
            LoggerInstance.LogDebug("IN PREFIX");
            if (TeleportHandler.isMod == true)
            {
                LoggerInstance.LogDebug("Ismod is true");
                ___isInverseTeleporter = true;
                ___hasBeenSpawnedThisSession = true;
                ___hasBeenSpawnedThisSessionInverse = true;
            }
        }

        [HarmonyPatch(typeof(ShipTeleporter), "SetRandomSeed")]
        [HarmonyPostfix]
        private static void SetRandomSeedPostFix(ref System.Random ___shipTeleporterSeed)
        {
            LoggerInstance.LogDebug("IN POSTFIX");
            if (TeleportHandler.isMod == true)
            {
                TeleportHandler._random = new System.Random(StartOfRound.Instance.randomMapSeed);
            }
        }
    }
}