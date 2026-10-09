using System.Collections.Generic;
using UnityEngine;

namespace ShonAdventure
{
    // Standalone episode 2: playable inventory / hotspot puzzle, no licensed AC source required.
    public sealed class HauntedDollSalon : MonoBehaviour
    {
        readonly HashSet<string> items = new HashSet<string>();
        readonly HashSet<string> clues = new HashSet<string>();
        string held = "";
        string speaker = "שון";
        string line = "זה הסלון? למה כל הבובות מסתכלות עלינו?";
        bool solved, scare, hint;
        int dialogue;
        Vector2 scroll;
        GUIStyle heading, body, btn, small;
        readonly Dictionary<string,string> itemNames = new Dictionary<string,string> {
            {"tornPhoto","חצי תמונה"}, {"glassEye","עין זכוכית"}, {"musicBoxKey","מפתח לתיבת נגינה"}, {"rustKey","מפתח חלוד"}
        };

        void ResetGame()
        {
            items.Clear(); clues.Clear();
            held = ""; solved = false; scare = false; hint = false; dialogue = 0;
            Say("שון","למה כל הבובות מסתכלות עלינו? אולי אנחנו פשוט מפורסמים.");
        }
        void Awake() { ResetGame(); }
        void Say(string who, string message) { speaker = who; line = message; }

        void Find(string id)
        {
            if (held != "") { string usingId = held; held = ""; Use(usingId,id); return; }
            switch(id)
            {
                case "painting":
                    clues.Add("painting");
                    Say("רומי","בתמונה הישנה חסרה חתיכה, ובפינה כתוב: 'המנגינה מחזירה את המבט'.");
                    break;
                case "photo":
                    items.Add("tornPhoto"); Say("ג'יימס","מצאתי חצי תמונה מתחת לשטיח. כן, בדקתי קודם שאין שם עכבישים."); break;
                case "doll":
                    if (!scare)
                    {
                        scare = true; Say("גרגורי","אאאה! היא הזיזה את הראש! תגידו לי שראיתם את זה!");
                    }
                    else Say("שון","הבובה מסתירה משהו במקום של העין השמאלית.");
                    break;
                case "shelf":
                    items.Add("glassEye"); Say("רומי","עין מזכוכית על המדף. כנראה היא שייכת לבובה."); break;
                case "box":
                    Say("ג'יימס", clues.Contains("eye") ? "בתיבה יש חור למפתח קטן. חפש על הרצפה." : "תיבת נגינה נעולה. על המכסה חרוטה עין."); break;
                case "floor":
                    if (clues.Contains("eye"))
                    {
                        items.Add("musicBoxKey"); Say("שון","מפתח קטן! הוא נפל מהבובה כשהעין חזרה למקום.");
                    }
                    else Say("גרגורי","הרצפה חורקת. ובפינת החדר יש אבק בן מאה שנה."); break;
                case "door":
                    Say("איציק","הדלת לספרייה נעולה. צריך מפתח חלוד כלשהו."); break;
                default: Say("שון","לא נראה שיש כאן משהו שימושי כרגע."); break;
            }
        }

        void Use(string item, string target)
        {
            if (!items.Contains(item)) { Say("שון","אין לנו את החפץ הזה."); return; }
            if (item=="tornPhoto" && target=="painting")
            {
                clues.Add("completePicture"); items.Remove(item);
                Say("רומי","התמונה הושלמה! זו הבובה שעל הכיסא, והעין שלה חסרה.");
            }
            else if (item=="glassEye" && target=="doll")
            {
                items.Remove(item); clues.Add("eye"); scare = true;
                Say("הבובה","תודה... עכשיו אפשר לנגן...");
            }
            else if (item=="musicBoxKey" && target=="box" && clues.Contains("eye"))
            {
                items.Remove(item); clues.Add("music");
                Say("ג'יימס","המנגינה נשמעת! רגע... למה מאחורי הווילון נדלק אור?");
            }
            else if (item=="rustKey" && target=="door")
            {
                solved=true; Say("שון","הצלחנו! הספרייה פתוחה. מה כבר יכול להשתבש בחדר הבא?");
            }
            else Say("גרגורי","זה שילוב כל כך מוזר שאפילו הבית לא קיבל אותו.");
        }

        void SetStyles()
        {
            if (heading != null) return;
            heading = new GUIStyle(GUI.skin.label) {fontSize=25,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,wordWrap=true};
            heading.normal.textColor = new Color(1f,.75f,.35f);
            body = new GUIStyle(GUI.skin.box) {fontSize=19,alignment=TextAnchor.MiddleCenter,wordWrap=true,padding=new RectOffset(14,14,14,14)};
            btn = new GUIStyle(GUI.skin.button) {fontSize=18,fixedHeight=48,wordWrap=true};
            small = new GUIStyle(GUI.skin.label) {fontSize=16,alignment=TextAnchor.MiddleCenter,wordWrap=true};
        }
        void OnGUI()
        {
            SetStyles();
            float width=Mathf.Min(Screen.width-24,850);
            GUILayout.BeginArea(new Rect((Screen.width-width)/2,8,width,Mathf.Max(200,Screen.height-20)),GUI.skin.window);
            GUILayout.Label("SHON ADVENTURE | סלון הבובות",heading);
            GUILayout.Label("פרק שני • חידה: המנגינה שמחזירה את המבט",small);
            GUILayout.Box(speaker + ": " + line,body,GUILayout.MinHeight(80));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("שיחה",btn))
            {
                dialogue++;
                switch(dialogue % 4)
                {
                    case 1: Say("רומי","אני רואה חצי תמונה, בובה בלי עין, ותיבת נגינה. מישהו ממש רוצה שנפתור את זה."); break;
                    case 2: Say("גרגורי","אני מצביע בעד לצאת מהבית. מישהו איתי? כרגיל, לא."); break;
                    case 3: Say("ג'יימס","אני מוכן לנגן, אבל לא על העצבנות של גרגורי."); break;
                    default: Say("איציק","אף אחד לא נוגע בבובה לבד. ראיתי מספיק סרטים בשביל זה."); break;
                }
            }
            if (GUILayout.Button("רמז",btn)) hint=!hint;
            if (GUILayout.Button("התחל מחדש",btn)) ResetGame();
            GUILayout.EndHorizontal();
            if (hint) GUILayout.Label("חצי תמונה + ציור, עין זכוכית + בובה, חפש מפתח על הרצפה, מפתח קטן + תיבת נגינה.",small);
            GUILayout.Label("התיק • בחר חפץ ואז לחץ על אובייקט בחדר",small);
            GUILayout.BeginHorizontal();
            foreach (string id in new List<string>(items))
            {
                GUI.backgroundColor=held == id ? Color.yellow : Color.white;
                if (GUILayout.Button(itemNames[id],btn,GUILayout.MaxWidth(200)))
                    held=held==id?"":id;
                GUI.backgroundColor=Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("חקירת הסלון",heading);
            scroll=GUILayout.BeginScrollView(scroll,GUILayout.MinHeight(170));
            ObjectButton("ציור משפחתי עם חלק חסר","painting");
            ObjectButton("חצי תמונה מתחת לשטיח","photo");
            ObjectButton("בובה על כיסא נדנדה","doll");
            ObjectButton("מדף עם עין מזכוכית","shelf");
            ObjectButton("תיבת נגינה נעולה","box");
            ObjectButton("רצפת העץ החורקת","floor");
            ObjectButton("דלת הספרייה","door");
            if (clues.Contains("music") && !items.Contains("rustKey") && !solved)
            {
                if (GUILayout.Button("להציץ מאחורי הווילון הזוהר",btn))
                {
                    items.Add("rustKey");
                    Say("רומי","יש כאן מפתח חלוד! אוקיי, אפילו אני קצת נבהלתי.");
                }
            }
            if (solved)
            {
                GUILayout.Label("המשימה הושלמה! הדלת לספרייה נפתחה.",heading);
                GUILayout.Label("הסצנה הבאה תפותח בהמשך; המשחק לא מחובר עדיין לסצנות Adventure Creator.",small);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        void ObjectButton(string name,string id)
        {
            if (GUILayout.Button(name,btn)) Find(id);
        }
    }
}
