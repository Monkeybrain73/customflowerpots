using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace customflowerpots
{
    public class BlockEntityCustomPlantContainer2x : BlockEntityCustomPlantContainerBase
    {
        public override int SlotCount => 2;

        protected override Vec3f SlotIndexToOffset(int i)
        {
            switch (i)
            {
                case 0: return new Vec3f(-0.15f, 0, 0); // left
                case 1: return new Vec3f(0.15f, 0, 0);  // right
                default: return Vec3f.Zero;
            }
        }

        protected override int GetSlotFromClick(BlockSelection blockSel)
        {
            double hx = blockSel.HitPosition.X;
            return hx < 0.5 ? 0 : 1;
        }
    }
}
