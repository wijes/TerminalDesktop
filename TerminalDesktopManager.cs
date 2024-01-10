using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerminalDesktopMod.Extentions;
using TerminalDesktopMod.Sync;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static TerminalApi.Events.Events;

namespace TerminalDesktopMod
{
    public class TerminalDesktopManager : NetworkBehaviour
    {
        public static TerminalDesktopManager Instance { get; set; }
        public List<UsbPort> UsbPorts = new List<UsbPort>();
        public List<DesktopIconBase> DesktopIcons { get; set; } = new List<DesktopIconBase>();
        public List<DesktopWindowBase> DesktopWindows { get; set; } = new List<DesktopWindowBase>();
        public Dictionary<GameObject,DesktopWindowBase> CollapsedWindows { get; set; } = new Dictionary<GameObject,DesktopWindowBase>();
        public UnityEvent<DesktopWindowBase> WindowAddedEvent { get; set; } = new UnityEvent<DesktopWindowBase>();
        public Canvas CanvasDesktop { get; private set; }
        public Camera EventUICamera { get; private set; }
        public bool IsUsingTerminal { get; private set; }
        private Terminal Terminal { get; set; }
        private NetworkVariable<int> UseEnergy { get; set; } = new NetworkVariable<int>();
        private NetworkVariable<int> MaxEnergy { get; set; } = new NetworkVariable<int>();
        private int BaseUpgradeCost { get; set; } = 75;
        [SerializeField] private Transform IconsParent;
        [SerializeField] private Transform WindowsParent;
        [SerializeField] private Transform CollapsedWindowParent;
        [SerializeField] private GameObject CollapsedWindowPrefab;
        [SerializeField] private TextMeshProUGUI TimeText;
        [SerializeField] private TextMeshProUGUI PowerText;
        private int WindowCounter { get; set; }
        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            Terminal = ReferencesStorage.Terminal;
            transform.parent = Terminal.terminalUIScreen.transform.parent;
            CanvasDesktop = GetComponentInChildren<Canvas>();

            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.Euler(0,0,0);
            transform.localPosition = Vector3.zero;
            
            TerminalBeganUsing += OnBeganUsing;
            TerminalExited += OnExitedUsing;
            MaxEnergy.OnValueChanged += OnMaxEnergyValueChanged;
            DesktopStorage.TerminalNodeChangeEvent += DesktopStorageOnTerminalNodeChangeEvent;
            DesktopStorage.ComputerPowerUpgrade.itemCost = BaseUpgradeCost * MaxEnergy.Value;
            foreach (var iconPrefab in DesktopStorage.IconsPrefabs)
                AddIcon(iconPrefab);
            if (IsServer)
                this.LoadDesktop();

            if (IsClient && !IsHost)
                StartCoroutine(WaitInitPlayer());
            
        }

        private void OnMaxEnergyValueChanged(int previousValue, int newValue)
        {
            DesktopStorage.ComputerPowerUpgrade.itemCost = BaseUpgradeCost * newValue;
        }

        private void DesktopStorageOnTerminalNodeChangeEvent(Terminal sender, TerminalNode newNode)
        {
            if (newNode != DesktopStorage.ComputerPowerUpgrade)
                return;
            if (ReferencesStorage.Terminal.groupCredits < newNode.itemCost)
            {
                newNode.displayText = "you could not afford these item";
                return;
            }
            newNode.displayText = $"Computer improved! \n Your new balance is [playerCredits].\n";
            ReferencesStorage.Terminal.ChangeCredits(-newNode.itemCost);
            SetMaxEnergy(MaxEnergy.Value + 1);
        }

        IEnumerator WaitInitPlayer()
        {
            while (GameNetworkManager.Instance.localPlayerController is null)
                yield return new WaitForSeconds(1);
            yield return new WaitForSeconds(1);
            GetSyncWindowsServerRpc();
        }
        void FixedUpdate()
        {
            if (!IsSpawned)
                return;
            TimeText.text = ReferencesStorage.DayTime is null? "??:??" : ReferencesStorage.DayTime;
            PowerText.text = $"{UseEnergy.Value} / {MaxEnergy.Value}";
            if (!IsUsingTerminal)
                return;
            EventUICamera.fieldOfView = GameNetworkManager.Instance.localPlayerController.gameplayCamera.fieldOfView;
        }
        public void AddIcon(DesktopIconBase iconBasePrefab)
        {
            var iconObj = Instantiate(iconBasePrefab.gameObject, IconsParent);
            var icon = iconObj.GetComponent<DesktopIconBase>();
            icon.ChangePosition(icon.DesktopNormalizedPosition);
            DesktopIcons.Add(icon);
        }

        public int GetUseEnergy()
        {
            return UseEnergy.Value;
        }
        public int GetMaxEnergy()
        {
            return MaxEnergy.Value;
        }
        public int GetFreeEnergy()
        {
            return MaxEnergy.Value - UseEnergy.Value;
        }
        public void ChangeUseEnergy(int count)
        {
            if (!IsServer)
                return;
            UseEnergy.Value += count;
        }
        public void SetUseEnergy(int count)
        {
            if (!IsServer)
                return;
            UseEnergy.Value = count;
        }
        public void SetMaxEnergy(int count)
        {
            SetMaxEnergyServerRpc(count);
        }
        [ServerRpc(RequireOwnership = false)]
        private void SetMaxEnergyServerRpc(int count)
        {
            MaxEnergy.Value = count;
        }
        /// <summary>
        /// Need for world space UI
        /// </summary>
        private void CreateEventCamera()
        {
            var player = GameNetworkManager.Instance.localPlayerController;
            var eventUICameraObj = Instantiate(player.gameplayCamera.gameObject,
                player.gameplayCamera.transform);
            eventUICameraObj.transform.localPosition = Vector3.zero;
            eventUICameraObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            Destroy(eventUICameraObj.GetComponent<AudioListener>());
            EventUICamera = eventUICameraObj.GetComponent<Camera>();
            EventUICamera.targetTexture = null;
            EventUICamera.depth = 10;
            EventUICamera.clearFlags = CameraClearFlags.Depth;
            eventUICameraObj.SetActive(false); // work event in disable state
            CanvasDesktop.worldCamera = EventUICamera;
            Terminal.playerScreenTexHighRes.width = EventUICamera.pixelWidth;
            Terminal.playerScreenTexHighRes.height = EventUICamera.pixelHeight;
        }
        private void OnBeganUsing(object sender, TerminalEventArgs e)
        {
            if (EventUICamera is null)
                CreateEventCamera();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            var player = GameNetworkManager.Instance.localPlayerController;
            player.isClimbingLadder = true; // free camera
            player.ladderCameraHorizontal = 0; // set camera horizontal to center monitor
            IsUsingTerminal = true;
        }

        private void OnExitedUsing(object sender, TerminalEventArgs e)
        {
            IsUsingTerminal = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GameNetworkManager.Instance.localPlayerController.isClimbingLadder = false;
        }
        public void CollapseWindow(DesktopWindowBase windowBase)
        {
            var collapsedWin = Instantiate(CollapsedWindowPrefab, CollapsedWindowParent);
            collapsedWin.GetComponent<Image>().sprite = windowBase.CollapseSprite;
            CollapsedWindows.Add(collapsedWin, windowBase);
            collapsedWin.SetActive(true);
        }
        public void ExpandWindow(GameObject collapsedIcon)
        {
            if (!CollapsedWindows.TryGetValue(collapsedIcon, out var windowBase))
                return;
            UpdateWindow(windowBase, new WindowSync()
            {
                ChangeCollapsed = true,
                IsCollapsed = false
            });
        }
        public void ExpandWindow(DesktopWindowBase window)
        {
            var collapsedIcon = CollapsedWindows
                .FirstOrDefault(x => x.Value == window).Key;
            if (collapsedIcon is null)
                return;
            CollapsedWindows.Remove(collapsedIcon);
            Destroy(collapsedIcon);
        }
        public DesktopWindowBase GetWindowByIndex(int windowIndex)
        {
            return DesktopWindows.FirstOrDefault(x => x.WindowIndex == windowIndex);
        }
        public void AddNotificationWindow(string notification)
        {
            var window = DesktopStorage.GetWindowByType(typeof(NotificationWindow).FullName);
            if (window is null)
            {
                Main.Log.LogError($"Not found notification window");
                return;
            }
            var windowObj = Instantiate(window.gameObject, WindowsParent);
            var win = windowObj.GetComponent<NotificationWindow>();
            win.WindowIndex = -1;
            win.SetText(notification);
        }
        public void AddWindow(DesktopWindowBase windowBasePrefab)
        {
            AddWindowServerRpc(windowBasePrefab.GetType().FullName);
        }
        [ServerRpc(RequireOwnership = false)]
        private void AddWindowServerRpc(string windowType)
        {
            WindowCounter++;
            AddWindowClientRpc(WindowCounter, windowType);
        }
        [ClientRpc]
        private void AddWindowClientRpc(int winIndex, string windowType, ClientRpcParams clientRpcParams = default)
        {
            var window = DesktopStorage.GetWindowByType(windowType);
            if (window is null)
            {
                Main.Log.LogError($"Not found {windowType} window");
                return;
            }
            var windowObj = Instantiate(window.gameObject, WindowsParent);
            var win = windowObj.GetComponent<DesktopWindowBase>();
            win.WindowIndex = winIndex;
            DesktopWindows.Add(win);
            WindowAddedEvent.Invoke(win);
        }
        public void CloseWindow(DesktopWindowBase windowBase)
        {
            CloseWindowServerRpc(windowBase.WindowIndex);
        }
        [ServerRpc(RequireOwnership = false)]
        private void CloseWindowServerRpc(int winIndex)
        {
            CloseWindowClientRpc(winIndex);
        }
        [ClientRpc]
        private void CloseWindowClientRpc(int windowIndex)
        {
            var window = GetWindowByIndex(windowIndex);
            if (window is null)
            {
                Main.Log.LogError($"Not found {windowIndex} window");
                return;
            }
            DesktopWindows.Remove(window);
            var collapsedIcon = CollapsedWindows
                .FirstOrDefault(x => x.Value == window).Key;
            if (collapsedIcon is not null)
            {
                CollapsedWindows.Remove(collapsedIcon);
                Destroy(collapsedIcon);
            }
            window.CloseWindow();
            Destroy(window.gameObject);
        }
        public void UpdateWindow(DesktopWindowBase window, WindowSync windowSync)
        {
            UpdateWindowServerRpc(window.WindowIndex, windowSync);
        }
        [ServerRpc(RequireOwnership = false)]
        private void UpdateWindowServerRpc(int indexWindow, WindowSync windowSync)
        {
            var window = GetWindowByIndex(indexWindow);
            if (window is null)
            {
                Main.Log.LogError($"Not found {indexWindow} window");
                return;
            }
            UpdateWindowClientRpc(indexWindow, windowSync);
        }
        [ClientRpc]
        private void UpdateWindowClientRpc(int indexWindow, WindowSync windowSync, ClientRpcParams clientRpcParams = default)
        {
            var window = GetWindowByIndex(indexWindow);
            if (window is null)
            {
                Main.Log.LogError($"Not found {indexWindow} window");
                return;
            }
            window.WindowSync(windowSync);
        }
        [ServerRpc(RequireOwnership = false)]
        private void GetSyncWindowsServerRpc(ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[]{clientId}
                }
            };
            foreach (var window in DesktopWindows)
                AddWindowClientRpc(window.WindowIndex, window.GetType().FullName, clientRpcParams);
            foreach (var window in DesktopWindows)
                UpdateWindowClientRpc(window.WindowIndex, window.GetFullWindowSync(), clientRpcParams);
        }

        public override void OnDestroy()
        {
            TerminalBeganUsing -= OnBeganUsing;
            TerminalExited -= OnExitedUsing;
            DesktopStorage.TerminalNodeChangeEvent -= DesktopStorageOnTerminalNodeChangeEvent;
        }
        /*public void Reset()
        {
            ResetServerRpc();
        }
        [ServerRpc(RequireOwnership = false)]
        private void ResetServerRpc()
        {
            ResetClientRpc();
        }
        [ClientRpc]
        private void ResetClientRpc()
        {
            this.ResetDesktop();
        }*/
    }
}