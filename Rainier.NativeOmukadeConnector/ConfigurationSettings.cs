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

using System;
using System.Collections.Generic;
using System.Text;

namespace Rainier.NativeOmukadeConnector
{
    internal class ConfigurationSettings
    {
        public string OmukadeEndpoint = "ws://localhost:10850";
        public bool ForceFriendsToBeOnline = false;
        public bool EnableAllCosmetics = false;
        public bool ForceAllLegalityChecksToSucceed = true;
        public string InvalidCardIdsFile = "BepInEx\\plugins\\Rainier.NativeOmukadeConnector\\invalid-card-ids.txt";
        public bool DumpManifestFileUrl = false;
        public bool AskServerForImplementedCards = false;
        public bool ShowManagerLoadingStatus = false;
        public bool UseProxyApi = false;
        public string ProxyApiEndpoint = "https://localhost:7166";
        public bool OverrideCardDefinition = false;
        public string CardDefinitionDirectory = "BepInEx\\plugins\\Rainier.NativeOmukadeConnector\\CardDefinitions";
        public bool EnableDebugInfo = false;
        public bool SetOwnedCardToMax = false;
    }
}
