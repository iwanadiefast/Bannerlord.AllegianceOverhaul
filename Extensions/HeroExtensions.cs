﻿using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Library;

using AllegianceOverhaul.Helpers;
using AllegianceOverhaul.LoyaltyRebalance;

namespace AllegianceOverhaul.Extensions
{
  public static class HeroExtensions
  {
    private delegate void GetPersonalityEffectsDelegate(DefaultDiplomacyModel instance, ref ExplainedNumber explainedNumber, Hero hero, Hero otherHero);
    private static readonly GetPersonalityEffectsDelegate deGetPersonalityEffects = AccessHelper.GetDelegate<GetPersonalityEffectsDelegate>(typeof(DefaultDiplomacyModel), "GetPersonalityEffects");

    public static List<Hero> GetAllSiblings(this Hero hero)
    {
      if (hero.Father != null && hero.Mother != null)
        return hero.Father.Children.Union(hero.Mother.Children).ToList();
      else
        return hero.Siblings.ToList();
    }

    public static int GetModifiedRelation(this Hero hero, Hero otherHero)
    {
      ExplainedNumber relationBetweenHeroes = new ExplainedNumber(CharacterRelationManager.GetHeroRelation(hero, otherHero), false, null);
      deGetPersonalityEffects(Campaign.Current.Models.DiplomacyModel is DefaultDiplomacyModel defaultDiplomacyModel ? defaultDiplomacyModel : new DefaultDiplomacyModel(), ref relationBetweenHeroes, hero, otherHero);
      return MBMath.Round(MBMath.ClampFloat(relationBetweenHeroes.ResultNumber + (RelativesHelper.BloodRelatives(hero, otherHero) ? 30f : 0f), -100f, 100f));
    }
  }
}
