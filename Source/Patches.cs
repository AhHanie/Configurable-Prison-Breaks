using RimWorld;
using Verse;

namespace PrisonBreaks
{
    public class Patches
    {
        public static bool CanParticipateInPrisonBreakPrefixPatch(ref bool __result, Pawn pawn)
        {
            if ((int)MoodThresholdExtensions.CurrentMoodThresholdFor(pawn) < LoadedModManager.GetMod<PrisonBreaks>().GetSettings<PrisonBreaksSettings>().val)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
