using BepInEx;
using HarmonyLib;
using System;
using System.Threading;

namespace CasualtiesUnknownMods
{
    [BepInPlugin("com.cetteon.casu.neuralboosterfix", "NeuralBooster Fix", "1.1.1")]
    public class NeuralBoosterFix : BaseUnityPlugin
    {
        private readonly Harmony _harmony = new Harmony("com.cetteon.casu.neuralboosterfix");
        private void Awake()
        {
            _harmony.PatchAll();
            Logger.LogInfo("NeuralBooster Fix loaded!");
        }
    }

    // patch SetupItems to override the neuralbooster useAction
    [HarmonyPatch(typeof(Item), "SetupItems")]
    class PatchSetupItems
    {
        static void Postfix()
        {
            if (Item.GlobalItems.TryGetValue("neuralbooster", out var info) && info.useAction != null)
            {
                var originalAction = info.useAction;

                info.useAction = (body, item) =>
                {
                    NeuralBoosterContext.IsUsingNeuralBooster = true;
                    try
                    {
                        originalAction?.Invoke(body, item);

                        body.maxSpeed /= 1.25f;
                        body.moveForce /= 1.25f;
                        body.jumpSpeed /= 1.2f;

                        int useCount = Interlocked.Increment(ref NeuralBoosterContext.UseCount);
                        float movescale = 1f + (0.25f / (float)Math.Sqrt(useCount));
                        float jumpscale = 1f + (0.2f / (float)Math.Sqrt(useCount));

                        body.maxSpeed *= movescale;
                        body.moveForce *= movescale;
                        body.jumpSpeed *= jumpscale;

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
        public static volatile bool IsUsingNeuralBooster;
        public static int UseCount;
    }

    // patch Body.RemoveEye to skip if called during neuralbooster use
    [HarmonyPatch(typeof(Body), nameof(Body.RemoveEye))]
    class PatchRemoveEye
    {
        static bool Prefix() => !NeuralBoosterContext.IsUsingNeuralBooster;
    }
}
