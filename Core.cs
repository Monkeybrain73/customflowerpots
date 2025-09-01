using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

[assembly: ModInfo("Custom Flowerpots",
                    Authors = new string[] { "xXx_Ape_xXx" },
                    Description = "Recycle old anvil molds, boots etc. as flowerpots",
                    Version = "1.3.0")]


namespace customflowerpots
{
    public class Core : ModSystem
    {
        private ICoreAPI api;

        public override void Start(ICoreAPI api)
        {

            base.Start(api);

            api.RegisterBlockClass("CustomPlanter", typeof(BlockCustomPlanter));
            api.RegisterBlockBehaviorClass("CF.BBName", typeof(BlockBehaviorName));

            // api.RegisterBlockClass("BlockSodSkep", typeof(BlockSodSkep));
            // api.RegisterBlockEntityClass("SodBeehive", typeof(BESodBeehive));

            api.World.Logger.Event("started 'Custom Flowerpots' mod");

        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;
            base.StartServerSide(api);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            this.api = api;
            base.StartClientSide(api);
        }

    }

}
