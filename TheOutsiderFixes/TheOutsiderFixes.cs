using HarmonyLib;
using OWML.ModHelper;
using System.Reflection;
using TheOutsider;
using UnityEngine;

namespace TheOutsiderFixes
{
    public static class TheOutsiderFixes
    {
        public static void Initialize()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            BugFixPatches.Initialize();
            NewHorizonsCompat.Initialize();

            Log.Print($"{nameof(TheOutsiderFixes)} is loaded!");

            LoadManager.OnCompleteSceneLoad += (OWScene _, OWScene currentScene) =>
            {
                if (currentScene == OWScene.SolarSystem)
                {
                    ModMain.Instance.ModHelper.Events.Unity.FireInNUpdates(ApplySolarSystemFixes, 60);
                }
            };
        }

        public static void ApplySolarSystemFixes()
        {
            var brambleRoot = GameObject.Find("DB_ExitOnlyDimension_Body/Sector_ExitOnlyDimension/Outsider Bramble Dimension Root");
            if (brambleRoot == null)
            {
                Log.Error("Couldn't find Bramble root");
            }
            else
            {
                brambleRoot.AddComponent<NHBrambleSectorController>().SetSector(brambleRoot.GetComponentInParent<Sector>());
            }
        }
    }
}