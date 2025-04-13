using OpenRA.Traits;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.Common.Effects;

namespace OpenRA.Mods.Common.Traits
{
    [Desc("Displays a visual effect when the shield is hit.")]
    public class ShieldVisualsInfo : ConditionalTraitInfo, Requires<ShieldInfo>, Requires<RenderSpritesInfo>
    {
        [Desc("Sequence to play when the shield is hit.")]
        public readonly string HitSequence = "shield-hit";

        [Desc("Palette to use for shield effect.")]
        [PaletteReference]
        public readonly string Palette = "effect";

        public override object Create(ActorInitializer init) { return new ShieldVisuals(init.Self, this); }
    }

    public class ShieldVisuals : ConditionalTrait<ShieldVisualsInfo>, IDamageModifier
    {
        readonly RenderSprites rs;
		Actor self;

        public ShieldVisuals(Actor self, ShieldVisualsInfo info)
            : base(info)
        {
            rs = self.Trait<RenderSprites>();
			this.self = self;
        }

        int IDamageModifier.GetDamageModifier(Actor attacker, Damage damage)
        {
            // Show hit animation if the shield is active and absorbs damage
            var shield = attacker.TraitOrDefault<Shield>();
            if (!IsTraitDisabled && shield != null && !shield.IsTraitDisabled)
            {
                var world = attacker.World;
                world.AddFrameEndTask(w => w.Add(new SpriteEffect(
                    self.CenterPosition,
                    w,
                    rs.GetImage(self),
                    Info.HitSequence,
                    Info.Palette)));
            }

            // Pass through to other damage modifiers
            return 100;
        }
    }
}
