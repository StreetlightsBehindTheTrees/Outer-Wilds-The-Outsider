using HarmonyLib;
using System.Reflection;
using TheOutsider;

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
        }
    }
}