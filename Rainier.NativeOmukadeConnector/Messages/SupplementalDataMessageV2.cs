﻿#nullable disable

using Newtonsoft.Json;
using ClientNetworking.Models.Inventory;
using ClientNetworking.Models.User;
using SharedLogicUtils.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

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

namespace Rainier.NativeOmukadeConnector.Messages
{
    public class SupplementalDataMessageV2
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PlayerId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CurrentRegion { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public CollectionData DeckInformation { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Outfit OutfitInformation { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PlayerDisplayName { get; set; }
    }
}
