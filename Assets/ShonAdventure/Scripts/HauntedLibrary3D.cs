using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShonAdventure
{
    // Third playable room: solve the rotating-books puzzle and open the secret passage.
    // Self-contained primitive environment; replaces no licensed Adventure Creator assets.
    public sealed class HauntedLibrary3D : MonoBehaviour
    {
        Camera cam;
        readonly Dictionary<GameObject,string> targets = new Dictionary<GameObject,string>();
        readonly HashSet<string> items = new HashSet<string>();
        readonly HashSet<string> flags = new HashSet<string>();
        readonly List<Transform> books = new List<Transform>();
        Transform hero;
        Vector3 destination;
        string selected = "";
        string speaker = "רומי";
        string text = "הספרייה נראית רגילה מדי. זה בדיוק מה שמדאיג אותי.";
        string sequence = "";
        bool solved;
        GUIStyle titleStyle, dialogueStyle, buttonStyle;

        Material Mat(Color c)
        {
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null) shader=Shader.Find("Standard");
            var m=new Material(shader);m.color=c;return m;
        }
        GameObject Block(string name, Vector3 at, Vector3 size, Color color, string id=null)
        {
            GameObject g=GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name=name;g.transform.position=at;g.transform.localScale=size;
            g.GetComponent<Renderer>().material=Mat(color);
            if(id!=null) targets.Add(g,id);
            return g;
        }
        void Start()
        {
            cam=Camera.main;
            if(cam==null)
            {
                var go=new GameObject("Main Camera");go.tag="MainCamera";cam=go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            cam.transform.position=new Vector3(0,5,-12);
            cam.transform.LookAt(new Vector3(0,1.5f,0));
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.035f,.045f,.07f);
            RenderSettings.ambientLight=new Color(.3f,.29f,.35f);
            Block("Floor",new Vector3(0,-.2f,0),new Vector3(13,.4f,10),new Color(.23f,.14f,.09f));
            Block("Back wall",new Vector3(0,3.5f,5),new Vector3(13,7,.2f),new Color(.15f,.18f,.18f));
            Block("Left wall",new Vector3(-6.5f,3,0),new Vector3(.2f,6,10),new Color(.15f,.18f,.18f));
            Block("Right wall",new Vector3(6.5f,3,0),new Vector3(.2f,6,10),new Color(.15f,.18f,.18f));
            for(int shelf=0;shelf<3;shelf++)
            {
                float x=-4.6f+shelf*4.5f;
                Block("Bookcase",new Vector3(x,2.2f,4.48f),new Vector3(3.25f,4.4f,.8f),new Color(.28f,.15f,.075f));
                for(int j=0;j<9;j++)
                {
                    float bx=x-1.3f+(j%5)*.62f;
                    float by=1f+(j/5)*1.7f;
                    Block("Decorative book",new Vector3(bx,by,3.98f),new Vector3(.27f,1.08f,.16f),
                        j%3==0?new Color(.25f,.4f,.34f):j%3==1?new Color(.32f,.18f,.37f):new Color(.53f,.28f,.14f));
                }
            }
            Block("Red rotating book",new Vector3(-5.0f,2.6f,3.87f),new Vector3(.4f,1.3f,.3f),new Color(.8f,.1f,.12f),"red");
            Block("Blue rotating book",new Vector3(-.6f,2.6f,3.87f),new Vector3(.4f,1.3f,.3f),new Color(.15f,.4f,.95f),"blue");
            Block("Green rotating book",new Vector3(3.6f,2.6f,3.87f),new Vector3(.4f,1.3f,.3f),new Color(.2f,.7f,.25f),"green");
            Block("Reading table",new Vector3(0,.9f,0),new Vector3(2.6f,1.3f,1.8f),new Color(.3f,.16f,.09f));
            Block("Torn diary page",new Vector3(-.5f,1.62f,-.1f),new Vector3(.7f,.035f,.7f),new Color(.93f,.85f,.61f),"page");
            Block("Brass clock",new Vector3(2.4f,1.3f,-.8f),new Vector3(.9f,1.4f,.5f),new Color(.78f,.57f,.21f),"clock");
            Block("Secret passage",new Vector3(5.4f,1.65f,4.69f),new Vector3(1.65f,3.3f,.2f),new Color(.18f,.13f,.11f),"passage");
            var lightObj=new GameObject("Reading lamp");
            var light=lightObj.AddComponent<Light>();light.type=LightType.Point;
            light.range=16;light.intensity=5;light.color=new Color(1f,.77f,.46f);
            lightObj.transform.position=new Vector3(0,4,-1f);
            hero=GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
            hero.name="Shon - placeholder";
            hero.localScale=new Vector3(.65f,1f,.65f);
            hero.position=new Vector3(-2,1,-2.4f);
            hero.GetComponent<Renderer>().material=Mat(new Color(.15f,.45f,.9f));
            Destroy(hero.GetComponent<Collider>());
            destination=hero.position;
        }
        bool Click(out Vector2 p)
        {
            p=Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var m=UnityEngine.InputSystem.Mouse.current;
            if(m==null || !m.leftButton.wasPressedThisFrame)return false;
            p=m.position.ReadValue();return true;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if(!Input.GetMouseButtonDown(0))return false;
            p=Input.mousePosition;return true;
#else
            return false;
#endif
        }
        void Update()
        {
            if(Click(out Vector2 pos) && pos.y>185f && cam!=null)
            {
                RaycastHit hit;
                if(Physics.Raycast(cam.ScreenPointToRay(pos),out hit,100))
                {
                    string id;
                    if(targets.TryGetValue(hit.collider.gameObject,out id)) Interact(id);
                    else if(hit.collider.gameObject.name=="Floor")
                        destination=new Vector3(Mathf.Clamp(hit.point.x,-5.8f,5.8f),1,Mathf.Clamp(hit.point.z,-4.5f,3.5f));
                }
            }
            if(hero!=null)
            {
                Vector3 dir=destination-hero.position;
                if(dir.sqrMagnitude>.01f)
                {
                    hero.position=Vector3.MoveTowards(hero.position,destination,Time.deltaTime*2.6f);
                    hero.rotation=Quaternion.Slerp(hero.rotation,Quaternion.LookRotation(dir),Time.deltaTime*9f);
                }
            }
        }
        void Say(string who,string sentence){speaker=who;text=sentence;}
        void Interact(string id)
        {
            if(selected=="page" && id=="clock")
            {
                selected="";
                flags.Add("clockRead");
                Say("רומי","הציור מהיומן מתאים לשעון: קודם כחול, אחר כך ירוק, ובסוף אדום.");
                return;
            }
            if(id=="page")
            {
                items.Add("page");
                Say("ג'יימס","מצאתי דף יומן! כתוב: 'השעה היא המפתח'.");
            }
            else if(id=="clock")
                Say("איציק","שעון מקולקל. החוגים מצוירים בשלושה צבעים.");
            else if(id=="red"||id=="blue"||id=="green")
            {
                if(!flags.Contains("clockRead"))
                {
                    Say("גרגורי","אני חושב שצריך להבין קודם את השעון. רצוי לפני שנהפוך לספרים.");
                    return;
                }
                sequence+=id.Substring(0,1);
                if(sequence.Length>3)sequence=id.Substring(0,1);
                if(!"bgr".StartsWith(sequence))
                {
                    sequence="";
                    Say("שון","שמעתי קליק לא טוב. כנראה הסדר שגוי.");
                }
                else if(sequence=="bgr")
                {
                    solved=true;
                    Say("רומי","שמעתם?! מדף הספרים זז. מצאנו מעבר סודי!");
                }
                else Say("ג'יימס","הספר זז! עוד "+(3-sequence.Length)+" ונראה מה קורה.");
            }
            else if(id=="passage")
            {
                if(solved)Say("שון","המעבר פתוח! ההרפתקה ממשיכה בחדר הבא.");
                else Say("איציק","הקיר לא זז. קודם נפענח את הספרים.");
            }
        }
        void OnGUI()
        {
            if(titleStyle==null)
            {
                titleStyle=new GUIStyle(GUI.skin.label){fontSize=25,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
                titleStyle.normal.textColor=new Color(1f,.74f,.31f);
                dialogueStyle=new GUIStyle(GUI.skin.label){fontSize=18,alignment=TextAnchor.MiddleCenter,wordWrap=true};
                dialogueStyle.normal.textColor=Color.white;
                buttonStyle=new GUIStyle(GUI.skin.button){fontSize=17,fixedHeight=38};
            }
            float w=Mathf.Min(870,Screen.width-16);
            GUILayout.BeginArea(new Rect((Screen.width-w)/2,8,w,50));
            GUILayout.Label("SHON ADVENTURE | הספרייה המקוללת",titleStyle);
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect((Screen.width-w)/2,Screen.height-173,w,167),GUI.skin.box);
            GUILayout.Label(speaker+": "+text,dialogueStyle);
            GUILayout.BeginHorizontal();
            if(items.Contains("page") && GUILayout.Button(selected=="page"?"דף יומן (נבחר)":"דף יומן",buttonStyle))
                selected=selected=="page"?"":"page";
            if(GUILayout.Button("רמז",buttonStyle))
                Say("רומי","קח את דף היומן, השתמש בו על השעון, ואז לחץ על הספרים בסדר הצבעים.");
            if(GUILayout.Button("חזור לסלון",buttonStyle))
                SceneManager.LoadScene("HauntedHouse_DollSalon3D");
            GUILayout.EndHorizontal();
            if(solved) GUILayout.Label("המעבר הסודי נפתח. הפרק הבא בפיתוח.",titleStyle);
            GUILayout.EndArea();
        }
    }
}
