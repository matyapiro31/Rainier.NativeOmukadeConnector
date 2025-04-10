/*************************************************************************
* Rainier Native Omukade Connector
* (c) 2022 Hastwell/Electrosheep Networks 
* 
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
* 
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
**************************************************************************/

using HarmonyLib;
using MatchLogic;
using MonoMod.Utils;
using Newtonsoft.Json;
using RainierClientSDK.Query;
using RainierClientSDK.source.CardInfo;
using RainierClientSDK.source.Friend.Implementations;
using SharedLogicUtils.DataTypes;
using SharedLogicUtils.source.Services.Query.Contexts;
using SharedLogicUtils.source.Services.Query.Responses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainier.NativeOmukadeConnector.Patches
{
    [HarmonyPatch]
    static class LocalCardSourceInjector
    {
        [HarmonyPrepare]
        static bool Prepare() => Plugin.Settings.OverrideCardDefinition;
        static MethodBase TargetMethod()
        {
            var parent = AccessTools.FirstInner(typeof(CardInfoQueries), t => t.FullName.StartsWith($"{typeof(CardInfoQueries)}+<GetCardDataAsync>d__") && AccessTools.GetDeclaredMethods(t).Any(m => m.Name == "MoveNext"));
            return AccessTools.Method(parent, "MoveNext");
        }
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Get the generic method for deserializing JSON
            MethodInfo jsondeserializerGeneric = typeof(JsonConvert).GetMethods()
                .Where(t => t.Name== "DeserializeObject")
                .Where(t => t.IsGenericMethod)
                .Where(t => t.GetParameters().Length == 2
                    && Array.Exists<ParameterInfo>(t.GetParameters(), p => p.ParameterType == typeof(JsonSerializerSettings)))
                .ToList().First();
            // Make the generic method for List<CardSource>
            MethodInfo jsondeserializer = jsondeserializerGeneric.MakeGenericMethod(typeof(List<CardSource>));
            MethodInfo jsondeserializerconcat = AccessTools.Method(typeof(OmukadeDeckUtils), nameof(OmukadeDeckUtils.LocalJsonDeserializer));;
            
            // Get the field info for the card IDs
            FieldInfo overrideCardIDs = AccessTools.Field(typeof(OmukadeDeckUtils), nameof(OmukadeDeckUtils.overrideCardIDs));
            // Get the field info for storing the original card IDs
            FieldInfo originalCardIDs = AccessTools.Field(typeof(OmukadeDeckUtils), nameof(OmukadeDeckUtils.originalCardIDs));
            FieldInfo filteredDeckCardIDs = AccessTools.Field(typeof(OmukadeDeckUtils), nameof(OmukadeDeckUtils.filteredDeckCardIDs));
            // Get the method info for FilterOverrideCardIDs
            MethodInfo filterOverrideCardIDs = AccessTools.Method(typeof(OmukadeDeckUtils), nameof(OmukadeDeckUtils.FilterOverrideCardIDs));
            MethodInfo resetCardIDs = AccessTools.Method(typeof(OmukadeDeckUtils), nameof(OmukadeDeckUtils.ResetCardIDs));
            // Get the field info for the card IDs
            CodeInstruction? ldfldCardIDs = instructions.Where(i => (i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "cardIDs")).FirstOrDefault();
            FieldInfo? cardIDs = ldfldCardIDs.operand as FieldInfo;
            foreach (var instruction in instructions)
            {
                // Filter card IDs to old and overrides.
                if (instruction.opcode == OpCodes.Ldloc_1)
                {
                    // Insert the parameter for filterOverrideCardIDs.
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    // Set the parameter as ref parameter.
                    yield return new CodeInstruction(OpCodes.Ldflda, cardIDs);
                    yield return new CodeInstruction(OpCodes.Ldsfld, overrideCardIDs);
                    yield return new CodeInstruction(OpCodes.Ldsflda, originalCardIDs);
                    yield return new CodeInstruction(OpCodes.Ldsflda, filteredDeckCardIDs);
                    yield return new CodeInstruction(OpCodes.Call, filterOverrideCardIDs);
                    yield return instruction;
                }
                else if (instruction.Calls(jsondeserializer))
                {
                    // Add new parameter to the deserializer method.
                    yield return new CodeInstruction(OpCodes.Ldsfld, filteredDeckCardIDs);
                    yield return new CodeInstruction(OpCodes.Call, jsondeserializerconcat);
                    // Set originalCardIDs to cardIDs.
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, cardIDs);
                    yield return new CodeInstruction(OpCodes.Call, resetCardIDs);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
