

using HarmonyLib;
using System.Text;
using TPCI.Rainier.Match.Cards;
using UnityEngine;

namespace Rainier.NativeOmukadeConnector.Patches
{
    [HarmonyPatch(typeof(CardAnimTimed), "SetTransform")]
    internal class CardAnimationTransformPatches
    {
        [HarmonyPrefix]
        static bool Prefix(ref CardAnimTimed __instance)
        {
            Behaviour behaviour = (Behaviour)__instance.positioner;
            if (__instance.positioner != null && __instance.positioner is Behaviour && behaviour.isActiveAndEnabled)
            {
                return true;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(CardAnimTimed), "Play")]
    internal class CardAnimationPlayPatches
    {
        [HarmonyPrefix]
        static bool Prefix(ref CardAnimTimed __instance, ICardPositioner positioner)
        {
            Behaviour behaviour = (Behaviour)positioner;
            if (__instance.space == Space.Self && positioner != null && positioner is Behaviour && behaviour.isActiveAndEnabled && positioner.Transform.localScale != null)
            {
                return true;
            }
            else if (__instance.space == Space.World)
            {
                return true;
            }
            if (__instance.startingLayer != -1 && __instance.startingLayer != positioner!.Transform!.gameObject.layer)
            {
                positioner.SetCardLayer(__instance.startingLayer);
            }
            //__instance.Init(positioner);
            AccessTools.Method(typeof(CardTransformAnimation), "Init").Invoke(__instance, new object[] { positioner! });
            return false;
        }
    }
    [HarmonyPatch(typeof(PositionCardInRect), "SetCardScale")]
    internal class CardScalePatches
    {
        [HarmonyPrefix]
        static bool Prefix(CardTransformAnimation anim, ICardPositioner positioner)
        {
            if (positioner is Behaviour && (positioner as Behaviour)!.isActiveAndEnabled)
            {
                return true;
            }
            return false;
        }
    }
}
