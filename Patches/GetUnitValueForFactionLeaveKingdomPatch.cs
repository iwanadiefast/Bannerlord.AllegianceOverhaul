﻿using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Barterables;

namespace AllegianceOverhaul.Patches
{
  [HarmonyPatch(typeof(LeaveKingdomAsClanBarterable), "GetUnitValueForFaction")]
  public class GetUnitValueForFactionLeaveKingdomPatch
  {
    public static void Postfix(IFaction faction, ref int __result, LeaveKingdomAsClanBarterable __instance)
    {
      try
      {
        Hero iOriginalOwner = __instance.OriginalOwner;
        Clan iOriginalOwnerClan = iOriginalOwner.Clan;
        Kingdom iOriginalOwnerKingdom = iOriginalOwnerClan.Kingdom;

        if
        (
          (!Settings.Instance.FixMinorFactionVassals || !SettingsHelper.FactionInScope(faction, Settings.Instance.EnsuredLoyaltyScope))
          && (!SettingsHelper.InDebugBranch || !Settings.Instance.EnableTechnicalDebugging || !SettingsHelper.FactionInScope(faction, Settings.Instance.EnsuredLoyaltyDebugScope))
        )
          return;

        IFaction mapFaction = iOriginalOwner.MapFaction;
        float CalculatedResult;
        if (faction == __instance.OriginalOwner.Clan)
          CalculatedResult = __instance.OriginalOwner.Clan.IsUnderMercenaryService ? (int)Campaign.Current.Models.DiplomacyModel.GetScoreOfMercenaryToLeaveKingdom(iOriginalOwnerClan, iOriginalOwnerKingdom) : (int)Campaign.Current.Models.DiplomacyModel.GetScoreOfClanToLeaveKingdom(iOriginalOwnerClan, iOriginalOwnerKingdom);
        else
        {
          if (faction == mapFaction)
          {
            CalculatedResult = (float)((!iOriginalOwnerClan.IsUnderMercenaryService ? (double)Campaign.Current.Models.DiplomacyModel.GetScoreOfClanToLeaveKingdom(iOriginalOwnerClan, iOriginalOwnerKingdom) : (double)Campaign.Current.Models.DiplomacyModel.GetScoreOfMercenaryToLeaveKingdom(iOriginalOwnerClan, iOriginalOwnerKingdom)) * (faction == iOriginalOwnerClan || faction == iOriginalOwnerKingdom ? -1.0 : 1.0));
          }
          else
          {
            float clanStrength = Campaign.Current.Models.DiplomacyModel.GetClanStrength(iOriginalOwnerClan);
            CalculatedResult = !faction.IsClan || !FactionManager.IsAtWarAgainstFaction(faction, (IFaction)iOriginalOwnerKingdom) ? (!FactionManager.IsAlliedWithFaction(faction, (IFaction)iOriginalOwnerKingdom) ? clanStrength * 0.01f : clanStrength * -0.5f) : clanStrength * 0.5f;
          }
        }

        if (SettingsHelper.InDebugBranch && Settings.Instance.EnableTechnicalDebugging && SettingsHelper.FactionInScope(faction, Settings.Instance.EnsuredLoyaltyDebugScope))
        {
          string UnitValueDebugInfo = String.Format("LeaveKingdom - UnitValueForFaction. faction: {0}. ScoreOfMercenaryToLeaveKingdom = {1}. ScoreOfClanToLeaveKingdom = {2}. CalculatedResult = {3}. Result = {4}",
            faction.Name,
            ((int)Campaign.Current.Models.DiplomacyModel.GetScoreOfMercenaryToLeaveKingdom(iOriginalOwnerClan, iOriginalOwnerKingdom)).ToString("N"),
            ((int)Campaign.Current.Models.DiplomacyModel.GetScoreOfClanToLeaveKingdom(iOriginalOwnerClan, iOriginalOwnerKingdom)).ToString("N"),
            CalculatedResult.ToString("N"), __result.ToString("N"));

          MessageHelper.TechnicalMessage(UnitValueDebugInfo);
        }
        if (Settings.Instance.FixMinorFactionVassals)
          __result = (int)CalculatedResult;
      }
      catch (Exception ex)
      {
        MethodInfo methodInfo = MethodBase.GetCurrentMethod() as MethodInfo;
        DebugHelper.HandleException(ex, methodInfo, "Harmony patch for LeaveKingdomAsClanBarterable.GetUnitValueForFaction");
      }
    }
    public static bool Prepare()
    {
      return (Settings.Instance.FixMinorFactionVassals || Settings.Instance.EnableTechnicalDebugging);
    }
  }
}