using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System;
using TheOutsider;
using TheOutsider.OuterWildsHandling;

namespace TheOutsiderFixes;

[HarmonyPatch]
public static class NewHorizonsCompat
{
    private static INewHorizons _newHorizonsAPI;

    public static void Initialize()
    {
        _newHorizonsAPI = ModMain.Instance.ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");

        if (IsNHInstalled)
        {
            // NH will handle the subtitle to ensure mod compat
            _newHorizonsAPI.AddSubtitle(ModMain.Instance, "assets/TheOutsiderLogo.png");
        }
    }

    // For NH compat only load the Outsider when in the base solar system
    public static bool ShouldLoadOutsider => !IsNHInstalled || _newHorizonsAPI.GetCurrentStarSystem() == "SolarSystem";

    public static bool IsNHInstalled => _newHorizonsAPI != null;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SolarSystemHandler), nameof(SolarSystemHandler.OnSceneLoad))]
    public static bool SkipOutsideSystem() => ShouldLoadOutsider;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TitleScreenHandler), nameof(TitleScreenHandler.OnUpdate))]
    public static bool Skip() => !IsNHInstalled;
}
