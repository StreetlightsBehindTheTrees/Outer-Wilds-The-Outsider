using OWML.Common;
using System.Collections.Generic;
using TheOutsider.OutsiderHandling;
using UnityEngine;

namespace TheOutsider.OuterWildsHandling
{
    public sealed class OWMusicReplacer
    {
        PlayerCharacterController controller;
        OutsiderAudio audio;

        GlobalMusicController musicController;
        OWAudioSource travelSource;
        AudioClip defaultSpaceMusic;
        float travellerVolume;

        public OWMusicReplacer(OutsiderAudio audio)
        {
            this.audio = audio;
        }
        public void OnLoopStart()
        {
            controller = Object.FindObjectOfType<PlayerCharacterController>();

            musicController = Object.FindObjectOfType<GlobalMusicController>();
            travelSource = musicController._travelSource;
            defaultSpaceMusic = travelSource._audioSource.clip;

            var managers = GameObject.Find("GlobalManagers");
            if (managers == null) {
                Log.Warning("Global Managers not found, finding with tag.");
                managers = GameObject.FindGameObjectWithTag("Global");
            }
            if (managers == null) {
                Log.Error("Global Managers not found!");
            }

            var audioManager = managers.GetComponent<AudioManager>();
            defaultSpaceMusic = audioManager.GetSingleAudioClip(AudioType.Travel_Theme, true);

            travelSource._audioLibraryClip = AudioType.None;  //Stop from replacing it every time play.
            travelSource._clipArrayLength = 0;
            travellerVolume = travelSource.GetMaxVolume();  //0.35f
        }

        public void OnUpdate()
        {
            if (OWPatches.ReturnToNormalTravelMusic)  //When enter stop trigger
            {
                if (travelSource.clip != defaultSpaceMusic)
                {
                    travelSource.SetMaxVolume(travellerVolume);
                    travelSource.clip = defaultSpaceMusic;
                    audio.SetDBAudioVolumesActive(true);
                }
                return;
            }

            if (controller._heldLanternItem != null)    //If visited dreamworld, return to normal music
            {
                if (travelSource.clip != audio.RiverGroove)
                {
                    musicController._playingFinalEndTimes = true;   //???
                    travelSource.SetMaxVolume(0.9f);
                    travelSource.clip = audio.RiverGroove;
                    audio.SetDBAudioVolumesActive(false);
                }
                if (travelSource.volume > 0.1f) //If is playing.
                {
                    if (PlayerState.IsInsideShip())
                    {
                        OWPatches.StopTravelMusicUpdate = true; //Don't fade out if in ship with artifact.
                    }
                    else
                    {
                        var db = Locator._darkBramble;
                        var player = Locator._playerBody;
                        if ((db.transform.position - player.GetPosition()).magnitude < 5000f)
                        {
                            OWPatches.StopTravelMusicUpdate = true; //Don't update/fade out if near Dark Bramble.
                        }
                        else
                        {
                            OWPatches.StopTravelMusicUpdate = false;
                        }
                    }
                }
            }
            else if (OWPatches.StopTravelMusicUpdate == false)
            {
                if (travelSource.clip != defaultSpaceMusic)
                {
                    if (travelSource.isPlaying) travelSource.FadeOut(3f, OWAudioSource.FadeOutCompleteAction.STOP); //When enter stop trigger
                    else
                    {
                        travelSource.SetMaxVolume(travellerVolume);
                        travelSource.clip = defaultSpaceMusic;
                        audio.SetDBAudioVolumesActive(true);
                    }
                }
            }
        }
    }
}