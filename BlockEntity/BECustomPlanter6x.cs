using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace customflowerpots
{
    public class BlockEntityCustomPlantContainer6x : BlockEntityCustomPlantContainerBase
    {
        public override int SlotCount => 6;

        protected override Vec3f SlotIndexToOffset(int i)
        {
            // Grid layout: 3 wide (x), 2 deep (z)
            switch (i)
            {
                case 0: return new Vec3f(-0.2f, 0, -0.15f); // front-left
                case 1: return new Vec3f(0f, 0, -0.15f); // front-center
                case 2: return new Vec3f(0.2f, 0, -0.15f); // front-right
                case 3: return new Vec3f(-0.2f, 0, 0.15f); // back-left
                case 4: return new Vec3f(0f, 0, 0.15f); // back-center
                case 5: return new Vec3f(0.2f, 0, 0.15f); // back-right
                default: return Vec3f.Zero;
            }
        }

        protected override int GetSlotFromClick(BlockSelection blockSel)
        {
            double hx = blockSel.HitPosition.X;
            double hz = blockSel.HitPosition.Z;

            int col = hx < 0.33 ? 0 : (hx < 0.66 ? 1 : 2);
            int row = hz < 0.5 ? 0 : 1;

            return row * 3 + col;
        }
    }
}
