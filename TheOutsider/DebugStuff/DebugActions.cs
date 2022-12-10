using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheOutsider.DebugStuff
{
    public static class DebugActions
    {
        public static void PrintCollision()
        {
            Log.Print("------------------------------");
            for (int i = 0; i < 32; i++)
            {
                string name = LayerMask.LayerToName(i);
                var ignoreCollision = Physics.GetIgnoreLayerCollision(i, i);
                Log.Print($"{i}: {name} collides with {name}: {!ignoreCollision}");
            }
            Log.Print("------------------------------");
            for (int i = 0; i < 32; i++)
            {
                string name = LayerMask.LayerToName(i);
                for (int j = 0; j < 32; j++)
                {
                    string name2 = LayerMask.LayerToName(j);
                    var ignoreCollision = Physics.GetIgnoreLayerCollision(i, j);

                    Log.Print($"{name} collides with {name2}: {!ignoreCollision}");
                }
            }
        }

        public static void HideNewGameObjects(Scene scene)
        {
            Transform parent = new GameObject("Hide Parent").transform;
            var rootObjs = scene.GetRootGameObjects();
            foreach (var obj in rootObjs)
            {
                if (obj.name.Contains("New Game Object"))
                {
                    obj.transform.parent = parent;
                }
            }
        }
    }
}
