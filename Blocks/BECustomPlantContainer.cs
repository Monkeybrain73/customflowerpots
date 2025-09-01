using HarmonyLib;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;


namespace CustomFlowerpots.Blocks
{

    [HarmonyPatch(typeof(PlantContainerProps), MethodType.Constructor)]
    public class Patch_PlantContainerProps_Constructor
    {
        static void Postfix(PlantContainerProps __instance)
        {
            __instance.RandomRotate = false; // Force it to always be false
        }
    }


}






