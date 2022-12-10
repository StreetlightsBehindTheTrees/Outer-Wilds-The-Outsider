using OWML.ModHelper;
using System.Collections.Generic;
using UnityEngine;
using TheOutsider.OuterWildsHandling;
using TheOutsider.OutsiderHandling;
using TheOutsider.DebugStuff;
using UnityEngine.SceneManagement;

namespace TheOutsider
{
    public static class Prefab
    {
        public const int Title = 0, Bramble = 1, Dream = 2, PowerStation = 3, BrambleDimension = 4, GiantsDeepColliders = 5;
    }
    public sealed class ModMain : ModBehaviour
    {
        AssetBundle darkBundle;
        AssetBundle endingBundle;

        GameObject[] prefabs;
        SolarSystemHandler solarSystem;
        TitleScreenHandler titleScreen;

        public static bool isDevelopmentVersion => false;
        public static DebugModShipLogMode debugModShipLog => DebugModShipLogMode.Off;
        public enum DebugModShipLogMode { Off, AutoCompleteAll, RemoveModLogs }
        public static bool IsLoaded { get; set; }

        bool setGamma = false;

        void Start()
        {
            //---------------- Start Up ----------------//
            Log.Initialize(ModHelper.Console);
            if (IsLoaded) {
                Log.Warning($"Error: Multiple Instances of The Outsider Detected.");
                return;
            }
            else Log.Success($"The Outsider Loaded!");
            IsLoaded = true;

            //---------------- Get all assets ----------------//
            darkBundle = ModHelper.Assets.LoadBundle("darkbundle");
            var extraMusicBundle = ModHelper.Assets.LoadBundle("extramusicbundle");
            prefabs = new GameObject[]
            {
                LoadPrefab("Title Root"),
                LoadPrefab("Bramble Root"),
                LoadPrefab("Dream Root"),
                LoadPrefab("Power Station Pivot"),
                LoadPrefab("Bramble Dimension Root"),
                LoadPrefab("Giants Deep Colliders Root")
                //LoadPrefab("Eye Root"),   //In Ending Bundle
            };

            solarSystem = new SolarSystemHandler(darkBundle, extraMusicBundle);
            titleScreen = new TitleScreenHandler();
            ModHelper.Events.Unity.OnUpdate += solarSystem.OnUpdate;
            ModHelper.Events.Unity.OnUpdate += titleScreen.OnUpdate;

            //---------------- On Load Scene ----------------//
            ModHelper.Events.Unity.FireOnNextUpdate(() => OnLoadScene(SceneManager.GetActiveScene()));
            LoadManager.OnCompleteSceneLoad += OnLoadScene;

            //---------------- Misc ----------------//
            OWPatches.SetUp(darkBundle, this);

            if (isDevelopmentVersion)
            {
                DebugControls debugControls = new DebugControls(ModHelper);
                ModHelper.Events.Unity.OnUpdate += debugControls.OnUpdate;
            }
        }
        void OnLoadScene(Scene scene) {
            if (scene.name == "TitleScreen") titleScreen.OnSceneLoad(prefabs[Prefab.Title]);
        }
        void OnLoadScene(OWScene oldScene, OWScene loadingScene)
        {
            if (!setGamma)
            {
                var graphics = PlayerData.GetGraphicSettings();
                graphics.gammaValue = 0.7f;
                graphics.ApplyAllGraphicSettings();
                setGamma = true;
            }

            if (loadingScene == OWScene.TitleScreen)
            {
                Log.Print("Reload Title.");
                OutsiderShipHandler.ResetInitialization();
                titleScreen.OnSceneLoad(prefabs[Prefab.Title]);
            }
            if (loadingScene == OWScene.SolarSystem) solarSystem.OnSceneLoad(prefabs);
            if (loadingScene == OWScene.EyeOfTheUniverse)
            {
                if (endingBundle == null) endingBundle = ModHelper.Assets.LoadBundle("endingbundle");
                NewEndingHandler.Initialize(endingBundle);
            }
        }

        GameObject LoadPrefab(string path)
        {
            path = $"{path}.prefab";    //Add file type.

            GameObject prefab = darkBundle.LoadAsset<GameObject>(path);

            if (prefab == null) Log.Error($"Prefab {path} is null.");
            else
            {
                HandleMaterials(prefab);
                prefab.SetActive(false);
            }
            return prefab;
        }
        void HandleMaterials(GameObject prefab)
        {
            var rndrs = prefab.GetComponentsInChildren<Renderer>();
            foreach (var rndr in rndrs)
            {
                foreach (var mat in rndr.sharedMaterials)
                {
                    if (mat == null) continue;

                    OutsiderMaterials.AddMaterial(mat);

                    //mat.shader = standardShader;
                    //mat.renderQueue = 2000;
                }
            }
        }
        //Shader standardShader = Shader.Find("Standard");
    }
}