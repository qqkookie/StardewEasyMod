using System;
using System.Collections.Generic;
using System.Linq;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyPlay
{
    using ModMain = EasyPlay;

    public static class DontAskToEat
    {
        private static ModConfig Config => ModMain.Config;

        internal static  bool DisableDontAskToEat;   // disable eat blocking
        internal static List<string> Foods;          // Edible foods name list
        internal static SButton EatPrefix;           // key to allow to eat food on hand
        internal static bool AllowEat = false;

        internal static void Setup()
        {
            Enum.TryParse(Config.EatPrefixKey, true, out EatPrefix);

            DisableDontAskToEat = Config.DisableDontAskToEat;

            Foods = Config.Foods.Select(s => s.ToLower()).ToList();

            ModMain.Events.Display.MenuChanged += OnMenuChanged;
            ModMain.Events.Input.ButtonPressed += OnButtonPressed;

            /*
            // To generate full list of ediable candiates
            List<string> edibles = new List<string>();
            foreach (var item in Game1.objectInformation)
            {
                string[] parts = item.Value.Split('/');
                if (Int32.Parse(parts[1]) > 0 && Int32.Parse(parts[2]) > -300)
                    edibles.Add(parts[4]);
            }
            */
        }

        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady || DisableDontAskToEat || Game1.currentLocation.lastQuestionKey != "Eat")
                return;

            if (e.NewMenu is DialogueBox dlg)
            {
                StardewValley.Object activeobj = Game1.player.ActiveObject;
                if (activeobj != null && activeobj.Price > 0 && activeobj.Edibility > -300
                    && !Foods.Contains(activeobj.DisplayName.ToLower()) && !Foods.Contains(activeobj.Name.ToLower()))
                {
                    if (!AllowEat)
                    {
                        // close Eat? Y/N quesion dialogue box.
                        ModMain.Reflection.GetField<bool>(dlg, "transitioning", true).SetValue(false);
                        ModMain.Reflection.GetField<int>(dlg, "safetyTimer", true).SetValue(0);
                        ModMain.Reflection.GetField<int>(dlg, "selectedResponse", true).SetValue(1);
                        dlg.receiveLeftClick(0, 0, false);
                    }
                    else
                        AllowEat = false;
                }
            }
            else if (e.OldMenu is DialogueBox && Game1.player.isEating )
            {
                // If player chose Yes to eat, add it to the foods list.

                Item item = Game1.player.itemToEat;
                if ( item != null && !Foods.Contains(item.DisplayName.ToLower()) && !Foods.Contains(item.Name.ToLower()))
                {
                    Foods.Add(item.DisplayName.ToLower());

                    Config.Foods.Add(item.DisplayName);
                    ModMain.ModHelper.WriteConfig(Config);
                }
            }
        }

        //  To eat something not in the foods list, select the edible and press prefix key
        //  followed by action button (click right). Then select yes on the eat confirm dialogue.
        //  Then the new edible will be added to the current foods list.
        internal static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.IsPlayerFree && e.Button == EatPrefix
                && (AllowEat || Game1.player.ActiveObject != null))
            {
                AllowEat = !AllowEat;
                Game1.playSound("dwoop");
            }
        }
    }
}


/*
// Full list of edible food candidates, FYI

// "English name", "localized name", // obj index: edibility, price

"Wild Horseradish", "야생 고추냉이",            // 16: 5, 50
"Daffodil", "수선화",            // 18: 0, 30
"Leek", "리크",            // 20: 16, 60
"Dandelion", "민들레",            // 22: 10, 40
"Parsnip", "파스닙",            // 24: 10, 35
"Lumber", "목재",            // 30: 10, 2
"Cave Carrot", "동굴 당근",            // 78: 12, 25
"Cactus Fruit", "선인장 열매",            // 90: 30, 75
"Sap", "수액",            // 92: -1, 2
"Pufferfish", "복어",            // 128: -40, 200
"Anchovy", "멸치",            // 129: 5, 30
"Tuna", "참치",            // 130: 15, 100
"Sardine", "정어리",            // 131: 5, 40
"Bream", "도미",            // 132: 5, 45
"Largemouth Bass", "큰입우럭",            // 136: 15, 100
"Smallmouth Bass", "작은입우럭",            // 137: 10, 50
"Rainbow Trout", "무지개송어",            // 138: 10, 65
"Salmon", "연어",            // 139: 15, 75
"Walleye", "월아이",            // 140: 12, 105
"Perch", "농어",            // 141: 10, 55
"Carp", "잉어",            // 142: 5, 30
"Catfish", "메기",            // 143: 20, 200
"Pike", "강꼬치고기",            // 144: 15, 100
"Sunfish", "개복치",            // 145: 5, 30
"Red Mullet", "숭어",            // 146: 10, 75
"Herring", "청어",            // 147: 5, 30
"Eel", "장어",            // 148: 12, 85
"Red Snapper", "붉은 퉁돔",            // 150: 10, 50
"Squid", "오징어",            // 151: 10, 80
"Seaweed", "해초",            // 152: 5, 20
"Green Algae", "녹조류",            // 153: 5, 15
"Sea Cucumber", "해삼",            // 154: -10, 75
"Super Cucumber", "슈퍼해삼",            // 155: 50, 250
"Ghostfish", "귀신물고기",            // 156: 15, 45
"White Algae", "흰 조류",            // 157: 8, 25
"Crimsonfish", "크림슨피쉬",            // 159: 15, 1500
"Angler", "아귀",            // 160: 10, 900
"Ice Pip", "아이스핍",            // 161: 15, 500
"Lava Eel", "용암 장어",            // 162: 20, 700
"Legend", "전설의 물고기",            // 163: 200, 5000
"Sandfish", "도루묵",            // 164: 5, 75
"Scorpion Carp", "전갈 잉어",            // 165: -50, 150
"Joja Cola", "조자 콜라",            // 167: 5, 25
"Egg", "달걀",            // 176: 10, 50
"Large Egg", "큰 달걀",            // 174: 15, 95
"Egg", "달걀",            // 180: 10, 50
"Large Egg", "큰 달걀",            // 182: 15, 95
"Milk", "우유",            // 184: 15, 125
"Large Milk", "큰 우유",            // 186: 20, 190
"Green Bean", "완두콩",            // 188: 10, 40
"Cauliflower", "콜리플라워",            // 190: 30, 175
"Potato", "감자",            // 192: 10, 80
"Fried Egg", "계란 프라이",            // 194: 20, 35
"Omelet", "오믈렛",            // 195: 40, 125
"Salad", "샐러드",            // 196: 45, 110
"Cheese Cauliflower", "치즈 콜리플라워",            // 197: 55, 300
"Baked Fish", "생선구이",            // 198: 30, 100
"Parsnip Soup", "설탕당근 스프",            // 199: 34, 120
"Vegetable Medley", "야채의 메들리",            // 200: 66, 120
"Complete Breakfast", "완벽한 아침",            // 201: 80, 350
"Fried Calamari", "오징어튀김",            // 202: 32, 150
"Strange Bun", "수상한 롤",            // 203: 40, 225
"Lucky Lunch", "행운의 점심",            // 204: 40, 250
"Fried Mushroom", "버섯구이",            // 205: 54, 200
"Pizza", "피자",            // 206: 60, 300
"Bean Hotpot", "콩 스튜",            // 207: 50, 100
"Glazed Yams", "맛탕",            // 208: 80, 200
"Carp Surprise", "깜짝잉어",            // 209: 36, 150
"Hashbrowns", "해시브라운",            // 210: 36, 120
"Pancakes", "팬케이크",            // 211: 36, 80
"Salmon Dinner", "연어 정찬",            // 212: 50, 300
"Fish Taco", "생선 타코",            // 213: 66, 500
"Crispy Bass", "우럭 튀김",            // 214: 36, 150
"Pepper Poppers", "페퍼 파퍼",            // 215: 52, 200
"Bread", "빵",            // 216: 20, 60
"Tom Kha Soup", "똠카 스프",            // 218: 70, 250
"Trout Soup", "송어 스프",            // 219: 40, 100
"Chocolate Cake", "초콜릿 케이크",            // 220: 60, 200
"Pink Cake", "핑크 케이크",            // 221: 100, 480
"Rhubarb Pie", "대황 파이",            // 222: 86, 400
"Cookie", "쿠키",            // 223: 36, 140
"Spaghetti", "스파게티",            // 224: 30, 120
"Fried Eel", "장어튀김",            // 225: 30, 120
"Spicy Eel", "매콤한 장어",            // 226: 46, 175
"Sashimi", "회",            // 227: 30, 75
"Maki Roll", "마키 롤",            // 228: 40, 220
"Tortilla", "또띠아",            // 229: 20, 50
"Red Plate", "붉은 정식",            // 230: 96, 400
"Eggplant Parmesan", "가지 파마산",            // 231: 70, 200
"Rice Pudding", "라이스 푸딩",            // 232: 46, 260
"Ice Cream", "아이스크림",            // 233: 40, 120
"Blueberry Tart", "블루베리 타르트",            // 234: 50, 150
"Autumn's Bounty", "가을의 수확",            // 235: 88, 350
"Pumpkin Soup", "호박죽",            // 236: 80, 300
"Super Meal", "슈퍼 건강식",            // 237: 64, 220
"Cranberry Sauce", "크랜베리 소스",            // 238: 50, 120
"Stuffing", "요리용 속",            // 239: 68, 165
"Farmer's Lunch", "농부의 점심",            // 240: 80, 150
"Survival Burger", "생존형 버거",            // 241: 50, 180
"Dish O' The Sea", "바다의 요리",            // 242: 60, 220
"Miner's Treat", "광부의 간식",            // 243: 50, 200
"Roots Platter", "뿌리채소 모음",            // 244: 50, 100
"Sugar", "설탕",            // 245: 10, 50
"Wheat Flour", "밀가루",            // 246: 5, 50
"Oil", "기름",            // 247: 5, 100
"Garlic", "마늘",            // 248: 8, 60
"Kale", "케일",            // 250: 20, 110
"Melon", "멜론",            // 254: 45, 250
"Tomato", "토마토",            // 256: 8, 60
"Morel", "곰보버섯",            // 257: 8, 150
"Blueberry", "블루베리",            // 258: 10, 50
"Fiddlehead Fern", "청나래고사리",            // 259: 10, 90
"Hot Pepper", "매운 고추",            // 260: 5, 40
"Radish", "무",            // 264: 18, 90
"Red Cabbage", "붉은 양배추",            // 266: 30, 260
"Starfruit", "스타프루트",            // 268: 50, 750
"Corn", "옥수수",            // 270: 10, 50
"Eggplant", "가지",            // 272: 8, 60
"Artichoke", "아티초크",            // 274: 12, 160
"Bok Choy", "청경채",            // 278: 10, 80
"Yam", "참마",            // 280: 18, 160
"Chanterelle", "살구버섯",            // 281: 30, 160
"Cranberries", "크랜베리",            // 282: 15, 75
"Holly", "호랑가시나무 열매",            // 283: -15, 80
"Beet", "사탕무",            // 284: 12, 100
"Salmonberry", "새먼베리",            // 296: 10, 5
"Amaranth", "아마란스",            // 300: 20, 150
"Pale Ale", "페일 에일",            // 303: 20, 300
"Hops", "홉",            // 304: 18, 25
"Void Egg", "공허의 달걀",            // 305: 15, 65
"Void Mayonnaise", "공허 마요네즈",            // 308: -30, 275
"Beer", "맥주",            // 346: 20, 200
"Wine", "와인",            // 348: 20, 400
"Energy Tonic", "자양강장제",            // 349: 200, 500
"Juice", "주스",            // 350: 30, 150
"Muscle Remedy", "근육 치료제",            // 351: 20, 500
"Coffee", "커피",            // 395: 1, 150
"Spice Berry", "백량금",            // 396: 10, 80
"Grape", "포도",            // 398: 15, 80
"Spring Onion", "파",            // 399: 5, 8
"Strawberry", "딸기",            // 400: 20, 120
"Sweet Pea", "스위트피",            // 402: 0, 50
"Field Snack", "수제 에너지바",            // 403: 18, 20
"Common Mushroom", "흔한 버섯",            // 404: 15, 40
"Wild Plum", "야생 자두",            // 406: 10, 80
"Hazelnut", "헤이즐넛",            // 408: 12, 90
"Blackberry", "블랙베리",            // 410: 10, 20
"Winter Root", "겨울뿌리",            // 412: 10, 70
"Crystal Fruit", "수정 과일",            // 414: 25, 150
"Snow Yam", "눈마",            // 416: 12, 100
"Crocus", "크로커스",            // 418: 0, 60
"Vinegar", "식초",            // 419: 5, 100
"Red Mushroom", "붉은 버섯",            // 420: -20, 75
"Sunflower", "해바라기",            // 421: 18, 80
"Purple Mushroom", "보라색 버섯",            // 422: 50, 250
"Rice", "쌀",            // 423: 5, 100
"Cheese", "치즈",            // 424: 50, 200
"Goat Cheese", "염소 치즈",            // 426: 50, 375
"Truffle", "송로버섯",            // 430: 5, 625
"Truffle Oil", "송로버섯 오일",            // 432: 15, 1065
"Stardrop", "별방울",            // 434: 100, 7777
"Goat Milk", "염소젖",            // 436: 25, 225
"L. Goat Milk", "큰 염소젖",            // 438: 35, 345
"Duck Egg", "오리알",            // 442: 15, 95
"Algae Soup", "녹조류 스프",            // 456: 30, 100
"Pale Broth", "창백한 죽",            // 457: 50, 150
"Mead", "벌꿀 술",            // 459: 30, 200
"Tulip", "튤립",            // 591: 18, 30
"Summer Spangle", "여름별꽃",            // 593: 18, 90
"Fairy Rose", "요정장미",            // 595: 18, 290
"Blue Jazz", "푸른 재즈",            // 597: 18, 50
"Poppy", "양귀비",            // 376: 18, 140
"Plum Pudding", "자두 푸딩",            // 604: 70, 260
"Artichoke Dip", "아티초크 소스",            // 605: 40, 210
"Stir Fry", "야채 볶음",            // 606: 80, 335
"Roasted Hazelnuts", "구운 헤이즐넛",            // 607: 70, 270
"Pumpkin Pie", "호박 파이",            // 608: 90, 385
"Radish Salad", "무 샐러드",            // 609: 80, 300
"Fruit Salad", "과일 샐러드",            // 610: 105, 450
"Blackberry Cobbler", "블랙베리 코블러",            // 611: 70, 260
"Cranberry Candy", "크랜베리 캔디",            // 612: 50, 175
"Apple", "사과",            // 613: 15, 100
"Bruschetta", "브루쉐타",            // 618: 45, 210
"Coleslaw", "코울슬로",            // 648: 85, 345
"Fiddlehead Risotto", "청나래고사리 리조또",            // 649: 90, 350
"Poppyseed Muffin", "양귀비씨 머핀",            // 651: 60, 250
"Apricot", "살구",            // 634: 15, 50
"Orange", "오렌지",            // 635: 15, 100
"Peach", "복숭아",            // 636: 15, 140
"Pomegranate", "석류",            // 637: 15, 140
"Cherry", "체리",            // 638: 15, 80
"Mutant Carp", "돌연변이 잉어",            // 682: 10, 1000
"Sturgeon", "철갑상어",            // 698: 10, 200
"Tiger Trout", "타이거 송어",            // 699: 10, 150
"Bullhead", "눈동자개",            // 700: 10, 75
"Tilapia", "틸라피아",            // 701: 10, 75
"Chub", "피라미",            // 702: 10, 50
"Dorado", "만새기",            // 704: 10, 100
"Albacore", "날개다랑어",            // 705: 10, 75
"Shad", "전어",            // 706: 10, 60
"Lingcod", "범노래미",            // 707: 10, 120
"Halibut", "넙치",            // 708: 10, 80
"Maple Syrup", "메이플 시럽",            // 724: 20, 200
"Chowder", "차우더",            // 727: 90, 135
"Lobster Bisque", "가재 비스크",            // 730: 90, 205
"Fish Stew", "생선 스튜",            // 728: 90, 175
"Escargot", "에스카르고",            // 729: 90, 125
"Maple Bar", "메이플 바",            // 731: 90, 300
"Crab Cakes", "게살 케이크",            // 732: 90, 275
"Woodskip", "숲고기",            // 734: 10, 75
"Oil of Garlic", "마늘즙",            // 772: 80, 1000
"Life Elixir", "생명의 영약",            // 773: 80, 500
"Glacierfish", "빙하고기",            // 775: 10, 1000
"Void Salmon", "공허의 연어",            // 795: 25, 150
"Slimejack", "슬라임잭",            // 796: 15, 100
"Midnight Squid", "자정오징어",            // 798: 15, 100
"Spook Fish", "통안어",            // 799: 15, 220
"Blobfish", "블롭피쉬",            // 800: 15, 500
*/
