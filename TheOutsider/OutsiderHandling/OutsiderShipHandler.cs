using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OWML.Common;
using UnityEngine.UI;

namespace TheOutsider.OutsiderHandling
{
    public sealed class OutsiderShipHandler
    {
        ShipLogLibrary logLibrarySO;
        TextAsset newDBAsset, PSAsset, newIPAsset;
        TextAsset qmAsset;

        Sprite dbUI, dbUIOutline, psUI, psUIOutline;

        static bool soInitialized = false;
        public static void ResetInitialization()
        {
            soInitialized = false;
        }

        public OutsiderShipHandler(AssetBundle bundle)
        {
            logLibrarySO = bundle.LoadAsset<ShipLogLibrary>("TheOutsiderShipLogLibrary.asset");
            newDBAsset = bundle.LoadAsset<TextAsset>("DarkBramble_TheOutsiderLog.bytes");
            newIPAsset = bundle.LoadAsset<TextAsset>("Stranger_TheOutsiderLog.bytes");
            PSAsset = bundle.LoadAsset<TextAsset>("PowerStation_TheOutsiderLog.bytes");
            qmAsset = bundle.LoadAsset<TextAsset>("QM_TheOutsiderLog.bytes");

            if (logLibrarySO != null) Log.Success("Loaded log library.");
            if (newDBAsset != null && newIPAsset != null && PSAsset != null && qmAsset != null) Log.Success("Loaded log xmls.");

            //---------------- Load Sprites ----------------//
            Sprite LoadIcon(string path)
            {
                var t = bundle.LoadAsset<Sprite>(path);
                if (t == null) Log.Error($"Sprite {path} failed to load.");
                return t;
            }
            dbUI = LoadIcon("UI_DarkBramble.png");
            dbUIOutline = LoadIcon("UI_DarkBramble_NewOutline.png");
            psUI = LoadIcon("UI_PowerStation.png");
            psUIOutline = LoadIcon("UI_PowerStation_Outline.png");
        }
        public void ShipLog_AwakePrefix(ShipLogManager manager)
        {
            //---------------- Ship Log Assets ----------------//
            var OWLogXML = manager._shipLogXmlAssets;
            TextAsset[] outsiderXML = new TextAsset[OWLogXML.Length + 4];

            for (int i = 0; i < OWLogXML.Length; i++) outsiderXML[i] = OWLogXML[i];
            outsiderXML[outsiderXML.Length - 4] = qmAsset;
            outsiderXML[outsiderXML.Length - 3] = newIPAsset;
            outsiderXML[outsiderXML.Length - 2] = newDBAsset;
            outsiderXML[outsiderXML.Length - 1] = PSAsset;

            manager._shipLogXmlAssets = outsiderXML;

            ModifyShipLogMap(); //Before return so still adds icons on each loop.

            if (soInitialized) return;  //Scriptable Object, so keeps info between loops.
            soInitialized = true;       //-> resets when return to title for some reason though

            //---------------- Entry Data ----------------//
            var OWLog = manager._shipLogLibrary.entryData;
            var outsiderLog = logLibrarySO.entryData;

            Vector2 offset = new Vector2(-250f, 0f);
            for (int i = 0; i < outsiderLog.Length; i++) outsiderLog[i].cardPosition += offset;

            EntryData[] entries = new EntryData[OWLog.Length + outsiderLog.Length];
            
            for (int i = 0; i < OWLog.Length; i++) entries[i] = OWLog[i];
            for (int i = 0; i < outsiderLog.Length; i++) entries[OWLog.Length + i] = outsiderLog[i];

            manager._shipLogLibrary.entryData = entries;
        }
        void ModifyShipLogMap()
        {
            var panRootGO = GameObject.Find("ShipLogCanvas/MapMode/ScaleRoot/PanRoot");
            if (panRootGO == null) return; //If at Eye.

            var panRoot = panRootGO.transform;
            var db = panRoot.Find("DarkBramble").transform;
            
            void SetSprites(Transform tf, Sprite sprite, Sprite outline)
            {
                tf.Find("Sprite").GetComponent<Image>().sprite = sprite;
                tf.Find("Outline").GetComponent<Image>().sprite = outline;
            }
            SetSprites(db, dbUI, dbUIOutline);

            var ps = GameObject.Instantiate(panRoot.Find("OrbitalProbeCannon"), panRoot);
            ps.localPosition += new Vector3(266f, 0f, 0f);
            SetSprites(ps, psUI, psUIOutline);

            //---------------- Add to Map Mode ----------------//
            var ao = ps.GetComponent<ShipLogAstroObject>();
            ao._id = "POWER_STATION";
            
            var mapMode = ps.GetComponentInParent<ShipLogMapMode>();
            var bottomRow = new ShipLogAstroObject[mapMode._bottomRow.Length + 1];
            for (int i = 0; i < mapMode._bottomRow.Length; i++) bottomRow[i] = mapMode._bottomRow[i];
            bottomRow[bottomRow.Length - 1] = ao;
            mapMode._bottomRow = bottomRow;
        }
    }
}