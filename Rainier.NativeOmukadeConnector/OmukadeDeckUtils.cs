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

using MatchLogic;
using Newtonsoft.Json;
using Rainier.NativeOmukadeConnector;
using SharedSDKUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rainier.NativeOmukadeConnector
{
    public class OmukadeDeckUtils
    {
        /// <summary>
        /// Deserialize JSON string to object with new json data from settings.
        /// </summary>
        /// <param name="oldjson">Data fetched by patched method.</param>
        /// <param name="settings">Json serializer settings.</param>
        /// <param name="filteredCardIDs">List of card IDs filtered.</param>
        /// <returns></returns>
        public static List<CardSource>? LocalJsonDeserializer(string oldjson, JsonSerializerSettings? settings, List<string>? filteredCardIDs)
        {
            // Load Card Definitions
            var cardDefinitions = filteredCardIDs.Select(filteredDeckCardID => File.ReadAllText(Path.Combine(Plugin.Settings.CardDefinitionDirectory, filteredDeckCardID + ".json")));
            var oldCardSources = JsonConvert.DeserializeObject<List<CardSource>>(oldjson, settings);
            foreach (var cardDefinition in cardDefinitions)
            {
                var cardSource = JsonConvert.DeserializeObject<CardSource>(cardDefinition, settings);
                if (cardSource != null)
                {
                    oldCardSources!.Add(cardSource);
                }
            }
            return oldCardSources;
        }

        public static bool ValidateDeck(DeckInfo deckInfo)
        {
            // Load Invalid Card IDs
            var invalidCardIds = FileIOUtils.LoadFile(Plugin.Settings.InvalidCardIdsFile);
            bool isValid = true;
            foreach (var card in deckInfo.cards)
            {
                if (invalidCardIds.Contains(card.Key))
                {
                    Plugin.SharedLogger.LogWarning($"Deck contains invalid card: {card.Key}");
                    isValid = false;
                }
            }
            return isValid;
        }
        public static void FilterOverrideCardIDs(ref List<string> cardIDs, List<string> overrideCardIDs, out List<string> originalCardIDs, out List<string> filteredDeckCardIDs)
        {
            originalCardIDs = new List<string>(cardIDs);
            filteredDeckCardIDs = new List<string>();
            foreach (string overrideCardID in overrideCardIDs)
            {
                foreach (string cardID in cardIDs)
                {
                    if (cardID == overrideCardID)
                    {
                        cardIDs.Remove(cardID);
                        filteredDeckCardIDs.Add(cardID);
                        break;
                    }
                }
            }
        }
        public static void ResetCardIDs(ref List<string> cardIDs)
        {
            cardIDs = new List<string>(originalCardIDs);
            originalCardIDs!.Clear();
            filteredDeckCardIDs!.Clear();
        }
        public static List<string>? overrideCardIDs;
        public static List<string>? cardDefinitionOverrides;
        public static List<string>? originalCardIDs;
        public static List<string>? filteredDeckCardIDs;
    }
}