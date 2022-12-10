using OWML.Common;
using System.Collections.Generic;
using TheOutsider.OutsiderHandling;
using UnityEngine;

namespace TheOutsider.OuterWildsHandling
{
    public sealed class SolarSystemHandler
    {
        AssetBundle darkBundle;

        ObjectModificationSS objMod;
        OWObjects objects => objMod.Objects;

        GiantsDeepGravityEffects gdEffects;
        GameObject brambleRoot;

        public SolarSystemHandler(AssetBundle darkBundle, AssetBundle extraMusicBundle)
        {
            this.darkBundle = darkBundle;

            objMod = new ObjectModificationSS(darkBundle, extraMusicBundle);
        }
        public void OnSceneLoad(GameObject[] prefabs)
        {
            //---------------- DB Root Set Up ----------------//
            brambleRoot =       InstantiatePrefab(prefabs[Prefab.Bramble],          "Outsider Bramble Root");
            var dimensionRoot = InstantiatePrefab(prefabs[Prefab.BrambleDimension], "Outsider Bramble Dimension Root");
            var dreamRoot =     InstantiatePrefab(prefabs[Prefab.Dream],            "Outsider Dream Root");
            var powerStation =  InstantiatePrefab(prefabs[Prefab.PowerStation],     "Power Station Root");
            var giantsDeepColls = InstantiatePrefab(prefabs[Prefab.GiantsDeepColliders], "Giants Deep Coll");

            OWPatches.ResetOverrides();
            objMod.OnLoopStart();
            objMod.ModifyDreamRoot(dreamRoot);  //Modify before Set Up to not effect other duplicated objects.

            SetUp(brambleRoot,    objects.DarkBramble,                    objects.DarkBrambleSector);
            SetUp(dimensionRoot,  objects.BrambleDimensionArea.transform, objects.BrambleDimensionArea);
            SetUp(dreamRoot,      objects.EndlessCanyonSector.transform,  objects.EndlessCanyonSector);
            SetUp(powerStation,   objects.DarkBramble);
            objMod.SetUpGDColliders(giantsDeepColls);

            OutsiderMaterials.SetMaterialSounds();

            gdEffects = new GiantsDeepGravityEffects(brambleRoot, dimensionRoot, dreamRoot, objects, darkBundle);

            ResetStuff();

            //if (ModMain.isDevelopmentVersion) DebugStuff.DebugActions.HideNewGameObjects(brambleRoot.scene);
        }
        GameObject InstantiatePrefab(GameObject prefab, string name)
        {
            var root = GameObject.Instantiate(prefab);
            root.name = name;
            return root;
        }
        void SetUp(GameObject root, Transform parent, Sector parentSector = null)
        {
            objMod.SetLocation(root.transform, parent);

            //If Destroyed on start like Power Station, fix so modified correctly.
            var modifyRoot = root.TryGetComponent(out DestroyOnStart _) ? root.transform.GetChild(0).gameObject : root;

            root.SetActive(true);
            modifyRoot.SetActive(true);
            objMod.ModifyObjects(modifyRoot, parentSector);
        }
        
        void ResetStuff()
        {
            killedInDream = false;
        }

        bool killedInDream = false;
        public void OnUpdate()
        {
            if (brambleRoot == null) return;
            
            objMod.OnUpdate();
            gdEffects.OnUpdate();

            var dreamController = Locator.GetDreamWorldController();
            if (dreamController.IsInDream() && !PlayerState.IsResurrected()) //In dream and not dead outside.
            {
                var sunController = Locator.GetSunController();
                if (sunController.IsPointInsideSupernova(objects.GiantsDeep.position))  //GD as loop ends before DB?
                {
                    if (killedInDream) return;
                    killedInDream = true;
                    var exploder = objects.DreamExplosionController;
                    exploder.enabled = true;
                    exploder._explodeTime = Time.time;
                }
            }
        }
    }
}