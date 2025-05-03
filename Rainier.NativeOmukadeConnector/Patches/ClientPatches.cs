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

// #define DEBUG_LOG_RX_MESSAGES
#nullable enable

using HarmonyLib;
using Newtonsoft.Json;
using Omukade.Cheyenne.CustomMessages;
using ClientNetworking;
using ClientNetworking.Stomp;
using ClientNetworking.Util;
using Rainier.NativeOmukadeConnector.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using ClientNetworking.Models.GameServer;
using ClientNetworking.Codecs;
using System.Runtime.InteropServices.ComTypes;

namespace Rainier.NativeOmukadeConnector.Patches
{
    [HarmonyPatch(typeof(ClientBuilder))]
    internal static class ClientBuilderPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ClientBuilder.Build), new Type[] { typeof(string) })]
        static void Build(ClientBuilder __instance)
        {
            //__instance.SetWebsocketReceiptsEnabled(false);
            __instance.SetWebsocketMessageLoggingEnabled(true);
        }
    }

    [HarmonyPatch(typeof(Client))]
    internal static class ClientPatches
    {
        internal static event Action<OnlineFriendsResponse>? ReceivedOnlineFriendsResponse;
        internal static MethodInfo forwardMessageForOFR = typeof(Client)
            .GetMethod("ForwardMessage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .MakeGenericMethod(typeof(OnlineFriendsResponse));

        internal static event Action<ImplementedExpandedCardsV1>? ReceivedImplementedExpandedCardsV1;
        internal static MethodInfo forwardMessageForExpandedCardsV1 = typeof(Client)
            .GetMethod("ForwardMessage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .MakeGenericMethod(typeof(ImplementedExpandedCardsV1));

        internal static HashSet<string>? ImplementedExpandedCardsFromServer = null;
        internal static string? ImplementedExpandedCardsFromServerChecksum = null;

        [HarmonyPostfix]
        [HarmonyPatch("CreateMessageHandlerMapping")]
        static void AddHanderForResponses(Client __instance, Dictionary<string, Action<string, ReusableBuffer>> __result)
        {
            //Plugin.SharedLogger.LogInfo($"Adding OFR frame to list of message handlers...");
            __result.Add(nameof(OnlineFriendsResponse), (contentType, buffer) =>
            {
                try
                {
                    forwardMessageForOFR.Invoke(__instance, new object[] { contentType, buffer, new MessageHandler<OnlineFriendsResponse>((c, e) => ReceivedOnlineFriendsResponse?.Invoke(e)) });
                }
                catch(Exception e)
                {
                    BetterExceptionLogger.LogException(e);
                    throw;
                }
            });

            __result.Add(nameof(ImplementedExpandedCardsV1), (contentType, buffer) =>
            {
                try
                {
                    forwardMessageForExpandedCardsV1.Invoke(__instance, new object[] { contentType, buffer, new MessageHandler<ImplementedExpandedCardsV1>((c, e) =>
                    {
                        ImplementedExpandedCardsFromServer = new HashSet<string>(e.ImplementedCardNames);
                        ImplementedExpandedCardsFromServerChecksum = e.Checksum;
                        ReceivedImplementedExpandedCardsV1?.Invoke(e);
                    }) });
                }
                catch (Exception e)
                {
                    BetterExceptionLogger.LogException(e);
                    throw;
                }
            });
        }
        /*
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Client.EnterRegionAsync))]
        static void WarnOfRegionChange()
        {
            StringBuilder sb = new StringBuilder($"{nameof(Client.EnterRegionAsync)} - Reconnecting due to change-of-region, which shouldn't be occuring under Omukade.\n");
            StackTrace st = new System.Diagnostics.StackTrace(true);
            BetterExceptionLogger.PrepareStacktraceString(sb, st);

            Plugin.SharedLogger.LogWarning(sb.ToString());
        }
        */
    }
    [HarmonyPatch(typeof(Client))]
    internal static class ClientPatchesDebug
    {
        [HarmonyPrepare]
        static bool Prepare() => Plugin.Settings.EnableDebugInfo;
        [HarmonyPostfix]
        [HarmonyPatch("Dispatch")]
        static void LogRxMessage(ref StompFrame frame, ReusableBuffer buffer)
        {
            string isJsonString = frame.ContentType == "application/json" ? "JSON" : "BINARY";
            if (buffer.ContentSegment.Array.Length > 2)
            {
                // deserialize the FlatBuffers message.
                string decodedContent = Encoding.UTF8.GetString(buffer.ContentSegment.Array, buffer.ContentSegment.Offset, buffer.ContentSegment.Array.Length).Split('\0')[0];
                
                Plugin.SharedLogger.LogInfo($"\t Websocket dump: \r\n\tContent Type: {isJsonString}\r\n\t MessageType: {frame.Payload}\r\n\t Content: {decodedContent}\r\n");
                //Open a file and write the decodedContent to it.
                string filePath = "BepInEx\\logs\\WebsocketDump.txt";
                using (StreamWriter sw = new StreamWriter(filePath, true))
                {
                    sw.WriteLine($"\t Websocket dump: \r\n\tContent Type: {isJsonString}\r\n\t MessageType: {frame.Payload}\r\n\t Content: {decodedContent}\r\n");
                }
            }
            else
            {
                Plugin.SharedLogger.LogInfo($"\t Websocket dump: Content Type: {isJsonString}\r\n\t  MessageType: {frame.Payload}\r\n");
            }
        }
    }
}
