using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TerminalDesktopMod
{
    public class WindowEvents: MonoBehaviour
    {
        public EventTrigger EventTriggerWindow;
        public EventTrigger EventTriggerTopBar;
        public EventTrigger EventTriggerClose;
        public EventTrigger EventTriggerCollapse;
        public EventTrigger EventTriggerScale;

        public EventTrigger.TriggerEvent WindowClickEvent { get; set; }= new EventTrigger.TriggerEvent();
        public EventTrigger.TriggerEvent DragWindowEvent { get; set; } = new EventTrigger.TriggerEvent();
        public EventTrigger.TriggerEvent DragStartWindowEvent { get; set; } = new EventTrigger.TriggerEvent();
        public EventTrigger.TriggerEvent DragEndWindowEvent { get; set; } = new EventTrigger.TriggerEvent();
        public EventTrigger.TriggerEvent CloseWindowEvent { get; set; } = new EventTrigger.TriggerEvent();
        public EventTrigger.TriggerEvent CollapseWindowEvent { get; set; } = new EventTrigger.TriggerEvent();
        public EventTrigger.TriggerEvent ScaleWindowEvent { get; set; } = new EventTrigger.TriggerEvent();
        public EventTrigger.TriggerEvent ScaleStartWindowEvent { get; set; } = new EventTrigger.TriggerEvent();
        public EventTrigger.TriggerEvent ScaleEndWindowEvent { get; set; } = new EventTrigger.TriggerEvent();
        public virtual void CreateUIEvents(DesktopWindowBase desktopWindowBase)
        {
            var dragWin = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.Drag,
                callback = DragWindowEvent
            };
            DragWindowEvent.AddListener(desktopWindowBase.MoveWindow);
            EventTriggerTopBar.triggers.Add(dragWin);
            
            var dragStartWin = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.InitializePotentialDrag,
                callback = DragStartWindowEvent
            };
            DragStartWindowEvent.AddListener(desktopWindowBase.StartMoveWindow);
            EventTriggerTopBar.triggers.Add(dragStartWin);
            
            var dragEndWin = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.EndDrag,
                callback = DragEndWindowEvent
            };
            DragEndWindowEvent.AddListener(desktopWindowBase.EndMoveWindow);
            EventTriggerTopBar.triggers.Add(dragEndWin);
            
            var scaleWin = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.Drag,
                callback = ScaleWindowEvent
            };
            ScaleWindowEvent.AddListener(desktopWindowBase.ScaleWindow);
            EventTriggerScale.triggers.Add(scaleWin);
            
            var scaleStartWin = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.InitializePotentialDrag,
                callback = ScaleStartWindowEvent
            };
            ScaleStartWindowEvent.AddListener(desktopWindowBase.StartScaleWindow);
            EventTriggerScale.triggers.Add(scaleStartWin);
            
            var scaleEndWin = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.EndDrag,
                callback = ScaleEndWindowEvent
            };
            ScaleEndWindowEvent.AddListener(desktopWindowBase.EndScaleWindow);
            EventTriggerScale.triggers.Add(scaleEndWin);
            
            var closeWin = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerClick,
                callback = CloseWindowEvent
            };
            CloseWindowEvent.AddListener(desktopWindowBase.ClickCloseWindow);
            EventTriggerClose.triggers.Add(closeWin);
            
            var collapseWin = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerClick,
                callback = CollapseWindowEvent
            };
            CollapseWindowEvent.AddListener(desktopWindowBase.ClickCollapseWindow);
            EventTriggerCollapse.triggers.Add(collapseWin);
            
            var clickWin = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerClick,
                callback = WindowClickEvent
            };
            WindowClickEvent.AddListener(desktopWindowBase.ClickWindow);
            EventTriggerWindow.triggers.Add(clickWin);
            EventTriggerTopBar.triggers.Add(clickWin);
        }
    }
}