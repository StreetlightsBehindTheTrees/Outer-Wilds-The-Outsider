using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheOutsider
{
    public sealed class NewEndingHandler : MonoBehaviour
    {
        AnimationSegment[] animationArray;
        //BPMSegment[] bpmArray; //Change how fast scale and move happens using beats per minute.
        float currentBPM;   //Instead of using BPM, use number for how many frames per segment (image should appear on beat).

        OWAudioSource music;

        GameObject eyeRoot;
        static NewEndingHandler staticHandler;

        static AssetBundle bundle;  //Static so animation segments can access.
        static Transform imageCanvasTF;
        static Transform imageRoot;
        static GameObject imageObjToClone;
        
        Image background;
        static Image whiteFade;
        float fadeInAmount;
        const float fadeTime = 10f;
        float extraWait;

        readonly Vector2 centreOffset = new Vector2(960f, 540f);

        OWTriggerVolume _trigger;

        bool playedAlready;

        public static void Initialize(AssetBundle endingBundle)
        {
            if (staticHandler != null) return;
            Log.Print("Initializing Ending Cutscene...");
            bundle = endingBundle;

            //---------------- Create Root ----------------//
            var eyeRoot = GameObject.Instantiate(endingBundle.LoadAsset<GameObject>("Eye Root"));
            var eyeRootTF = eyeRoot.transform;

            //---------------- Get Canvas Objects ----------------//
            imageCanvasTF = eyeRootTF.Find("Canvas");
            imageCanvasTF.gameObject.SetActive(true);
            imageRoot = imageCanvasTF.Find("ImageRoot");
            imageObjToClone = imageRoot.Find("ImageObjToClone").gameObject;
            imageCanvasTF.SetParent(null, true); //Just in case.

            var background = imageCanvasTF.Find("Background").GetComponent<Image>();
            whiteFade = imageCanvasTF.Find("WhiteFade").GetComponent<Image>();

            //---------------- Set Up Root ----------------//
            var parent = GameObject.Find("EyeOfTheUniverse_Body/Sector_EyeOfTheUniverse");
            eyeRootTF.parent = parent.transform;
            eyeRootTF.localPosition = Vector3.zero;
            eyeRootTF.localRotation = Quaternion.identity;

            var rootSector = parent.GetComponent<Sector>();
            rootSector.SetParentSector(eyeRootTF.GetComponentInChildren<Sector>());

            //---------------- Set Up Trigger and Handler ----------------//
            var endTrigger = GameObject.Find("OutsiderEndingTrigger");
            var handler = endTrigger.AddComponent<NewEndingHandler>();
            handler.eyeRoot = eyeRoot;
            handler.enabled = false;
            staticHandler = handler;
            handler.background = background;

            //---------------- Set Up Music ----------------//
            handler.music = eyeRootTF.Find("MUSIC_ENDING").GetComponent<OWAudioSource>();
            var clip = endingBundle.LoadAsset<AudioClip>("ost_Duality.wav");
            handler.music.clip = clip;
            handler.music.loop = false;

            var eyeAudio = eyeRootTF.Find("Sector_OutsiderEye/OUTSIDER_EYE_AUDIO").GetComponent<OWAudioSource>();
            eyeAudio.clip = endingBundle.LoadAsset<AudioClip>("eyeAmbienceV2.mp3");
            eyeAudio.loop = false;
            eyeAudio.SetMaxVolume(0.7f);

            //---------------- Create Animation List ----------------//
            handler.CreateAnimationList();

            //---------------- Hide UI ----------------//
            imageCanvasTF.gameObject.SetActive(false);
        }
        void CreateAnimationList()
        {
            Log.Print("Creating Animation List...");

            string eyeRed = "EyeRed";
            string eyeAbove = "EyeAbove";

            string eyeDestruction = "EyeDestruction";
            string eyeBigBang = "EyeBigBang";
            string bigBang = "BigBangHearthian";
            string bigBangReaction = $"{bigBang}Reaction";
            string newUniverse = "NewUniverse";

            string vesselLight = "Vessel_NewUniverse_7";
            string vesselShadow = "VesselShadow";
            string qm = "QuantumMoon";

            string vesselFall = "QM_VesselFall";
            string vesselLand = "QM_VesselLanded";
            string vesselWarp = "QM_TimeWarp";

            string nuPlanet = "NewUniverseScene2_Planet";
            string nu2Planets = "NewUniverseScene3_Planets";

            string solVessel1 = "SolanumVessel_Scene1";
            string solVessel2 = "SolanumVessel_Scene2";
            string solVesselBack = "SolanumVessel_Back";
            string solVesselReact = "SolanumVessel_Reaction";

            string vesselNU = "Vessel_NewUniverse";

            string final = "Vessel_Final";

            animationArray = new AnimationSegment[]
            {
                new AnimationSegment(new Vector3Int(0, 0, 0), 0, $"{eyeAbove}_0", 6f, 1f, 8),
                new AnimationSegment(new Vector3Int(0, 0, 0), 1),
                new AnimationSegment(new Vector3Int(0, 0, 0), 2),   //Layers have to start off empty as well.

                //---------------- Eye Charges Up ----------------//
                new AnimationSegment(new Vector3Int(0, 3, 23), 0, $"{eyeAbove}_1"),
                new AnimationSegment(new Vector3Int(0, 3, 23), 1, $"{eyeRed}_1"),
                new AnimationSegment(new Vector3Int(0, 4, 22), 0, $"{eyeAbove}_2"),
                new AnimationSegment(new Vector3Int(0, 4, 22), 1, $"{eyeRed}_2"),
                new AnimationSegment(new Vector3Int(0, 5, 21), 0, $"{eyeAbove}_3"),
                new AnimationSegment(new Vector3Int(0, 5, 21), 1, $"{eyeRed}_3"),
                new AnimationSegment(new Vector3Int(0, 6, 20), 0, $"{eyeAbove}_4"),
                new AnimationSegment(new Vector3Int(0, 6, 20), 1, $"{eyeRed}_4"),
                new AnimationSegment(new Vector3Int(0, 7, 19), 0, $"{eyeAbove}_5"),
                new AnimationSegment(new Vector3Int(0, 7, 19), 1, $"{eyeRed}_5"),
                new AnimationSegment(new Vector3Int(0, 8, 18), 0, $"{eyeAbove}_6"),
                new AnimationSegment(new Vector3Int(0, 8, 18), 1, $"{eyeRed}_6"),
                new AnimationSegment(new Vector3Int(0, 9, 17), 0, $"{eyeAbove}_7"),
                new AnimationSegment(new Vector3Int(0, 9, 17), 1, $"{eyeRed}_7"),
                new AnimationSegment(new Vector3Int(0, 10, 16), 0, $"{eyeAbove}_8"),
                new AnimationSegment(new Vector3Int(0, 10, 16), 1, $"{eyeRed}_8"),

                new AnimationSegment(new Vector3Int(0, 11, 11), 1), //Empty layer one.

                //---------------- Eye Destroys Solar System ----------------//
                new AnimationSegment(new Vector3Int(0, 11, 11), 0, $"{eyeDestruction}_SS_0", 0.8f, 1f, 4 * 2),
                new AnimationSegment(new Vector3Int(0, 14, 05), 0, $"{eyeDestruction}_SS_1"),
                new AnimationSegment(new Vector3Int(0, 14, 27), 0, $"{eyeDestruction}_SS_2"),
                new AnimationSegment(new Vector3Int(0, 15, 19), 0, $"{eyeDestruction}_SS_3"),
                new AnimationSegment(new Vector3Int(0, 16, 11), 0, $"{eyeDestruction}_SS_4"),
                new AnimationSegment(new Vector3Int(0, 17, 03), 0, $"{eyeDestruction}_SS_5"),
                new AnimationSegment(new Vector3Int(0, 17, 21), 0, $"{eyeDestruction}_SS_6"),
                new AnimationSegment(new Vector3Int(0, 18, 09), 0, $"{eyeDestruction}_SS_7"),

                //---------------- Eye Destroys Ringworld ----------------//
                new AnimationSegment(new Vector3Int(0, 18, 27), 0, $"{eyeDestruction}_RW_0"),
                new AnimationSegment(new Vector3Int(0, 19, 16), 0, $"{eyeDestruction}_RW_1"),
                new AnimationSegment(new Vector3Int(0, 20, 05), 0, $"{eyeDestruction}_RW_2"),
                new AnimationSegment(new Vector3Int(0, 20, 24), 0, $"{eyeDestruction}_RW_3"),
                new AnimationSegment(new Vector3Int(0, 21, 13), 0, $"{eyeDestruction}_RW_4"),
                new AnimationSegment(new Vector3Int(0, 21, 27), 0, $"{eyeDestruction}_RW_5"),
                new AnimationSegment(new Vector3Int(0, 22, 11), 0, $"{eyeDestruction}_RW_6"),
                new AnimationSegment(new Vector3Int(0, 22, 25), 0, $"{eyeDestruction}_RW_7"),
                new AnimationSegment(new Vector3Int(0, 23, 09), 0, $"{eyeDestruction}_RW_8"),
                new AnimationSegment(new Vector3Int(0, 23, 23), 0, $"{eyeDestruction}_RW_9"),

                //---------------- Eye Destroys Other Nomai ----------------//
                new AnimationSegment(new Vector3Int(0, 24, 07), 0, $"{eyeDestruction}_NOM_0", 1.1f, 1f, 4 * 8),
                new AnimationSegment(new Vector3Int(0, 26, 28), 0, $"{eyeDestruction}_NOM_1"),
                new AnimationSegment(new Vector3Int(0, 27, 11), 0, $"{eyeDestruction}_NOM_2"),
                new AnimationSegment(new Vector3Int(0, 27, 24), 0, $"{eyeDestruction}_NOM_3"),
                new AnimationSegment(new Vector3Int(0, 28, 07), 0, $"{eyeDestruction}_NOM_4"),
                new AnimationSegment(new Vector3Int(0, 28, 20), 0, $"{eyeDestruction}_NOM_5"),
                new AnimationSegment(new Vector3Int(0, 29, 03), 0, $"{eyeDestruction}_NOM_6"),
                new AnimationSegment(new Vector3Int(0, 29, 16), 0, $"{eyeDestruction}_NOM_7"),

                //---------------- Eye Big Bang ----------------//
                new AnimationSegment(new Vector3Int(0, 29, 29), 0, $"{eyeBigBang}_0"),
                new AnimationSegment(new Vector3Int(0, 30, 28), 0, $"{eyeBigBang}_1"),
                new AnimationSegment(new Vector3Int(0, 31, 27), 0, $"{eyeBigBang}_2"),
                new AnimationSegment(new Vector3Int(0, 32, 24), 0, $"{eyeBigBang}_3"),
                new AnimationSegment(new Vector3Int(0, 33, 21), 0, $"{eyeBigBang}_4"),
                new AnimationSegment(new Vector3Int(0, 34, 20), 0, $"{eyeBigBang}_5"),

                //---------------- Big Bang Reaction ----------------//
                new AnimationSegment(new Vector3Int(0, 35, 18), 0, bigBang, new Vector2(960f, 241f), new Vector2(960f, 548f), 2f, 2f, 12 * 8),
                new AnimationSegment(new Vector3Int(0, 41, 07), 0, $"{bigBangReaction}_0", 1f, 1.3f, 16 * 2),
                new AnimationSegment(new Vector3Int(0, 43, 03), 0, $"{bigBangReaction}_1", 1f, 1.1f, 16 * 2),
                new AnimationSegment(new Vector3Int(0, 44, 26), 0, $"{bigBangReaction}_2", 1f, 1.02f),
                new AnimationSegment(new Vector3Int(0, 45, 28), 0, $"{bigBangReaction}_3", 1.02f, 1.04f),

                new AnimationSegment(new Vector3Int(0, 46, 25), 0, $"{bigBangReaction}_4", 1.04f, 1.06f),
                new AnimationSegment(new Vector3Int(0, 47, 22), 0, $"{bigBangReaction}_5", 1.06f, 1.08f),

                //---------------- New Universe ----------------//
                new AnimationSegment(new Vector3Int(0, 48, 21), 0, $"{newUniverse}_BG", new Vector2(1053f, 590f), new Vector2(870f, 497f), 1.1f, 1.1f, 32 * 2),

                new AnimationSegment(new Vector3Int(0, 48, 21), 1, $"{nuPlanet}_N3"),
                new AnimationSegment(new Vector3Int(0, 50, 15), 1, $"{nuPlanet}_N2"),
                new AnimationSegment(new Vector3Int(0, 51, 11), 1, $"{nuPlanet}_N1"),
                new AnimationSegment(new Vector3Int(0, 52, 07), 1, $"{nuPlanet}_0"),
                new AnimationSegment(new Vector3Int(0, 53, 03), 1, $"{nuPlanet}_1"),
                new AnimationSegment(new Vector3Int(0, 53, 29), 1, $"{nuPlanet}_2"),
                new AnimationSegment(new Vector3Int(0, 54, 25), 1, $"{nuPlanet}_3"),
                new AnimationSegment(new Vector3Int(0, 55, 21), 1, $"{nuPlanet}_4"),
                new AnimationSegment(new Vector3Int(0, 56, 17), 1, $"{nuPlanet}_5"),
                new AnimationSegment(new Vector3Int(0, 57, 13), 1, $"{nuPlanet}_6"),
                new AnimationSegment(new Vector3Int(0, 58, 09), 1, $"{nuPlanet}_7"),
                new AnimationSegment(new Vector3Int(0, 59, 05), 1, $"{nuPlanet}_8"),
                new AnimationSegment(new Vector3Int(1, 00, 01), 1, $"{nuPlanet}_9"),
                new AnimationSegment(new Vector3Int(1, 00, 27), 1, $"{nuPlanet}_10"),
                new AnimationSegment(new Vector3Int(1, 01, 24), 1, $"{nuPlanet}_11"),

                new AnimationSegment(new Vector3Int(1, 02, 19), 0, $"{newUniverse}Scene2_BG", new Vector2(1047f, 492f), new Vector2(872f, 589f), 1.1f, 1.1f, 24 * 2),
                
                new AnimationSegment(new Vector3Int(1, 02, 19), 1, $"{nu2Planets}_0"),
                new AnimationSegment(new Vector3Int(1, 03, 16), 1, $"{nu2Planets}_1"),
                new AnimationSegment(new Vector3Int(1, 04, 12), 1, $"{nu2Planets}_2"),
                new AnimationSegment(new Vector3Int(1, 05, 08), 1, $"{nu2Planets}_3"),
                new AnimationSegment(new Vector3Int(1, 06, 04), 1, $"{nu2Planets}_4"),
                new AnimationSegment(new Vector3Int(1, 07, 00), 1, $"{nu2Planets}_5"),
                new AnimationSegment(new Vector3Int(1, 07, 26), 1, $"{nu2Planets}_6"),
                new AnimationSegment(new Vector3Int(1, 08, 22), 1, $"{nu2Planets}_7"),
                new AnimationSegment(new Vector3Int(1, 09, 18), 1, $"{nu2Planets}_8"),
                new AnimationSegment(new Vector3Int(1, 10, 14), 1, $"{nu2Planets}_9"),
                new AnimationSegment(new Vector3Int(1, 11, 10), 1, $"{nu2Planets}_10"),
                new AnimationSegment(new Vector3Int(1, 12, 06), 1, $"{nu2Planets}_11"),

                //---------------- Eye in New Universe ----------------//
                new AnimationSegment(new Vector3Int(1, 13, 04), 0, $"{newUniverse}Scene3_BG"),

                new AnimationSegment(new Vector3Int(1, 13, 04), 1, $"{eyeAbove}_1"),
                new AnimationSegment(new Vector3Int(1, 13, 17), 1, $"{eyeAbove}_2"),
                new AnimationSegment(new Vector3Int(1, 13, 29), 1, $"{eyeAbove}_3"),
                new AnimationSegment(new Vector3Int(1, 14, 11), 1, $"{eyeAbove}_4"),
                new AnimationSegment(new Vector3Int(1, 14, 25), 1, $"{eyeAbove}_5"),
                new AnimationSegment(new Vector3Int(1, 15, 07), 1, $"{eyeAbove}_6"),
                new AnimationSegment(new Vector3Int(1, 15, 20), 1, $"{eyeAbove}_7"),
                new AnimationSegment(new Vector3Int(1, 16, 03), 1, $"{eyeAbove}_8"),

                //---------------- Vessel at Eye in New Universe ----------------//
                new AnimationSegment(new Vector3Int(1, 16, 15), 0, $"{newUniverse}Scene4_BG", new Vector2(1050f, 540f), new Vector2(881f, 540f), 1.1f, 1.1f, 32 * 2),
                new AnimationSegment(new Vector3Int(1, 16, 15), 1, $"VesselEyeBack", new Vector2(938f, 540f), new Vector2(1025f, 540f), 32 * 2),   //Eye Side on
                new AnimationSegment(new Vector3Int(1, 16, 15), 2, vesselShadow, new Vector2(1044f, 540f), new Vector2(960f, 540f), 32 * 2),

                //---------------- Quantum Moon in New Universe ----------------//
                new AnimationSegment(new Vector3Int(1, 20, 01), 0, $"{newUniverse}Scene3_BG", new Vector2(1315f, 700f), new Vector2(1200f, 738f), 1.4f, 1.4f, 16 * 2),
                new AnimationSegment(new Vector3Int(1, 20, 01), 1, qm, 0.8f, 1f, 16),
                new AnimationSegment(new Vector3Int(1, 20, 01), 2),

                //---------------- Solanum ----------------//
                new AnimationSegment(new Vector3Int(1, 23, 12), 0, $"{qm}_Scene1", 1f, 1.1f, 16),
                new AnimationSegment(new Vector3Int(1, 23, 12), 1),

                new AnimationSegment(new Vector3Int(1, 26, 28), 0, $"{qm}_Scene2", 1f, 1.1f, 16),

                //---------------- Vessel falls to QM ----------------//
                new AnimationSegment(new Vector3Int(1, 30, 13), 0, $"{newUniverse}Scene3_BG", new Vector2(960f, 540f), new Vector2(878f, 582f), 1.4f, 1.4f, 16),
                new AnimationSegment(new Vector3Int(1, 30, 13), 1, vesselLight, new Vector2(1300f, 470f), new Vector2(1170f, 505f), 0.6f, 0.6f, 16),
                new AnimationSegment(new Vector3Int(1, 30, 13), 2, qm, new Vector2(627f, 783f), new Vector2(860f, 650f), 0.8f, 0.8f, 16),

                new AnimationSegment(new Vector3Int(1, 33, 29), 1),
                new AnimationSegment(new Vector3Int(1, 33, 29), 2),

                new AnimationSegment(new Vector3Int(1, 33, 29), 0, $"{vesselFall}_0"),
                new AnimationSegment(new Vector3Int(1, 34, 25), 0, $"{vesselFall}_1"),
                new AnimationSegment(new Vector3Int(1, 35, 21), 0, $"{vesselFall}_2"),
                new AnimationSegment(new Vector3Int(1, 36, 17), 0, $"{vesselFall}_3"),

                new AnimationSegment(new Vector3Int(1, 37, 13), 0, $"{vesselLand}_0"),
                new AnimationSegment(new Vector3Int(1, 38, 09), 0, $"{vesselLand}_1"),
                new AnimationSegment(new Vector3Int(1, 39, 05), 0, $"{vesselLand}_2"),
                new AnimationSegment(new Vector3Int(1, 40, 02), 0, $"{vesselLand}_3"),

                //---------------- Vessel Time Warp ----------------//
                new AnimationSegment(new Vector3Int(1, 40, 28), 0, $"{vesselWarp}_0"),
                new AnimationSegment(new Vector3Int(1, 41, 23), 0, $"{vesselWarp}_1"),
                new AnimationSegment(new Vector3Int(1, 42, 19), 0, $"{vesselWarp}_2"),
                new AnimationSegment(new Vector3Int(1, 43, 16), 0, $"{vesselWarp}_3"),
                new AnimationSegment(new Vector3Int(1, 44, 12), 0, $"{vesselWarp}_4"),
                new AnimationSegment(new Vector3Int(1, 45, 08), 0, $"{vesselWarp}_5"),
                new AnimationSegment(new Vector3Int(1, 46, 04), 0, $"{vesselWarp}_6"),
                new AnimationSegment(new Vector3Int(1, 46, 29), 0, $"{vesselWarp}_7"),

                //---------------- Solanum on Vessel ----------------//
                new AnimationSegment(new Vector3Int(1, 47, 27), 0, $"{solVessel1}_0"),
                new AnimationSegment(new Vector3Int(1, 49, 17), 0, $"{solVessel1}_1"),

                new AnimationSegment(new Vector3Int(1, 50, 14), 0, $"{solVessel2}_0"),
                new AnimationSegment(new Vector3Int(1, 51, 06), 0, $"{solVessel2}_1"),
                new AnimationSegment(new Vector3Int(1, 51, 15), 0, $"{solVessel2}_2"),
                new AnimationSegment(new Vector3Int(1, 51, 24), 0, $"{solVessel2}_3"),
                new AnimationSegment(new Vector3Int(1, 52, 03), 0, $"{solVessel2}_4"),
                new AnimationSegment(new Vector3Int(1, 52, 12), 0, $"{solVessel2}_5"),
                new AnimationSegment(new Vector3Int(1, 52, 21), 0, $"{solVessel2}_6"),
                new AnimationSegment(new Vector3Int(1, 52, 27), 0, $"{solVessel2}_7"),

                //---------------- Vessel in New Universe ----------------//
                new AnimationSegment(new Vector3Int(1, 53, 03), 0, $"{vesselNU}_BG", new Vector2(1000f, 548f), new Vector2(926f, 527f), 1.05f, 1.05f, 16 * 2),
                new AnimationSegment(new Vector3Int(1, 53, 03), 1, $"{vesselNU}_0"),
                new AnimationSegment(new Vector3Int(1, 53, 10), 1, $"{vesselNU}_1"),
                new AnimationSegment(new Vector3Int(1, 53, 17), 1, $"{vesselNU}_2"),
                new AnimationSegment(new Vector3Int(1, 53, 24), 1, $"{vesselNU}_3"),
                new AnimationSegment(new Vector3Int(1, 54, 01), 1, $"{vesselNU}_4"),
                new AnimationSegment(new Vector3Int(1, 54, 08), 1, $"{vesselNU}_5"),
                new AnimationSegment(new Vector3Int(1, 54, 15), 1, $"{vesselNU}_6"),
                new AnimationSegment(new Vector3Int(1, 54, 22), 1, $"{vesselNU}_7"),

                //---------------- Solanum Back ----------------//
                new AnimationSegment(new Vector3Int(2, 00, 02), 1),
                new AnimationSegment(new Vector3Int(2, 00, 02), 0, $"{solVesselBack}_K0_0"),
                new AnimationSegment(new Vector3Int(2, 01, 26), 0, $"{solVesselBack}_K0_1"),
                new AnimationSegment(new Vector3Int(2, 02, 09), 0, $"{solVesselBack}_K0_2"),
                new AnimationSegment(new Vector3Int(2, 02, 22), 0, $"{solVesselBack}_K1_0"),
                new AnimationSegment(new Vector3Int(2, 03, 05), 0, $"{solVesselBack}_K1_1"),
                new AnimationSegment(new Vector3Int(2, 03, 18), 0, $"{solVesselBack}_K1_2"),
                new AnimationSegment(new Vector3Int(2, 04, 01), 0, $"{solVesselBack}_K2_0"),
                new AnimationSegment(new Vector3Int(2, 06, 02), 0, $"{solVesselBack}_K2_1"),
                new AnimationSegment(new Vector3Int(2, 06, 10), 0, $"{solVesselBack}_K2_2"),
                new AnimationSegment(new Vector3Int(2, 06, 18), 0, $"{solVesselBack}_K2_3"),
                new AnimationSegment(new Vector3Int(2, 06, 26), 0, $"{solVesselBack}_K3_0"),

                //---------------- Solanum Front React ----------------//
                new AnimationSegment(new Vector3Int(2, 08, 23), 0, $"{solVesselReact}_0_0"),
                new AnimationSegment(new Vector3Int(2, 10, 12), 0, $"{solVesselReact}_0_1"),
                new AnimationSegment(new Vector3Int(2, 10, 20), 0, $"{solVesselReact}_0_2"),
                new AnimationSegment(new Vector3Int(2, 10, 28), 0, $"{solVesselReact}_0_3"),
                new AnimationSegment(new Vector3Int(2, 11, 10), 0, $"{solVesselReact}_1"),
                new AnimationSegment(new Vector3Int(2, 11, 16), 0, $"{solVesselReact}_2"),
                new AnimationSegment(new Vector3Int(2, 11, 22), 0, $"{solVesselReact}_3"),
                new AnimationSegment(new Vector3Int(2, 12, 04), 0, $"{solVesselReact}_4"),
                new AnimationSegment(new Vector3Int(2, 12, 10), 0, $"{solVesselReact}_5"),

                //---------------- Final ----------------//
                new AnimationSegment(new Vector3Int(2, 15, 22), 0, $"{final}_7"),
                new AnimationSegment(new Vector3Int(2, 16, 07), 0, $"{final}_6"),
                new AnimationSegment(new Vector3Int(2, 16, 22), 0, $"{final}_5"),
                new AnimationSegment(new Vector3Int(2, 17, 07), 0, $"{final}_4"),
                new AnimationSegment(new Vector3Int(2, 17, 22), 0, $"{final}_3"),
                new AnimationSegment(new Vector3Int(2, 18, 07), 0, $"{final}_2"),
                new AnimationSegment(new Vector3Int(2, 18, 22), 0, $"{final}_1"),
                new AnimationSegment(new Vector3Int(2, 19, 07), 0, $"{final}_0"),

                new AnimationSegment(new Vector3Int(2, 27, 05), 0),
            };
        }
        public static void Static_PlayAnimation()
        {
            staticHandler.PlayAnimation();
        }
        void PlayAnimation()
        {
            Log.Print("Playing Animation...");
            Locator.GetShipLogManager().RevealFact("OUTSIDER_EYE_OTU_X1", true, false);

            //Prevent Pausing
            //OWTime.Pause(OWTime.PauseType.Loading);
            OuterWildsHandling.OWPatches.PreventPausing = true;

            //---------------- Enable UI ----------------//
            imageCanvasTF.gameObject.SetActive(true);
            whiteFade.gameObject.SetActive(false);

            background.color = Color.clear;
            fadeInAmount = 0f;

            //---------------- Start Playing ----------------//
            enabled = true;
            playedAlready = false;
        }
        void Awake()
        {
            _trigger = gameObject.GetRequiredComponent<OWTriggerVolume>();
            _trigger.OnEntry += OnEntry;
        }
        void OnDestroy() => _trigger.OnEntry -= OnEntry;
        void OnEntry(GameObject hitObj) {
            if (hitObj.CompareTag(OWConstants.PlayerDetector)) PlayAnimation();
        }
        void OnFadedToBlack()
        {
            OWInput.ChangeInputMode(InputMode.None);    //Stop player from moving.
            Locator.GetToolModeSwapper().UnequipTool();
            GUIMode.SetRenderMode(GUIMode.RenderMode.Hidden);
        }
        void GoToCredits()
        {
            music.Pause();
            //if (!ModMain.isDevelopmentVersion)
                PlayerData.SaveEyeCompletion(); //Return to normal game on reload.

            GUIMode.SetRenderMode(GUIMode.RenderMode.FPS);
            LoadManager.LoadScene(OWScene.Credits_Fast, LoadManager.FadeType.ToBlack, 1f, true);
            enabled = false;

            OuterWildsHandling.OWPatches.PreventPausing = false;
        }
        void Update()
        {
            float time = music.time;

            if (time < 40f)
            {
                if (playedAlready) //Music still looping for some reason. Just in case, so cutscene doesn't play multiple times.
                {
                    GoToCredits();
                    return;
                }
            }

            if (fadeInAmount < fadeTime)
            {
                fadeInAmount += Time.deltaTime;

                float t = fadeInAmount / fadeTime;
                background.color = Color.Lerp(Color.clear, Color.black, t);

                extraWait = 0f;

                if (fadeInAmount >= fadeTime) OnFadedToBlack();

                return;
            }
            if (extraWait < 4f)
            {
                extraWait += Time.deltaTime;
                return;
            }
            if (!music.isPlaying)
            {
                //music.pitch = 0.95f;
                music.Play();
            }

            if (Log.ErrorIf(animationArray == null, "Animation array is null")) return;
            if (Log.ErrorIf(animationArray.Length == 0, "Animation array is null or empty.")) return;

            int layers = 3;

            //---------------- Hide previous ----------------//
            for (int i = 0; i < animationArray.Length; i++)
            {
                if (animationArray[i].imageObj != null) animationArray[i].imageObj.SetActive(false);
            }

            //---------------- Find relevant segments ----------------//
            for (int layer = 0; layer < layers; layer++) //For each layer.
            {
                AnimationSegment last = null;
                AnimationSegment next = null;

                for (int i = 0; i < animationArray.Length; i++) //Find last and next segments.
                {
                    AnimationSegment segment = animationArray[i];
                    if (segment.layer != layer) continue;

                    if (segment.startTime >= time) //Assumed to be in order (per layer).
                    {
                        next = segment;
                        last = segment;
                        i--;

                        for (; i >= 0; i--) //Find previous in layer.
                        {
                            segment = animationArray[i];
                            if (segment.layer != layer) continue;

                            last = segment;
                            break;
                        }
                        break;
                    }
                }

                if (last == null) continue;
                if (last.imageObj == null) continue;

                DrawFrame(time, layer, last, next);
            }

            WhiteFade(time);

            //---------------- Cutscene Finishing ----------------//
            if (time > 50f)
            {
                playedAlready = true; //Just in case loops.
            }
            if (time > 147.5f) //2m 27.5s
            {
                GoToCredits();
            }
        }
        void WhiteFade(float time)
        {
            Color c0 = new Color(1f, 1f, 1f, 0f);
            Color c1 = new Color(1f, 1f, 1f, 1f);

            void SetActiveIfNot(bool active)
            {
                if (whiteFade.gameObject.activeSelf != active) whiteFade.gameObject.SetActive(active);
            }

            if (time > 53.1f) SetActiveIfNot(false); //After
            else if (time > 49.45f) //Fade out
            {
                float t = Mathf.InverseLerp(49.45f, 53.1f, time);
                whiteFade.color = Color.Lerp(c1, c0, t);
                
                SetActiveIfNot(true);
            }
            else if (time > 48.7f)
            {
                whiteFade.color = c1;
                SetActiveIfNot(true); //Hold
            }
            else if (time > 48.5f) //Fade in - Causes screen to go black????
            {
                //float t = Mathf.InverseLerp(48.5f, 48.7f, time);
                //whiteFade.color = Color.Lerp(c0, c1, t * t);
                //SetActiveIfNot(true);
            }
        }

        void DrawFrame(float time, int layer, AnimationSegment last, AnimationSegment next)
        {
            last.imageObj.SetActive(true);

            //---------------- Get T ----------------//
            //float segmentTime = time - last.startTime;
            float t = 0f;
            if (last != next)
            {
                t = Mathf.InverseLerp(last.startTime, next.startTime, time);
                t = Mathf.FloorToInt(t * last.frameCount) / last.frameCount;
            }

            //---------------- Move/Zoom ----------------//
            Vector2 posL = Vector2.Lerp(last.startPos, last.endPos, t);
            posL -= centreOffset;
            posL.y = -posL.y; //Invert y.

            Vector3 pos = new Vector3(posL.x, posL.y, -layer * 0.1f);
            float scale = Mathf.Lerp(last.startZoom, last.endZoom, t);

            var rtf = last.rectTF;
            rtf.anchoredPosition3D = pos;
            rtf.localScale = new Vector3(scale, scale, 1f);
        }

        public sealed class AnimationSegment
        {
            public GameObject imageObj;
            public RectTransform rectTF;
            public float startTime;
            public int layer = 0;

            public Vector2 startPos = new Vector2(960f, 540f);
            public Vector2 endPos = new Vector2(960f, 540f);

            public float startZoom = 1f;
            public float endZoom = 1f;

            public float frameCount = 4f;

            Sprite LoadIcon(string path)
            {
                var t = bundle.LoadAsset<Sprite>(path);
                if (t == null) Log.Error($"Failed to load: {path}");
                return t;
            }
            void CalculateStartTime(Vector3Int startTimeCode)
            {
                startTime = startTimeCode.x * 60f;  //Minute
                startTime += startTimeCode.y;       //Second
                startTime += startTimeCode.z / 30f; //Frame (out of 30)
            }
            public AnimationSegment(Vector3Int startTimeCode, int layer) //If image name is empty, remove image.
            {
                this.layer = layer;
                CalculateStartTime(startTimeCode);
            }
            public AnimationSegment(Vector3Int startTimeCode, int layer, string imageName)
            {
                if (!string.IsNullOrEmpty(imageName))
                {
                    var image = LoadIcon($"{imageName}.png");

                    imageObj = GameObject.Instantiate(imageObjToClone); //Change to clone object in prefab.
                    imageObj.name = $"{startTimeCode}_{imageName}";

                    rectTF = imageObj.GetComponent<RectTransform>();
                    rectTF.SetParent(imageRoot, true);
                    imageObj.GetComponent<Image>().sprite = image;
                    //imageObj.SetActive(false); //Should already be inactive.
                }

                this.layer = layer;
                CalculateStartTime(startTimeCode);
            }
            public AnimationSegment(Vector3Int startTimeCode, int layer, string imageName, float startZoom, float endZoom, int frameCount = 4)
                : this(startTimeCode, layer, imageName)
            {
                this.startZoom = startZoom;
                this.endZoom = endZoom;
                this.frameCount = frameCount;
            }
            public AnimationSegment(Vector3Int startTimeCode, int layer, string imageName, Vector2 startPos, Vector2 endPos, int frameCount = 4)
                : this(startTimeCode, layer, imageName)
            {
                this.startPos = startPos;
                this.endPos = endPos;
                this.frameCount = frameCount;
            }
            public AnimationSegment(Vector3Int startTimeCode, int layer, string imageName, Vector2 startPos, Vector2 endPos,
                float startZoom, float endZoom, int frameCount = 4) : this(startTimeCode, layer, imageName)
            {
                this.startPos = startPos;
                this.endPos = endPos;
                this.startZoom = startZoom;
                this.endZoom = endZoom;
                this.frameCount = frameCount;
            }
        }
    }
}