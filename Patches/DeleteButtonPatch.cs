using System.IO;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
namespace TerminalDesktopMod
{
    [HarmonyPatch(typeof(DeleteFileButton))]
    public static partial class DeleteButtonPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("DeleteFile")]
        private static void DeleteFile(DeleteFileButton __instance)
        {
            string filePath = Path.Combine(Application.persistentDataPath, $"TD_{__instance.fileToDelete}.json");
            if(File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
