using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx.Logging;
using JetBrains.Annotations;
using Object = UnityEngine.Object;
using LethalNetworkAPI;

namespace ForceTeleportAll
{
    internal class TeleportHandler : MonoBehaviour
    {
        private static readonly ManualLogSource LoggerInstance = ForceTeleportAllBase.LoggerInstance;
        //private static bool _tBounce;
        public static bool isMod;

        public static System.Random _random;

        private static PlayerControllerB PlayerToTeleport;
        private static AudioSource PlayerAudioSource;

        private AudioClip ShipTeleporterSpinInverseSFX;
        private AudioClip ShipTeleporterBeamSFX;

        public static Vector3 GetRandomPosition()
        {
            LoggerInstance.LogDebug($"Getting random teleport position");
            int _num = _random.Next(0, RoundManager.Instance.insideAINodes.Length);
            Vector3 _position = RoundManager.Instance.insideAINodes[_num].transform.position;
            LoggerInstance.LogDebug($"Teleport position set to {_position}");
            return _position;
        }

        public void SetTeleportData(ulong clientId)
        {
            PlayerToTeleport = clientId.GetPlayerController();
            PlayerAudioSource = (AudioSource)PlayerToTeleport.gameObject.AddComponent(typeof(AudioSource));

            PlayerAudioSource.spatialBlend = 1f;
            PlayerAudioSource.volume = 10f;

            //NetworkManagement.GetAudioClips();
            
            try
            {
                ShipTeleporterSpinInverseSFX = NetworkManagement.ShipTeleporterSpinInverseSFX.Value;
                ShipTeleporterBeamSFX = NetworkManagement.ShipTeleporterBeamSFX.Value;
            }
            catch (Exception ex)
            {
                LoggerInstance.LogError($"An error occurred while setting teleport data: {ex.Message}");
            }
        }
        public void StartTeleport()
        {
            LoggerInstance.LogDebug("Inside StartTeleport");
            StartCoroutine(InverseTeleportPlayer());
            LoggerInstance.LogDebug("Finished StartTeleport");
        }
        private IEnumerator InverseTeleportPlayer()
        {
            //LoggerInstance.LogDebug("Inside InverseTeleportPlayer");
            //LoggerInstance.LogDebug("TEST");
            Vector3 _teleportPos = GetRandomPosition();

            //LoggerInstance.LogDebug("Inside TeleportPlayer");
            if (_teleportPos != Vector3.zero/* && !_tBounce*/)
            {
                //_tBounce = true;
                //LoggerInstance.LogDebug("_tBounce set to true");

                PlayerToTeleport.beamUpParticle.Play();
                PlayAudioOnPlayerLocal(false);
                yield return new WaitForSeconds(4.5f);
                PlayAudioOnPlayerLocal(true);
                yield return new WaitForSeconds(0.1f);
                PlayerToTeleport.TeleportPlayer(_teleportPos);
                yield return new WaitForSeconds(0.1f);
                PlayAudioOnPlayerLocal(true);
                //StartCoroutine(TeleportDebounce());
                
                LoggerInstance.LogDebug($"Finished Teleport for {PlayerToTeleport.playerUsername}");
            }
        }

        /*private IEnumerator TeleportDebounce()
        {
            //LoggerInstance.LogDebug("TeleportDebounce coroutine started");
            yield return new WaitForSeconds(0.5f);
            //LoggerInstance.LogDebug("TeleportDebounce coroutine waited for 0.5 seconds");
            _tBounce = false;
            //LoggerInstance.LogDebug("_tBounce set to false");
        }*/

        public static ShipTeleporter GetTeleporter(bool selectInverse = false)
        {
            ShipTeleporter[] array = FindObjectsOfType<ShipTeleporter>();

            for (int i = 0; i < array.Length; i++)
            {
                if (selectInverse == array[i].isInverseTeleporter)
                {
                    return array[i];
                }
            }
            return null;
        }

        private void PlayAudioOnPlayerLocal(bool playOneShot)
        {
            try
            {
                if (playOneShot)
                {
                    PlayerAudioSource.transform.position = PlayerToTeleport.transform.position;
                    PlayerAudioSource.PlayOneShot(ShipTeleporterBeamSFX);
                }
                else
                {
                    PlayerAudioSource.clip = ShipTeleporterSpinInverseSFX;
                    PlayerAudioSource.transform.position = PlayerToTeleport.transform.position;
                    PlayerAudioSource.Play();
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.LogError($"PlayAudioOnPlayerLocal failed: {ex.Message}");
            }
        }
    }
}
