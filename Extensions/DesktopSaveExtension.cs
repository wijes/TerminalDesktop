using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace TerminalDesktopMod
{
    [Serializable]
    public class TerminalDesktopSaveModel
    {
        public int MaxEnergy { get; set; } = 1;
        public Dictionary<int, string> FlashSaves { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> UsbPortsSaves { get; set; } = new Dictionary<int, string>();
        public List<string> CustomDatas { get; set; } = new List<string>();
    }
    public static class DesktopSaveExtension
    {
        public static void SaveDesktop(this TerminalDesktopManager desktopManager)
        {
            var save = DesktopStorage.TerminalDesktopSaveModel;
            save.MaxEnergy = desktopManager.GetMaxEnergy();
            foreach (var port in desktopManager.UsbPorts)
                save.UsbPortsSaves.Add(port.PortId.Value, port.GetSaveString());
            
            string saveNum = GameNetworkManager.Instance.saveFileNum.ToString();
            string filePath = Path.Combine(Application.persistentDataPath, $"TD_{saveNum}.json");
            string json = JsonConvert.SerializeObject(save);
            File.WriteAllText(filePath, json);
        }
        public static void LoadDesktop(this TerminalDesktopManager desktopManager)
        {
            var load = DesktopStorage.TerminalDesktopSaveModel;
            desktopManager.SetMaxEnergy(load.MaxEnergy);
        }
        public static void ResetDesktop(this TerminalDesktopManager desktopManager)
        {
            var collapsed = new List<GameObject>(desktopManager.CollapsedWindows.Keys);
            foreach (var collapsedIcon in collapsed)
                GameObject.Destroy(collapsedIcon);
            desktopManager.CollapsedWindows.Clear();
            
            var windows = new List<DesktopWindowBase>(desktopManager.DesktopWindows);
            foreach (var window in windows)
                GameObject.Destroy(window.gameObject);
            desktopManager.DesktopWindows.Clear();
            
            if (!desktopManager.IsServer)
                return;
            DesktopStorage.TerminalDesktopSaveModel = new TerminalDesktopSaveModel();
            desktopManager.SetUseEnergy(0);
            desktopManager.SetMaxEnergy(1);
            desktopManager.SaveDesktop();
        }
    }
}