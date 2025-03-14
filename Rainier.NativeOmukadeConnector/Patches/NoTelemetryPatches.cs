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

#nullable disable
 
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using TPCI.Commands;
using TPCI.NetworkSystem;
using TPCI.NewRelic;
using TPCI.Telemetry;

namespace Rainier.NativeOmukadeConnector.Patches
{
    [HarmonyPatch(typeof(Endpoint_ProdPipeline))]
    static class NoTelemetryPatches
    {
        [HarmonyPatch(nameof(Endpoint_ProdPipeline.SendEvent))]
        [HarmonyPrefix]
        static bool SendEventNowSendsNoTelemetry(string eventName)
        {
            // Telemetry be gone!
            UnityEngine.Debug.Log($"[NoTelemetry] Swallowed telemetry event {eventName}");
            return false;
        }
    }

    [HarmonyPatch(typeof(TelemetryManager))]
    static class NoTpciTelemetry
    {
        [HarmonyPatch("WriteLog")]
        [HarmonyPatch("sendEventWithPrewrittenMetadata")]
        [HarmonyPatch(nameof(TelemetryManager.Send))]
        [HarmonyPatch(nameof(TelemetryManager.AddToQueue))]
        [HarmonyPrefix]
        static bool SilenceTpciTelemetry() => false;
    }
}
