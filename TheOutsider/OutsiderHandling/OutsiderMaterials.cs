using System.Collections.Generic;
using UnityEngine;

namespace TheOutsider.OutsiderHandling
{
    /// <summary> <see cref="OuterWildsHandling.OWMaterials"/> | <see cref="ObjectModificationSS"/> </summary>
    public static class OutsiderMaterials
    {
        static HashSet<Material> materials = new HashSet<Material>();
        public static Material LightsOn { get; private set; }
        public static Material LightsOff { get; private set; }

        public static void AddMaterial(Material mat)
        {
            if (mat == null) return;

            materials.Add(mat);

            string name = mat.name;
            if (name == "GlowOrange") LightsOn = mat;
            if (name == "Black") LightsOff = mat;
        }
        public static void SetMaterialSounds()
        {
            SurfaceManager surfaceManager = Object.FindObjectOfType<SurfaceManager>();

            foreach (Material mat in materials)
            {
                SurfaceType type = GetSurfaceType(mat);
                surfaceManager._lookupTable.Add(mat, type);
            }
        }
        static SurfaceType GetSurfaceType(Material mat)
        {
            switch (mat.name)
            {
                case "Structure_HEA_DarkWoodBeams_mat 1":
                case "Structure_HEA_WoodBeams_mat":
                    return SurfaceType.Wood;

                case "TreeLeaves":
                    return SurfaceType.Grass;

                case "Structure_NOM_CopperOld_mat":
                    return SurfaceType.MetalNomai;

                default: return SurfaceType.None;
            }
        }
    }
}