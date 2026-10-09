using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShonAdventure
{
    // Playable point-and-click puzzle slice, separate from the 20-step prototype.
    // Uses built-in IMGUI. Does not depend on licensed Adventure Creator APIs.
    public sealed class HauntedAdventureSlice : MonoBehaviour
    {
        enum Place { Street, Gate, Courtyard, Foyer }
        Place place;
        readonly HashSet<string> inventory = new HashSet<string>();
        readonly HashSet<string> flags = new HashSet<string>();
        string selected = "";
        string speaker = "שון";
        string speech = "חבר'ה, בית רדוף! בואו נצלם פה סרטון.";
        string entryRoute = "";
        int dialogue;
        bool showHints;
        Vector2 scroll;

        static readonly string[] placeNames = {"רחוב ליל כל הקדושים", "שער האחוזה", "החצר", "המבואה"};
        static readonly string[] cast = {"שון", "ג'יימס", "גרגורי", "רומי", "איציק"};
        readonly Dictionary<string,string> names = new Dictionary<string,string> {
            {"phone", "טלפון"}, {"stone", "אבן"}, {"ladder", "סולם"}, {"battery", "סוללה"}, {"key", "מפתח חלוד"}
        };

        void Awake() { NewGame(); }
        void NewGame()
        {
            place = Place.Street; inventory.Clear(); flags.Clear(); selected = "";
            entryRoute = ""; dialogue = 0; Talk("שון","המצלמה מוכנה. מי מתערב שזה הבית הכי מפחיד בעיר?");
        }
        void Talk(string who, string what) {speaker=who; speech=what; }
        void Pickup(string id, string who, string line)
        {
            if (inventory.Add(id)) Talk(who, line);
            else Talk(who, "כבר יש לנו את זה בתיק.");
        }
        void Inspect(string id)
        {
            if (selected != "")
            {
                string usingItem = selected; selected = "";
                Use(usingItem,id); return;
            }
            switch (id)
            {
                case "phone": Pickup("phone","שון","מצאתי את הטלפון. עכשיו רק צריך אומץ שלא נמכר בחנות."); break;
                case "stone": Pickup("stone","ג'יימס","אבן טובה. עם קצת מזל היא לא תהרוס שום דבר."); break;
                case "bin": Talk("גרגורי","פח מתכת רעשני. מרגיש כמו התזמורת של השכנים."); break;
                case "guard": Talk("השומר","השטח סגור. תחזרו הביתה לפני שאני צריך לעבוד."); break;
                case "ladder": Pickup("ladder","גרגורי","אני לא עולה על הדבר הרעוע הזה... רגע, למה אני סוחב אותו?"); break;
                case "fence": Talk("רומי","הגדר גבוהה, אבל סולם יעזור. ואם לא, ננסה משהו יותר חכם."); break;
                case "door": Talk("שון","דלת ענקית. בטח גם חשבון החשמל שלה ענק."); break;
                case "dark": Talk("רומי","אין אור. צריך למצוא סוללה ולהפעיל את לוח החשמל."); break;
                case "battery": Pickup("battery","רומי","סוללה! עכשיו נחפש איפה מפעילים את החשמל."); break;
                case "panel": Talk("איציק","לוח חשמל ישן. תנו לי סוללה ואז אולי יהיה כאן קצת פחות מסוכן."); break;
                case "key": Pickup("key","ג'יימס","מפתח חלוד. בדרך כלל זה אומר שיש גם דלת שלא רצינו לפתוח."); break;
                default: Talk("שון","זה נראה חשוד, אבל לא מצאתי פה משהו שימושי."); break;
            }
        }
        void Use(string item, string target)
        {
            if (!inventory.Contains(item)) {Talk("שון","אין לנו את החפץ הזה."); return;}
            if (item == "phone" && target == "mansion")
            {
                flags.Add("filmed"); Talk("שון","מצלמה פועלת! בואו ניכנס לפרק הכי מטורף שלנו."); return;
            }
            if (item == "stone" && target == "bin" && place == Place.Gate)
            {
                flags.Add("distracted"); Talk("השומר","מי זורק דברים לפח?! טוב, לפחות מישהו סוף סוף ממחזר."); return;
            }
            if (item == "ladder" && target == "fence" && place == Place.Gate)
            {
                entryRoute="סולם"; flags.Add("entered"); Talk("גרגורי","למה אנחנו מטפסים על הגדר של בית רדוף? זה ספורט חדש?"); place=Place.Courtyard; return;
            }
            if (item == "battery" && target == "panel" && place == Place.Foyer)
            {
                flags.Add("power"); inventory.Remove(item);
                Talk("רומי","יש אור! אבל מי אמר שאור בבית רדוף זה בהכרח דבר טוב?"); return;
            }
            Talk("ג'יימס","ניסיתי. זה לא עובד, אבל לפחות זה היה מביך במיוחד.");
        }
        void NextDialogue()
        {
            dialogue++;
            switch (dialogue)
            {
                case 1: Talk("ג'יימס","יאללה, אני בפנים. זה הולך להיות הסרטון של השנה!"); break;
                case 2: Talk("גרגורי","אני מפחד, אבל אתם החברים היחידים שלי. תודה רבה באמת."); break;
                case 3: Talk("שון","גרגורי, אתה מצלם את הדרך בחזרה. ככה תהיה שימושי."); break;
                default: Talk("רומי","שון, תחזור הביתה. יש לי הרגשה שמשהו לא בסדר בבית הזה."); break;
            }
        }

        GUIStyle panel, heading, text, action, subtitle;
        void Styles()
        {
            if (panel != null) return;
            panel = new GUIStyle(GUI.skin.box) {padding=new RectOffset(14,14,12,12)};
            heading = new GUIStyle(GUI.skin.label) {fontSize=24,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,wordWrap=true};
            subtitle = new GUIStyle(GUI.skin.label) {fontSize=17,alignment=TextAnchor.MiddleCenter,wordWrap=true};
            text = new GUIStyle(GUI.skin.box) {fontSize=18,alignment=TextAnchor.MiddleCenter,wordWrap=true,padding=new RectOffset(12,12,15,15)};
            action = new GUIStyle(GUI.skin.button) {fontSize=17,fixedHeight=43,wordWrap=true};
            heading.normal.textColor = new Color(1f,.75f,.38f);
            subtitle.normal.textColor = Color.white;
        }
        void OnGUI()
        {
            Styles();
            float width = Mathf.Min(850, Screen.width-30);
            float height = Mathf.Min(Screen.height-12, 740);
            GUILayout.BeginArea(new Rect((Screen.width-width)/2f, 6, width, height), panel);
            GUILayout.Label("SHON ADVENTURE  |  הבית הרדוף", heading);
            GUILayout.Label(placeNames[(int)place] + "  •  פרק ראשון: אף אחד לא ביקש להיכנס", subtitle);
            GUILayout.Box(speaker + ": " + speech, text, GUILayout.MinHeight(75));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("שיחה",action)) NextDialogue();
            if (GUILayout.Button("רמז",action)) showHints = !showHints;
            if (GUILayout.Button("משחק חדש",action)) NewGame();
            GUILayout.EndHorizontal();
            if (showHints) GUILayout.Label(Hint(), subtitle);
            GUILayout.Label("לחץ על חפץ בתיק ואז על מקום כדי להשתמש בו",subtitle);
            GUILayout.BeginHorizontal();
            foreach (string id in new List<string>(inventory))
            {
                GUI.backgroundColor = selected == id ? Color.yellow : Color.white;
                if (GUILayout.Button(names[id], action, GUILayout.MaxWidth(150))) selected = selected == id ? "" : id;
                GUI.backgroundColor=Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("לחקור", heading);
            scroll = GUILayout.BeginScrollView(scroll);
            switch(place)
            {
                case Place.Street:
                    Btn("טלפון צילום", "phone");
                    Btn("אבן ליד המדרכה", "stone");
                    Btn("האחוזה באופק", "mansion");
                    if (flags.Contains("filmed") && GUILayout.Button("להתקדם לשער הבית", action)) { place=Place.Gate; Talk("גרגורי","כמובן. סוף סוף מקום עם שומר שיגיד לנו ללכת.");}
                    break;
                case Place.Gate:
                    Btn("השומר", "guard");
                    Btn("פח מתכת", "bin");
                    Btn("סולם מאחורי המחסן", "ladder");
                    Btn("גדר ברזל", "fence");
                    if (flags.Contains("distracted") && GUILayout.Button("השומר מוסח: להתגנב דרך שער השירות", action))
                    {
                        entryRoute="שומר"; flags.Add("entered"); place=Place.Courtyard;
                        Talk("ג'יימס","בחיים לא האמנתי שהפח באמת יציל אותנו.");
                    }
                    break;
                case Place.Courtyard:
                    Btn("דלת העץ הגדולה", "door");
                    GUILayout.Label("נכנסנו דרך: " + entryRoute, subtitle);
                    if (GUILayout.Button("לפתוח את הדלת ולהיכנס",action))
                    {
                        place=Place.Foyer; Talk("גרגורי","הדלת נטרקה! מה יש לכם נגד דלתות שנפתחות החוצה?");
                    }
                    break;
                case Place.Foyer:
                    Btn("חושך", "dark"); Btn("סוללה ישנה", "battery"); Btn("לוח חשמל", "panel");
                    if (flags.Contains("power"))
                    {
                        GUILayout.Label("האורות נדלקו. סוף הפרק המשחקי הראשון!", heading);
                        GUILayout.Label("בפרק הבא: סלון הבובות, מראות, ספרייה ופורטל.", subtitle);
                    }
                    break;
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        void Btn(string label, string id) { if (GUILayout.Button(label,action)) Inspect(id); }
        string Hint()
        {
            switch (place)
            {
                case Place.Street: return "קח את הטלפון, לחץ עליו בתיק ואז לחץ על האחוזה.";
                case Place.Gate: return "דרך א: אבן על פח ואז שער השירות. דרך ב: סולם על הגדר.";
                case Place.Courtyard: return "פתח את דלת העץ.";
                default: return "הרם סוללה, בחר בה בתיק ואז לחץ על לוח החשמל.";
            }
        }
    }
}
