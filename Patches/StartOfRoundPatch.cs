using System.IO;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using TerminalDesktopMod.Extentions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace TerminalDesktopMod
{
    [HarmonyPatch(typeof(StartOfRound))]
    public static partial class StartOfRoundPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void Awake(ref StartOfRound __instance)
        {
            foreach (var scrap in DesktopStorage.SpawnableScraps)
            {
                foreach (var level in __instance.levels)
                {
                    if (level.spawnableScrap.Count(x => x.spawnableItem == scrap) == 0)
                        level.spawnableScrap.Add(new SpawnableItemWithRarity()
                        {
                            spawnableItem = scrap,
                            rarity = 70
                        });
                }
                
                if (!__instance.allItemsList.itemsList.Contains(scrap))
                    __instance.allItemsList.itemsList.Add(scrap);
            }
        }
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void Start(ref StartOfRound __instance)
        {
            WalkieTalkie.allWalkieTalkies.Clear();
            FlashDriveProp.FlashLoadedEvent = new UnityEvent<FlashDriveProp>();
            UsbPort.UsbPortChangeEvent = new UnityEvent<UsbPort>();
            DesktopStorage.ClearTerminalNodeEvent();            

            if (!__instance.IsServer)
                return;
            string saveNum = GameNetworkManager.Instance.saveFileNum.ToString();
            string filePath = Path.Combine(Application.persistentDataPath, $"TD_{saveNum}.json");
            DesktopStorage.TerminalDesktopSaveModel = new TerminalDesktopSaveModel();
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var load = JsonConvert.DeserializeObject<TerminalDesktopSaveModel>(json);
                DesktopStorage.TerminalDesktopSaveModel = load;
            }
            
            var canvas = GameObject.Instantiate(DesktopStorage.DesktopPrefab);
            var networkObj = canvas.GetComponent<NetworkObject>();
            networkObj.Spawn();
            
            var usbPort = GameObject.Instantiate(DesktopStorage.UsbFlashPort);
            var usbPortObj = usbPort.GetComponent<NetworkObject>();
            usbPortObj.Spawn();
            usbPortObj.GetComponentInChildren<UsbPort>().LoadPortById(1);
        }
    }
}
