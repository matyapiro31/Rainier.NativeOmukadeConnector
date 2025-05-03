#nullable disable

using HarmonyLib;
using Newtonsoft.Json;
using SharedSDKUtils.DeckValidation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Rainier.NativeOmukadeConnector.Patches
{
    [HarmonyPatch(typeof(ModifiedDeckPanel))]
    static class DeckPanelPatches
    {
        /*[HarmonyPatch("PopulateForCategory")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DeckPanelIgnoresUnownedCards(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo totalOwnedQuantityCall = typeof(ArchetypeDBCards).GetMethod(nameof(ArchetypeDBCards.TotalOwnedQuantity));

            foreach(var instruction in instructions)
            {
                if(instruction.Calls(totalOwnedQuantityCall))
                {
                    /// Replace the call to <see cref="ArchetypeDBCards.TotalOwnedQuantity"/>
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, 999);
                }
                else
                {
                    yield return instruction;
                }
            }
        }*/
    }

    [HarmonyPatch(typeof(ArchetypeDBCards))]
    static class ArchetypeDBCardsPatches
    {
        [HarmonyPrepare]
        static bool Prepare() => Plugin.Settings.SetOwnedCardToMax;

        [HarmonyPatch(nameof(ArchetypeDBCards.QuantityForCard))]
        [HarmonyPrefix]
        static bool QuantityForCard(ref int __result)
        {
            __result = 60;
            return false;
        }

        [HarmonyPatch(nameof(ArchetypeDBCards.TotalOwnedQuantity))]
        [HarmonyPrefix]
        static bool TotalOwnedQuantity(ref int __result)
        {
            __result = 60;
            return false;
        }

        [HarmonyPatch(nameof(ArchetypeDBCards.TotalOwnedQuantityForSpecificCard))]
        [HarmonyPrefix]
        static bool TotalOwnedQuantityForSpecificCard(ref int __result)
        {
            __result = 60;
            return false;
        }
    }
    [HarmonyPatch(typeof(HUBEditDeckController), nameof(HUBEditDeckController.CanAddCard))]
    static class AddCardPatchHUBEditDeckController
    {
        static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(TPCI.DeckValidation.DefaultDeckValidationController), nameof(TPCI.DeckValidation.DefaultDeckValidationController.CanAddCard))]
    static class AddCardPatchDefaultDeckValidationController
    {
        static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}
