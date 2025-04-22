using HarmonyLib;
using MatchLogic;

namespace Rainier.NativeOmukadeConnector.Patches
{
    [HarmonyPatch(typeof(UserDataNumber), nameof(UserDataNumber.GetVariableValue))]
    static class PrizeCountPatch
    {
        static bool Prefix(MatchEntity selfEntity, StateInformation stateInformation, ref UserDataNumber __instance, ref NumberVariableValue __result, ref EntityCategoryPlayerSubset ___player, ref UserDataNumber.UserField ___userField)
        {
            if (___userField != UserDataNumber.UserField.RemainingPrizes)
            {
                return true;
            }
            int counter = 0;
            bool isP1orP2 = EntityLocation.ShouldUseP1(selfEntity, stateInformation, ___player);
            if (isP1orP2)
            {
                lock (stateInformation.matchOperation.workingBoard.p1Prize)
                {
                    foreach (CardEntity prize in stateInformation.matchOperation.workingBoard.p1Prize)
                    {
                        if (prize != null)
                        {
                            counter++;
                        }
                    }
                }
            }
            else
            {
                lock (stateInformation.matchOperation.workingBoard.p2Prize)
                {
                    foreach (CardEntity prize in stateInformation.matchOperation.workingBoard.p2Prize)
                    {
                        if (prize != null)
                        {
                            counter++;
                        }
                    }
                }
            }
            __result =  new UserDataNumberValue(counter, ___userField, false);
            return false;
        }
    }
}
