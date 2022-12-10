using OWML.Common;
using System.Collections.Generic;
using UnityEngine;
using TheOutsider.OutsiderHandling;
using System.Linq;
using TheOutsider.MonoBehaviours;

namespace TheOutsider.OuterWildsHandling
{
    public static class OWPatches
    {
        static OutsiderShipHandler shipLog;
        public static void SetUp(AssetBundle darkBundle, ModMain main)
        {
            shipLog = new OutsiderShipHandler(darkBundle);

            IHarmonyHelper harmony = main.ModHelper.HarmonyHelper;
            var type = typeof(OWPatches);

            //---------------- Text and Ship Log ----------------//
            harmony.AddPrefix<TextTranslation>("_Translate", type, nameof(OWPatches._Translate));
            harmony.AddPrefix<TextTranslation>("_Translate_ShipLog", type, nameof(OWPatches._Translate_ShipLog));
            harmony.AddPostfix<ShipLogEntryCard>("OnEnterComputer", type, nameof(OWPatches.Post_OnEnterComputer));
            harmony.AddPrefix<ShipLogManager>("Awake", type, nameof(OWPatches.ShipLogManager_Awake));

            harmony.AddPrefix<ShipLogAstroObject>("GetName", type, nameof(OWPatches.ShipLogAstroObject_GetName));
            harmony.AddPrefix<TranslatorWord>("UpdateDisplayText", type, nameof(OWPatches.UpdateDisplayText));

            harmony.AddPrefix<NomaiRemoteCameraPlatform>("IDToPlanetString", type, nameof(OWPatches.NomaiRemoteCameraPlatform_IDToPlanetString));

            //---------------- Orbit ----------------//
            harmony.AddPrefix<AstroObject>("Awake", type, nameof(OWPatches.AstroObject_Awake));
            harmony.AddPrefix<OrbitLine>("Update", type, nameof(OWPatches.OrbitLine_Update));

            //---------------- Bramble Seed ----------------//
            harmony.AddPrefix<FogWarpVolume>("Awake", type, nameof(OWPatches.FogWarp_Awake));
            harmony.AddPrefix<InnerFogWarpVolume>("Start", type, nameof(OWPatches.Inner_Start));
            harmony.AddPrefix<OuterFogWarpVolume>("OnAwake", type, nameof(OWPatches.Outer_OnAwake));
            harmony.AddPostfix<SphericalFogWarpVolume>("RepositionWarpedBody", type, nameof(OWPatches.Post_RepositionWarpedBody));
            harmony.AddPrefix<FogWarpDetector>("TrackFogWarpVolume", type, nameof(OWPatches.TrackFogWarpVolume));

            //harmony.AddPrefix<FogWarpDetector>("ReceiveWarpedDetector", type, nameof(OWPatches.ReceiveWarpedDetector));
            //harmony.AddPrefix<FogWarpDetector>("WarpDetector", type, nameof(OWPatches.WarpDetector));

            //---------------- Dream Fire ----------------//
            //harmony.AddPrefix<DreamCampfire>("Awake", type, nameof(OWPatches.DreamFire_Awake));
            //harmony.AddPrefix<DreamArrivalPoint>("Awake", type, nameof(OWPatches.DreamArrivalPoint_Awake));
            //harmony.AddPrefix<DreamLanternItem>("OnEnterDreamWorld", type, nameof(OWPatches.DreamLanternItem_OnEnterDreamWorld));
            harmony.AddPrefix<DreamLanternItem>("OnExitDreamWorld", type, nameof(OWPatches.DreamLanternItem_OnExitDreamWorld));
            harmony.AddPrefix<RingWorldController>("OnExitDreamWorld", type, nameof(OWPatches.RingWorldController_OnExitDreamWorld));

            //---------------- Dream Audio ----------------//
            harmony.AddPrefix<DreamWorldAudioController>("OnSolarSailStart", type, nameof(OWPatches.OnSolarSailStart));
            harmony.AddPrefix<DreamWorldAudioController>("OnSolarSailStop", type, nameof(OWPatches.OnSolarSailStop));
            harmony.AddPrefix<DreamWorldAudioController>("OnStationLightFlicker", type, nameof(OWPatches.OnStationLightFlicker));
            harmony.AddPrefix<DreamWorldAudioController>("OnDamBroken", type, nameof(OWPatches.OnDamBroken));
            harmony.AddPrefix<DreamWorldAudioController>("Update", type, nameof(OWPatches.DreamWorldAudioController_Update));
            harmony.AddPrefix<SunController>("OnTriggerSupernova", type, nameof(OWPatches.OnTriggerSupernova));

            //---------------- Stop Alarm from Waking ----------------//
            harmony.AddPrefix<AlarmSequenceController>("PlaySingleChime", type, nameof(OWPatches.PlaySingleChime));
            harmony.AddPrefix<AlarmSequenceController>("IsAlarmWakingPlayer", type, nameof(OWPatches.IsAlarmWakingPlayer));
            harmony.AddPrefix<AlarmSequenceController>("UpdateWakeFraction", type, nameof(OWPatches.UpdateWakeFraction));

            //---------------- Misc ----------------//
            harmony.AddPrefix<PlayerCharacterController>("UpdateGrounded", type, nameof(OWPatches.UpdateGrounded));
            harmony.AddPrefix<SectorLightsCullGroup>("Awake", type, nameof(OWPatches.SectorLightsCullGroup_Awake));
            harmony.AddPrefix<GlobalMusicController>("UpdateTravelMusic", type, nameof(OWPatches.UpdateTravelMusic));
            harmony.AddPostfix<ShipDamageController>("Explode", type, nameof(OWPatches.Post_ShipDamageController_Explode));
            
            harmony.AddPrefix<NomaiTranslatorProp>("UpdateTimeFreeze", type, nameof(OWPatches.UpdateTimeFreeze));
            harmony.AddPrefix<ShipLogController>("EnterShipComputer", type, nameof(OWPatches.EnterShipComputer));

            harmony.AddPrefix<PlayerResources>("OnImpact", type, nameof(OWPatches.OnImpact));

            harmony.AddPrefix<GlobalMusicController>("UpdateEndTimesMusic", type, nameof(OWPatches.GlobalMusicController_UpdateEndTimesMusic));
            harmony.AddPrefix<GlobalMusicController>("OnExitTimeLoopCentral", type, nameof(OWPatches.GlobalMusicController_OnExitTimeLoopCentral));
            harmony.AddPrefix<GlobalMusicController>("OnPlayerEnterBrambleDimension", type, nameof(OWPatches.GlobalMusicController_OnPlayerEnterBrambleDimension));
            harmony.AddPrefix<GlobalMusicController>("OnPlayerExitBrambleDimension", type, nameof(OWPatches.GlobalMusicController_OnPlayerExitBrambleDimension));

            harmony.AddPrefix<PauseMenuManager>("TryOpenPauseMenu", type, nameof(OWPatches.PauseMenuManager_TryOpenPauseMenu));

            //---------------- Friend Conversation ----------------//
            harmony.AddPrefix<SolanumAnimController>("Awake", type, nameof(OWPatches.SolanumAnimController_Awake));
            harmony.AddPrefix<SolanumAnimController>("LateUpdate", type, nameof(OWPatches.SolanumAnimController_LateUpdate));
            harmony.AddPrefix<SolanumAnimController>("IsPlayerLooking", type, nameof(OWPatches.SolanumAnimController_IsPlayerLooking));
            harmony.AddPrefix<SolanumAnimController>("PlayRaiseCairns", type, nameof(OWPatches.SolanumAnimController_PlayRaiseCairns));

            harmony.AddPostfix<SolanumAnimController>("StartWritingMessage", type, nameof(OWPatches.Post_SolanumAnimController_StartWritingMessage));
            harmony.AddPrefix<SolanumAnimController>("StopWritingMessage", type, nameof(OWPatches.SolanumAnimController_StopWritingMessage));

            harmony.AddPrefix<NomaiConversationStone>("GetDisplayName", type, nameof(OWPatches.NomaiConversationStone_GetDisplayName));

            harmony.AddPrefix<NomaiConversationManager>("OnWriteResponse", type, nameof(OWPatches.NomaiConversationManager_OnWriteResponse));
            harmony.AddPrefix<NomaiConversationManager>("OnFinishDialogue", type, nameof(OWPatches.NomaiConversationManager_OnFinishDialogue));


            //---------------- DEBUG - TEMPORARY ----------------//
            if (!ModMain.isDevelopmentVersion) return;

            harmony.AddPrefix<ShipLogManager>("Start", type, nameof(OWPatches.ShipLogManager_Start)); //Complete Ship Log
            //harmony.AddPrefix<SectorDetector>("OnAddSector", type, nameof(OWPatches.OnAddSector));
        }

        //--------------------------------------------- Text and Ship Log ---------------------------------------------//
        /// <summary> Replacement for TextTranslation._Translate() to stop printing errors. </summary>
        public static bool _Translate(TextTranslation __instance, ref string __result, string __0)  //Nomai Text and Dialogue
        {
            __result = __0;
            if (__instance.m_table == null || __instance.m_table.Get(__0) == null)
            {
                if (!__0.Contains("_"))
                {
                    __0 = __0.Replace("Eye", "<color=lightblue>Eye</color>");

                    __0 = __0.Replace("Dark Bramble", "<color=lightblue>Dark Bramble</color>");
                    __0 = __0.Replace("Vessel", "<color=lightblue>Vessel</color>");

                    __0 = __0.Replace("DATURA", "<color=lightblue>DATURA</color>");
                    __0 = __0.Replace("Datura", "<color=lightblue>Datura</color>");
                    __0 = __0.Replace("FRIEND", "<color=lime>FRIEND</color>");
                    __0 = __0.Replace("Friend", "<color=lime>Friend</color>");
                    //Base game names replaced in UpdateDisplayText so effects base game text.

                    __result = __0;
                    return false;    //Nomai Text
                }

                var split = __0.Split('_');
                __result = split[split.Length - 1];   //Set text to the stuff after the names and node stuff. 
                    //Last specifically as sometimes "_merge" is used.
                return false;
            }
            return true; //False = Normal return. True = Skip/Continue to original code.
        }
        /// <summary> Stop errors, and return detail text removed from entry name. </summary>
        public static bool _Translate_ShipLog(TextTranslation __instance, ref string __result, string __0)
        {
            if (__instance.m_table == null) return false;
            if (__instance.m_table.GetShipLog(__0) == null)
            {
                //key has entry name attached to front. (E.g. Bramble VillageI found a village.)
                //Find where lower case next to upper case, remove before.

                string detail = __0; 
                for (int i = 0; i < detail.Length - 1; i++)
                {
                    var c1 = detail[i].ToString();
                    var c2 = detail[i + 1].ToString();

                    bool isLowercase = c1 == c1.ToLower() && !string.IsNullOrWhiteSpace(c1);
                    bool nextIsUppercase = c2 == c2.ToUpper() && !string.IsNullOrWhiteSpace(c2);

                    if (isLowercase && nextIsUppercase)
                    {
                        detail = detail.Remove(0, i + 1);
                        break;
                    }
                }

                __result = detail;
                return false;
            }
            return true;
        }
        /// <summary> Set entry card colors. </summary>
        public static void Post_OnEnterComputer(ShipLogEntryCard __instance)
        {
            string name = __instance.name;
            if (name.Contains("DB_")) //If Dark Bramble and not base game.
            {
                bool flag = false;
                flag |= name == "DB_FELDSPAR";
                flag |= name == "DB_FROZEN_JELLYFISH";
                flag |= name == "DB_ESCAPE_POD";
                flag |= name == "DB_NOMAI_GRAVE";
                flag |= name == "DB_VESSEL";
                
                if (!flag)
                {
                    Color color = new Color(0.58f, 0.7f, 0.63f, 1f);
                    __instance._border.color = color;
                    __instance._origBorderColor = (__instance._borderColor = __instance._border.color);
                }
            }
        }
        /// <summary> Slip in The Outsider entry data. </summary>
        public static bool ShipLogManager_Awake(ShipLogManager __instance)
        {
            shipLog.ShipLog_AwakePrefix(__instance);
            return true;
        }
        public static bool ShipLogAstroObject_GetName(ShipLogAstroObject __instance, ref string __result)
        {
            if (__instance._id == OutsiderConstants.LogPowerStation)
            {
                __result = "Bramble Power Station";
                return false;
            }
            return true;
        }

        public static void UpdateDisplayText(TranslatorWord __instance, float __0)
        {
            if (__instance.TranslatedText.Contains("<"))
            {
                float timeToGiantsDeepApproach = Mathf.Max(0f, 1210f - TimeLoop.GetSecondsElapsed());

                string newValue = string.Concat(Mathf.Floor(timeToGiantsDeepApproach / 60f));
                __instance.TranslatedText = __instance.TranslatedText.Replace("<TimeMinutesGDApproach>", newValue);

                newValue = string.Concat(Mathf.Floor(timeToGiantsDeepApproach % 60f));
                __instance.TranslatedText = __instance.TranslatedText.Replace("<TimeSecondsGDApproach>", newValue);
            }

            //Replacing here replaces all base game instances too.
            string text = __instance.TranslatedText;
            text = text.Replace("SOLANUM", "<color=orange>SOLANUM</color>");
            text = text.Replace("Solanum", "<color=orange>Solanum</color>");
            text = text.Replace("FILIX", "<color=cyan>FILIX</color>");
            text = text.Replace("Filix", "<color=cyan>Filix</color>");
            text = text.Replace("BELLS", "<color=magenta>BELLS</color>");
            text = text.Replace("Bells", "<color=magenta>Bells</color>");
            text = text.Replace("YARROW", "<color=yellow>YARROW</color>");
            text = text.Replace("Yarrow", "<color=yellow>Yarrow</color>");

            if (text.Contains("PRUNSEL"))
            {
                string l = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                string replace = "";
                for (int i = 0; i < 7; i++)
                {
                    replace += l[Random.Range(0, l.Length)];
                }
                text = text.Replace("PRUNSEL", replace);
            }

            __instance.TranslatedText = text;
        }

        public static bool NomaiRemoteCameraPlatform_IDToPlanetString(NomaiRemoteCameraPlatform.ID id, ref string __result)
        {
            if (id == OutsiderConstants.DBNomaiWarpID)
            {
                __result = UITextLibrary.GetString(UITextType.LocationDB);
                return false;
            }

            return true;
        }

        //--------------------------------------------- Orbit ---------------------------------------------//
        public static bool AstroObject_Awake(AstroObject __instance)
        {
            if (__instance.gameObject.name == OutsiderConstants.PowerStation)
            {
                var db = GameObject.Find("DarkBramble_Body").GetComponent<AstroObject>();
                db._satellite = __instance;
                __instance.SetPrimaryBody(db);
                
                var initialMotion = __instance.GetComponent<InitialMotion>();
                initialMotion.SetPrimaryBody(db.GetAttachedOWRigidbody());

                var forceDetector = __instance.GetComponentInChildren<ConstantForceDetector>();
                forceDetector._detectableFields = new ForceVolume[1] { db.GetGravityVolume() };

                var orbitLine = __instance.GetComponentInChildren<OrbitLine>();
                var lineRenderer = orbitLine.GetComponent<LineRenderer>();
                orbitLine._lineRenderer = lineRenderer;

                //var mat = Resources.FindObjectsOfTypeAll<Material>().First((x) => x.name == "Effects_SPA_OrbitLine_mat");
                //mat = new Material(mat);
                //lineRenderer.material = mat;
                orbitLine.InitializeLineRenderer();
                /*
                float dist = (db.transform.position - __instance.transform.position).magnitude;
                for (int i = 0; i < lineRenderer.positionCount; i++) lineRenderer.SetPosition(i, lineRenderer.GetPosition(i) * dist);
                */
            }

            return true;
        }
        public static bool OrbitLine_Update(OrbitLine __instance) //WHAT IS HAPPENING? WHY IS THIS BROKEN??????
        {
            var t = __instance;
            var lr = t._lineRenderer;
            string name = t.gameObject.name;

            if (name == "OrbitLine_PowerStation")
            {
                var orbitingTF = t._astroObject._primaryBody.transform;
                t.transform.position = orbitingTF.position;
                Vector3 vector = t._astroObject.transform.position - orbitingTF.position;
                Vector3 normalized = -Vector3.up;
                t.transform.rotation = Quaternion.LookRotation(vector.normalized, normalized);

                float magnitude = vector.magnitude;
                t.transform.localScale = Vector3.one * magnitude;
                
                float num = __instance.DistanceToOrbitLine(orbitingTF.position, normalized, magnitude, Locator.GetActiveCamera().transform.position);
                num = Mathf.InverseLerp(5000f, 10000f, num);

                lr.startWidth = Mathf.Lerp(8f, 0f, num);
                lr.endWidth = 0f;

                Color col = Color.white;
                lr.startColor = col;
                col.a = 0f;
                lr.endColor = col;

                return false;
            }
            return true;
        }

        //--------------------------------------------- Bramble Seed - Needs to be here as in prefab, not spawned. ---------------------------------------------//
        /// <summary> Set Fog Warp (Bramble Seed) Sectors. </summary>
        public static bool FogWarp_Awake(FogWarpVolume __instance)
        {
            if (__instance._sector == null) __instance._sector = __instance.FindSector();
            return true;
        }
        /// <summary> Get ID from name. </summary>
        public static bool Inner_Start(InnerFogWarpVolume __instance)
        {
            string name = __instance.name;
            if (name.Contains("Index"))  __instance._linkedOuterWarpName = GetIndexFromName(name);

            return true;
        }
        /// <summary> Set ID from name. </summary>
        public static bool Outer_OnAwake(OuterFogWarpVolume __instance)
        {
            string name = __instance.name;
            if (name.Contains("Index")) __instance._name = GetIndexFromName(name);

            return true;
        }
        static OuterFogWarpVolume.Name GetIndexFromName(string name)
        {
            string endNumber = $"{name[name.Length - 2]}{name[name.Length - 1]}";
            return (OuterFogWarpVolume.Name)int.Parse(endNumber);
        }
        public static void Post_RepositionWarpedBody(SphericalFogWarpVolume __instance, OWRigidbody __0, Vector3 __1, Vector3 __2, Quaternion __3)
        {
            string name = __instance.name;
            //Flip velocity if seed-to-seed to stop scout being fired backwards into seed over and over.
            if (__0.CompareTag("Probe") && name.Contains("Index"))
            {
                //if (name.Contains("Index1")) Locator.GetShipLogManager().RevealFact("PS_POWER_STATION_X4");   //from station
                if (name.Contains("Index2")) Locator.GetShipLogManager().RevealFact("PS_POWER_STATION_R1");     //from planet

                Vector3 vector = __0.GetVelocity() - __instance._attachedBody.GetVelocity();
                vector = -vector;
                vector = vector.normalized * 5f; //Fixed out speed.

                //if (name.Contains("Index11")) vector *= 0.5f; //Slow Projection Pool House seed exit speed.

                __0.SetVelocity(vector + __instance._attachedBody.GetVelocity());

                __0.GetComponentInChildren<FogWarpDetector>()._outerWarpVolume = null;    //Outer Warp not exiting properly
            }

            //Add extra speed for Anglerfish part as slowness is just annoying now.
            else if (__0.CompareTag("Ship") && name == "OuterWarp_AnglerNest")
            {
                Vector3 vector = __0.GetVelocity() - __instance._attachedBody.GetVelocity();
                vector = vector.normalized * 120f; //Increase speed

                __0.SetVelocity(vector + __instance._attachedBody.GetVelocity());
            }
        }

        public static bool TrackFogWarpVolume(FogWarpDetector __instance, FogWarpVolume __0)
        {
            FogWarpVolume volume = __0;
            var d = __instance;

            if (!d._warpVolumes.Contains(volume))
            {
                if (volume.IsOuterWarpVolume())
                {
                    //Prevent setting outer warp for portal seeds, as only meant for dimension seeds.
                    if (__0.gameObject.name.Contains("Index")) return false;
                    
                    if (d._outerWarpVolume != null) //Set to new warp volume instead of keeping old one. Fixes looped dimension signal.
                    {
                        //Debug.LogError("Entering an outer warp volume before leaving the old one!");
                        d._outerWarpVolume = null;
                    }
                }
            }

            return true;
        }

        /// <summary> Might need fix here to stop to make ATP projection work. </summary>
        public static bool WarpDetector(FogWarpVolume __instance, FogWarpDetector __0, FogWarpVolume __1)
        {

            return true;
        }

        //--------------------------------------------- Dream Fire ---------------------------------------------//
        public static bool DreamLanternItem_OnExitDreamWorld(DreamLanternItem __instance)    //Called just before move player.
        {
            var dreamworldController = Locator.GetDreamWorldController();
            dreamworldController._planetBody = dreamworldController._dreamCampfire.GetComponentInParent<AstroObject>().GetOWRigidbody();
            return true;
        }
        public static bool RingWorldController_OnExitDreamWorld(RingWorldController __instance)
        {
            var dreamworldController = Locator.GetDreamWorldController();
            if (dreamworldController._planetBody != Locator._ringWorld) return false; //Don't add ringworld volumes if not there

            return true;
        }

        //--------------------------------------------- Dream Audio ---------------------------------------------//
        //False = don't continue if sleeping at DB Fire
        public static bool OnSolarSailStart(DreamWorldAudioController __instance) => !SleepingAtDBFire(__instance);
        public static bool OnSolarSailStop(DreamWorldAudioController __instance) => !SleepingAtDBFire(__instance);
        public static bool OnStationLightFlicker(DreamWorldAudioController __instance) => !SleepingAtDBFire(__instance);
        public static bool OnDamBroken(DreamWorldAudioController __instance) => !SleepingAtDBFire(__instance);
        static bool SleepingAtDBFire(DreamWorldAudioController __instance) {
            return __instance._dreamWorldController.IsPlayerSleepingAtLocation(OutsiderConstants.DBDreamWarpID);
        }
        public static bool DreamWorldAudioController_Update(DreamWorldAudioController __instance)
        {
            if (SleepingAtDBFire(__instance)) {
                __instance._dreamFireRiverLerpPos = -1f;    //Stops river wave audio from happening.
            }

            return true;
        }
        static bool SleepingAtDBFire() => Locator.GetDreamWorldController().IsPlayerSleepingAtLocation(OutsiderConstants.DBDreamWarpID);
        public static bool PlaySingleChime(AlarmSequenceController __instance) => !SleepingAtDBFire();
        public static bool IsAlarmWakingPlayer(AlarmSequenceController __instance, ref bool __result)   //Not needed?
        {
            if (SleepingAtDBFire())
            {
                __result = false;
                return false;
            }
            return true;
        }
        public static bool UpdateWakeFraction(AlarmSequenceController __instance)
        {
            if (SleepingAtDBFire()) //Stop from waking at DB fire.
            {
                __instance._alarmCounter = 0;
                __instance._wakeFraction = 0f;
            }

            return true;
        }
        public static OWAudioSource DreamAudioSource { get; set; }
        public static bool OnTriggerSupernova(SunController __instance)
        {
            DreamAudioSource.PlayOneShot(OutsiderAudio.DreamSupernova);
            return true;
        }

        //--------------------------------------------- Misc ---------------------------------------------//
        public static bool AllowPlayerGrounded { get; set; } = true;
        public static bool ReturnToNormalTravelMusic { get; set; } = false;
        public static bool StopTravelMusicUpdate { get; set; } = false;
        public static bool DontKillByImpact { get; set; } = false;
        public static void ResetOverrides()
        {
            AllowPlayerGrounded = true;
            ReturnToNormalTravelMusic = false;
            StopTravelMusicUpdate = false;
            DontKillByImpact = false;
        }
        public static bool UpdateGrounded(PlayerCharacterController __instance)
        {
            if (AllowPlayerGrounded) return true;

            __instance._isGrounded = false;
            return false;
        }
        public static bool SectorLightsCullGroup_Awake(SectorLightsCullGroup __instance)
        {
            if (__instance.gameObject.name == "PointLight_Cull") __instance._sector = __instance.FindSector();

            return true;
        }
        public static bool UpdateTravelMusic(GlobalMusicController __instance)
        {
            if (ReturnToNormalTravelMusic) return true;
            if (StopTravelMusicUpdate) return false;

            return true;
        }
        public static void Post_ShipDamageController_Explode(ShipDamageController __instance, bool __0)  //Post
        {
            var doorVolumes = GameObject.FindObjectsOfType<ShipExplodeDoorVolume>();
            foreach (var doorVolume in doorVolumes) doorVolume.OnShipExplode();
        }
        public static Sector DBSector { get; set; }
        public static Sector FriendSector { get; set; }
        public static bool UpdateTimeFreeze(NomaiTranslatorProp __instance, bool __0, NomaiText __1)
        {
            if (DBSector._playerInTriggerVolume) return false; //Don't freeze time while reading text at Dark Bramble.
            if (InDream) return false; //Don't freeze time while talking to Friend either.

            return true;
        }
        public static bool EnterShipComputer(ShipLogController __instance)
        {
            PlayerData._settingsSave.freezeTimeWhileReadingShipLog = false;
            return true;
        }

        public static bool OnImpact(PlayerResources __instance, ImpactData __0)
        {
            if (DontKillByImpact)
            {
                float num = Mathf.Clamp01((__0.speed - __instance.GetMinImpactSpeed())
                    / (__instance.GetMaxImpactSpeed() - __instance.GetMinImpactSpeed()));
                if (PlayerState.InUndertowVolume() && num > 0.001f)
                {
                    num = Mathf.Clamp(num, 0.2f, 0.8f);
                }
                bool flag = __instance.ApplyInstantDamage(100f * num, InstantDamageType.Impact);
                if (__instance._currentHealth < 1f)
                {
                    __instance._currentHealth = 1f;
                }

                return false;
            }

            return true;
        }

        public static bool GlobalMusicController_UpdateEndTimesMusic(GlobalMusicController __instance)
        {
            if (__instance._playingFinalEndTimes)
            {
                if (!ShouldPlayNewFinalVoyageMusic()) return true; //Skip to normal if shouldn't play

                if (TimeLoop.IsTimeLoopEnabled()) //Stop final end times.
                {
                    __instance._playingFinalEndTimes = false;
                    //__instance._finalEndTimesIntroSource.FadeOut(5f, OWAudioSource.FadeOutCompleteAction.STOP, 0f);
                    __instance._finalEndTimesLoopSource.FadeOut(5f, OWAudioSource.FadeOutCompleteAction.STOP, 0f);
                    if (TimeLoop.GetSecondsRemaining() > 85f)
                    {
                        Locator.GetAudioMixer().UnmixEndTimes(5f);
                    }
                }

                return false; //Stop from changing to DB source.
            }

            return true;
        }
        public static bool GlobalMusicController_OnExitTimeLoopCentral(GlobalMusicController __instance, OWRigidbody __0)
        {
            if (!ShouldPlayNewFinalVoyageMusic()) return true; //Skip to normal if shouldn't play

            if (__0.CompareTag("Player") && !TimeLoop.IsTimeLoopEnabled() && !__instance._playingFinalEndTimes)
            {
                Locator.GetAudioMixer().MixEndTimes(5f);

                var source = __instance._finalEndTimesLoopSource;
                source._audioLibraryClip = AudioType.None;
                source.clip = OutsiderAudio.FinalVoyage;
                source.SetMaxVolume(0.6f);
                //source._audioSource.volume = 0.6f;
                source.FadeIn(2f, false, false, 1f);
                __instance._playingFinalEndTimes = true;
            }
            return false;
        }
        public static bool GlobalMusicController_OnPlayerEnterBrambleDimension(GlobalMusicController __instance)
        {
            return !ShouldPlayNewFinalVoyageMusic();    //If new music, return false = don't do normal code.
        }
        public static bool GlobalMusicController_OnPlayerExitBrambleDimension(GlobalMusicController __instance)
        {
            return !ShouldPlayNewFinalVoyageMusic();
        }
        public static bool ShouldPlayNewFinalVoyageMusic()
        {
            var log = Locator.GetShipLogManager();
            return log.IsFactRevealed("OUTSIDER_EYE_OTU_R1") || log.IsFactRevealed("IP_DREAM_HOME_X2");
        }
        public static bool PreventPausing { get; set; }
        public static bool PauseMenuManager_TryOpenPauseMenu(NomaiConversationManager __instance)
        {
            if (PreventPausing) return false;
            return true;
        }

        //--------------------------------------------- Friend Conversation ---------------------------------------------//
        static bool InDream => Locator.GetDreamWorldController().IsInDream();
        public static bool SolanumAnimController_Awake(SolanumAnimController __instance)
        {
            var animator = __instance.GetComponentInChildren<Animator>();
            if (animator != null) 
            {
                __instance._animator = animator;
                __instance._headBoneTransform = __instance._animator.GetBoneTransform(HumanBodyBones.Head);
                __instance._leftHandTransform = __instance._animator.GetBoneTransform(HumanBodyBones.LeftHand);
                __instance._rightHandTransform = __instance._animator.GetBoneTransform(HumanBodyBones.RightHand);
                __instance._staffUnlocked.SetActive(false);
            }
            return false;
        }
        public static bool SolanumAnimController_LateUpdate(SolanumAnimController __instance)
        {
            if (__instance._animator == null)
            {
                __instance.Awake();
                return false;
            }

            return true;
        }
        public static bool SolanumAnimController_PlayRaiseCairns(SolanumAnimController __instance)
        {
            FriendConversationFixes.Instance.MakePillarsAppear();
            return true;
        }
        public static bool SolanumAnimController_IsPlayerLooking(SolanumAnimController __instance, ref bool __result)
        {
            __result = true;
            return false;
        }
        public static void Post_SolanumAnimController_StartWritingMessage(SolanumAnimController __instance)
        {
            if (InDream) //Because not being called by animator.
            {
                FriendTorchActivation(true);

                FriendConversationFixes.Instance.BeginWriting(__instance.AnimEvent_WriteResponse);
            }
        }
        public static bool SolanumAnimController_StopWritingMessage(SolanumAnimController __instance)
        {
            if (InDream)
            {
                FriendTorchActivation(false);
            }
            return true;
        }
        public static MindProjectorTrigger FriendStaff { get; set; }
        public static void FriendTorchActivation(bool active)
        {
            //Activate/Deactivate Staff.
            if (FriendStaff._triggerVolume._shape != null)
            {
                GameObject.Destroy(FriendStaff._triggerVolume._shape); //Stop from doing mind projection.
            }

            FriendStaff.SetProjectorActive(active);
        }
        public static bool NomaiConversationStone_GetDisplayName(NomaiConversationStone __instance, ref string __result)
        {
            if (InDream)
            {
                string objName = "Disc";
                switch (__instance._word)
                {
                    case NomaiWord.Identify:    __result = $"'Identify' {objName}";     break;
                    case NomaiWord.QuantumMoon: __result = $"'Quantum Moon' {objName}"; break;

                    case NomaiWord.Explain:     __result = $"'Explain' {objName}";      break;
                    case NomaiWord.Eye:         __result = $"'Prisoner' {objName}";     break;
                    case NomaiWord.You:         __result = $"'Friend' {objName}";       break;
                    case NomaiWord.Me:          __result = $"'Me' {objName}";           break;
                    case NomaiWord.TheNomai:    __result = $"'Datura' {objName}";       break;
                }
                return false;
            }
            else return true;
        }
        public static bool NomaiConversationManager_OnWriteResponse(NomaiConversationManager __instance, int __0)
        {
            return true;
        }
        public static bool NomaiConversationManager_OnFinishDialogue(NomaiConversationManager __instance)
        {
            if (InDream)
            {
                Locator.GetShipLogManager().RevealFact("IP_DREAM_HOME_X1", true, false);
                __instance._solanumAnimController.EndConversation();
                __instance._characterDialogueTree.GetInteractVolume().DisableInteraction();
                __instance._dialogueComplete = true;

                FriendConversationFixes.Instance.BeginSymbolsAppearing();
                //for (int i = 0; i < __instance._conversationStones.Length; i++) __instance.OnTouchRock(i);

                return false;
            }
            else return true;
        }

        //--------------------------------------------- TEST ---------------------------------------------//
        public static bool ShipLogManager_Start(ShipLogManager __instance)
        {
            switch (ModMain.debugModShipLog)
            {
                case ModMain.DebugModShipLogMode.AutoCompleteAll: __instance.RevealAllFacts(); break;
                case ModMain.DebugModShipLogMode.RemoveModLogs:
                    Log.Error("Not implemented!");
                    break;
            }

            return true;
        }

        //--------------------------------------------- Extensions ---------------------------------------------//
        public static Sector FindSector(this Component obj)
        {
            var sector = obj.GetComponentInParent<Sector>();
            if (sector == null) Log.Error($"Sector not found for {obj.name}.");
            return sector;
        }
    }
}