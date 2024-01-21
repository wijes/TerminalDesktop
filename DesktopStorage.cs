using System.Collections.Generic;
using JetBrains.Annotations;
using TerminalDesktopMod.Extentions;
using UnityEngine;

namespace TerminalDesktopMod
{
    public static class DesktopStorage
    {
        public static TerminalDesktopSaveModel TerminalDesktopSaveModel { get; set; } = new TerminalDesktopSaveModel();
        public static GameObject UsbFlashPort { get; set; }
        public static GameObject DesktopPrefab { get; set; }

        public static Item FlashDriveItem { get; set; }
        public static TerminalNode ComputerPowerUpgrade { get; set; }
        public static event TerminalNodeChangedEventHandler TerminalNodeChangeEvent;
        
        public static List<Item> SpawnableScraps { get; private set; } = new List<Item>();
        
        public static List<DesktopIconBase> IconsPrefabs { get; private set; } = new List<DesktopIconBase>();
        public static Dictionary<string, DesktopWindowBase> WindowsPrefabs { get; private set; } =
            new Dictionary<string, DesktopWindowBase>();
        
        public static void AddIcon(DesktopIconBase desktopIconBase)
        {
            IconsPrefabs.Add(desktopIconBase);
            Main.Log.LogInfo($"Desktop added icon:{desktopIconBase.name}");
        }
        public static void AddWindow(DesktopWindowBase desktopWindowBase)
        {
            WindowsPrefabs.Add(desktopWindowBase.GetType().FullName, desktopWindowBase);
            Main.Log.LogInfo($"Desktop added window:{desktopWindowBase.name}");
        }
        [CanBeNull]
        public static DesktopWindowBase GetWindowByType(string desktopWindowType)
        {
            return WindowsPrefabs.TryGetValue(desktopWindowType, out var prefab) ? prefab : null;
        }
        public static void InvokeChangeTerminalNode(Terminal terminal, TerminalNode node)
        {
            TerminalNodeChangeEvent?.Invoke(terminal, node);
        }

        public static void ClearTerminalNodeEvent()
        {
            TerminalNodeChangeEvent = null;
        }
        public delegate void TerminalNodeChangedEventHandler(
            Terminal sender,
            TerminalNode newNode);
    }
}