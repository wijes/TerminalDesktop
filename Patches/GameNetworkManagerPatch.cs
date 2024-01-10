using HarmonyLib;
using Unity.Netcode;

namespace TerminalDesktopMod
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    public static class GameNetworkManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void StartPatch()
        {
            NetworkManager.Singleton.AddNetworkPrefab(DesktopStorage.DesktopPrefab);
            NetworkManager.Singleton.AddNetworkPrefab(DesktopStorage.UsbFlashPort);

            foreach (var scrap in DesktopStorage.SpawnableScraps)
                NetworkManager.Singleton.AddNetworkPrefab(scrap.spawnPrefab);
        }
        [HarmonyPatch("SaveGame")]
        [HarmonyPrefix]
        private static void PrefixSaveGame(GameNetworkManager __instance)
        {
            if(!__instance.isHostingGame)
                return;
            DesktopStorage.TerminalDesktopSaveModel = new TerminalDesktopSaveModel();
        }
        [HarmonyPatch("SaveGame")]
        [HarmonyPostfix]
        private static void PostfixSaveGame(GameNetworkManager __instance)
        {
            if(!__instance.isHostingGame)
                return;
            TerminalDesktopManager.Instance.SaveDesktop();
        }
        [HarmonyPrefix]
        [HarmonyPatch("ResetSavedGameValues")]
        private static void ResetSavedGameValues(GameNetworkManager __instance)
        {
            TerminalDesktopManager.Instance.ResetDesktop();
        }
    }
}
