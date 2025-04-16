/*
MIT License

Copyright (c) 2025 Hastwell, stm876

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RainierClientSDK;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Rainier.NativeOmukadeConnector.Patches
{
    [HarmonyPatch(typeof(OfflineAdapter), "CreateGame")]
    static class CreateGame
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // This codes is dependent on the .NET compiler.
            var tmp = instructions.Where(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name.EndsWith("<>t__builder")).FirstOrDefault();
            FieldInfo builder = (tmp.operand as FieldInfo)!;
            tmp = instructions.Where(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name.EndsWith("<>1__state")).FirstOrDefault();
            FieldInfo state = (tmp.operand as FieldInfo)!;
            Type compilerGeneratedType = (tmp.operand as FieldInfo)!.DeclaringType;
            MethodInfo AsyncTaskMethodBuilderCreate = AccessTools.Method(typeof(AsyncTaskMethodBuilder), nameof(AsyncTaskMethodBuilder.Create));
            yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
            yield return new CodeInstruction(OpCodes.Call, AsyncTaskMethodBuilderCreate);
            yield return new CodeInstruction(OpCodes.Stfld, builder);
            yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Stfld, compilerGeneratedType.GetMembers(AccessTools.all).Where(m => m.Name == "state").First());
            yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Stfld, compilerGeneratedType.GetMembers(AccessTools.all).Where(m => m.Name == "messageID").First());
            yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Stfld, compilerGeneratedType.GetMembers(AccessTools.all).Where(m => m.Name == "gameMode").First());
            yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Stfld, compilerGeneratedType.GetMembers(AccessTools.all).Where(m => m.Name == "matchMode").First());
            yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
            yield return new CodeInstruction(OpCodes.Ldc_I4_M1);
            yield return new CodeInstruction(OpCodes.Stfld, state);
            yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
            yield return new CodeInstruction(OpCodes.Ldflda, builder);
            yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AsyncTaskMethodBuilder), nameof(AsyncTaskMethodBuilder.Start)).MakeGenericMethod([compilerGeneratedType]));
            yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
            yield return new CodeInstruction(OpCodes.Ldflda, builder);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(AsyncTaskMethodBuilder), nameof(AsyncTaskMethodBuilder.Task)));
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
    static class CreateCardDataCacheMessage
    {
        [HarmonyPrepare]
        static bool Prepare() => Plugin.Settings.OverrideCardDefinition;
        static IEnumerable<MethodBase> TargetMethods() => typeof(OfflineAdapter)
            .GetNestedTypes(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(nt => nt.Name.StartsWith($"<CreateCardDataCacheMessage>"))
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            .Where(m => m.Name == "MoveNext")
            .Take(1);
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Beq)
                {
                    yield return new CodeInstruction(OpCodes.Bge, instruction.operand);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
/** Original IL code.
0	0000	ldloca.s	V_0 (0)
1	0002	call	valuetype [netstandard]System.Runtime.CompilerServices.AsyncTaskMethodBuilder [netstandard]System.Runtime.CompilerServices.AsyncTaskMethodBuilder::Create()
2	0007	stfld	valuetype [netstandard]System.Runtime.CompilerServices.AsyncTaskMethodBuilder RainierClientSDK.OfflineAdapter/'<CreateGame>d__18'::'<>t__builder'
3	000C	ldloca.s	V_0 (0)
4	000E	ldarg.0
5	000F	stfld	class [SharedLogicUtils]SharedSDKUtils.GameState RainierClientSDK.OfflineAdapter/'<CreateGame>d__18'::state
6	0014	ldloca.s	V_0 (0)
7	0016	ldarg.1
8	0017	stfld	string RainierClientSDK.OfflineAdapter/'<CreateGame>d__18'::messageID
9	001C	ldloca.s	V_0 (0)
10	001E	ldarg.2
11	001F	stfld	valuetype [MatchLogic]MatchLogic.GameMode RainierClientSDK.OfflineAdapter/'<CreateGame>d__18'::gameMode
12	0024	ldloca.s	V_0 (0)
13	0026	ldarg.3
14	0027	stfld	valuetype [MatchLogic]MatchLogic.MatchMode RainierClientSDK.OfflineAdapter/'<CreateGame>d__18'::matchMode
15	002C	ldloca.s	V_0 (0)
16	002E	ldc.i4.m1
17	002F	stfld	int32 RainierClientSDK.OfflineAdapter/'<CreateGame>d__18'::'<>1__state'
18	0034	ldloca.s	V_0 (0)
19	0036	ldflda	valuetype [netstandard]System.Runtime.CompilerServices.AsyncTaskMethodBuilder RainierClientSDK.OfflineAdapter/'<CreateGame>d__18'::'<>t__builder'
20	003B	ldloca.s	V_0 (0)
21	003D	call	instance void [netstandard]System.Runtime.CompilerServices.AsyncTaskMethodBuilder::Start<valuetype RainierClientSDK.OfflineAdapter/'<CreateGame>d__18'>(!!0&)
22	0042	ldloca.s	V_0 (0)
23	0044	ldflda	valuetype [netstandard]System.Runtime.CompilerServices.AsyncTaskMethodBuilder RainierClientSDK.OfflineAdapter/'<CreateGame>d__18'::'<>t__builder'
24	0049	call	instance class [netstandard]System.Threading.Tasks.Task [netstandard]System.Runtime.CompilerServices.AsyncTaskMethodBuilder::get_Task()
25	004E	ret
*/