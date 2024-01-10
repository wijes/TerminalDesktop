using UnityEngine.EventSystems;

namespace TerminalDesktopMod
{
    public class TerminalEvents: WindowEvents
    {
        public EventTrigger EventTriggerTerminal;

        public EventTrigger.TriggerEvent InputFocusWindowEvent { get; set; } = new EventTrigger.TriggerEvent();
        public override void CreateUIEvents(DesktopWindowBase desktopWindowBase)
        {
            base.CreateUIEvents(desktopWindowBase);
            if (desktopWindowBase is not TerminalWindow terminalWindow)
                return;
            var terminalClick  = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerClick,
                callback = InputFocusWindowEvent
            };
            InputFocusWindowEvent.AddListener(terminalWindow.InputFocus);
            EventTriggerTerminal.triggers.Add(terminalClick);
        }
    }
}