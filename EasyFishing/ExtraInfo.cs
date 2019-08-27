using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyFishing
{
    using ModMain = EasyFishing;

    internal static class ExtraInfo
    {
        private static Dictionary<int, string> ExtraFishDesc = new Dictionary<int, string>();
        private static Dictionary<string, string[]> Location4SeasonFish = new Dictionary<string, string[]>();

        private static string[] SeasonDisplay = { "spring", "summer", "fall", "winter" };
        private static readonly string[] Weathers = { "sunny", "rainy", "both", "unknown" };
        private static Dictionary<string, string> TransDic = new Dictionary<string, string>()
        {
            { "UndergroundMine", "location.undergroundmine" },
            { "Desert",     "location.desert" },
            { "BusStop",    "location.busstop" },
            { "Forest",     "location.forest" },
            { "Town",       "location.town"},
            { "Mountain",   "location.mountain" },
            { "Backwoods",  "location.backwoods" },
            { "Railroad",   "location.railroad" },
            { "Beach",      "location.beach" },
            { "Woods",      "location.woods" },
            { "BugLand",    "location.bugland" },
            { "WitchSwamp", "location.witchswamp" },
            { "fishingGame", "location.fishinggame" },

            { "freshwater", "location.freshwater" },
            { "ocean",      "location.ocean" },
            { "Sewer",      "location.sewer" },
            { "Various",    "location.various" },

            { "sunny",      "weather.sunny" },
            { "rainy",      "weather.rainy" },
            { "both",       "weather.both" },
            { "unknown",    "weather.unknown" },
        };

        private static readonly string Delim1 = "-", Delim2 = " ", Delim3 = ".";  // delimter between items
        private static readonly string NL = Environment.NewLine;
        private static readonly string NL2 = Environment.NewLine + Environment.NewLine;

        /// <summary>Read fish and location data and set up extra info.</summary>
        internal static void SetupExtraInfo(IModHelper helper)
        {
            IDictionary<int, string> fishDatabase
                = helper.Content.Load<Dictionary<int, string>>("Data/Fish", ContentSource.GameContent);
            Dictionary<string, string> locationDatabase
                = helper.Content.Load<Dictionary<string, string>>("Data/Locations", ContentSource.GameContent);

            foreach (string key in TransDic.Keys.ToArray())
                TransDic[key] = helper.Translation.Get(TransDic[key]);

            foreach (int ii in Enumerable.Range(0, 4))
                SeasonDisplay[ii] = Game1.content.LoadString("Strings\\StringsFromCSFiles:" + SeasonDisplay[ii]);

            // Split each location data into 4 season sections
            foreach (var locationData in locationDatabase)
            {
                if (locationData.Key == "Temp") //|| locationData.Key == "fishingGame")
                    continue;
                string[] fourseasons = new string[4];
                Array.Copy(locationData.Value.Split('/'), 4, fourseasons, 0, 4);
                Location4SeasonFish[locationData.Key] = fourseasons;
            }

            // get fish info from each fish record
            foreach (var fishData in fishDatabase)
            {
                string[] fishItems = fishData.Value.Split('/');

                // sections are 0-12 for all except trapper fish which are 0-6 [see sdv wiki for more info]
                if (fishItems[1] == "trap") //  Trapper fish. 7, 8 items
                {
                    ExtraFishDesc[fishData.Key] = $"{Delim1}{TransDic[fishItems[4]]}{NL}";
                }
                else  //  All other fish. 13, 14 items
                {
                    // ModMain.Logger.Log($"{fishData.Key} = {fishData.Value}");

                    string weather = FormatWeather(fishItems[7]);
                    string activetimes = FormatActiveTimes(fishItems[5]);
                    string fishingspots = FishingSpotInfo(fishData.Key, fishItems[0]);
                    ExtraFishDesc[fishData.Key] = $"{weather} {activetimes}{NL}{fishingspots}";
                }
            }
            Location4SeasonFish = null;
            TransDic = null;
            // Delim2 = Delim3; // to silence warning
        }

        /// <summary>Get fishing info by locations</summary>
        private static string FishingSpotInfo(int fishId, string name)
        {
            string idstr = fishId.ToString();
            string locInfo = "";

            if (name == "Green Algae")     // special case of Green Algae
                return $"{Delim1}{TransDic["Various"]}{NL}";

            // for each location, which season can we catch the fish?
            foreach (var locEntry in Location4SeasonFish)
            {
                List<string> season = new List<string>();

                foreach (int iseason in Enumerable.Range(0, 4))
                    if (locEntry.Value[iseason].Contains(idstr))
                        season.Add(SeasonDisplay[iseason]);

                if (season.Count == 0)   // not found
                    continue;

                string head = TransDic[locEntry.Key];
                string tail = "";
                if (season.Count < 4)
                    tail = $"({String.Join(Delim2, season)})";

                locInfo += $"{Delim1}{head} {tail}{NL}";
            }
            return locInfo;
        }

        [Obsolete("Unused")]
        /// <summary>Get fishing info by season</summary>: UNUSED
        private static string GetLocationBySeason(int fishId, string name)
        {
            string idstr = fishId.ToString();
            string SeasonInfo = "";

            // for each season, where can we catch the fish?
            foreach (int iseason in Enumerable.Range(0, 4))
            {
                List<string> location = new List<string>();

                foreach (var locEntry in Location4SeasonFish)
                    if (locEntry.Value[iseason].Contains(idstr))
                        location.Add(TransDic[locEntry.Key]);

                if (location.Count == 0)    // not found
                    continue;

                string head = SeasonDisplay[iseason];
                string tail = String.Join(Delim2, location);

                SeasonInfo += $"{Delim1}{head} {tail}{NL}";
            }
            return SeasonInfo;
        }

        /// <summary>Format weather like "sunny" from Fish.xnb</summary>
        private static string FormatWeather(string weather)
        {
            return TransDic[Array.Exists(Weathers, w => (w == weather)) ? weather : "unknown"];
        }

        /// <summary>Format active time pairs from Fish.xnb</summary>
        /// <returns>list of times like "XX:XX-YY:YY"</returns>
        private static string FormatActiveTimes(string times)
        {
            string[] active = times.Split(' ');
            Debug.Assert(active.Length == 2 || active.Length == 4);

            string formatted = $"{FormatTime(active[0])}-{FormatTime(active[1])}";
            if (active.Length == 4)
                formatted += $"{NL}     & {FormatTime(active[2])}-{FormatTime(active[3])}";

            return formatted;
        }

        /// <summary>Format time like "623", "1262", "2742" to "06:23", "12:02", "03:42".</summary>
        /// <returns>Formatted string in "XX:XX" form</returns>
        private static string FormatTime(string stime)
        {
            Debug.Assert(stime.Length == 3 || stime.Length == 4);
            int itime = Convert.ToInt32(stime);
            Debug.Assert(itime < 3000 && itime > 100);

            return String.Format("{0:00}:{1:00}", ((itime / 100) % 24), ((itime % 100) % 60));
        }

        /// <summary>Draw hover text on main game menu > collections tab > fish collections page is up.</summary>
        internal static void DrawFishHover(RenderedActiveMenuEventArgs args)
        {
            if (!(Game1.activeClickableMenu is GameMenu MainMenu)
                || MainMenu.currentTab != GameMenu.collectionsTab)
                return;

            CollectionsPage colTab = (CollectionsPage)ModMain.Reflection.GetField<List<IClickableMenu>>
                (MainMenu, "pages").GetValue()[GameMenu.collectionsTab];

            int currentTab = ModMain.Reflection.GetField<int>(colTab, "currentTab").GetValue();
            if (currentTab != CollectionsPage.fishTab)
                return;

            var fishTab = colTab.collections[CollectionsPage.fishTab][0]; // no 2nd page
            var item = fishTab.FirstOrDefault(comp => comp.containsPoint(Game1.getMouseX(), Game1.getMouseY()));

            if (item == null || item.name == null
                || (!item.name.Contains("True") && !ModMain.Config.UnknownFishCheat))
                return;

            int objId = Convert.ToInt32(item.name.Split(' ')[0]);
            if (!ExtraFishDesc.ContainsKey(objId))
                return;

            int value;
            string hoverText = CreateDescription(objId, out value);
            // value = ModMain.Reflection.GetField<int>(colTab, "value").GetValue();

            IClickableMenu.drawHoverText(args.SpriteBatch, hoverText, Game1.smallFont, 0, 0, value);

            return;
        }

        private static string CreateDescription(int index, out int price)
        {
            string[] objInfo = (Game1.objectInformation[index].Split('/'));
            string desc = objInfo[4] + NL2 + Game1.parseText(objInfo[5], Game1.smallFont, Game1.tileSize * 5)
                + NL2 + ExtraFishDesc[index];

            price = Convert.ToInt32(objInfo[1]);

            int[] recs;
            if (Game1.player.fishCaught.TryGetValue(index, out recs))
            {
                desc += NL + Game1.content.LoadString("Strings\\UI:Collections_Description_FishCaught", recs[0]);  // count
                if (recs[0] > 0 && recs[1] > 0)
                {
                    int size = recs[1];  // max size
                    string record;
                    if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en
                        && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ko)
                        record = Game1.content.LoadString("Strings\\UI:Collections_Description_BiggestCatch", Math.Round(size * 2.54));
                    else if (ModMain.Config.MetricSize)
                        record = Game1.content.LoadString("Strings\\UI:Collections_Description_BiggestCatch", Math.Round(size * 2.54))
                            .Replace((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko ? "ÀÎÄ¡" : "in."), "cm");
                    else
                        record = Game1.content.LoadString("Strings\\UI:Collections_Description_BiggestCatch", size);
                    desc += NL + record;

                }
            }
            return desc;
        }
    }
}
