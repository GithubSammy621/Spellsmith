using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace Spellsmith
{
	public class Helper
	{
		public static bool CheckCircularCollision(Vector2 center, Rectangle hitbox, int radius)
		{
			if (Vector2.Distance(center, hitbox.TopLeft()) <= radius)
			{
				return true;
			}
			if (Vector2.Distance(center, hitbox.TopRight()) <= radius)
			{
				return true;
			}
			if (Vector2.Distance(center, hitbox.BottomLeft()) <= radius)
			{
				return true;
			}
			return Vector2.Distance(center, hitbox.BottomRight()) <= radius;
		}
		public static bool IsTargetValid(NPC npc) => npc.type == NPCID.TargetDummy || (npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage);

		public static void CircleAOEDamage(Vector2 center, Player player, int damage, float knockBack, int hitDirection, bool crit, int radius)
		{
			foreach (NPC npc in Main.npc.Where(n => n.active && CheckCircularCollision(center, n.Hitbox, radius)))
            {
				if (IsTargetValid(npc))
				{
					player.ApplyDamageToNPC(npc, damage, knockBack, hitDirection, crit);
				}
            }
		}
	}
}