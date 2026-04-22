using BepInEx;
using HarmonyLib;
using System;

namespace CasualtiesUnknownMods
{
    [BepInPlugin("com.cetteon.casu.neuralboosterfix", "NeuralBooster Fix", "1.1.0")]
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
                    NeuralBoosterContext.IsUsingNeuralBooster = true;
                    try
                    {
                        // run original action but suppress eye removal
                        originalAction(body, item);

                        // apply diminishing scaling to benefits
                        int useCount = ++NeuralBoosterContext.UseCount;
                        float scale = 1f + (0.25f / (float)Math.Sqrt(useCount));

                        body.maxSpeed *= scale;
                        body.moveForce *= scale;
                        body.jumpSpeed *= scale;

                        // drawbacks remain unchanged
                    }
                    finally
                    {
                        NeuralBoosterContext.IsUsingNeuralBooster = false;
                    }
                };

            }
        }
    }

    // static context to track usage
    public static class NeuralBoosterContext
    {
        public static bool IsUsingNeuralBooster = false;
        public static int UseCount = 0;
    }

    // patch Body.RemoveEye to skip if called during neuralbooster use
    [HarmonyPatch(typeof(Body), nameof(Body.RemoveEye))]
    class PatchRemoveEye
    {
        static bool Prefix()
        {
            if (NeuralBoosterContext.IsUsingNeuralBooster)
            {
                return false; // suppress eye removal only during neuralbooster use
            }
            return true;
        }
    }
}
