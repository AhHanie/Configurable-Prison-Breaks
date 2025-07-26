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
            var harmony = new Harmony("com.lessprisonbreaks");
            HarmonyPatcher.instance = harmony;
            LongEventHandler.QueueLongEvent(Init, "SK.PrisonBreaks.Init", true, null);
        }

        public override async void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label($"{"SK_PrisonBreaks_PawnBreakThresholdLabel".Translate()}: " + settings.getlevel());
            listingStandard.IntSetter(ref settings.val, (int)MoodThreshold.Extreme + 1, "SK_PrisonBreaks_NeverButton".Translate());
            listingStandard.IntSetter(ref settings.val, (int)MoodThreshold.Minor, "SK_PrisonBreaks_MinorButton".Translate());
            listingStandard.IntSetter(ref settings.val, (int)MoodThreshold.Major, "SK_PrisonBreaks_MajorButton".Translate());
            listingStandard.IntSetter(ref settings.val, (int)MoodThreshold.Extreme, "SK_PrisonBreaks_ExtremeButton".Translate());
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Configurable Prison Breaks";
        }

        public void Init()
        {
            this.settings = GetSettings<PrisonBreaksSettings>();
            HarmonyPatcher.PatchVanillaMethods();
        }
    }
}