using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace UltimateScaling
{
	// Need one class that extends "Mod"
	public class UltimateScaling : Mod
	{
	}

	// Allows classes to save and share the variables that are made here
	public static class ShowDamage
	{
		private static float boostedDmg = 0f;
		public static float BoostedDmg
		{
			get { return boostedDmg; }
			set { boostedDmg = value; }
		}

		private static bool isBoosted;
		public static bool IsBoosted
		{
			get { return isBoosted; }
			set { isBoosted = value; }
		}
	}

	//Extends "ModPlayer" in order to use ModifyWeaponDamage
	public class ModifyDamage : ModPlayer
	{
		// Send the item to be checked if it can be boosted
		public override void ModifyWeaponDamage(Item item, ref float add, ref float mult, ref float flat)
		{
			flat += BoostDmg(item);
		}

		// Check the sent item to determine total boss kills and if it falls under DPS value to boost
		private float BoostDmg(Item item)
		{
			List<bool> bosses = new List<bool>
			{
				NPC.downedSlimeKing,
				NPC.downedBoss1,
				NPC.downedBoss2,
				NPC.downedBoss3,
				NPC.downedQueenBee,
				NPC.downedMechBoss1,
				NPC.downedMechBoss2,
				NPC.downedMechBoss3,
				NPC.downedFishron,
				NPC.downedPlantBoss,
				NPC.downedGolemBoss,
				NPC.downedAncientCultist,
				NPC.downedMoonlord
			};

			float boost = 0.5f;
			int total = 0;
			int max = bosses.Count;


			// Get all bosses that have been downed and add to the 'total'
			foreach (bool downed in bosses)
			{
				if (downed)
				{
					total++;
				}
			}

			// Exponentially add damage onto the weapon based on world bosses killed
			for (int i = 0; i < total; i++)
			{
				boost *= 1.125f;
			}

			// Calculate the DPS after adding the boost and if it "outscales" the world progress
			// We could use Math.Pow(total, 3) here, but just multiplying it by itself computes faster.
			// Since this is being done every frame, speed has more importance here
			// TODO: Add this to a loop to allow boosting up to an amount that doesn't surpass the "DPS Limit"
			// Save off the highest amount allowed to boost and return that instead of tossing a "no" for if it ever surpasses
			if ((item.damage * boost) * (60 / item.useTime) >= total * total * total / 1.75)
			{
				ShowDamage.IsBoosted = false;
				return 0f;
			}

			ShowDamage.BoostedDmg = item.damage * boost;
			ShowDamage.IsBoosted = true;
			return item.damage * boost;
		}
	}

	// Add Tooltips to show current boost of weapons and add a simple DPS line
	public class ShowBoostedDamage : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			int dmg = 0;
			float dps = item.damage * (60 / item.useTime);

			// Determine if weapon should show any boosted damage in tooltip line
			if (ShowDamage.IsBoosted)
			{
				dmg = (int)ShowDamage.BoostedDmg;
				dps = (dmg + item.damage) * (60 / item.useTime);
			}
			// Determine if an item is a weapon or not.
			if (item.damage > 0)
			{
				var curBoost = new TooltipLine(mod, "Boost", "[Boost] (+" + $"{dmg}) Damage");
				var curDPS = new TooltipLine(mod, "DPS", "[DPS] " + $"{dps}/s");
				tooltips.Add(curBoost);
				tooltips.Add(curDPS);
			}
		}
	}

}