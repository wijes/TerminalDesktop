using HarmonyLib;
using Unity.Netcode;
namespace TerminalDesktopMod
{
    [HarmonyPatch(typeof(WalkieTalkie))]
    public static partial class WalkiePatch
    {
        [HarmonyPatch("OnDisable")]
        [HarmonyPrefix]
        public static bool OnDisable(ref WalkieTalkie __instance)
        {
            if (WalkieWindow.TerminalWalkieTalkie is null || WalkieWindow.TerminalWalkieTalkie.Equals(null))
                return true;
            if (WalkieWindow.TerminalWalkieTalkie == __instance) // skip destroy walkie terminal
                return false;
            return true;
        }
    }
}
