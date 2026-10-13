using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace PrisonBreaks
{
    public class HarmonyCompatibilityStatus
    {
        public bool Available;
        public bool HasExternalPatches;
        public List<string> ExternalOwnerIds = new List<string>();
    }

    public class HarmonyPatcher
    {
        public const string HarmonyId = "com.lessprisonbreaks";

        public static Harmony instance;

        private static MethodInfo canParticipateInPrisonBreakMethod;

        private static HarmonyCompatibilityStatus cachedCompatibilityStatus;

        public static void PatchVanillaMethods()
        {
            canParticipateInPrisonBreakMethod = AccessTools.Method(typeof(PrisonBreakUtility), "CanParticipateInPrisonBreak", new Type[] { typeof(Pawn) });
            if (canParticipateInPrisonBreakMethod == null)
            {
                Log.Error("[PrisonBreaks] Could not find PrisonBreakUtility.CanParticipateInPrisonBreak(Pawn); the mod's patch was not applied and its compatibility status will be unavailable.");
                return;
            }

            HarmonyMethod canParticipateInPrisonBreakPrefixPatch = new HarmonyMethod(typeof(Patches).GetMethod("CanParticipateInPrisonBreakPrefixPatch"));
            instance.Patch(canParticipateInPrisonBreakMethod, canParticipateInPrisonBreakPrefixPatch);
        }

        // Cached on first call: settings can't open until all mods' Init long events (and their patching) have run.
        public static HarmonyCompatibilityStatus GetExternalCanParticipatePatches()
        {
            if (cachedCompatibilityStatus != null)
            {
                return cachedCompatibilityStatus;
            }

            HarmonyCompatibilityStatus status = new HarmonyCompatibilityStatus();

            if (canParticipateInPrisonBreakMethod == null)
            {
                status.Available = false;
                cachedCompatibilityStatus = status;
                return status;
            }

            try
            {
                HarmonyLib.Patches patches = Harmony.GetPatchInfo(canParticipateInPrisonBreakMethod);
                // Patches.Owners covers every Harmony 3 category, including inner prefixes, postfixes and finalizers.
                List<string> owners = (patches == null ? Enumerable.Empty<string>() : patches.Owners)
                    .Select(owner => string.IsNullOrEmpty(owner) ? "SK_PrisonBreaks_UnknownOwner".Translate().ToString() : owner)
                    .Where(owner => !string.Equals(owner, HarmonyId, StringComparison.Ordinal))
                    .Distinct()
                    .OrderBy(owner => owner, StringComparer.Ordinal)
                    .ToList();

                status.Available = true;
                status.HasExternalPatches = owners.Count > 0;
                status.ExternalOwnerIds = owners;
            }
            catch (Exception ex)
            {
                Log.Warning("[PrisonBreaks] Failed to inspect Harmony patch registry for compatibility status: " + ex);
                status.Available = false;
            }

            cachedCompatibilityStatus = status;
            return status;
        }
    }
}
