using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TheOutsider;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

namespace TheOutsiderFixes
{
    [HarmonyPatch]
    public static class BugFixPatches
    {
        static bool _fixShipLogCardPosition = false;
        static int _fixShipLogCardPositionCount = 0;
        static bool _projecting = false;

        public static void Initialize()
        {
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene == OWScene.TitleScreen)
                {
                    _fixShipLogCardPosition = true; // It cannot be true when the title is loaded at first because Initialized() has not run yet, I think
                }
                if ((loadScene == OWScene.SolarSystem && NewHorizonsCompat.ShouldLoadOutsider) || loadScene == OWScene.EyeOfTheUniverse)
                {
                    OutsiderSaveData.Load();
                    AddNewlyRevealedFactIDsFromBaseGameSaveFile();
                }
                if (loadScene == OWScene.SolarSystem)
                {
                    if (_tuneStatureIslandColliderCoroutine != null)
                    {
                        ModMain.Instance.StopCoroutine(_tuneStatureIslandColliderCoroutine);
                        _tuneStatureIslandColliderCoroutine = null;
                    }

                    if (_fixEndlessCanyonElevatorCoroutine != null)
                    {
                        ModMain.Instance.StopCoroutine(_fixEndlessCanyonElevatorCoroutine);
                        _fixEndlessCanyonElevatorCoroutine = null;
                    }

                    if (_fixDreamGravityinatorCoroutine != null)
                    {
                        ModMain.Instance.StopCoroutine(_fixDreamGravityinatorCoroutine);
                        _fixDreamGravityinatorCoroutine = null;
                    }

                    if (NewHorizonsCompat.ShouldLoadOutsider)
                    {
                        _tuneStatureIslandColliderCoroutine = ModMain.Instance.StartCoroutine(TuneStatureIslandCollider());
                        _fixEndlessCanyonElevatorCoroutine = ModMain.Instance.StartCoroutine(FixEndlessCanyonElevator());
                        _fixDreamGravityinatorCoroutine = ModMain.Instance.StartCoroutine(FixDreamGravityinator());
                    }
                }
            };

            Log.Print($"{nameof(BugFixPatches)} is initialized.");
        }

        // Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/7
        [HarmonyFinalizer]
        [HarmonyPatch(typeof(NomaiTranslatorProp), nameof(NomaiTranslatorProp.UpdateTimeFreeze))]
        public static Exception NomaiTranslatorProp_UpdateTimeFreeze_Finalizer(Exception __exception)
        {
            return null;
        }

        // Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/8
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.Awake))]
        [HarmonyAfter(new string[] { "SBtT.TheOutsider" })]
        public static void ShipLogManager_Awake_Prefix(ShipLogManager __instance)
        {
            if (!_fixShipLogCardPosition)
            {
                return;
            }
            _fixShipLogCardPosition = false; // Fixing is only done after reloading from the title screen.
            ++_fixShipLogCardPositionCount; // Shifts are accumulated, so it should be multiplied.

            var offset = new Vector2(-250f, 0);
            bool isInOutsiderShipLog = false;
            for (int i = 0; i < __instance._shipLogLibrary.entryData.Length; ++i)
            {
                if (__instance._shipLogLibrary.entryData[i].id == "DB_NORTHERN_OBSERVATORY")
                {
                    isInOutsiderShipLog = true;
                }
                if (!isInOutsiderShipLog)
                {
                    continue;
                }

                __instance._shipLogLibrary.entryData[i].cardPosition -= offset * _fixShipLogCardPositionCount;
                //TranslationForTheOutsider.Instance.Log($"{__instance._shipLogLibrary.entryData[i].id}'s cardPosition is fixed! ({__instance._shipLogLibrary.entryData[i].cardPosition})");
            }

            Log.Print("ShipLog's card positions are fixed.");
        }

        // ### Start: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/9 ###
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.SwitchToRemoteCamera))]
        public static bool NomaiRemoteCameraPlatform_SwitchToRemoteCamera_Prefix()
        {
            //TranslationForTheOutsider.Instance.Log("SwitchToRemoteCamera now.");
            _projecting = true;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.SwitchToPlayerCamera))]
        public static bool NomaiRemoteCameraPlatform_SwitchToPlayerCamera_Prefix()
        {
            //TranslationForTheOutsider.Instance.Log("SwitchToPlayerCamera now.");
            _projecting = false;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.WarpDetector))]
        public static bool FogWarpVolume_WarpDetector_Prefix()
        {
            //TranslationForTheOutsider.Instance.Log("Warped maybe.");
            return !_projecting;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FogWarpDetector), nameof(FogWarpDetector.FixedUpdate))]
        public static bool FogWarpDetector_FixedUpdate_Prefix()
        {
            return !_projecting;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NomaiRemoteCameraStreaming), nameof(NomaiRemoteCameraStreaming.NomaiRemoteCameraPlatformIDToSceneName))]
        public static bool NomaiRemoteCameraStreaming_NomaiRemoteCameraPlatformIDToSceneName_Prefix(NomaiRemoteCameraPlatform.ID id, ref string __result)
        {
            // DarkBramble's SharedStone (i.e., projection stone) in Ash Twin Project has an id (i.e., _connectedPlatform), 100.
            // This code replace an empty string result as StreamingGroup of "DarkBramble" in L50 of NomaiRemoteCameraStreaming.cs (in its FixedUpdate)
            //   - _sceneName of StreamingGroup of DarkBramble is "DarkBramble", so it works.
            // The Outsider did not fix this function but fixed IDToPlanetString, see https://github.com/StreetlightsBehindTheTrees/Outer-Wilds-The-Outsider/blob/65d297e9e2a9e02c7ea40c72b1a109c440a0b2e0/TheOutsider/OuterWildsHandling/OWPatches.cs#L283
            if ((int)id == 100)
            {
                //TranslationForTheOutsider.Instance.Log("id to darkbramble.");
                __result = "DarkBramble";
                return false;
            }
            return true;
        }
        // ### End: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/9 ###

        // Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/11
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NomaiWallText), nameof(NomaiWallText.Show))]
        public static void NomaiWallText_Show_Postfix(NomaiWallText __instance)
        {
            if (__instance.name == "Text_YarrowOtherSide")
            {
                __instance.GetComponent<BoxCollider>().enabled = true;
                Log.Print($"Fixed the collider of {__instance.name}.");
            }
        }

        // ### Start: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/12 ###
        static bool _gettingEntries = false;
        static List<string> _outsiderFactIDPrefixes; // {"PS_POWER_STATION", "DB_NORTHERN_OBSERVATORY", ...}
        static List<string> _newlyRevealedFactIDsFromBaseGameSaveFile = null;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.Awake))]
        [HarmonyAfter(new string[] { "SBtT.TheOutsider" })]
        public static void ShipLogManager_Awake_Prefix_Issue12(ShipLogManager __instance)
        {
            _newlyRevealedFactIDsFromBaseGameSaveFile = null;

            var length = __instance._shipLogXmlAssets.Length;

            _outsiderFactIDPrefixes = new List<string>();
            _gettingEntries = true;
            try
            {
                for (int i = 1; i <= 4; ++i)
                {
                    __instance.GenerateEntriesFromXml(__instance._shipLogXmlAssets[length - i]);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
            finally
            {
                _gettingEntries = false;
            }

            // If the user played The Outsider with a previous version of Translation and Patches for The Outsider, the save file may have newly revealed facts of The Outsider.
            // So, we check it, and if they exist, move the newly revealed facts of The Outsider into our New Horizons based save file.
            bool save = false;
            for (int i = PlayerData._currentGameSave.newlyRevealedFactIDs.Count - 1; i >= 0; --i)
            {
                //TranslationForTheOutsider.Instance.Log($"saved newlyRevealedFactIDs in base game save file: {id}");
                var id = PlayerData._currentGameSave.newlyRevealedFactIDs[i];
                if (IsModdedFact(id))
                {
                    Log.Print($"newly revealed fact mod id: {id} is found in the base game save file, so it is moved to the mod save file.");
                    if (_newlyRevealedFactIDsFromBaseGameSaveFile == null)
                    {
                        _newlyRevealedFactIDsFromBaseGameSaveFile = new List<string>();
                    }
                    _newlyRevealedFactIDsFromBaseGameSaveFile.Add(id);
                    //NewHorizonsData.AddNewlyRevealedFactID(id); // it cannot work because Load() has not run yet.
                    PlayerData._currentGameSave.newlyRevealedFactIDs.RemoveAt(i);
                    save = true;
                }
            }
            if (save)
            {
                PlayerData.SaveCurrentGame();
            }
        }

        static void AddNewlyRevealedFactIDsFromBaseGameSaveFile()
        {
            if (_newlyRevealedFactIDsFromBaseGameSaveFile != null)
            {
                foreach (var id in _newlyRevealedFactIDsFromBaseGameSaveFile)
                {
                    OutsiderSaveData.AddNewlyRevealedFactID(id);
                }
                _newlyRevealedFactIDsFromBaseGameSaveFile = null;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.AddEntry))]
        public static bool ShipLogManager_AddEntry_Prefix(ShipLogEntry entry)
        {
            if (_gettingEntries)
            {
                //TranslationForTheOutsider.Instance.Log($"{entry._id}, {entry._name}");
                _outsiderFactIDPrefixes.Add(entry._id);
                return false;
            }
            return true;
        }

        static bool IsModdedFact(string id)
        {
            foreach (var idPrefixes in _outsiderFactIDPrefixes)
            {
                if (id.StartsWith(idPrefixes))
                {
                    Log.Print($"{id} is modded fact");
                    return true;
                }
            }
            return false;
        }

        // ###### See https://github.com/Outer-Wilds-New-Horizons/new-horizons/blob/e2a07d33106f52667a30fec85c5fc2e957086dad/NewHorizons/Patches/PlayerPatches/PlayerDataPatches.cs#L96-L142 ######
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.AddNewlyRevealedFactID))]
        public static bool PlayerData_AddNewlyRevealedFactID(string id)
        {
            if (IsModdedFact(id))
            {
                OutsiderSaveData.AddNewlyRevealedFactID(id);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.GetNewlyRevealedFactIDs))]
        public static bool PlayerData_GetNewlyRevealedFactIDs_Prefix(ref List<string> __result)
        {
            var newHorizonsNewlyRevealedFactIDs = OutsiderSaveData.GetNewlyRevealedFactIDs();
            if (newHorizonsNewlyRevealedFactIDs != null)
            {
                __result = PlayerData._currentGameSave.newlyRevealedFactIDs.Concat(newHorizonsNewlyRevealedFactIDs).ToList();
                return false;
            }
            Log.Error("Newly Revealed Fact IDs is null!");
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.GetNewlyRevealedFactIDs))]
        public static void PlayerData_GetNewlyRevealedFactIDs_Postfix(ref List<string> __result)
        {
            var manager = Locator.GetShipLogManager();
            __result = __result.Where(id => manager.GetFact(id) != null).ToList();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.ClearNewlyRevealedFactIDs))]
        public static void PlayerData_ClearNewlyRevealedFactIDs()
        {
            OutsiderSaveData.ClearNewlyRevealedFactIDs();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.ResetGame))]
        public static void PlayerData_ResetGame()
        {
            OutsiderSaveData.Reset();
        }

        // ### End: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/12 ###

        // ### Start: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/17 ###
        const string STATUE_ISLAND_BEACH_PATH = "StatueIsland_Body/Sector_StatueIsland/Geometry_StatueIsland/OtherComponentsGroup/ControlledByProxy_StatueIsland/Terrain_GD_StatueIsland_v2/LandingBeach_Floor";
        const string STATUE_ISLAND_KINEMATIC_COLLIDER = "StatueIsland_Body/KinematicColliders_StatueIsland";
        static Coroutine _tuneStatureIslandColliderCoroutine = null;

        static IEnumerator TuneStatureIslandCollider()
        {
            GameObject kinematicCollidersStatueIsland = null;
            while (true)
            {
                kinematicCollidersStatueIsland = GameObject.Find(STATUE_ISLAND_KINEMATIC_COLLIDER);
                if (kinematicCollidersStatueIsland)
                {
                    break;
                }
                yield return null;
            }

            // kinematicCollidersStatueIsland has three box colliders, so we have to get the appropriate one.
            var boxCollider = kinematicCollidersStatueIsland.GetComponents<BoxCollider>().First(x => Mathf.Abs(x.center.x - (-22.9023f)) < 0.1f);

            boxCollider.center = new Vector3(-22.9023f, -48.9804f, -75.8466f);
            boxCollider.size = new Vector3(95.8044f, 76.6496f, 67.5408f);

            //TranslationForTheOutsider.Instance.Log("box collider of statue island is fixed");
        }

        // ### End: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/17 ###

        // ### Start: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/20 ###
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.Awake))]
        [HarmonyAfter(new string[] { "SBtT.TheOutsider" })]
        public static void ShipLogManager_Awake_Prefix_Issue20(ShipLogManager __instance)
        {
            var removedIndicesList = new List<int>();
            for (var i = 1; i < __instance._shipLogLibrary.entryData.Length; ++i)
            {
                for (var j = 0; j < i; ++j)
                {
                    if (__instance._shipLogLibrary.entryData[i].id == __instance._shipLogLibrary.entryData[j].id)
                    {
                        Log.Print($"duplicated entry: {__instance._shipLogLibrary.entryData[i].id} is found. it is removed");
                        removedIndicesList.Add(i);
                    }
                }
            }

            if (removedIndicesList.Count > 0)
            {
                __instance._shipLogLibrary.entryData = __instance._shipLogLibrary.entryData.Select((x, index) => new { x, index })
                                                                                           .Where(x => !removedIndicesList.Contains(x.index))
                                                                                           .Select(x => x.x).ToArray();
            }
        }
        // ### End: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/20 ###

        // ### Start: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/22 ###
        const string ENDLESS_CANYON_ELEVATOR_UPPER_DESTINATION_PATH = "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_3/Interactibles_DreamZone_3/Elevator_Raft/ElevatorDestinations/UpperDestination";
        const string ENDLESS_CANYON_ELEVATOR_CAGE_ELEVATOR_PATH = "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_3/Interactibles_DreamZone_3/Elevator_Raft/Prefab_IP_DW_CageElevator";
        const string FRIEND_ELEVATOR_CAGE_PATH = "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_3/Outsider Dream Root/SectorDB_FriendHouseInSim/Friends Dream House/GhostElevator/Elevator_Raft(Clone)/ElevatorDestinations/UpperDestination/Cage_Body";
        static Coroutine _fixEndlessCanyonElevatorCoroutine = null;

        static IEnumerator FixEndlessCanyonElevator()
        {
            GameObject upperDestinationOfEndlessCanyonElevator = null;
            while (true)
            {
                upperDestinationOfEndlessCanyonElevator = GameObject.Find(ENDLESS_CANYON_ELEVATOR_UPPER_DESTINATION_PATH);
                if (upperDestinationOfEndlessCanyonElevator)
                {
                    break;
                }
                yield return null;
            }

            GameObject friendElevatorCage = null;
            while (true)
            {
                friendElevatorCage = GameObject.Find(FRIEND_ELEVATOR_CAGE_PATH);
                if (friendElevatorCage)
                {
                    break;
                }
                yield return null;
            }

            var clonedCage = GameObject.Instantiate(friendElevatorCage);
            clonedCage.transform.parent = upperDestinationOfEndlessCanyonElevator.transform;
            var clonedCageKinematicRigidbody = clonedCage.GetComponent<KinematicRigidbody>();
            clonedCage.transform.localPosition = Vector3.zero;
            clonedCage.transform.localRotation = Quaternion.identity;
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: transform fix end");

            var cageElevatorOfEndlessCanyonElevator = GameObject.Find(ENDLESS_CANYON_ELEVATOR_CAGE_ELEVATOR_PATH).GetComponent<CageElevator>();
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: finding cageelevator end");
            var eclipseElevatorControllerOfEndlessCanyonElevator = clonedCage.transform.Find("Prop_IP_EclipseInterfaceElevator").GetComponent<EclipseElevatorController>();
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: finding eclipse elevator controller end");
            cageElevatorOfEndlessCanyonElevator._ghostInterface = eclipseElevatorControllerOfEndlessCanyonElevator;
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting ghost interface end");
            eclipseElevatorControllerOfEndlessCanyonElevator.OnActivate += cageElevatorOfEndlessCanyonElevator.OnSwitchLevel;
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting on activate end");
            eclipseElevatorControllerOfEndlessCanyonElevator.OnDownSelected += cageElevatorOfEndlessCanyonElevator.GoDown;
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting go down end");
            eclipseElevatorControllerOfEndlessCanyonElevator.OnUpSelected += cageElevatorOfEndlessCanyonElevator.GoUp;
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting go up end");

            var clonedCageOWRigidbody = clonedCage.GetComponent<OWRigidbody>();
            clonedCageOWRigidbody._origParent = cageElevatorOfEndlessCanyonElevator.transform;
            clonedCageOWRigidbody._origParentBody = friendElevatorCage.GetComponent<OWRigidbody>()._origParentBody;
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting orig parents end");

            eclipseElevatorControllerOfEndlessCanyonElevator._lightSensors[0].GetComponent<SingleLightSensor>().enabled = true;
            eclipseElevatorControllerOfEndlessCanyonElevator._lightSensors[0].GetComponent<SphereShape>().enabled = true;
            eclipseElevatorControllerOfEndlessCanyonElevator._lightSensors[1].GetComponent<SingleLightSensor>().enabled = true;
            eclipseElevatorControllerOfEndlessCanyonElevator._lightSensors[1].GetComponent<SphereShape>().enabled = true;
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: enabled some components of light sensors end");

            cageElevatorOfEndlessCanyonElevator._doorAnimatorLeft = clonedCage.transform.Find("Structure_IP_Elevator_v2/elevator_door01").GetComponent<TransformAnimator>();
            cageElevatorOfEndlessCanyonElevator._doorAnimatorRight = clonedCage.transform.Find("Structure_IP_Elevator_v2/elevator_door02").GetComponent<TransformAnimator>();
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting doors end");
            cageElevatorOfEndlessCanyonElevator._doorAnimatorLeft._origLocalRotation = Quaternion.identity;
            cageElevatorOfEndlessCanyonElevator._doorAnimatorLeft._rotateDuration = 0;
            cageElevatorOfEndlessCanyonElevator._doorAnimatorLeft._rotateStartTime = 0;
            cageElevatorOfEndlessCanyonElevator._doorAnimatorLeft._startLocalRotation = Quaternion.identity;
            cageElevatorOfEndlessCanyonElevator._doorAnimatorLeft._targetLocalRotation = Quaternion.identity;
            cageElevatorOfEndlessCanyonElevator._doorAnimatorRight._origLocalRotation = Quaternion.identity;
            cageElevatorOfEndlessCanyonElevator._doorAnimatorRight._rotateDuration = 0;
            cageElevatorOfEndlessCanyonElevator._doorAnimatorRight._rotateStartTime = 0;
            cageElevatorOfEndlessCanyonElevator._doorAnimatorRight._startLocalRotation = Quaternion.identity;
            cageElevatorOfEndlessCanyonElevator._doorAnimatorRight._targetLocalRotation = Quaternion.identity;
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: fix parameters of doors end");
            cageElevatorOfEndlessCanyonElevator._killTriggerUpper = clonedCage.transform.Find("KillTrigger_Upper").GetComponent<OWTriggerVolume>(); // _killTriggerLower is already correct.
            cageElevatorOfEndlessCanyonElevator._killTriggerUpper.OnEntry += cageElevatorOfEndlessCanyonElevator.OnEnterKillTrigger;
            cageElevatorOfEndlessCanyonElevator._killTriggerUpper.OnExit += cageElevatorOfEndlessCanyonElevator.OnExitKillTrigger;
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting killtriggerupper functions end");
            cageElevatorOfEndlessCanyonElevator._loopingAudio = clonedCage.transform.Find("Audio/Audio_Loop_Rattle").GetComponent<OWAudioSource>();
            cageElevatorOfEndlessCanyonElevator._oneShotAudio = clonedCage.transform.Find("Audio/Audio_OneShot").GetComponent<OWAudioSource>();
            cageElevatorOfEndlessCanyonElevator._platformBody = clonedCage.GetComponent<OWRigidbody>();
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting platform body end");
            cageElevatorOfEndlessCanyonElevator._chainsRenderer = clonedCage.transform.Find("Chains").GetComponent<OWRenderer>();
            cageElevatorOfEndlessCanyonElevator._chainsRoot = clonedCage.transform.Find("Chains");
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting chains end");

            var owFlameControllerOfEndlessCanyonElevator = cageElevatorOfEndlessCanyonElevator.GetComponent<OWFlameController>();
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: getting ow flame controller end");
            owFlameControllerOfEndlessCanyonElevator._flameRenderers = new OWEmissiveRenderer[] { clonedCage.transform.Find("Prop_IP_Candle_Hanging/Candle_Hanging_Flame").GetComponent<OWEmissiveRenderer>() };
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting flame renderers end");
            owFlameControllerOfEndlessCanyonElevator._lights = new OWLight2[] { clonedCage.transform.Find("PointLight_Candle_Large").GetComponent<OWLight2>() };
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting lights end");
            owFlameControllerOfEndlessCanyonElevator._renderers = new OWEmissiveRenderer[] {
                clonedCage.transform.Find("Prop_IP_Candle_Hanging/Candle_Hanging_Wax").GetComponent<OWEmissiveRenderer>(),
                clonedCage.transform.Find("Prop_IP_Candle_Hanging/Candle_Hanging_Wick").GetComponent<OWEmissiveRenderer>(),
                clonedCage.transform.Find("Prop_IP_Lantern_Hanging").GetComponent<OWEmissiveRenderer>(),
            };
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting renderers end");

            clonedCageKinematicRigidbody.enabled = false;
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: disabling cloned cage kinematicrigidbody end");
            clonedCageOWRigidbody._suspended = true;
            clonedCageOWRigidbody._suspensionBody = friendElevatorCage.GetComponent<OWRigidbody>()._suspensionBody;
            //TranslationForTheOutsider.Instance.Log("elevator of endless canyon is fixing: setting cloned cage owrigidbody suspension settings end");

            //clonedCageOWRigidbody._suspended = false;
            //clonedCageOWRigidbody.Suspend();
            clonedCageOWRigidbody._childColliders = clonedCage.transform.Find("Structure_IP_Elevator_v2").GetComponentsInChildren<Collider>();
            foreach (var child in clonedCageOWRigidbody._childColliders)
            {
                child.GetComponent<OWCollider>().ListenForParentBodySuspension();
            }

            Log.Print("elevator of endless canyon is fixed");
        }
        // ### End: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/22 ###

        // ### Start: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/24 ###
        const string FRIEND_HEAD_PATH = "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_3/Outsider Dream Root/SectorDB_FriendHouseInSim/ConversationPivot/Character_Friend/StrangerInhabitant_Friend/Ghostbird_IP_ANIM(Clone)/Ghostbird_Skin_01:Ghostbird_Rig_V01:Base/Ghostbird_Skin_01:Ghostbird_Rig_V01:Root/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine01/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine02/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine03/Ghostbird_Skin_01:Ghostbird_Rig_V01:Spine04/Ghostbird_Skin_01:Ghostbird_Rig_V01:Neck01/Ghostbird_Skin_01:Ghostbird_Rig_V01:Neck02/Ghostbird_Skin_01:Ghostbird_Rig_V01:Head";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BadMarshmallowCan), nameof(BadMarshmallowCan.OnEatMarshmallow))]
        public static void BadMarshmallowCan_OnEatMarshmallow_Postfix(BadMarshmallowCan __instance)
        {
            if (__instance._bigHeadModeEnabled)
            {
                var friendHead = GameObject.Find(FRIEND_HEAD_PATH);
                friendHead.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);

                Log.Print("Friend has a big head");
            }
        }
        // ### End: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/24 ###

        // ### Start: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/28 ###
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RingWorldController), nameof(RingWorldController.OnExitDreamWorld))]
        [HarmonyBefore(new string[] { "SBtT.TheOutsider" })]
        public static bool RingWorldController_OnExitDreamWorld_Prefix(RingWorldController __instance)
        {
            var dreamworldController = Locator.GetDreamWorldController();
            if (dreamworldController._planetBody.gameObject != Locator._ringWorld.gameObject)
            {
                //TranslationForTheOutsider.Instance.Log("is NOT ring world");
                return false;
            }
            //TranslationForTheOutsider.Instance.Log("is ring world");

            for (int i = 0; i < __instance._enterOnWakeVolumes.Length; ++i)
            {
                __instance._enterOnWakeVolumes[i].AddObjectToVolume(Locator.GetPlayerDetector());
                __instance._enterOnWakeVolumes[i].AddObjectToVolume(Locator.GetPlayerCameraDetector());
            }
            return false;
        }
        // ### End: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/28 ###

        // ### Start: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/29 ###
        static bool SleepingAtDBFire() => Locator.GetDreamWorldController().IsPlayerSleepingAtLocation((DreamArrivalPoint.Location)450); // See https://github.com/StreetlightsBehindTheTrees/Outer-Wilds-The-Outsider/blob/1e30bffea6ed17b7d801267707099e95ea2e4482/TheOutsider/OutsiderHandling/OutsiderConstants.cs#L6 and https://github.com/StreetlightsBehindTheTrees/Outer-Wilds-The-Outsider/blob/1e30bffea6ed17b7d801267707099e95ea2e4482/TheOutsider/OuterWildsHandling/OWPatches.cs#L478
        static Coroutine _fixDreamGravityinatorCoroutine = null;
        static IEnumerator FixDreamGravityinator()
        {
            AstroObject darkBramble;
            while (true)
            {
                yield return null;
                darkBramble = Locator._darkBramble;
                if (darkBramble)
                {
                    break;
                }
            }
            AstroObject giantsDeep;
            while (true)
            {
                yield return null;
                giantsDeep = Locator._giantsDeep;
                if (giantsDeep)
                {
                    break;
                }
            }

            //TranslationForTheOutsider.Instance.Log("before pulling");
            while (true)
            {
                yield return null;
                var dist = (giantsDeep.transform.position - darkBramble.transform.position).magnitude;
                if (dist < 4250f)
                { // See https://github.com/StreetlightsBehindTheTrees/Outer-Wilds-The-Outsider/blob/1e30bffea6ed17b7d801267707099e95ea2e4482/TheOutsider/OuterWildsHandling/GiantsDeepGravityEffects.cs#L26
                    break;
                }
            }

            //TranslationForTheOutsider.Instance.Log("start pulling");
            if (SleepingAtDBFire())
            {
                yield break;
            }
            //TranslationForTheOutsider.Instance.Log("start tuning gravity");

            // See https://github.com/StreetlightsBehindTheTrees/Outer-Wilds-The-Outsider/blob/1e30bffea6ed17b7d801267707099e95ea2e4482/TheOutsider/OuterWildsHandling/GiantsDeepGravityEffects.cs#L141-L169
            var gravity = GameObject.Find("Sector_DreamWorld/Volumes_DreamWorld/DreamWorldVolume").GetComponent<DirectionalForceVolume>();
            var defaultValue = gravity._fieldMagnitude;
            var time = 0f;
            while (time < 20 + 30 + 30)
            {
                yield return null;
                if (!gravity)
                {
                    break;
                }
                gravity._fieldMagnitude = defaultValue;
                gravity._affectsAlignment = true;
                time += Time.deltaTime;
            }
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(SunController), nameof(SunController.OnTriggerSupernova))]
        //public static void SunController_OnTriggerSupernova_Prefix() 
        [HarmonyPrefix]
        [HarmonyPatch(typeof(OWAudioSource), nameof(OWAudioSource.PlayOneShot), new[] { typeof(AudioClip) })]
        public static bool OWAudioSource_PlayOneShot_Prefix(OWAudioSource __instance)
        {
            if (__instance.name == "SOUND_CREAK_0" && __instance.transform.parent && __instance.transform.parent.parent && __instance.transform.parent.parent.name == "Outsider Dream Root" && !SleepingAtDBFire())
            {
                //TranslationForTheOutsider.Instance.Log("avoid playoneshot");
                return false;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(OWAudioSource), nameof(OWAudioSource.Play), new Type[] { })]
        public static bool OWAudioSource_Play_Prefix(OWAudioSource __instance)
        {
            if (__instance.name == "SOUND_CREAK_0" && __instance.transform.parent && __instance.transform.parent.parent && __instance.transform.parent.parent.name == "Outsider Dream Root" && !SleepingAtDBFire())
            {
                //TranslationForTheOutsider.Instance.Log("avoid play");
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DreamExplosionController), nameof(DreamExplosionController.FixedUpdate))]
        public static bool DreamExplosionController_FixedUpdate_Prefix(DreamExplosionController __instance)
        {
            if (__instance.transform.parent && __instance.transform.parent.parent && __instance.transform.parent.parent.parent && __instance.transform.parent.parent.parent.name == "Friends Dream House" && !SleepingAtDBFire())
            {
                return false;
            }
            return true;
        }
        // ### End: Deal with https://github.com/TRSasasusu/TranslationForTheOutsider/issues/29 ###
    }
}
