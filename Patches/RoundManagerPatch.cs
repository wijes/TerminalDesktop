using System.Linq;
using HarmonyLib;

namespace TerminalDesktopMod
{
    [HarmonyPatch(typeof(RoundManager))]
    public static partial class RoundManagerPatch
    {
        [HarmonyPatch("SpawnScrapInLevel")]
        [HarmonyPrefix]
        public static void SpawnScrapInLevel(ref RoundManager __instance)
        {
            var level = __instance.currentLevel;
            foreach (var scrap in DesktopStorage.SpawnableScraps)
            {
                if (level.spawnableScrap.Count(x => x.spawnableItem == scrap) == 0)
                    level.spawnableScrap.Add(new SpawnableItemWithRarity()
                    {
                        spawnableItem = scrap,
                        rarity = 70
                    });
            }
        }
    }
}
