using TheOutsider.MonoBehaviours;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheOutsider.OuterWildsHandling;
using Object = UnityEngine.Object;

namespace TheOutsider.OutsiderHandling
{
    /// <summary> <see cref="OutsiderMaterials"/> | <see cref="OuterWildsHandling.OWMaterials"/> </summary>
    public sealed class ObjectModificationSS
    {
        MBsToAdd[] allToAdd;
        
        OWMaterials materials;
        OWObjects objects;

        OutsiderAudio audio;
        OWMusicReplacer OWAudio;

        AssetBundle bundle;

        Light dbAmbientLight;
        Texture2D basicOcclusionLightTexture;

        public OWObjects Objects => objects;
        public ObjectModificationSS(AssetBundle darkBundle, AssetBundle extraMusicBundle)
        {
            this.bundle = darkBundle;
            allToAdd = new MBsToAdd[]
            {
                new MBsToAdd(typeof(DarkBrambleDetacher), false,    //Start of inactive so not doing update and fixed update whole time.
                OutsiderSector.HuntingBlind,     //Whole sectors

                OutsiderSector.EyeShack, OutsiderSector.ArtHouse, OutsiderSector.FriendsHouse, OutsiderSector.DaturaHouse,"LandingPlatform",  //South Pole
                OutsiderSector.StudyTowerRoot, "Projection_Pool_House_Platform", "Projection_Pool_House_Platform (1)", //Study Tower
                "Shuttle", "UFO_Destroyed", "ShuttleCrusher_CentreOfMass", "ShuttleCrusherController", "LaunchPlatform", //Shuttle Crusher
                OutsiderSector.StarterHouse,

                "BreakAwayTree"),

                new MBsToAdd(typeof(GiantsDeepPull), false, OutsiderConstants.PowerStation),   //Dark Bramble Detacher also is this.
                
                new MBsToAdd(typeof(ObservatoryFrontDoor), "Observatory_FrontDoor"),

                new MBsToAdd(typeof(StopOverrideTravellersThemeVolume), "Sim_Indoors_Volume"),

                new MBsToAdd(typeof(ShuttleCrusher), false, "ShuttleCrusher"),
                new MBsToAdd(typeof(ShipExplodeDoorVolume), "ShipExplodeDoorVolume"),

                new MBsToAdd(typeof(BrambleLightsSwitch),
                OutsiderLightSwitch.Observatory, OutsiderLightSwitch.BrambleDimension, OutsiderLightSwitch.SouthPole,
                OutsiderLightSwitch.HuntingBlind, OutsiderLightSwitch.ShuttleCrusher, OutsiderLightSwitch.PowerStation),

                new MBsToAdd(typeof(ResetStuffOnEnterDreamworld), OutsiderConstants.FriendMusic),
                new MBsToAdd(typeof(FriendConversationFixes), "FriendConversation"),

                //new MBsToAdd(typeof(ATPTextFix), "Text_YarrowOtherSide"),
            };
            
            audio = new OutsiderAudio(darkBundle, extraMusicBundle);
            OWAudio = new OWMusicReplacer(audio);
        }
        public void OnLoopStart()
        {
            materials = new OWMaterials(bundle);
            objects = new OWObjects();
            OWAudio.OnLoopStart();
            audio.OnLoopStart();

            MiscModifications();
        }
        void MiscModifications()
        {
            //---------------- New Converstations ----------------//
            var dialogue = GameObject.Find(OWPath.FeldsparDialogue).GetComponent<CharacterDialogueTree>();
            dialogue._xmlCharacterDialogueAsset = bundle.LoadAsset<TextAsset>(OutsiderAsset.FeldsparDialogue);

            dialogue = GameObject.Find(OWPath.SlateDialogue).GetComponent<CharacterDialogueTree>();
            dialogue._xmlCharacterDialogueAsset = bundle.LoadAsset<TextAsset>(OutsiderAsset.SlateDialogue);

            //---------------- Dark Bramble New Ambient Light ----------------//
            dbAmbientLight = GameObject.Find(OWPath.DBAtmoCookie).GetComponent<Light>();
            dbAmbientLight.cookie = bundle.LoadAsset<Texture>(OutsiderAsset.DBAtmoCookie);

            //---------------- Remove extra Inhabitant from Stranger ----------------//
            var circle = GameObject.Find("Interactables_DreamFireHouse_Zone3/DreamFireChamber_DFH_Zone3/MummyCircle");
            if (circle != null)
            {
                var circleController = circle.GetComponent<MummyCircleController>();
                
                var newAnims = new Animator[20];    //Remove at index 14 and 15
                for (int i = 0; i < 14; i++)  newAnims[i] = circleController._animators[i];
                for (int i = 14; i < 20; i++) newAnims[i] = circleController._animators[i + 2];
                circleController._animators = newAnims;

                var m = circle.transform.Find("MummyPivot (8)/Prefab_IP_SleepingMummy_v2");
                if (m != null)
                {
                    GameObject.Destroy(m.Find("Mummy_IP_Anim").gameObject);
                    GameObject.Destroy(m.Find("Mummy_IP_ArtifactAnim").gameObject);
                    GameObject.Destroy(m.Find("Pointlight_IP_Mummy").gameObject);
                }
            }
        }
        public void OnUpdate()
        {
            OWAudio.OnUpdate();
        }
        public void ModifyObjects(GameObject root, Sector parentSector = null)
        {
            bool includeInactive = true;
            bool f = includeInactive;

            //--------------------------------------------- Setting ---------------------------------------------//
            //---------------- Add MonoBehaviours ----------------//
            Log.Print($"--- Root: {root.name} ---");
            Transform[] children = root.transform.GetComponentsInChildren<Transform>(f);
            foreach (Transform child in children)
            {
                foreach (MBsToAdd toAdd in allToAdd)
                {
                    foreach (string name in toAdd.objectNames)
                    {
                        if (child.name == name)
                        {
                            var c = child.gameObject.AddComponent(toAdd.type);
                            if (c is Behaviour b) b.enabled = toAdd.startActive;
                        }
                    }
                }
            }

            //---------------- Set Materials ----------------//
            var rndrs = root.GetComponentsInChildren<Renderer>(f);
            foreach (var item in rndrs)
            {
                Material mat = materials.GetMatByName(item.gameObject.name);
                if (mat != null) item.sharedMaterial = mat;
            }

            //---------------- Sort Sectors ----------------//
            if (parentSector != null)
            {
                var sectors = root.GetComponentsInChildren<Sector>(f);
                foreach (var sector in sectors)
                {
                    sector.SetParentSector(parentSector);
                }
            }

            foreach (Transform child in children)
            {
                //---------------- Set Music ----------------//
                string name = child.name;
                if (name.Contains("MUSIC") || name.Contains("AMBIENCE"))
                {
                    var audioSource = child.GetComponent<OWAudioSource>();
                    audioSource._audioLibraryClip = AudioType.None;
                    audioSource.clip = audio.GetAudioFromName(name);
                    audioSource.loop = true;
                    if (audioSource.TryGetComponent(out AudioVolume av)) audio.AddVolume(av);
                }

                //---------------- Bramble Seed Signal Fixes ----------------//
                if (name == OutsiderConstants.LogPPH) {  //Fix Mark Location on HUB signal.
                    child.GetComponent<ShipLogEntryLocation>()._outerFogWarpVolume = objects.BrambleDimensionWarpVolume;
                }
                if (name == OutsiderConstants.SeedPPH) { //Fix seed so scout signal appears properly.
                    child.GetComponent<InnerFogWarpVolume>()._containerWarpVolume = objects.BrambleDimensionWarpVolume;
                }
            }

            /*
            var audioSignals = root.GetComponentsInChildren<AudioSignal>();
            foreach (var signal in audioSignals)
            {
                var name = signal.name;
                if (name == "QuantumSignal_Datura")
                {
                    signal._name = (SignalName)1200;
                }
            }
            */

            //---------------- Occlusion ----------------//
            var occlusionGroups = root.GetComponentsInChildren<SectorVolumeOcclusionGroup>(f);
            foreach (var group in occlusionGroups)  //Occlusion scripts aren't brought over from project.
            {
                var occlusionRenderers = group.GetComponentsInChildren<MeshFilter>();
                foreach (var temp in occlusionRenderers)
                {
                    var o = temp.gameObject.AddComponent<VolumeOcclusionRenderer>();
                    o.occlusionStrength = 1f;
                    o.mesh = temp.sharedMesh;

                    if (temp.gameObject.TryGetComponent(out MeshRenderer mr)) GameObject.Destroy(mr);
                    GameObject.Destroy(temp);
                }
                var occlusionLights = group.GetComponentsInChildren<BoxShape>();
                foreach (var temp in occlusionLights)
                {
                    var o = temp.gameObject.AddComponent<VolumeOcclusionLight>();
                    o.startSize = temp.size;
                    o.endSize = temp.size + temp.center;
                    o.range = temp.size.z;

                    o.intensity = 1f;
                    o.distanceBlur = true;

                    string name = temp.name;
                    if (name.Contains("Fade")) {
                        //don't have cookie
                    }
                    else if (name.Contains("StudyTower")) o.cookie = bundle.LoadAsset<Texture2D>(OutsiderAsset.OcclusionLightST);
                    else
                    {
                        if (basicOcclusionLightTexture == null) {
                            basicOcclusionLightTexture = bundle.LoadAsset<Texture2D>(OutsiderAsset.OcclusionLightBasic);
                        }
                        o.cookie = basicOcclusionLightTexture;
                    }
                    
                    GameObject.Destroy(temp);
                }

                group.BuildGroup();
            }

            //--------------------------------------------- Instantiating ---------------------------------------------//
            //---------------- Ghost Matter Particle Trail (when something touches) ----------------//
            var ghostMatterVolumes = root.GetComponentsInChildren<DarkMatterVolume>(f);
            foreach (var ghostMatter in ghostMatterVolumes)
            {
                var trial = Object.Instantiate(objects.GhostMatterObjectTrail);
                ParentAndMoveTo(trial.transform, ghostMatter.transform);
                ghostMatter._particleTrail = trial.GetComponent<ParticleSystem>();
                /*
                var ghostMatterDust = GameObject.Instantiate(objects.GhostMatterDust.gameObject);
                ParentAndMoveTo(ghostMatterDust.transform, ghostMatter.transform);
                ghostMatterDust.SetActive(true);
                */
            }
            //---------------- Misc ----------------//
            foreach (Transform child in children)
            {
                string name = child.name;

                void InstantiateTo(string containsName, Transform objToInstantiate, Vector3 pos = default) {
                    if (name.Contains(containsName)) ParentAndMoveTo(Object.Instantiate(objToInstantiate), child, pos);
                }
                void InstantiateToAndModify(string containsName, Transform objToInstantiate, Action<Transform> action)
                {
                    if (name.Contains(containsName))
                    {
                        bool activeState = objToInstantiate.gameObject.activeSelf;
                        if (activeState) objToInstantiate.gameObject.SetActive(false);  //Deactivate if active

                        var tf = Object.Instantiate(objToInstantiate, child);
                        tf.localPosition = Vector3.zero;
                        tf.localRotation = Quaternion.identity;
                        action(tf);

                        if (activeState)    //Reactivate if was active
                        {
                            tf.gameObject.SetActive(activeState);   //Do Awake after modifying.
                            objToInstantiate.gameObject.SetActive(activeState);
                        }
                    }
                }

                //As is
                InstantiateTo("Aurora", objects.GhostMatterProbeAurora, new Vector3(0f, -1f));
                //InstantiateTo("BlackWarpCore", objects.BlackWarpCore);
                //InstantiateTo("WhiteWarpCore", objects.WhiteWarpCore);

                //Requires modifying
                InstantiateToAndModify("NormalCampfire", objects.NormalCampfire, ModifyCampfire);
                InstantiateToAndModify("DreamCampfire", objects.DreamFire, ModifyDreamFire);
                InstantiateToAndModify("DreamArrivalCampfire", objects.DreamArrivalPoint, ModifyDreamArrivalPoint);
                InstantiateToAndModify("DBSeed", objects.BrambleSeed, ModifyBrambleSeed);

                InstantiateToAndModify("NomaiComputer", objects.NomaiComputer, ModifyComputer);

                InstantiateToAndModify("RemoteViewer", objects.RemoteViewer, ModifyRemoteViewer);
                InstantiateToAndModify("WhiteboardPedestal", objects.WhiteboardPedestal, ModifyPedestal);
                InstantiateToAndModify("WhiteboardStone", objects.WhiteboardStone, ModifyWhiteboardStone);

                InstantiateToAndModify("NomaiSwitchOrb", objects.NomaiSwitchOrb, ModifyNomaiSwitchOrb);

                InstantiateToAndModify("GhostElevator", objects.StrangerElevator, ModifyStrangerElevator);
                InstantiateToAndModify("GhostLantern", objects.StrangerLantern, ModifyStrangerLantern);
                InstantiateToAndModify("StrangerArtifact", objects.StrangerArtifact, ModifyStrangerArtifact);

                InstantiateToAndModify("HearthianRecorder", objects.HearthianRecorder, ModifyHearthianRecorder);

                InstantiateToAndModify("StrangerInhabitant", objects.StrangerInhabitant, ModifyStrangerInhabitant);
            }
        }

        //--------------------------------------------- Modify Instantiated Objects ---------------------------------------------//
        #region AddingToSectorCull
        void FindAddToSectorCullGroup(Transform tf, Sector sector)
        {
            var group = sector.GetComponent<SectorCollisionGroup>();
            if (group == null)
            {
                group = sector.GetComponentInParent<SectorCollisionGroup>();
                if (Log.ErrorIf(group == null, $"No Sector Collision Group found for {tf.name} in {sector.name}!")) return;
            }

            AddToSectorCullGroup(tf, group);
        }
        void AddToSectorCullGroup(Transform tf, SectorCollisionGroup group)
        {
            var shapes = tf.GetComponentsInChildren<Shape>();
            var colliders = tf.GetComponentsInChildren<OWCollider>();
            if (shapes != null && shapes.Length != 0) group._shapes.AddRange(shapes);
            if (colliders != null && colliders.Length != 0) group._colliders.AddRange(colliders);
        }
        void AddToOWRendererCull(Transform tf, SectorCullGroup cullGroup)
        {
            var owRenderers = tf.GetComponentsInChildren<OWRenderer>();
            for (int i = 0; i < owRenderers.Length; i++)
            {
                cullGroup._dynamicRenderers.Add(owRenderers[i]);
            }
        }
        void AddLightsToSectorCull(Transform tf, Sector sector)
        {
            var lights = tf.GetComponentsInChildren<Light>();
            var lightsCullGroup = sector.GetComponent<SectorLightsCullGroup>();
            if (Log.ErrorIf(lightsCullGroup == null, $"Lights cull group not found for {tf.name}!")) return;

            foreach (var light in lights)
            {
                lightsCullGroup._staticLights.Add(new LightsCullGroup.LightData(light, light.intensity));
            }
        }
        void RemoveLights(Transform tf)
        {
            var lights = tf.GetComponentsInChildren<Light>();
            foreach (var light in lights) GameObject.Destroy(light.gameObject);
        }
        #endregion

        #region ModifyNomaiStuff
        void ModifyComputer(Transform tf)   //Modify called before Awake and after moved.
        {
            var computer = tf.GetComponent<NomaiComputer>();
            var sector = computer.FindSector();
            computer._sector = sector;

            string name = tf.parent.name;
            if (name.Contains(OutsiderConstants.ComputerParentPowerStation))
            {
                computer._nomaiTextAsset = bundle.LoadAsset<TextAsset>(OutsiderAsset.PowerStationComputer);
                sector.GetComponent<SectorLightsCullGroup>().BuildLightsCullGroup();
            }
            else if (name.Contains(OutsiderConstants.ComputerParentObservatory))
            {
                computer._nomaiTextAsset = bundle.LoadAsset<TextAsset>(OutsiderAsset.ObservatoryComputer);
                RemoveLights(tf);
            }
        }
        void ModifyRemoteViewer(Transform tf)
        {
            var sector = tf.FindSector();

            tf.GetComponentInChildren<NomaiRemoteCameraStreaming>()._sector = sector;
            var platform = tf.GetComponent<NomaiRemoteCameraPlatform>();
            platform._visualSector = sector;
            platform._visualSector2 = null;

            platform._id = OutsiderConstants.DBNomaiWarpID;

            ModifyPedestal(tf);
        }
        void ModifyPedestal(Transform tf)
        {
            var uvMapper = tf.GetComponentInChildren<QuadUVMapper>();
            uvMapper._index = 13;   //Dark Bramble

            var sector = tf.FindSector();
            var stoneSocket = tf.GetComponentInChildren<SharedStoneSocket>();
            stoneSocket._sector = sector;

            FindAddToSectorCullGroup(tf, sector);
        }
        void ModifyWhiteboardStone(Transform tf)
        {
            string name = tf.parent.name;
            var stone = tf.GetComponent<SharedStone>();
            var uvMapper = tf.GetComponentInChildren<QuadUVMapper>();

            if (name.Contains("Datura"))
            {
                var thisArc = tf.parent.Find("Text_YarrowOtherSide");
                var interiorSector = GameObject.Find(OWPath.TimeLoopSector).GetComponent<Sector>();

                //---------------- Position stone ----------------//
                var atpWhiteboard = GameObject.Find($"{OWPath.TimeLoopInteractablesHidden}/SharedWhiteboardPivot").transform;
                tf.parent = atpWhiteboard;
                tf.localPosition = new Vector3(10.67f, -25.64f, -1.69f);
                tf.localRotation = Quaternion.Euler(21.339f, -70.584f, 7.309f);

                var whiteboard = atpWhiteboard.GetComponentInChildren<NomaiSharedWhiteboard>();

                //---------------- Add New ID ----------------//
                var ids = new NomaiRemoteCameraPlatform.ID[whiteboard._remoteIDs.Length + 1];
                for (int i = 0; i < whiteboard._remoteIDs.Length; i++) ids[i] = whiteboard._remoteIDs[i];
                ids[ids.Length - 1] = OutsiderConstants.DBNomaiWarpID;
                whiteboard._remoteIDs = ids;

                //---------------- Add New Text to array ----------------//
                var arcSocket = atpWhiteboard.Find("Prefab_NOM_Whiteboard_Shared/ArcSocket");
                ParentAndMoveTo(thisArc, arcSocket);
                var thisArcWallText = thisArc.GetComponent<NomaiWallText>();
                thisArcWallText._sector = interiorSector;

                thisArcWallText.StartCoroutine(HideProperlyinator(whiteboard, thisArcWallText, stone));

                //---------------- Add New Text to Array ----------------//
                var texts = new NomaiWallText[whiteboard._nomaiTexts.Length + 1];
                for (int i = 0; i < whiteboard._nomaiTexts.Length; i++) texts[i] = whiteboard._nomaiTexts[i];
                texts[texts.Length - 1] = thisArcWallText;
                whiteboard._nomaiTexts = texts;

                stone._connectedPlatform = OutsiderConstants.DBNomaiWarpID;
                uvMapper._index = 13;   //Dark Bramble

                stone._sector = interiorSector;

                var interactablesHidden = GameObject.Find(OWPath.TimeLoopInteractablesHidden);
                var collGroup = interactablesHidden.GetComponent<SectorCollisionGroup>();
                var rndrGroup = interactablesHidden.GetComponent<SectorCullGroup>();
                AddToSectorCullGroup(tf, collGroup);
                AddToOWRendererCull(tf, rndrGroup);

                return; //FindSector will return wrong sector.
            }

            if (name.Contains("Bells"))
            {
                stone._connectedPlatform = NomaiRemoteCameraPlatform.ID.BH_GravityCannon;
                uvMapper._index = 10;    //Brittle Hollow
            }
            if (name.Contains("Solanum"))
            {
                stone._connectedPlatform = NomaiRemoteCameraPlatform.ID.HGT_TLE;
                uvMapper._index = 6;    //Ember Twin - mention going to go to Brittle Hollow
            }
            if (name.Contains("Yarrow"))
            {
                stone._connectedPlatform = NomaiRemoteCameraPlatform.ID.HGT_TimeLoop;
                uvMapper._index = 7;    //Broken Ash Twin   (Normal: 5)
            }

            var sector = tf.FindSector();
            stone._sector = sector;
            FindAddToSectorCullGroup(tf, sector);
        }
        IEnumerator HideProperlyinator(NomaiSharedWhiteboard whiteboard, NomaiWallText thisArcWallText, SharedStone stone)
        {
            yield return new WaitUntil(() => thisArcWallText._textLines.Length != 0);

            thisArcWallText.HideImmediate();    //Makes most hide but not first.
            yield return null;

            for (int i = 0; i < whiteboard._remoteIDs.Length; i++)  //Properly hide, without playing audio
            {
                if (stone.GetRemoteCameraID() == whiteboard._remoteIDs[i])
                {
                    var text = whiteboard._nomaiTexts[i];

                    text.VerifyInitialized();
                    text._collider.SetActivation(false);
                    text._animationState = NomaiWallText.AnimationState.HIDE;
                    text._hideAnimDuration = 1f;
                    text.enabled = true;
                }
            }
        }
        void ModifyNomaiSwitchOrb(Transform tf)
        {
            var forceDetector = tf.GetComponent<ConstantForceDetector>();
            forceDetector._detectableFields = new ForceVolume[0] { };

            var sector = tf.FindSector();
            var orb = tf.GetComponent<NomaiInterfaceOrb>();
            orb._sector = sector;
            orb.transform.localScale = Vector3.one * 0.8f;
            orb._slotRoot = tf.parent.parent.Find("Slots").gameObject;

            FindAddToSectorCullGroup(tf, sector);
            AddLightsToSectorCull(tf, sector);
            //tf.parent = null; //Not supposed to be under parent but might handle automatically?
        }
        
        #endregion

        #region ModifyFire
        void ModifyCampfire(Transform tf)
        {
            var campfire = tf.GetComponentInChildren<Campfire>();
            var sector = campfire.FindSector();
            campfire._sector = sector;

            HandleBaseCampfire(tf, sector);
        }
        void ModifyDreamFire(Transform tf)
        {
            var campfire = tf.GetComponentInChildren<DreamCampfire>();
            campfire._dreamArrivalLocation = OutsiderConstants.DBDreamWarpID;

            var sector = campfire.FindSector();
            campfire._sector = sector;
            tf.GetComponentInChildren<DreamCampfireStreaming>()._sector = sector;

            campfire._alarmBell = null;
            campfire._entrywayVolumes = new OWTriggerVolume[0];
            campfire._mummyCircleFlameController = null;

            HandleBaseCampfire(tf, sector);
        }
        void HandleBaseCampfire(Transform tf, Sector sector)
        {
            FindAddToSectorCullGroup(tf, sector);

            var lf = tf.GetComponentsInChildren<LightFlicker2>();
            foreach (var item in lf) item._sector = sector;

            AddLightsToSectorCull(tf, sector);
            //sector.GetComponent<SectorLightsCullGroup>().BuildLightsCullGroup();
        }
        void ModifyDreamArrivalPoint(Transform tf)
        {
            var arrivalPoint = tf.GetComponent<DreamArrivalPoint>();
            arrivalPoint._location = OutsiderConstants.DBDreamWarpID;

            var sector = arrivalPoint.FindSector();
            arrivalPoint._sector = sector;
            arrivalPoint.GetComponentInChildren<Campfire>()._sector = sector;

            FindAddToSectorCullGroup(tf, sector);

            objects.DreamExplosionController = tf.GetComponentInChildren<DreamExplosionController>();
        }
        #endregion
        
        #region ModifyStrangerStuff
        //Issue: original elevator is missing.
        void ModifyStrangerElevator(Transform tf)
        {
            var destinations = tf.Find("ElevatorDestinations");
            var bottom = destinations.Find("LowerDestination");
            var top = destinations.Find("UpperDestination");

            Vector3 pos = top.localPosition;
            pos.y = -52f;
            bottom.localPosition = pos; //Move bottom destination

            var sector = tf.FindSector();
            var lightSensors = tf.GetComponentsInChildren<SingleLightSensor>();
            var lightFlicker = tf.GetComponentsInChildren<LightFlicker2>();
            foreach (var item in lightSensors) item._sector = sector;
            foreach (var item in lightFlicker) item._sector = sector;

            //AddLightsToSectorCull(tf, sector);
            FindAddToSectorCullGroup(tf, sector);  //Return Gear
        }
        void ModifyStrangerLantern(Transform tf)
        {
            var sector = tf.FindSector();
            var lantern = tf.GetComponent<SimpleLanternItem>();
            lantern._sector = sector;
        }
        void ModifyStrangerArtifact(Transform tf)
        {
            if (tf.TryGetComponent(out OWLight2 light2)) GameObject.Destroy(light2);    //Not needed?

            if (tf.parent.name.Contains("Bright"))
            {
                var light = tf.GetComponentInChildren<Light>();
                light.range = 6f;
            }
            
            var sector = tf.FindSector();
            AddLightsToSectorCull(tf, sector);
        }
        void ModifyStrangerInhabitant(Transform tf)
        {
            try
            {
                var gb = "Ghostbird_Skin_01:Ghostbird_v004:";
                var rig = "Ghostbird_Skin_01:Ghostbird_Rig_V01:";

                var antlerParent = tf.Find($"{gb}Ghostbird_IP/{gb}Ghostbird_Accessories");

                var antlerRootLeft = antlerParent.Find($"{gb}Antlers_Left");
                var antlerRootRight = antlerParent.Find($"{gb}Antlers_Right");

                var oldAntlerLeft = antlerRootLeft.Find($"{gb}Antler_Upward");
                var oldAntlerRight = antlerRootRight.Find($"{gb}Antler_Broken 1");

                string name = tf.parent.name;
                if (name.Contains("Friend"))
                {
                    //---------------- Antlers ----------------//
                    oldAntlerLeft.gameObject.SetActive(false);
                    oldAntlerRight.gameObject.SetActive(false);

                    antlerRootLeft.Find($"{gb}Antler_Curvy").gameObject.SetActive(true);
                    antlerRootRight.Find($"{gb}Antler_Backward 1").gameObject.SetActive(true);

                    //---------------- Rescaling ----------------//
                    Transform root = tf.Find($"{rig}Base/{rig}Root");
                    Transform pelvis = root.Find($"{rig}Pelvis");

                    var spine4 = root.Find($"{rig}Spine01/{rig}Spine02/{rig}Spine03/{rig}Spine04");
                    Transform neck2 = spine4.Find($"{rig}Neck01/{rig}Neck02");

                    root.localScale = new Vector3(0.85f, 0.85f, 0.85f);
                    pelvis.localScale = new Vector3(1.15f, 1.15f, 1.15f);
                    neck2.localScale = new Vector3(1.2f, 1.2f, 1.2f);

                    //---------------- Solanum Animation ----------------//
                    var a = GameObject.Find("ConversationPivot/Character_NOM_Solanum/Nomai_ANIM_SkyWatching_Idle").GetComponent<Animator>();
                    var animatorSolanum = a.runtimeAnimatorController;

                    var animator = tf.GetComponentInChildren<Animator>();
                    animator.runtimeAnimatorController = animatorSolanum;

                    //---------------- Other Hand ----------------//
                    var shoulder = spine4.Find($"{rig}ClavicleR/{rig}ShoulderR");
                    var elbow = shoulder.Find($"{rig}ElbowR");

                    ResetStuffOnEnterDreamworld.Shoulder = shoulder;
                    ResetStuffOnEnterDreamworld.Elbow = elbow;
                    ResetStuffOnEnterDreamworld.Neck = neck2;
                    ResetStuffOnEnterDreamworld.Jaw = neck2.Find($"{rig}Head/{rig}Jaw");
                    ResetStuffOnEnterDreamworld.Friend = tf;

                    //---------------- Staff ----------------//
                    var wrist = spine4.Find($"{rig}ClavicleL/{rig}ShoulderL/{rig}ElbowL/{rig}WristL");
                    var hand = wrist .Find($"{rig}HandAttachL/VisionTorchSocket");
                    var staff = Object.Instantiate(objects.StrangerStaff, hand).transform;
                    staff.localPosition = new Vector3(0.83f, -2.4f, -0.32f);
                    staff.localRotation = Quaternion.Euler(0.2f, 20f, 20f);
                    staff.localScale = new Vector3(1.5f, 2f, 1.3f);
                    OWPatches.FriendStaff = staff.gameObject.GetComponentInChildren<MindProjectorTrigger>();

                    void RotateFinger(string finger, float r1 = 20f, float r2 = 50f, float r3 = 50f)
                    {
                        var f = wrist.Find($"{rig}{finger}01");
                        var e = f.localEulerAngles;
                        e.z += r1;
                        f.localEulerAngles = e;

                        f = f.Find($"{rig}{finger}02");
                        e = f.localEulerAngles;
                        e.z += r2;
                        f.localEulerAngles = e;

                        f = f.Find($"{rig}{finger}03");
                        e = f.localEulerAngles;
                        e.z += r3;
                        f.localEulerAngles = e;
                    }

                    RotateFinger("IndexL", 0f);
                    RotateFinger("MiddleL", 10f);
                    RotateFinger("RingL");
                }
            }
            catch
            {
                Log.Warning($"Ghost bird {tf.name} not set up properly.");
            }

            try
            {
                var prisonerEffects = tf.GetComponentInChildren<PrisonerEffects>();
                GameObject.Destroy(prisonerEffects);
            }
            catch
            {
                Log.Warning("Prisoner Effects not destroyed.");
            }
        }
        #endregion

        #region ModifyMisc
        void ModifyBrambleSeed(Transform tf)
        {
            var vol1 = Object.Instantiate(objects.SeedPunctureVolume);
            var vol2 = Object.Instantiate(objects.SeedFogVolume);
            ParentAndMoveTo(vol1, tf);
            ParentAndMoveTo(vol2, tf);

            var sector = tf.FindSector();
            FindAddToSectorCullGroup(tf, sector);
            FindAddToSectorCullGroup(vol1, sector);
            FindAddToSectorCullGroup(vol2, sector);
            //AddToOWRendererCull       //DB Sector cull?
        }
        void ModifyHearthianRecorder(Transform tf)
        {
            var dialogue = tf.GetComponent<CharacterDialogueTree>();

            string name = tf.parent.name;
            if (name.Contains("Feldspar1"))
            {
                dialogue._xmlCharacterDialogueAsset = bundle.LoadAsset<TextAsset>(OutsiderAsset.FeldsparRecording1);
                var light = tf.GetComponentInChildren<Light>();
                GameObject.Destroy(light.gameObject);
            }

            var sector = tf.FindSector();
            FindAddToSectorCullGroup(tf, sector);
            AddLightsToSectorCull(tf, sector);
        }
        #endregion

        //--------------------------------------------- Misc ---------------------------------------------//
        public void SetLocation(Transform tf, Transform targetTF)
        {
            ParentAndMoveTo(tf, targetTF);

            //---------------- Fix References ----------------//
            EffectVolume[] volumes = tf.GetComponentsInChildren<EffectVolume>(true);
            foreach (var volume in volumes) volume.ResetAttachedBody();
        }
        void ParentAndMoveTo(Transform child, Transform parent, Vector3 pos = default)
        {
            child.parent = parent;
            child.localPosition = pos;
            child.localRotation = Quaternion.identity;
        }

        //--------------------------------------------- Specific Modifications ---------------------------------------------//
        public void SetUpGDColliders(GameObject root)
        {
            var tf = root.transform;

            void SetParent(string childName, string tfName)
            {
                var child = tf.Find(childName);
                child.parent = GameObject.Find(tfName).transform;
                child.localPosition = Vector3.zero;
                child.localRotation = Quaternion.identity;
                child.localScale = Vector3.one;

                Debug.Log(child.name);  //Not Called, even though seems to be working????
            }

            SetParent("KinematicColliders_QuantumTrials", OWPath.IslandQuantum);
            SetParent("KinematicColliders_StatueIsland", OWPath.IslandStatue);
            SetParent("KinematicColliders_GabbroIsland", OWPath.IslandGabbro);
            SetParent("KinematicColliders_BrambleIsland", OWPath.IslandBramble);
            SetParent("KinematicColliders_ConstructionYardIsland", OWPath.IslandYard);
        }
        public void ModifyDreamRoot(GameObject root)
        {
            var dreamHouse = root.transform.Find("SectorDB_FriendHouseInSim/Friends Dream House").GetComponentsInChildren<Renderer>();
            materials.ReplaceWithStrangerWood(dreamHouse);

            var matrixHouse = root.transform.Find("SectorDB_FriendHouseInSim/MatrixHouse").GetComponentsInChildren<Renderer>();
            materials.ReplaceDreamMatrix(matrixHouse);
        }

        public sealed class MBsToAdd
        {
            public Type type;
            public string[] objectNames;
            public bool startActive;

            public MBsToAdd(Type type, params string[] names)
            {
                this.type = type;
                objectNames = names;
                this.startActive = true;
            }
            public MBsToAdd(Type type, bool startActive, params string[] names)
            {
                this.type = type;
                objectNames = names;
                this.startActive = startActive;
            }
        }
    }
}