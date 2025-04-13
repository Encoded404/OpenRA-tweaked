using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Effects;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
    [Desc("This actor has a shield that absorbs damage.")]
    public class ShieldInfo : ConditionalTraitInfo, Requires<HealthInfo>
    {
        [Desc("Amount of damage the shield can absorb before failing.")]
        public readonly int HP = 50000;

        [Desc("Shield regeneration per tick.")]
        public readonly int RegenRate = 50;

        [Desc("Delay in ticks before shield starts regenerating.")]
        public readonly int RegenDelay = 50;

        [Desc("Display shield as a bar under the unit.")]
        public readonly bool ShowBar = true;

        [Desc("Color of the shield bar.")]
        public readonly Color Color = Color.Blue;

        [Desc("Shield damage types that are 100% effective.")]
        public readonly BitSet<DamageType> EffectiveAgainst = default;

        [Desc("Shield damage types that are 0% effective.")]
        public readonly BitSet<DamageType> ImmuneAgainst = default;

        [Desc("Sound played when shield is hit.")]
        public readonly string HitSound = null;

        [Desc("Damage percentage applies to shield. Default is 100%.")]
        public readonly int DamageModifier = 100;

        public override object Create(ActorInitializer init) { return new Shield(init.Self, this); }
    }

    public class Shield : ConditionalTrait<ShieldInfo>, ISync, ITick, IDamageModifier, ISelectionBar
    {
        readonly Health health;
        [Sync] int hp;
        [Sync] int regenTicks;

        public Shield(Actor self, ShieldInfo info)
            : base(info)
        {
            health = self.Trait<Health>();
            hp = info.HP;
        }

        void ITick.Tick(Actor self)
        {
            if (IsTraitDisabled || hp >= Info.HP)
                return;

            if (regenTicks > 0)
                regenTicks--;
            else
                hp = (hp + Info.RegenRate).Clamp(0, Info.HP);
        }

        int IDamageModifier.GetDamageModifier(Actor attacker, Damage damage)
        {
            if (IsTraitDisabled || hp <= 0)
                return 100;

            // Damage types that bypass the shield
            if (damage.DamageTypes.Overlaps(Info.ImmuneAgainst))
                return 100;

            // Reset regen counter
            regenTicks = Info.RegenDelay;

            // Calculate actual damage to shield
            int shieldDamage = damage.Value * Info.DamageModifier / 100;

            // Shield is more effective against certain damage types
            if (damage.DamageTypes.Overlaps(Info.EffectiveAgainst))
                shieldDamage = shieldDamage * 2;

            hp -= shieldDamage;

            // Play hit sound if defined
            if (!string.IsNullOrEmpty(Info.HitSound))
                Game.Sound.Play(SoundType.World, Info.HitSound, attacker.CenterPosition);

            // Shield handles all damage if it has health left
            if (hp > 0)
                return 0;

            // Shield is depleted, pass remaining damage through
            hp = 0;
            return 100;
        }

        float ISelectionBar.GetValue()
        {
            if (IsTraitDisabled || !Info.ShowBar)
                return 0;

            return (float)hp / Info.HP;
        }

        Color ISelectionBar.GetColor() { return Info.Color; }
        bool ISelectionBar.DisplayWhenEmpty { get { return false; } }
    }
}
