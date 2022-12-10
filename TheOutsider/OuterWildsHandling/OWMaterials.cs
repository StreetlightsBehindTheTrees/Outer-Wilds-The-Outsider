using OWML.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheOutsider.OuterWildsHandling
{
    public sealed class OWMaterials
    {
        //readonly Material BrambleBranch, BrambleThorn, IceFront, IceBack;

        readonly Material NomaiSandstone;
        //readonly Material NomaiSandstoneCracked; //, NomaiCopper, NomaiSandstoneZigZag;
        readonly Material NomaiSolidGlassOpaque, NomaiSolidGlassTransparent;
        readonly Material NomaiText;
        readonly Material GhostMatterCrystal;
        readonly Material GravityCrystal;

        readonly Material MatrixMat, MatrixMatAlt;
        readonly Material StrangerWoodDark, StrangerWoodLight;
        readonly Material DreamLanternFlame;

        //readonly Material HearthianMetal, HearthianPlanks, HearthianCloth;
        readonly Material Atmosphere; // AtmosphereFog;

        readonly Material LightBeam;

        readonly Material FriendConversationSymbols;
        readonly Material FriendConversationSymbolsAlt;

        //Wood old metallic: 0.18
        public OWMaterials(AssetBundle bundle)
        {
            var mats = Resources.FindObjectsOfTypeAll<Material>();

            Material FindMat(string name)
            {
                var mat = mats.First(x => x.name == name);
                if (mat == null) Log.Error($"Material not found: {name}");
                return mat;
            }
            Texture LoadTex(string path)
            {
                var t = bundle.LoadAsset<Texture>(path);
                if (t == null) Log.Error($"Texture {path} failed to load.");
                return t;
            }

            //---------------- Find OW Materials ----------------//
            NomaiSandstone = FindMat("Structure_NOM_SandStone_mat");
            //NomaiSandstoneCracked = FindMat("Structure_NOM_SandStone_Cracked_mat");
            NomaiSolidGlassTransparent = FindMat("Structure_NOM_Glass_Transparent_mat");
            NomaiSolidGlassOpaque = FindMat("Structure_NOM_Glass_Opaque_mat");

            LightBeam = FindMat("Effects_IP_RaftLight_mat");

            GhostMatterCrystal = FindMat("Terrain_GM_Crystal_mat");
            GravityCrystal = FindMat("Props_NOM_GravityCrystal_mat");
            NomaiText = FindMat("Effects_NOM_TextMain_mat");

            MatrixMat = FindMat("Terrain_IP_DreamGrid_mat");
            MatrixMatAlt = FindMat("Terrain_IP_DreamGridCeiling_mat");
            //Terrain_IP_DreamGridVP_mat, Terrain_IP_DreamGridDesaturated_mat, Terrain_IP_DreamGridBurned_mat
            //Terrain_IP_DreamGridChains_mat
            StrangerWoodDark = FindMat("Structure_DW_Mangrove_Wood_Dark_mat");
            StrangerWoodLight = FindMat("Structure_DW_Mangrove_Wood_Light_mat");

            DreamLanternFlame = FindMat("Effects_IP_LanternFlame_mat");

            //Structure_NOM_OrbTrack_mat
            //Structure_NOM_ProbeWindow_mat
            //Structure_NOM_RotatingDoor_mat
            //Structure_NOM_Silver_mat
            //Structure_NOM_SilverPorcelainGlow_mat

            //---------------- Create New Materials ----------------//
            Atmosphere = new Material(FindMat("Atmosphere_TH_Day_mat"));
            ModifyAtmosphereMat();
            ModifyFog();

            //var fogMat = fog.GetComponent<Renderer>().material;
            //fogMat.SetColor("_Color", new Color(0.15f, 0.4f, 0.3f, 0.1f));

            FriendConversationSymbols = new Material(FindMat("Decal_NOM_Symbols_02_mat"));
            var newIconsTexture = LoadTex("Decal_NOM_Symbols_03_NewMat.png");
            SetSymbols(FriendConversationSymbols, newIconsTexture);
            SetSymbolColors(FriendConversationSymbols, new Color(0.08f, 0.91f, 0.25f)); //, new Color(0.79f, 1f, 0.88f));
            FriendConversationSymbolsAlt = new Material(FriendConversationSymbols);
            SetSymbolColors(FriendConversationSymbolsAlt, new Color(0.91f, 0.25f, 0.08f));

            //---------------- Modify Existing Materials ----------------//
            newIconsTexture = LoadTex("Decal_NOM_Symbols_01_New.png");
            var stoneIconMat = FindMat("Decal_NOM_Symbols_01gold_mat");
            var pedestalMat = FindMat("Decal_NOM_Symbols_01_mat");
            SetSymbols(stoneIconMat, newIconsTexture);
            SetSymbols(pedestalMat, newIconsTexture);

            var posterTexture = LoadTex("PlayerShip_NewPosters.png");
            var posterMat = FindMat("Structure_HEA_PlayerShip_Posters_mat");
            posterMat.SetTexture("_MainTex", posterTexture);
        }
        void SetSymbols(Material mat, Texture newIconsTexture)
        {
            mat.SetTexture("_MainTex", newIconsTexture);
            mat.SetTexture("_EmissionMap", newIconsTexture);
        }
        void SetSymbolColors(Material mat, Color color)
        {
            mat.SetColor("_Color", color);
            mat.SetColor("_EmissionColor", color);
        }

        void ModifyAtmosphereMat()
        {
            Atmosphere.SetFloat("_InnerRadius", 550f);
            Atmosphere.SetFloat("_OuterRadius", 1000f);
            Atmosphere.SetFloat("_RimPower", 8f);
            Atmosphere.SetFloat("_InvFade", 0.2f);

            float t = 0.5f;

            Atmosphere.SetColor("_AtmosFar", new Color(0.2f, 0.25f, 0.25f, 0.7f) * t);
            Atmosphere.SetColor("_AtmosNear", new Color(0.3f, 0.4f, 0.25f, 0.7f) * t);

            Atmosphere.SetColor("_RimColor", new Color(0.25f, 0.5f, 0.4f, 0f) * t);
            Atmosphere.SetColor("_SkyColor", new Color(0.2f, 0.37f, 0.3f, 0.7f) * t);

            Atmosphere.SetColor("_InvWavelength", new Color(0.2f, 1.3f, 0.45f, 0.7f) * t);
        }
        void ModifyFog()
        {
            var atmo = GameObject.Find("DarkBramble_Body/Atmosphere_DB").transform;
            var fogObj = atmo.Find("FogSphere_DB");
            var fog = fogObj.GetComponent<PlanetaryFogController>();
            fog.fogRadius = 1000f;
            fog.fogTint = new Color(0.13f, 0.16f, 0.145f, 0.3f);
            fog.fogExponent = 1.5f;
            fog._skyboxFactor = 0.1f;

            atmo.Find("FogBackdrop").gameObject.SetActive(true);    //Highlight around core.
        }

        public Material GetMatByName(string name)
        {
            //General
            if (name.Contains("Arc ")) return NomaiText; //Set Nomai Text mat
            if (name.Contains("GhostMatter")) return GhostMatterCrystal;

            //Specific.
            switch (name)
            {
                case "Props_NOM_GravityCrystal":
                    return GravityCrystal;

                case "LightBeam":
                    return LightBeam;
                case OutsiderSector.StarterHouse:
                    return NomaiSandstone;

                case "SolarPanels":
                case "GlassSphere":
                case "Glass_Proxy":
                case "NomaiShuttle_Glass":
                    return NomaiSolidGlassOpaque;
                case "GlassSphere-Inside":
                //case "StudyTower_Glass":  //Causes ocean to disappear on GD.
                    return NomaiSolidGlassTransparent;
                case "Atmosphere":
                    return Atmosphere;
                case "GreenFire":
                    return DreamLanternFlame;

                case "FriendConversationSymbol":
                    return FriendConversationSymbols;
                case "FriendConversationSymbolAlt":
                    return FriendConversationSymbolsAlt;
            }
            return null;
        }
        public void ReplaceWithStrangerWood(Renderer[] rndrs)
        {
            foreach (var r in rndrs)
            {
                Material[] newMats = new Material[r.sharedMaterials.Length];
                for (int m = 0; m < newMats.Length; m++)
                {
                    string name = r.sharedMaterials[m].name;

                    //if (name == OutsiderMatName.LightWood) newMats[m] = StrangerWoodLight;
                    if (name == OutsiderMatName.DarkWood) newMats[m] = StrangerWoodDark;
                    else newMats[m] = r.sharedMaterials[m];
                }
                r.sharedMaterials = newMats;
            }
        }
        public void ReplaceDreamMatrix(Renderer[] rndrs)
        {
            foreach (var r in rndrs)
            {
                r.gameObject.layer = 28;   //Dream simulation layer

                Material[] newMats = new Material[r.sharedMaterials.Length];
                for (int m = 0; m < newMats.Length; m++)
                {
                    string name = r.sharedMaterials[m].name;

                    if (name == OutsiderMatName.DarkWood) newMats[m] = MatrixMatAlt;
                    else newMats[m] = MatrixMat;
                }
                r.sharedMaterials = newMats;
            }
        }
    }
}