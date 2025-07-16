using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;


namespace PrisonBreaks
{
    public class PrisonBreaksSettings : ModSettings
    {
        //threshhold to enable/disable
        public int val;

        public string getlevel()
        {
            switch ((MoodThreshold)val)
            {
                case MoodThreshold.Extreme:
                    return "Extreme";
                case MoodThreshold.Minor:
                    return "Minor";
                case MoodThreshold.Major:
                    return "Major";
                default:
                    return "Never";
            }
        }

        //save settings
        public override void ExposeData()
        {
            Scribe_Values.Look(ref val, "val", 1);
            base.ExposeData();
        }
    }

    public class PrisonBreaks : Mod
    {
        PrisonBreaksSettings settings;

        public PrisonBreaks(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<PrisonBreaksSettings>();
        }

        public override async void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Threshold for prison break: " + settings.getlevel());
            listingStandard.IntSetter(ref settings.val, (int)MoodThreshold.Extreme + 1, "Never");
            listingStandard.IntSetter(ref settings.val, (int)MoodThreshold.Minor, "Minor");
            listingStandard.IntSetter(ref settings.val, (int)MoodThreshold.Major, "Major");
            listingStandard.IntSetter(ref settings.val, (int)MoodThreshold.Extreme, "Extreme");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Configurable Prison Breaks";
        }
    }

    [StaticConstructorOnStartup]
    public static class PrisonBreakPatch
    {
        static PrisonBreakPatch()
        {
            var harmony = new Harmony("com.lessprisonbreaks");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Log.Message("Configureable Prison breaks loaded");
        }

        [HarmonyPatch(typeof(PrisonBreakUtility), nameof(PrisonBreakUtility.CanParticipateInPrisonBreak))]
        class Patch
        {
            [HarmonyPrefix]
            static bool Prefix(ref bool __result, Pawn pawn)
            {
                if ((int)MoodThresholdExtensions.CurrentMoodThresholdFor(pawn) < LoadedModManager.GetMod<PrisonBreaks>().GetSettings<PrisonBreaksSettings>().val)
                {
                    __result = false;
#if DEBUG
                    Log.Message("patching prison break for: " + pawn.Name.ToString());
#endif
                    return false;
                }
                return true;
            }
        }
    }
}