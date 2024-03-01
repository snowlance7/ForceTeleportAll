using BepInEx.Logging;
using ForceTeleportAll;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForceTeleportEveryone.Patches
{
    [HarmonyPatch(typeof(ManualCameraRenderer))]
    internal static class ManualCameraRendererPatch
    {
        private static readonly ManualLogSource LoggerInstance = ForceTeleportAllBase.LoggerInstance;

        [HarmonyPatch("")]
        [HarmonyPostfix]
        private static void temp(ManualCameraRenderer __instance)
        {
            throw new NotImplementedException();
        }
    }
}
