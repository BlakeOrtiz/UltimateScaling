using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace UltimateScaling
{
	// Need one class that extends "Mod"
	public class UltimateScaling : Mod
	{
		// Dictionary of all prefixes that increase or decrease damage
		public static Dictionary<int, int> prefixes = new Dictionary<int, int>()
		{
			{ PrefixID.Broken, -30 },
			{ PrefixID.Annoying, -20 },
			{ PrefixID.Terrible, -15 },
			{ PrefixID.Dull, -15 },
			{ PrefixID.Awful, -15 },
			{ PrefixID.Damaged, -15 },
			{ PrefixID.Frenzying, -15 },
			{ PrefixID.Shameful, -10 },
			{ PrefixID.Ignorant, -10 },
			{ PrefixID.Deranged, -10 },
			{ PrefixID.Shoddy, -10 },
			{ PrefixID.Manic, -10},
			{ PrefixID.Dangerous, 5 },
			{ PrefixID.Bulky, 5 },
			{ PrefixID.Nasty, 5 },
			{ PrefixID.Unpleasant, 5 },
			{ PrefixID.Murderous, 7 },
			{ PrefixID.Savage, 10 },
			{ PrefixID.Pointy, 10 },
			{ PrefixID.Sighted, 10 },
			{ PrefixID.Deadly, 10 },
			{ PrefixID.Staunch, 10 },
			{ PrefixID.Mystic, 10 },
			{ PrefixID.Intense, 10 },
			{ PrefixID.Celestial, 10 },
			{ PrefixID.Superior, 10 },
			{ PrefixID.Deadly2, 10 },
			{ PrefixID.Hurtful, 10 },
			{ PrefixID.Sharp, 15 },
			{ PrefixID.Powerful, 15 },
			{ PrefixID.Masterful, 15 },
			{ PrefixID.Furious, 15 },
			{ PrefixID.Godly, 15 },
			{ PrefixID.Demonic, 15 },
			{ PrefixID.Legendary, 15 },
			{ PrefixID.Unreal, 15 },
			{ PrefixID.Mythical, 15 },
			{ PrefixID.Ruthless, 18 }
		};
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

		// Get the Weapon's Damage before its prefix is applied
		private static int GetTrueDmg(Item item)
		{
			if (item == null) return item.damage;

			var prefixes = UltimateScaling.prefixes;
			int id = item.prefix;
			int value;
			int adjust;

			if (prefixes.ContainsKey(id))
			{
				bool hasValue = prefixes.TryGetValue(id, out value);
				if (hasValue)
				{
					adjust = -value;
					return item.damage + (item.damage * adjust) / 100;
				}
			}
			return item.damage;
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
			if ((GetTrueDmg(item) * boost) * (60 / item.useTime) >= total * total * total / 1.75)
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