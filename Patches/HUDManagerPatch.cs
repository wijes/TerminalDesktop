using HarmonyLib;
using Unity.Netcode;
namespace TerminalDesktopMod
{
    [HarmonyPatch(typeof(HUDManager))]
    public static partial class HUDManagerPatch
    {
        [HarmonyPatch("SetClock")]
        [HarmonyPostfix]
        private static void StartPatch(ref string __result)
        {
            ReferencesStorage.DayTime = __result;
        }
    }
}
