using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

[assembly: ModInfo("Custom Flowerpots",
                    Authors = new string[] { "xXx_Ape_xXx" },
                    Description = "Recycle old anvil molds, boots etc. as flowerpots",
                    Version = "1.3.2")]


namespace customflowerpots
{
    public class Core : ModSystem
    {

        public override void Start(ICoreAPI api)
        {

            base.Start(api);

            api.RegisterBlockClass("CustomPlanter", typeof(BlockCustomPlanter));
            api.RegisterBlockClass("CustomPlanter2x", typeof(BlockPlantContainer2x));
            api.RegisterBlockClass("CustomPlanter4x", typeof(BlockPlantContainer4x));
            api.RegisterBlockClass("CustomPlanter6x", typeof(BlockPlantContainer6x));

            api.RegisterBlockEntityClass("CustomPlanter2x", typeof(BlockEntityCustomPlantContainer2x));
            api.RegisterBlockEntityClass("CustomPlanter4x", typeof(BlockEntityCustomPlantContainer4x));
            api.RegisterBlockEntityClass("CustomPlanter6x", typeof(BlockEntityCustomPlantContainer6x));

            api.RegisterBlockBehaviorClass("CF.BBName", typeof(BlockBehaviorName));

            // api.RegisterBlockClass("BlockSodSkep", typeof(BlockSodSkep));
            // api.RegisterBlockEntityClass("SodBeehive", typeof(BESodBeehive));

            api.World.Logger.Event("started 'Custom Flowerpots' mod");

        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
        }

    }

}
