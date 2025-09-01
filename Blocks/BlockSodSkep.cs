using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace customflowerpots.Blocks
{
    internal class BlockSodSkep : BlockBeehive,IHarvestableDrops
    {

        float beemobSpawnChance = 0.2f;

        public bool IsEmpty()
        {
            return Variant["type"] == "empty";
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            string collectibleCode = byPlayer.InventoryManager.ActiveHotbarSlot?.Itemstack?.Collectible.Code.Path;
            if (collectibleCode == "beenade-opened" || collectibleCode == "beenade-closed") return false;



            if (byPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative && byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Collectible.Code.Path.Contains("honeycomb") == true)
            {
                BESodBeehive beh = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BESodBeehive;
                if (beh != null && !beh.Harvestable)
                {
                    beh.Harvestable = true;
                    beh.MarkDirty(true);
                }
                return true;
            }

//            if (byPlayer.InventoryManager.TryGiveItemstack(new ItemStack(world.BlockAccessor.GetBlock(this.CodeWithVariant("side", "east")))))
//            {
//                world.BlockAccessor.SetBlock(0, blockSel.Position);
//                world.PlaySoundAt(new AssetLocation("game:sounds/block/planks"), blockSel.Position, -0.5, byPlayer, false);

//                return true;
//            }

            return false;
        }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            beemobSpawnChance = Attributes?["beemobSpawnChance"].AsFloat(0.2f) ?? 0.2f;
        }


        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);

            if (world.Side == EnumAppSide.Server && !IsEmpty() && world.Rand.NextDouble() < beemobSpawnChance)     // Only test the chance and spawn the entity on the server side
            {
                EntityProperties type = world.GetEntityType(new AssetLocation("beemob"));
                Entity entity = world.ClassRegistry.CreateEntity(type);

                if (entity != null)
                {
                    entity.ServerPos.X = pos.X + 0.5f;
                    entity.ServerPos.Y = pos.Y + 0.5f;
                    entity.ServerPos.Z = pos.Z + 0.5f;
                    entity.ServerPos.Yaw = (float)world.Rand.NextDouble() * 2 * GameMath.PI;
                    entity.Pos.SetFrom(entity.ServerPos);

                    entity.Attributes.SetString("origin", "brokenbeehive");
                    world.SpawnEntity(entity);
                }
            }
        }

        public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
        {
            return GetHandbookDropsFromBreakDrops(handbookStack, forPlayer);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (IsEmpty())
            {
                return new ItemStack[] { new ItemStack(world.BlockAccessor.GetBlock(this.CodeWithVariant("side", "east"))) };
            }

            BESodBeehive beh = world.BlockAccessor.GetBlockEntity(pos) as BESodBeehive;
            if (beh == null || !beh.Harvestable)
            {
                return new ItemStack[] { new ItemStack(world.BlockAccessor.GetBlock(this.CodeWithVariant("side", "east"))) };
            }

            if (Drops == null) return null;
            List<ItemStack> todrop = new List<ItemStack>();

            for (int i = 0; i < Drops.Length; i++)
            {
                if (Drops[i].Tool != null && (byPlayer == null || Drops[i].Tool != byPlayer.InventoryManager.ActiveTool)) continue;

                ItemStack stack = Drops[i].GetNextItemStack(dropQuantityMultiplier);
                if (stack == null) continue;

                todrop.Add(stack);
                if (Drops[i].LastDrop) break;
            }

            return todrop.ToArray();
        }



    }
}