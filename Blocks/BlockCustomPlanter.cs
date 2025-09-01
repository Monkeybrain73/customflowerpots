using System.Collections.Generic;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace customflowerpots;

public class BlockCustomPlanter : Block
{

    WorldInteraction[] interactions = new WorldInteraction[0];

    public string ContainerSize => Attributes["plantContainerSize"].AsString();

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);

        LoadColorMapAnyway = true;

        List<ItemStack> stacks = new List<ItemStack>();

        if (Variant["contents"] != "empty")
        {
            return;
        }

        foreach (var block in api.World.Blocks)
        {
            if (block.IsMissing) continue;

            if (block.Attributes?["plantContainable"].Exists == true)
            {
                stacks.Add(new ItemStack(block));
            }
        }

        foreach (var item in api.World.Items)
        {
            if (item.Code == null || item.IsMissing) continue;

            if (item.Attributes?["plantContainable"].Exists == true)
            {
                stacks.Add(new ItemStack(item));
            }
        }

        interactions = new WorldInteraction[]
        {
            new WorldInteraction()
            {
                ActionLangCode = "blockhelp-flowerpot-plant",
                MouseButton = EnumMouseButton.Right,
                Itemstacks = stacks.ToArray()
            }
        };
    }

    public ItemStack GetContents(IWorldAccessor world, BlockPos pos)
    {
        BlockEntityPlantContainer be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPlantContainer;
        return be?.GetContents();
    }


    public override void OnDecalTesselation(IWorldAccessor world, MeshData decalMesh, BlockPos pos)
    {
        base.OnDecalTesselation(world, decalMesh, pos);
        BlockEntityPlantContainer bept = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityPlantContainer;
        if (bept != null)
        {
            decalMesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, bept.MeshAngle, 0);
        }
    }


    public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
    {
        bool val = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack);

        if (val)
        {
            BlockEntityPlantContainer bect = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityPlantContainer;
            if (bect != null)
            {
                // No rotation adjustments needed.
            }
        }

        return val;
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
    {
        base.OnBlockBroken(world, pos, byPlayer);

        ItemStack contents = GetContents(world, pos);
        if (contents != null)
        {
            world.SpawnItemEntity(contents, pos);
        }
    }

    public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
    {
        return GetHandbookDropsFromBreakDrops(handbookStack, forPlayer);
    }

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
    {
        return base.OnPickBlock(world, pos);
    }


    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        BlockEntityPlantContainer be = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntityPlantContainer;

        if (byPlayer.InventoryManager?.ActiveHotbarSlot?.Empty == false && be != null)
        {
            return be.TryPutContents(byPlayer.InventoryManager.ActiveHotbarSlot, byPlayer);
        }

        return false;
    }

    public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
    {
        return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
    }
}
