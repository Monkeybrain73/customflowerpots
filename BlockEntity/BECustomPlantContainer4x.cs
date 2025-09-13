using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

#nullable disable

namespace customflowerpots
{

    public class BlockEntityCustomPlantContainer4x : BlockEntityContainer, ITexPositionSource, IRotatable
    {
        InventoryGeneric inv;
        public override InventoryBase Inventory => inv;
        public override string InventoryClassName => "pottedplant";

        public virtual float MeshAngle { get; set; }
        public string ContainerSize => Block.Attributes?["plantContainerSize"].AsString();

        MeshData potMesh;
        MeshData[] contentMeshes;
        RoomRegistry roomReg;

        bool hasSoil => inv.Any(slot => !slot.Empty);

        public BlockEntityCustomPlantContainer4x()
        {
            inv = new InventoryGeneric(4, null, null, null);
            inv.OnAcquireTransitionSpeed += slotTransitionSpeed;
        }

        private float slotTransitionSpeed(EnumTransitionType transType, ItemStack stack, float mulByConfig)
        {
            return 0;
        }

        protected override void OnTick(float dt)
        {
            // Don't tick inventory contents
        }

        PlantContainerProps PlantContProps => GetProps(inv[0].Itemstack);

        ICoreClientAPI capi;
        ITexPositionSource contentTexSource;
        PlantContainerProps curContProps;
        Dictionary<string, AssetLocation> shapeTextures;

        public Size2i AtlasSize => capi.BlockTextureAtlas.Size;
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                AssetLocation textureLoc = null;

                if (curContProps?.Textures != null && curContProps.Textures.TryGetValue(textureCode, out CompositeTexture compTex))
                {
                    textureLoc = compTex.Base;
                }

                if (textureLoc == null && shapeTextures != null)
                {
                    shapeTextures.TryGetValue(textureCode, out textureLoc);
                }

                if (textureLoc != null)
                {
                    TextureAtlasPosition texPos = capi.BlockTextureAtlas[textureLoc];
                    if (texPos == null)
                    {
                        BitmapRef bmp = capi.Assets.TryGet(textureLoc.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"))?.ToBitmap(capi);
                        if (bmp != null)
                        {
                            capi.BlockTextureAtlas.GetOrInsertTexture(textureLoc, out _, out texPos, () => bmp);
                            bmp.Dispose();
                        }
                    }
                    return texPos;
                }

                for (int i = 0; i < inv.Count; i++)
                {
                    if (inv[i].Empty) continue;

                    ItemStack content = inv[i].Itemstack;
                    if (content.Class == EnumItemClass.Item && content.Item.Textures.TryGetValue(textureCode, out CompositeTexture tex))
                    {
                        textureLoc = tex.Base;
                        BitmapRef bmp = capi.Assets.TryGet(textureLoc.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"))?.ToBitmap(capi);
                        if (bmp != null)
                        {
                            capi.BlockTextureAtlas.GetOrInsertTexture(textureLoc, out _, out TextureAtlasPosition texPos, () => bmp);
                            bmp.Dispose();
                            return texPos;
                        }
                    }
                    else if (content.Class == EnumItemClass.Block && content.Block.Textures.TryGetValue(textureCode, out tex))
                    {
                        textureLoc = tex.Base;
                        BitmapRef bmp = capi.Assets.TryGet(textureLoc.Clone().WithPathPrefixOnce("textures/").WithPathAppendixOnce(".png"))?.ToBitmap(capi);
                        if (bmp != null)
                        {
                            capi.BlockTextureAtlas.GetOrInsertTexture(textureLoc, out _, out TextureAtlasPosition texPos, () => bmp);
                            bmp.Dispose();
                            return texPos;
                        }
                    }
                }

                return contentTexSource?[textureCode];
            }
        }

        public ItemStack[] GetContents()
        {
            return inv.Where(slot => !slot.Empty).Select(slot => slot.Itemstack).ToArray();
        }

        public List<ItemStack> GetAllContents()
        {
            var ret = new List<ItemStack>(inv.Count);

            for (int i = 0; i < inv.Count; i++)
            {
                ItemStack stack = inv[i].Itemstack;
                ret.Add(stack?.Clone());
            }

            return ret;
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            capi = api as ICoreClientAPI;

            if (api.Side == EnumAppSide.Client && potMesh == null)
            {
                genMeshes();
                MarkDirty(true);

                roomReg = api.ModLoader.GetModSystem<RoomRegistry>();
            }
        }

        public bool TryPutContents(ItemSlot fromSlot, IPlayer player, BlockSelection blockSel)
        {
            if (fromSlot.Empty) return false;

            // Work out which quadrant of the pot was clicked
            int targetSlot = GetSlotFromClick(blockSel);

            if (targetSlot < 0 || targetSlot >= inv.Count) return false;

            ItemStack stack = fromSlot.Itemstack;
            if (GetProps(stack) == null) return false;

            if (inv[targetSlot].Empty)
            {
                if (fromSlot.TryPutInto(Api.World, inv[targetSlot], 1) > 0)
                {
                    if (Api.Side == EnumAppSide.Server)
                    {
                        Api.World.PlaySoundAt(new AssetLocation("sounds/block/plant"), Pos, 0);
                    }

                    (player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                    fromSlot.MarkDirty();
                    MarkDirty(true);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determine which of the 4 slots should be used based on click position.
        /// </summary>
        private int GetSlotFromClick(BlockSelection blockSel)
        {
            // Normalized hit position (0..1)
            double hx = blockSel.HitPosition.X;
            double hz = blockSel.HitPosition.Z;

            // Decide quadrant (X = left/right, Z = top/bottom)
            bool right = hx >= 0.5;
            bool bottom = hz >= 0.5;

            if (!right && !bottom) return 0; // top-left
            if (right && !bottom) return 1;  // top-right
            if (!right && bottom) return 2;  // bottom-left
            return 3;                        // bottom-right
        }

        public bool TrySetContents(ItemStack stack)
        {
            if (GetProps(stack) == null) return false;

            inv[0].Itemstack = stack;
            MarkDirty(true);
            return true;
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            MeshAngle = tree.GetFloat("meshAngle", MeshAngle);

            if (capi != null)
            {
                genMeshes();
                MarkDirty(true);
            }
        }

        private void genMeshes()
        {
            if (Block.Code == null) return;

            potMesh = GenPotMesh(capi.Tesselator);
            if (potMesh != null)
            {
                potMesh = potMesh.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, MeshAngle, 0);
            }

            contentMeshes = new MeshData[inv.Count];
            for (int i = 0; i < inv.Count; i++)
            {
                ItemStack content = inv[i].Itemstack;
                if (content == null) continue;

                MeshData[] meshes = GenContentMeshes(capi.Tesselator, content);
                if (meshes != null && meshes.Length > 0)
                {
                    MeshData chosen = meshes[GameMath.MurmurHash3Mod(Pos.X + i, Pos.Y, Pos.Z, meshes.Length)];

                    Vec3f offset = slotIndexToOffset(i);
                    chosen = chosen.Clone().Translate(offset.X, offset.Y, offset.Z);

                    if (PlantContProps?.RandomRotate == true)
                    {
                        float radY = GameMath.MurmurHash3Mod(Pos.X + i, Pos.Y, Pos.Z, 16) * 22.5f * GameMath.DEG2RAD;
                        chosen = chosen.Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, radY, 0);
                    }

                    contentMeshes[i] = chosen;
                }
            }
        }

        private Vec3f slotIndexToOffset(int i)
        {
            switch (i)
            {
                case 0: return new Vec3f(-0.15f, 0, -0.15f);
                case 1: return new Vec3f(0.15f, 0, -0.15f);
                case 2: return new Vec3f(-0.15f, 0, 0.15f);
                case 3: return new Vec3f(0.15f, 0, 0.15f);
                default: return Vec3f.Zero;
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetFloat("meshAngle", MeshAngle);
        }

        private MeshData GenPotMesh(ITesselatorAPI tesselator)
        {
            Dictionary<string, MeshData> meshes = ObjectCacheUtil.GetOrCreate(Api, "plantContainerMeshes", () =>
            {
                return new Dictionary<string, MeshData>();
            });


            string key = Block.Code.ToString() + (hasSoil ? "soil" : "empty");

            if (meshes.TryGetValue(key, out MeshData mesh))
            {
                return mesh;
            }

            if (hasSoil && Block.Attributes != null)
            {
                CompositeShape compshape = Block.Attributes["filledShape"].AsObject<CompositeShape>(null, Block.Code.Domain);
                Shape shape = null;
                if (compshape != null)
                {
                    shape = Shape.TryGet(Api, compshape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"));
                }

                if (shape != null)
                {
                    tesselator.TesselateShape(Block, shape, out mesh);
                }
                else
                {
                    Api.World.Logger.Error("Plant container, asset {0} not found,", compshape?.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"));
                    return mesh;
                }
            }
            else
            {
                mesh = capi.TesselatorManager.GetDefaultBlockMesh(Block);
            }

            return meshes[key] = mesh;
        }

        private MeshData[] GenContentMeshes(ITesselatorAPI tesselator, ItemStack content)
        {
            if (content == null) return null;

            Dictionary<string, MeshData[]> meshes = ObjectCacheUtil.GetOrCreate(Api, "plantContainerContentMeshes", () =>
            {
                return new Dictionary<string, MeshData[]>();
            });

            float fillHeight = Block.Attributes == null ? 0.4f : Block.Attributes["fillHeight"].AsFloat(0.4f);

            string containersize = this.ContainerSize;
            string key = content.ToString() + "-" + containersize + "f" + fillHeight;

            if (meshes.TryGetValue(key, out MeshData[] meshwithVariants))
            {
                return meshwithVariants;
            }

            curContProps = GetProps(content);
            if (curContProps == null) return null;

            CompositeShape compoShape = curContProps.Shape;
            if (compoShape == null)
            {
                // Must clone, otherwise it modifies the base item/block shape
                compoShape = content.Class == EnumItemClass.Block ? content.Block.Shape.Clone() : content.Item.Shape.Clone();
            }

            ModelTransform transform = curContProps.Transform;
            if (transform == null)
            {
                transform = new ModelTransform().EnsureDefaultValues();
                transform.Translation.Y = fillHeight;
            }

            contentTexSource = content.Class == EnumItemClass.Block
                ? capi.Tesselator.GetTextureSource(content.Block)
                : capi.Tesselator.GetTextureSource(content.Item);

            List<IAsset> assets;
            if (compoShape.Base.Path.EndsWith('*'))
            {
                assets = Api.Assets.GetManyInCategory("shapes", compoShape.Base.Path.Substring(0, compoShape.Base.Path.Length - 1), compoShape.Base.Domain);
            }
            else
            {
                assets = new List<IAsset>();
                assets.Add(Api.Assets.TryGet(compoShape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json")));
            }

            if (assets != null && assets.Count > 0)
            {
                ShapeElement.locationForLogging = compoShape.Base;
                meshwithVariants = new MeshData[assets.Count];

                for (int i = 0; i < assets.Count; i++)
                {
                    IAsset asset = assets[i];
                    Shape shape = asset.ToObject<Shape>();
                    shapeTextures = shape.Textures;
                    MeshData mesh;

                    try
                    {
                        byte climateColorMapId = content.Block?.ClimateColorMapResolved == null ? (byte)0 : (byte)(content.Block.ClimateColorMapResolved.RectIndex + 1);
                        byte seasonColorMapId = content.Block?.SeasonColorMapResolved == null ? (byte)0 : (byte)(content.Block.SeasonColorMapResolved.RectIndex + 1);

                        tesselator.TesselateShape("plant container content shape", shape, out mesh, this, null, 0, climateColorMapId, seasonColorMapId);
                    }
                    catch (Exception e)
                    {
                        Api.Logger.Error(e.Message + " (when tesselating " + compoShape.Base.WithPathPrefixOnce("shapes/") + ")");
                        Api.Logger.Error(e);
                        meshwithVariants = null;
                        break;
                    }

                    mesh.ModelTransform(transform);
                    meshwithVariants[i] = mesh;
                }
            }
            else
            {
                Api.World.Logger.Error("Plant container, content asset {0} not found,", compoShape.Base.WithPathPrefixOnce("shapes/").WithPathAppendixOnce(".json"));
            }

            return meshes[key] = meshwithVariants;
        }


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (potMesh == null) return false;

            mesher.AddMeshData(potMesh);

            if (contentMeshes != null)
            {
                foreach (var mesh in contentMeshes)
                {
                    if (mesh == null) continue;

                    bool enableWind = Api.World.BlockAccessor.GetDistanceToRainFall(Pos, 6, 2) < 20;
                    if (!enableWind)
                    {
                        var cloned = mesh.Clone();
                        cloned.ClearWindFlags();
                        mesher.AddMeshData(cloned);
                    }
                    else
                    {
                        mesher.AddMeshData(mesh);
                    }
                }
            }

            return true;
        }


        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            bool any = false;
            for (int i = 0; i < inv.Count; i++)
            {
                if (inv[i].Empty) continue;

                if (!any)
                {
                    dsc.AppendLine(Lang.Get("Planted:"));
                    any = true;
                }

                dsc.AppendLine($"- {inv[i].Itemstack.GetName()}");
            }

            if (!any)
            {
                dsc.AppendLine(Lang.Get("Empty slot"));
            }
        }

        public PlantContainerProps GetProps(ItemStack stack)
        {
            if (stack == null) return null;
            return stack.Collectible.Attributes?["plantContainable"]?[ContainerSize + "Container"]?.AsObject<PlantContainerProps>(null, stack.Collectible.Code.Domain);
        }

        public void OnTransformed(IWorldAccessor worldAccessor, ITreeAttribute tree, int degreeRotation, Dictionary<int, AssetLocation> oldBlockIdMapping, Dictionary<int, AssetLocation> oldItemIdMapping, EnumAxis? flipAxis)
        {
            MeshAngle = tree.GetFloat("meshAngle");
            MeshAngle -= degreeRotation * GameMath.DEG2RAD;
            tree.SetFloat("meshAngle", MeshAngle);
        }
    }
}
