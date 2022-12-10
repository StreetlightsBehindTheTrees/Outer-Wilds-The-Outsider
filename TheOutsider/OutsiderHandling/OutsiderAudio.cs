using OWML.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheOutsider.OutsiderHandling
{
    public sealed class OutsiderAudio
    {
        static AudioClip[] creaking;
        public static AudioClip insideCreaking;
        static AudioClip[] buildingDetach;
        AudioClip DaturaRuins;
        AudioClip Observatory;
        AudioClip ArtistHouse;
        AudioClip SeedStation;
        AudioClip SimBuilding;
        AudioClip FriendDream;
        AudioClip RuinsAltVer;
        public AudioClip RiverGroove;

        AudioClip DBAmbience;

        public static AudioClip DreamSupernova { get; private set; }
        public static AudioClip FinalVoyage { get; private set; }

        public static AudioClip CrushStart { get; private set; }
        public static AudioClip CrushHit { get; private set; }
        public static AudioClip CrushReturn { get; private set; }
        public static AudioClip CrushReturnHit { get; private set; }

        public OutsiderAudio(AssetBundle darkBundle, AssetBundle extraMusicBundle)
        {
            AudioClip Get(string name, bool isInExtraMusicBundle = false) {
                var clip = isInExtraMusicBundle ?
                    extraMusicBundle.LoadAsset<AudioClip>($"{name}.wav") :
                    darkBundle.LoadAsset<AudioClip>($"{name}.wav");

                if (clip == null) Log.Error($"Clip not found: {name}");
                return clip;
            }

            //---------------- Music ----------------//
            DaturaRuins = Get("ost_BrambleRuins");
            RuinsAltVer = Get("ost_StudyTower");
            ArtistHouse = Get("ost_ArtHouse");
            SeedStation = Get("ost_PowerStation");

            SimBuilding = Get("ost_WhenWorldsCollide", true);
            RiverGroove = Get("ost_RiversGrooveV2", true);
            FriendDream = Get("ost_FriendV2", true);
            Observatory = Get("ost_Datura", true);
            FinalVoyage = Get("ost_ConflictedVoyageV2", true);
            //ReplacedEnd = Get("ost_Duality");

            //---------------- Other ----------------//
            DBAmbience = Get("DB_Ambience");

            //---------------- Sound Effects ----------------//
            creaking = new AudioClip[]
            {
                Get("sfx_creakingPart1"),
                Get("sfx_creakingPart2"),
                Get("sfx_creakingPart3-V2"),
            };
            insideCreaking = Get("sfx_creakingPart2+3_insideAndDream");

            buildingDetach = new AudioClip[]
            {
                Get("sfx_buildingDetach1"),
                Get("sfx_buildingDetach2"),
                Get("sfx_buildingDetach3"),
            };

            DreamSupernova = Get("sfx_supernova_dream_V2");

            CrushStart = Get("sfx_crushStart");
            CrushHit = Get("sfx_crushHit");
            CrushReturn = Get("sfx_crushReturn");
            CrushReturnHit = Get("sfx_crushReturnHit");
        }

        public AudioClip GetAudioFromName(string name)
        {
            switch (name)
            {
                case "MUSIC_OBSERVATORY": return Observatory;
                case "MUSIC_RUINS": return DaturaRuins;
                case "MUSIC_RUINS_ALT": return RuinsAltVer;
                case "MUSIC_POWER_STATION": return SeedStation;
                case "Sim_Indoors_Volume_MUSIC": return SimBuilding;
                case "MUSIC_ART_ROOM": return ArtistHouse;
                case OutsiderConstants.FriendMusic: return FriendDream;

                case "DB_AMBIENCE": return DBAmbience;
            }
            return null;
        }

        public static AudioClip GetCreakSound(int index) => creaking[index];
        public static AudioClip GetRandomDetatchSound() => buildingDetach[Random.Range(0, buildingDetach.Length)];

        //--------------------------------------------- Audio Volumes ---------------------------------------------//
        List<AudioVolume> audioVolumes;
        public void OnLoopStart()
        {
            audioVolumes = new List<AudioVolume>();
        }
        public void AddVolume(AudioVolume av) => audioVolumes.Add(av);
        public void SetDBAudioVolumesActive(bool active)
        {
            foreach (var volume in audioVolumes)
            {
                volume.gameObject.SetActive(active);
            }
        }
    }
}