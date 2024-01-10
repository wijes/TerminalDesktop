using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using TerminalApi;
using Unity.Netcode;
using static TerminalApi.TerminalApi;
namespace TerminalDesktopMod
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("atomic.terminalapi")]
    public class Main : BaseUnityPlugin
    {
        public const string ModGUID = "wijes.desktop.terminal";
        public const string ModName = "Terminal Desktop";
        public const string ModVersion = "0.5.0";
        internal static ManualLogSource Log;
        private readonly Harmony harmony = new Harmony(ModGUID);

        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"Plugin Terminal Desktop is loading! v {ModVersion}");
            var path = $"{Paths.PluginPath}/TerminalDesktop/terminaldesktop.bundle";
            AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
            LoadFlashItem(assetBundle);
            
            var desktopPrefab = assetBundle.LoadAsset<GameObject>("Desktop");
            GenerateRpc(desktopPrefab);
            DesktopStorage.DesktopPrefab = desktopPrefab;
            
            LoadUsbPortObject(assetBundle);
            LoadDesktopIcons(assetBundle);
            LoadDesktopWindows(assetBundle);
            harmony.PatchAll();

            GenerateTerminalCommand();
            Logger.LogInfo($"Plugin Terminal Desktop is loaded! v {ModVersion}");
        }

        private void LoadFlashItem(AssetBundle assetBundle)
        {
            var flashItem = assetBundle.LoadAsset<Item>("FlashDrive");
            DesktopStorage.FlashDriveItem = flashItem;
            Renderer rend = flashItem.spawnPrefab.GetComponent<Renderer> ();
            var texture = assetBundle.LoadAsset<Texture2D>("FlashDriveAlbedo");
                        
            rend.material = new Material(Shader.Find("HDRP/Lit"));
            rend.tag = "PhysicsProp";
            rend.material.mainTexture = texture;
            DesktopStorage.SpawnableScraps.Add(flashItem);
        }

        private void LoadUsbPortObject(AssetBundle assetBundle)
        {
            var usbPortObject = assetBundle.LoadAsset<GameObject>("UsbPort");
            var rends = usbPortObject.GetComponentsInChildren<Renderer>();
            foreach (var rend in rends)
            {
                var listMat = new List<Material>();
                for (int i = 0; i < rend.materials.Count(); i++)
                {
                    var color = rend.materials[i].color;
                    var texture = rend.materials[i].mainTexture;
                    var mat = new Material(Shader.Find("HDRP/Lit"));
                    mat.mainTexture = texture;
                    mat.color = color;
                    listMat.Add(mat);
                }

                rend.SetMaterials(listMat);
                rend.tag = "InteractTrigger";
                rend.gameObject.layer = LayerMask.NameToLayer("InteractableObject");
            }
            DesktopStorage.UsbFlashPort = usbPortObject;
            GenerateRpc(usbPortObject);
        }

        private void LoadDesktopIcons(AssetBundle assetBundle)
        {
            var icons = assetBundle.LoadAllAssets<GameObject>()
                .Where(x => x.GetComponent<DesktopIconBase>());
            foreach (var icon in icons)
            {
                var desktopIcon = icon.GetComponent<DesktopIconBase>();
                if (desktopIcon is not null && desktopIcon.name != "DesktopIcon")
                    DesktopStorage.AddIcon(desktopIcon);
            }
        }
        private void LoadDesktopWindows(AssetBundle assetBundle)
        {
            var windows = assetBundle.LoadAllAssets<GameObject>()
                .Where(x => x.GetComponent<DesktopWindowBase>());
            foreach (var window in windows)
            {
                var desktopWindow = window.GetComponent<DesktopWindowBase>();
                if (desktopWindow is not null)
                    DesktopStorage.AddWindow(desktopWindow);
            }
        }
        private void GenerateTerminalCommand()
        {
            TerminalNode buyNode =
                CreateTerminalNode($"Computer improved! \n Your new balance is [playerCredits].\n", true);
            buyNode.creatureName = "upgrade computer";
            TerminalKeyword buyVerbKeyword = CreateTerminalKeyword("upgrade", true);
            TerminalKeyword buyKeyword = CreateTerminalKeyword("computer");

            buyVerbKeyword = buyVerbKeyword.AddCompatibleNoun(buyKeyword, buyNode);
            buyKeyword.defaultVerb = buyVerbKeyword;

            AddTerminalKeyword(buyVerbKeyword);
            AddTerminalKeyword(buyKeyword);
            DesktopStorage.ComputerPowerUpgrade = buyNode;
        }
        /// <summary>
        /// InitializeRPCS methods are not called automatically, so let's call them manually
        /// </summary>
        /// <param name="gameObject"></param>
        private void GenerateRpc(GameObject gameObject)
        {
            var nets = gameObject.GetComponentsInChildren<NetworkBehaviour>();
            if (nets is null) return;
            foreach (var net in nets)
            {
                var methods = net.GetType().GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                foreach (var initMethod in methods)
                {
                    if (initMethod.DeclaringType is null)
                        continue;
                    if (initMethod.DeclaringType.Namespace is null)
                        continue;
                    if (!initMethod.DeclaringType.Namespace.Contains(nameof(TerminalDesktopMod)))
                        continue;
                    if (initMethod.GetParameters().Length == 0)
                        initMethod.Invoke(null, null);
                }
            }
        }
    }
}