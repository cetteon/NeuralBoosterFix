using BepInEx;
using HarmonyLib;

namespace CasualtiesUnknownMods
{
    [BepInPlugin("com.cetteon.casu.neuralboosterfix", "NeuralBooster Fix", "1.0.2")]
    public class NeuralBoosterFix : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony harmony = new Harmony("com.cetteon.casu.neuralboosterfix");
            harmony.PatchAll();
            Logger.LogInfo("NeuralBooster Fix loaded!");
        }
    }

    // patch SetupItems to override the neuralbooster useAction
    [HarmonyPatch(typeof(Item), "SetupItems")]
    class PatchSetupItems
    {
        static void Postfix()
        {
            if (Item.GlobalItems.TryGetValue("neuralbooster", out ItemInfo info))
            {
                var originalAction = info.useAction;

                info.useAction = (Body body, Item item) =>
                {
                    // run the original action but suppress eye removal
                    NeuralBoosterContext.IsUsingNeuralBooster = true;
                    try
                    {
                        originalAction(body, item);
                    }
                    finally
                    {
                        NeuralBoosterContext.IsUsingNeuralBooster = false;
                    }
                };
            }
        }
    }

    // static flag to track when neuralbooster is being used
    public static class NeuralBoosterContext
    {
        public static bool IsUsingNeuralBooster = false;
    }

    // patch Body.RemoveEye to skip if called during neuralbooster use
    [HarmonyPatch(typeof(Body), nameof(Body.RemoveEye))]
    class PatchRemoveEye
    {
        static bool Prefix()
        {
            if (NeuralBoosterContext.IsUsingNeuralBooster)
            {
                // suppress eye removal only during neuralbooster use
                return false;
            }
            return true;
        }
    }
}
