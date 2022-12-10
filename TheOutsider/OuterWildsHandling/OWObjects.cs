using OWML.Common;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace TheOutsider.OuterWildsHandling
{
    public class OWObjects
    {
        //---------------- Locations ----------------//
        public readonly Transform TimberHearth, GiantsDeep, DarkBramble;
        public readonly Sector DarkBrambleSector;
        public readonly Sector BrambleDimensionArea, EndlessCanyonSector;
        public readonly OuterFogWarpVolume BrambleDimensionWarpVolume;

        //---------------- Nomai ----------------//
        public readonly Transform NomaiComputer, NomaiSwitchOrb;
        public readonly Transform WhiteboardPedestal, WhiteboardStone, RemoteViewer;

        //---------------- Stranger ----------------//
        public readonly Transform AlarmBell, DreamFire, DreamArrivalPoint;
        public readonly Transform StrangerDoor, StrangerWallText, StrangerElevator, StrangerLantern, StrangerStaff;
        public readonly Transform StrangerArtifact, StrangerInhabitant;

        //---------------- Bramble ----------------//
        public readonly Transform BrambleSeed, SeedFogVolume, SeedPunctureVolume;

        //---------------- Misc ----------------//
        public readonly Transform GhostMatter, GhostMatterDust, GhostMatterObjectTrail, GhostMatterProbeAurora;
        public readonly Transform NormalCampfire;
        public readonly Transform HearthianRecorder;


        public static GravityVolume GiantsDeepGravity { get; private set; }
        public static GravityVolume DarkBrambleGravity { get; private set; }
        public static SphereOceanFluidVolume GiantsDeepOcean { get; private set; }
        public static SimpleFluidVolume GiantsDeepAtmosphere { get; private set; }
        public static CloudLayerFluidVolume GiantsDeepClouds { get; private set; }
        public static CharacterDialogueTree GabbroDialogue { get; private set; }

        public DreamExplosionController DreamExplosionController { get; set; }

        public OWObjects()
        {
            Transform Find(string s) => GameObject.Find(s).transform;

            //--------------------------------------------- References ---------------------------------------------//
            try
            {
                TimberHearth = Find("TimberHearth_Body/Sector_TH");
                GiantsDeep = Find("GiantsDeep_Body/Sector_GD");
                DarkBramble = Find("DarkBramble_Body/Sector_DB");
                DarkBrambleSector = DarkBramble.GetComponent<Sector>();
                OWPatches.DBSector = DarkBrambleSector;

                BrambleDimensionArea = Find("DB_ExitOnlyDimension_Body/Sector_ExitOnlyDimension").GetComponent<Sector>();
                BrambleDimensionWarpVolume = BrambleDimensionArea.GetComponentInChildren<OuterFogWarpVolume>();

                GiantsDeepGravity = Find("GiantsDeep_Body/GravityWell_GD").GetComponent<GravityVolume>();
                DarkBrambleGravity = Find("DarkBramble_Body/GravityWell_DB").GetComponent<GravityVolume>();

                var volumes = GiantsDeep.Find("Volumes_GD");
                GiantsDeepOcean = volumes.GetComponentInChildren<SphereOceanFluidVolume>();
                GiantsDeepAtmosphere = volumes.GetComponentInChildren<SimpleFluidVolume>();
                GiantsDeepClouds = volumes.GetComponentInChildren<CloudLayerFluidVolume>();

                GabbroDialogue = Find("Interactables_GabbroIsland/Traveller_HEA_Gabbro/ConversationZone_Gabbro").GetComponent<CharacterDialogueTree>();
            }
            catch {
                Log.Error($"WOOWOO BIG ERROR: A planet is missing! The culprit is likely a New Horizons mod!");
            }

            //--------------------------------------------- To Instantiate - DLC ---------------------------------------------//
            try
            {
                var endlessCanyon = Find("Sector_DreamWorld/Sector_DreamZone_3");
                EndlessCanyonSector = endlessCanyon.GetComponent<Sector>();
                DreamArrivalPoint = endlessCanyon.Find(
                    "Structures_DreamZone_3/DreamFireHouse_Zone_3/Interactibles_DreamFireHouse_3/Prefab_IP_DreamArrivalPoint_Zone3");

                var prisonerVault = Find("Sector_PrisonDocks/Sector_PrisonInterior/Interactibles_PrisonInterior");
                DreamFire = prisonerVault.Find("Prefab_IP_DreamCampfire");

                StrangerElevator = Find("Sector_DreamZone_3/Interactibles_DreamZone_3/Elevator_Raft");
                //Use Stranger's Elevator instead of Raft Elevator as breaks it for some reason?
                //StrangerElevator = Find("City/ZoteHouse/Elevator");

                StrangerLantern =
                    Find("PrisonControlRoom_Zone4/Interactables_PrisonControlRoom/Prefab_IP_SimpleLanternItem_Zone4PrisonControlRoom_1");
                StrangerArtifact = Find("Prefab_IP_SleepingMummy_v2/Mummy_IP_ArtifactAnim/ArtifactPivot");

                StrangerInhabitant = Find("GhostDirector_Prisoner/Prefab_IP_GhostBird_Prisoner/Ghostbird_IP_ANIM");
                StrangerStaff = Find("PrisonerSequence/VisionTorchWallSocket/Prefab_IP_VisionTorchItem/Prefab_IP_VisionTorchProjector");

                //LightDoor = Find("");
                //GhostWallText = Find("");
            }
            catch {
                Log.Error($"WOOWOO BIG ERROR: DLC not found! Make sure it's installed! Or perhaps a New Horizons mod is messing with something.");
            }

            //--------------------------------------------- To Instantiate - Base Game ---------------------------------------------//
            try
            {
                NomaiComputer = Find("Interactables_ConstructionYard/OrbitalControl/Prefab_NOM_Computer");
                NomaiSwitchOrb = Find("Props_NOM_InterfaceOrb");    //Props_ instead of Prefab_ specifies orb in Ember Twin City.

                var mines = Find("Sector_TH/Sector_NomaiMines/Interactables_NomaiMines");
                RemoteViewer = mines.Find("Prefab_NOM_RemoteViewer");
                WhiteboardPedestal = mines.Find("Prefab_NOM_Whiteboard_Shared/PedestalAnchor/Prefab_NOM_SharedPedestal");
                WhiteboardStone = mines.Find("Prefab_NOM_SharedStone");

                GhostMatter = Find("Interactables_BrambleIsland/DarkMatter_Submergible/DarkMatterVolume");
                GhostMatterDust = GhostMatter.Find("ProbeVisuals");
                GhostMatterObjectTrail = GhostMatter.Find("ObjectTrail");
                GhostMatterProbeAurora = Find("Sector_CometInterior/Effects_CometInterior/Effects_GM_AuroraWisps");

                var feldsparDimension = Find("Sector_PioneerDimension/Interactables_PioneerDimension/SeedWarp_ToPioneer (1)");
                BrambleSeed = feldsparDimension.Find("Terrain_DB_BrambleSphere_Seed_V2 (2)");
                SeedFogVolume = feldsparDimension.Find("VolumetricFogSphere (2)");
                SeedPunctureVolume = feldsparDimension.Find("Prefab_SeedPunctureVolume (2)");
                
                //NormalCampfire = Find("DB_PioneerDimension_Body/Sector_PioneerDimension/Interactables_PioneerDimension/Prefab_HEA_Campfire");
                NormalCampfire = Find("Sector_BrambleIsland/Interactables_BrambleIsland/Prefab_HEA_Campfire");

                HearthianRecorder = Find("Sector_DB/Interactables_DB/Prefab_HEA_Recorder");

                //BlackWarpCore = Find("Sector_TimeLoopExperiment/WarpCoreWallSockets/WarpCoreSocket (1)/Props_NOM_WarpCoreBlack (2)");
                //WhiteWarpCore = Find("Sector_NomaiCrater/Interactables_NomaiCrater/Prefab_NOM_WarpReceiver/Props_NOM_WarpCoreWhite");
                //WhiteWarpPad = ;
            }
            catch {
                Log.Error($"WOOWOO BIG ERROR: Object not found! The culprit is likely a New Horizons mod!");
            }
        }
    }
}