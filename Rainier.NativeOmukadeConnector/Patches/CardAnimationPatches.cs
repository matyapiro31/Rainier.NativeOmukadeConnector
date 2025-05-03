

using HarmonyLib;
using System;
using TPCI.Rainier.Match.Cards;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Reflection;

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
        static bool Prefix(MethodBase __originalMethod, ref CardAnimTimed __instance, ref CardMover ___dataProvidingMover, ref TweenerCore<float, float, FloatOptions> ___positionTween,
            ref Vector3 ___startPos, ref Quaternion ___startRot,
            ref Vector3 ___startScale, ref Vector3 ___endPos, ref Quaternion ___endRot, ref Vector3 ___endScale,
            ICardPositioner positioner)
        {
            Behaviour behaviour = (Behaviour)positioner;
            if (__instance.space == Space.World)
            {
                return true;
            }
            if (positioner != null && positioner is Behaviour && behaviour.isActiveAndEnabled)
            {
                positioner.Transform!.GetLocalPositionAndRotation(out Vector3 localPosition, out Quaternion localRotation);
                if (localPosition != null && localRotation != null)
                {
                    positioner.Transform!.localPosition = localPosition;
                    positioner.Transform!.localRotation = localRotation;
                    return true;
                }
                else if (localRotation != null)
                {
                    if (positioner.Transform!.localPosition == null)
                    {
                        positioner.Transform!.localPosition = Vector3.zero;
                    }
                    positioner.Transform!.localRotation = localRotation;
                    return true;
                }
            }
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
