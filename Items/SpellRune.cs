using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;
using Terraria;
using System.Collections.Generic;
using Spellsmith.Items.EnchantedRunes;
using System.Linq;
using System;

namespace Spellsmith.Items
{
	public class SpellRune : ModItem
	{
        public List<Item> runes = new List<Item>();
        public List<SpellEffect> effects = new List<SpellEffect>();
        bool setupRunes = false;
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            int maxCooldown = 0; 
            string effectsString = "";

            foreach (SpellEffect effect in effects)
            {
                if (effect.maxCooldown > maxCooldown)
                {
                    maxCooldown = (int)effect.maxCooldown;
                }
            }
            foreach (Item rune in runes)
            {
                effectsString += rune.Name + " ";
            }
            if (setupRunes)
            {
                tooltips.Add(new TooltipLine(Spellsmith.instance, "SpellRuneTooltip", "Contains a spell"));
                tooltips.Add(new TooltipLine(Spellsmith.instance, "SpellRuneEffects", effectsString));
                string manaCostTooltip = GetManaCost(Main.LocalPlayer) == 0 ? "Uses no mana" : "Uses " + GetManaCost(Main.LocalPlayer) + " mana";
                tooltips.Add(new TooltipLine(Spellsmith.instance, "SpellRuneManaCost", manaCostTooltip));
                string cooldown = maxCooldown == 0 ? "No Cooldown" : maxCooldown / 60 + " Second cooldown";
                tooltips.Add(new TooltipLine(Spellsmith.instance, "SpellRuneCooldown", cooldown));
            }
            base.ModifyTooltips(tooltips);
        }

        public override void SetDefaults() 
		{
			item.width = 40;
			item.height = 40;
			item.value = 1000;
			item.rare = ItemRarityID.Green;
            item.ammo = item.type;
		}
        public override bool CanRightClick()
        {
            return true;
        }

        public override ModItem Clone(Item item)
        {
            var newItem = base.Clone(item) as SpellRune;
            var oldItem = item.modItem as SpellRune;

            if (oldItem.setupRunes)
            {
                newItem.runes = oldItem.runes;
                newItem.effects = oldItem.effects;
                newItem.setupRunes = true;
            }
            return newItem;
        }
        public override ModItem Clone()
        {
            var newItem = base.Clone() as SpellRune;
            var oldItem = item.modItem as SpellRune;

            if (oldItem.setupRunes)
            {
                newItem.runes = oldItem.runes;
                newItem.effects = oldItem.effects;
                newItem.setupRunes = true;
            }
            return newItem;
        }
        public override TagCompound Save()
        {
            if (setupRunes)
            {
                TagCompound newCompound = new TagCompound();
                newCompound.Add("rune_count", runes.Count);
                for (int i = 0; i < runes.Count; i++)
                {
                    newCompound.Add("rune_" + i, runes[i]);
                }
                return newCompound;
            }
            return null;
        }
        public override void Load(TagCompound tag)
        {
            if (tag.ContainsKey("rune_count"))
            {
                List<Item> testRunes = new List<Item>();
                int runeCount = tag.GetInt("rune_count");
                for (int i = 0; i < runeCount; i++)
                {
                    testRunes.Add(tag.Get<Item>("rune_" + i));
                }
                CompleteSet(testRunes);
            }
            base.Load(tag);
        }
        public override void RightClick(Player player)
        {
            //temporary
            Main.NewText(setupRunes);
            foreach (Item rune in runes)
            {
                Main.NewText(rune);
            }
            foreach (SpellEffect effect in effects)
            {
                Main.NewText(effect);
            }
            if (!setupRunes)
            {
                int runeCount = 0;
                for (int i = 0; i < player.inventory.Count(); i++)
                {
                    Item item = (Item)player.inventory.GetValue(i);
                    if (runeCount >= 5)
                    {
                        TryCreateSet(player);
                        break;
                    }
                    if (item.modItem is EnchantedRune)
                    {
                        runes.Add(item.Clone());
                        runeCount++;
                        item.stack--;
                    }
                    else
                    {
                        if (runes.Count > 0)
                        {
                            TryCreateSet(player);
                        }
                        break;
                    }
                }
            }
            item.stack++;
            base.RightClick(player);
        }
        public void TryCreateSet(Player player)
        {
            int runeCount = 0;
            List<Item> testRunes = new List<Item>();
            for (int i = 0; i < player.inventory.Count(); i++)
            {
                Item item = (Item)player.inventory.GetValue(i);
                if (item.modItem is EnchantedRune)
                {
                    testRunes.Add(item.Clone());
                    runeCount++;
                }
                else
                {
                    CompleteSet(testRunes);
                    break;
                }
                if (runeCount >= 5)
                {
                    CompleteSet(testRunes);
                    break;
                }
            }
        }
        public void CompleteSet(List<Item> testRunes)
        {
            runes = testRunes;
            foreach (Item rune in testRunes)
            {
                rune.stack--;
                if (rune.modItem is EffectRune)
                {
                    EffectRune effectRune = (EffectRune)rune.modItem;
                    effectRune.effect.setup();
                    effects.Add(effectRune.effect);
                }
                if (effects.Count != 0)
                {
                    BaseRune baseRune = (BaseRune)rune.modItem;
                    baseRune.setup(effects.Last());
                }
                else if (rune.modItem is ModifierRune)
                {
                    ModifierRune modifierRune = (ModifierRune)rune.modItem;
                    if (modifierRune.fuckUpEffect != null)
                    {
                        effects.Add(modifierRune.fuckUpEffect);
                        modifierRune.setup(effects.Last());
                    }
                }
            }
            foreach(SpellEffect effect in effects)
            {
                effect.bundledEffects = effects.Count;
            }
            setupRunes = true;
        }
        public int GetManaCost(Player player)
        {
            int combinedEffectCost = 0;
            foreach(SpellEffect effect in effects)
            {
                combinedEffectCost += effect.getTotalManaCost(player);
            }
            return combinedEffectCost;
        }
    }
}