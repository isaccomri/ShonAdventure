using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShonAdventure
{
    // Runnable stand-alone vertical slice of the 20-room story, independent from licensed AC APIs.
    public sealed class HauntedHousePrototype : MonoBehaviour
    {
        static readonly string[] Rooms = {
            "רחוב ליל כל הקדושים", "שער האחוזה", "חצר השומר", "הגדר", "מבואת הכניסה",
            "סלון הבובות", "המטבח", "מסדרון המראות", "הספרייה", "מעבדת הפורטל",
            "המעבר הנסתר", "כניסת השוטר", "חדר המעקב", "חדר הגבנון", "אי הקופים",
            "המרתף", "מעבדת האנרגיה", "אולם המנגנונים", "גג האחוזה", "שער היציאה"
        };
        static readonly string[] Goals = {
            "להפעיל את המצלמה", "לבחון את השומר ואת הגדר", "לבחור דרך להיכנס", "לעבור פנימה",
            "להחזיר את החשמל", "למצוא מפתח בין הבובות", "למצוא שלושה רמזים במטבח",
            "למצוא את ההשתקפות המזויפת", "לפתור את קוד הספרים", "להפעיל את הפורטל",
            "להשיג מפה", "לצרף את איציק", "לשחזר את ההקלטה", "לענות לגבנון",
            "לנגן רצף קופים", "להפעיל את הגנרטור", "לאסוף את רכיבי הפורטל",
            "להפעיל שני מתגים יחד", "לסגור את הקרע", "לצאת מהאחוזה"
        };
        static readonly string[] Speakers = {
            "שון", "גרגורי", "ג'יימס", "רומי", "איציק", "גרגורי", "ג'יימס", "רומי", "שון", "ג'יימס",
            "גרגורי", "איציק", "רומי", "הגבנון", "איציק", "שון", "רומי", "רומי", "שון", "כולם"
        };
        static readonly string[] Lines = {
            "בואו נצלם שם סרטון. זה הולך להתפוצץ!",
            "אני אומר לכם שהשומר הזה מפחד יותר ממני.",
            "יש שער ויש סולם. סוף סוף החלטה טובה!",
            "אני רק מזכיר שלא קיבלנו אישור כניסה.",
            "הדלת נסגרה מעצמה? זה לא חלק מהצילום!",
            "למה הבובה מסתכלת דווקא עליי?",
            "מקרר שמדבר זה כבר מוגזם, אפילו בשבילכם.",
            "אל תסמכו על המראה השלישית.",
            "מצאתי ביומן: שלושה ספרים פותחים את המעבדה.",
            "מי שם מכשיר פורטלים עם מדבקת קוקה קולה?",
            "אם נחזור בחיים, אני מוחק את הסרטון.",
            "שלום משטרה! מי החליט להיכנס לבית הזה?",
            "תראו את המצלמה: מישהו כבר היה פה!",
            "מי שפוחד לשאול לעולם לא ימצא את הדרך.",
            "עשרים שנה במשטרה, ואף קוף לא חקר אותי.",
            "בלי חשמל לא נצא מהמרתף.",
            "שלושת הרכיבים צריכים להתחבר ביחד.",
            "שון, אני מחזיקה מתג אחד. תחזיק את השני!",
            "כולם אחורה. סוגרים את השער עכשיו!",
            "חכו רגע... יש לנו מיליון צפיות?!"
        };

        const string SaveKey = "ShonHauntedHouse.Prototype.Stage";
        int stage;
        int progress;
        bool gateChosen;
        string route = "";
        bool finished;
        bool showHelp;
        Vector2 scroll;
        AudioSource audioSource;
        Texture2D pixel;
        GUIStyle title, label, small, button, dialogue;
        bool stylesReady;
        readonly List<GameObject> decoration = new List<GameObject>();

        void Start()
        {
            stage = Mathf.Clamp(PlayerPrefs.GetInt(SaveKey, 0), 0, 19);
            BuildRoom();
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        void OnDestroy()
        {
            if (pixel != null) Destroy(pixel);
        }

        void SetStage(int index)
        {
            stage = Mathf.Clamp(index, 0, 19);
            progress = 0;
            gateChosen = false;
            PlayerPrefs.SetInt(SaveKey, stage);
            PlayerPrefs.Save();
            BuildRoom();
        }

        void BuildRoom()
        {
            foreach (GameObject obj in decoration) if (obj != null) Destroy(obj);
            decoration.Clear();
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Prototype Camera");
                cam = go.AddComponent<Camera>();
                go.tag = "MainCamera";
                decoration.Add(go);
            }
            cam.transform.position = new Vector3(0, 6, -14);
            cam.transform.rotation = Quaternion.Euler(19, 0, 0);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.035f, .045f, .105f);
            AddPrimitive(PrimitiveType.Cube, "Stone Floor", new Vector3(0, -.6f, 3), new Vector3(16, 1, 13), new Color(.12f,.15f,.2f));
            AddPrimitive(PrimitiveType.Cube, "Mansion Back Wall", new Vector3(0, 3, 9), new Vector3(16, 8, .7f), new Color(.09f,.12f,.17f));
            for (int i = -2; i <= 2; i++)
            {
                AddPrimitive(PrimitiveType.Cube, "Glowing Window", new Vector3(i*2.8f, 3.5f, 8.56f), new Vector3(1.1f,2.1f,.1f), new Color(.03f,.56f,.50f));
            }
            for (int i = 0; i < 5; i++)
            {
                AddPrimitive(PrimitiveType.Capsule, "Character stand-in " + i,
                    new Vector3(-4 + i*2, .9f, .5f+i*.22f),
                    new Vector3(.65f,1.4f,.65f),
                    new Color[] {new Color(.80f,.30f,.15f),new Color(.19f,.48f,.78f),new Color(.31f,.35f,.22f),new Color(.98f,.53f,.23f),new Color(.15f,.22f,.50f)}[i]);
            }
            AddPrimitive(PrimitiveType.Sphere, "Moon", new Vector3(5.4f, 7, 8), new Vector3(1.4f,1.4f,.3f), new Color(.78f,.85f,1f));
            RenderSettings.ambientLight = new Color(.50f,.52f,.72f);
            if (FindFirstObjectByType<Light>() == null)
            {
                var go = new GameObject("Prototype Lighting");
                var light = go.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.6f;
                go.transform.rotation = Quaternion.Euler(35,-40,0);
                decoration.Add(go);
            }
        }

        void AddPrimitive(PrimitiveType kind, string objName, Vector3 pos, Vector3 scale, Color color)
        {
            var obj = GameObject.CreatePrimitive(kind);
            obj.name = objName;
            obj.transform.SetPositionAndRotation(pos, Quaternion.identity);
            obj.transform.localScale = scale;
            var rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                material.color = color;
                rend.material = material;
            }
            decoration.Add(obj);
        }

        void SetupGUI()
        {
            if (stylesReady) return;
            title = new GUIStyle(GUI.skin.label) {fontSize=25, fontStyle=FontStyle.Bold, alignment=TextAnchor.MiddleCenter, wordWrap=true};
            label = new GUIStyle(GUI.skin.label) {fontSize=19, alignment=TextAnchor.MiddleCenter, wordWrap=true};
            small = new GUIStyle(GUI.skin.label) {fontSize=15, alignment=TextAnchor.MiddleCenter, wordWrap=true};
            dialogue = new GUIStyle(GUI.skin.box) {fontSize=18, alignment=TextAnchor.MiddleCenter, wordWrap=true, padding=new RectOffset(14,14,15,15)};
            button = new GUIStyle(GUI.skin.button) {fontSize=19, fixedHeight=53, wordWrap=true};
            title.normal.textColor = new Color(.97f,.73f,.33f);
            label.normal.textColor = Color.white;
            small.normal.textColor = Color.white;
            stylesReady = true;
        }

        void OnGUI()
        {
            SetupGUI();
            float w = Mathf.Min(710, Screen.width - 26);
            float left = (Screen.width - w)/2f;
            var rect = new Rect(left, 8, w, Mathf.Min(Screen.height - 14, 540));
            GUILayout.BeginArea(rect, GUI.skin.window);
            GUILayout.Label("SHON ADVENTURE | הבית הרדוף", title, GUILayout.Height(38));
            GUILayout.Label("משימה " + (stage+1) + " מתוך 20  •  " + Rooms[stage], label);
            GUILayout.Label("מטרה: " + Goals[stage], small);
            GUILayout.Space(7);
            GUILayout.Box(Speakers[stage] + ": " + Lines[stage], dialogue, GUILayout.MinHeight(84));
            scroll = GUILayout.BeginScrollView(scroll);
            if (finished)
            {
                GUILayout.Label("סוף הדמו! הצלחתם לברוח. הסרטון הפך לוויראלי.", label);
                if (GUILayout.Button("משחק חדש", button)) { finished = false; SetStage(0); }
            }
            else if (stage == 2)
            {
                GUILayout.Label("בחרו אחת משתי דרכי הכניסה", label);
                if (GUILayout.Button("להסיח את דעת השומר בעזרת פח", button)) { route="guard"; gateChosen=true; SetStage(3); }
                if (GUILayout.Button("למצוא סולם ולטפס על הגדר", button)) { route="ladder"; gateChosen=true; SetStage(3); }
            }
            else if (stage == 17)
            {
                GUILayout.Label("רומי ושון חייבים להחזיק שני מתגים יחד", label);
                if (GUILayout.Button("רומי מחזיקה במתג הראשון", button)) progress |= 1;
                if (GUILayout.Button("שון מחזיק במתג השני", button)) progress |= 2;
                if (progress == 3 && GUILayout.Button("שני המתגים הופעלו! להמשיך", button)) Advance();
            }
            else if (stage == 14)
            {
                GUILayout.Label("חידת אי הקופים: בחרו את הרצף הנכון", label);
                if (GUILayout.Button("תוף - פעמון - תוף", button)) Advance();
                if (GUILayout.Button("פעמון - פעמון - תוף", button)) PlayTone();
            }
            else if (stage == 8)
            {
                GUILayout.Label("בחרו את הקוד ביומן", label);
                if (GUILayout.Button("314", button)) Advance();
                if (GUILayout.Button("413", button)) PlayTone();
            }
            else
            {
                if (GUILayout.Button(stage == 19 ? "לצאת ולסיים" : "לבצע את המשימה ולהמשיך", button))
                    Advance();
            }
            if (GUILayout.Button("הוראות / סטטוס", button)) showHelp = !showHelp;
            if (showHelp) GUILayout.Label("גרסת אבטיפוס ניתנת למשחק: סביבות ודמויות הן צורות זמניות. אין עדיין אנימציה או דיבוב מלא. הדרך שנבחרה: " + (route == "" ? "טרם נבחרה" : route), small);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void Advance()
        {
            if (stage == 19)
            {
                finished = true;
                PlayerPrefs.DeleteKey(SaveKey);
                PlayerPrefs.Save();
            }
            else SetStage(stage+1);
        }

        void PlayTone()
        {
            // Wrong answer feedback can later be replaced with an authored audio asset.
            Debug.Log("נסה שוב - הפתרון מסתתר ברמזים בחדר.");
        }
    }
}
