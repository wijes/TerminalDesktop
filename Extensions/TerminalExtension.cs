using UnityEngine;

namespace TerminalDesktopMod.Extentions
{
    public static class TerminalExtension
    {
        public static void ChangeCredits(this Terminal terminal, int creditChange)
        {
            var newCredits = terminal.groupCredits += creditChange;
            newCredits = Mathf.Clamp(newCredits, 0, newCredits);
            terminal.SyncGroupCreditsServerRpc(newCredits, terminal.numberOfItemsInDropship);
        }
    }
}