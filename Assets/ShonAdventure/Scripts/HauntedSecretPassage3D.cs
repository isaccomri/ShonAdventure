using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShonAdventure
{
    // Chapter four: interactive underground corridor with a two-switch cooperative puzzle.
    public sealed class HauntedSecretPassage3D : MonoBehaviour
    {
        Camera cam;
        readonly Dictionary<GameObject,string> hot = new Dictionary<GameObject,string>();
        readonly HashSet<string> state = new HashSet<string>();
        string speaker="גרגורי", line="נפלא. ספרייה עם מעבר תת־קרקעי. בטח זה עומד בתקני בטיחות.";
        bool doorOpen;
        float doorProgress;
        Transform gate;
        GUIStyle title, words, button;
        Material Mat(Color c) {
            Shader sh=Shader.Find("Universal Render Pipeline/Lit");
            if(sh==null)sh=Shader.Find("Standard");
            var m=new Material(sh);m.color=c;return m;
        }
        GameObject Cube(string n,Vector3 p,Vector3 scale,Color c,string id=null) {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name=n;g.transform.position=p;g.transform.localScale=scale;
            g.GetComponent<Renderer>().material=Mat(c);
            if(id!=null)hot.Add(g,id);
            return g;
        }
        void Start() {
            cam=Camera.main;
            if(cam==null) {var g=new GameObject("Main Camera");g.tag="MainCamera";cam=g.AddComponent<Camera>();g.AddComponent<AudioListener>();}
            cam.transform.position=new Vector3(0,5,-11);
            cam.transform.LookAt(new Vector3(0,1.4f,1));
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.025f,.028f,.055f);
            RenderSettings.ambientLight=new Color(.24f,.27f,.34f);
            Cube("Passage floor",new Vector3(0,-.2f,0),new Vector3(12,.4f,13),new Color(.18f,.2f,.23f));
            Cube("Back stone wall",new Vector3(0,3.4f,6),new Vector3(12,6.8f,.4f),new Color(.19f,.22f,.27f));
            Cube("Left stone wall",new Vector3(-6,3,0),new Vector3(.4f,6,13),new Color(.17f,.20f,.26f));
            Cube("Right stone wall",new Vector3(6,3,0),new Vector3(.4f,6,13),new Color(.17f,.20f,.26f));
            for(int i=0;i<4;i++) {
                Cube("Stone seam left "+i,new Vector3(-5.77f,.6f+i*1.3f,1),new Vector3(.07f,.05f,10),new Color(.12f,.14f,.18f));
                Cube("Stone seam right "+i,new Vector3(5.77f,.6f+i*1.3f,1),new Vector3(.07f,.05f,10),new Color(.12f,.14f,.18f));
            }
            Cube("Red wall switch",new Vector3(-3.7f,1.7f,3.7f),new Vector3(.85f,1f,.35f),new Color(.85f,.17f,.16f),"red");
            Cube("Blue wall switch",new Vector3(3.7f,1.7f,3.7f),new Vector3(.85f,1f,.35f),new Color(.12f,.46f,.95f),"blue");
            Cube("Cryptic plaque",new Vector3(0,3.1f,5.7f),new Vector3(2.7f,1f,.2f),new Color(.6f,.54f,.32f),"plaque");
            gate=Cube("Heavy gate",new Vector3(0,1.6f,5.4f),new Vector3(2.3f,3.2f,.4f),new Color(.29f,.33f,.39f),"gate").transform;
            var torch=new GameObject("Blue torch");var light=torch.AddComponent<Light>();
            light.type=LightType.Point;light.range=14;light.intensity=5;light.color=new Color(.4f,.65f,1f);
            torch.transform.position=new Vector3(0,4,0);
            for(int i=0;i<4;i++) {
                var actor=GameObject.CreatePrimitive(PrimitiveType.Capsule);
                actor.name=new[]{"Shon placeholder","Romi placeholder","James placeholder","Gregory placeholder"}[i];
                actor.transform.position=new Vector3(-2.4f+i*1.6f,1,-2.5f);
                actor.transform.localScale=new Vector3(.65f,.8f,.65f);
                actor.GetComponent<Renderer>().material=Mat(new[]{new Color(.2f,.48f,.92f),new Color(.9f,.24f,.58f),new Color(.15f,.67f,.4f),new Color(.9f,.6f,.2f)}[i]);
                Destroy(actor.GetComponent<Collider>());
            }
        }
        bool MousePress(out Vector2 pos) {
            pos=Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var mouse=UnityEngine.InputSystem.Mouse.current;
            if(mouse==null||!mouse.leftButton.wasPressedThisFrame)return false;
            pos=mouse.position.ReadValue();return true;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if(!Input.GetMouseButtonDown(0))return false;
            pos=Input.mousePosition;return true;
#else
            return false;
#endif
        }
        void Update() {
            if(MousePress(out Vector2 p)&&p.y>180f) {
                RaycastHit hit;
                if(Physics.Raycast(cam.ScreenPointToRay(p),out hit,100f)) {
                    string id;
                    if(hot.TryGetValue(hit.collider.gameObject,out id))Interact(id);
                }
            }
            if(doorOpen&&gate!=null) {
                doorProgress=Mathf.MoveTowards(doorProgress,3.5f,Time.deltaTime*1.1f);
                gate.position=new Vector3(0,1.6f+doorProgress,5.4f);
            }
        }
        void Say(string who,string msg) {speaker=who;line=msg;}
        void Interact(string id) {
            if(id=="plaque") {state.Add("read");Say("רומי","כתוב: 'שתי ידיים, שתי הבטחות. האדום ראשון, הכחול אחריו.'");}
            else if(id=="red") {
                state.Add("red");state.Remove("blue");
                Say("שון","הפעלתי את המתג האדום! רומי, עכשיו הכחול!");
            } else if(id=="blue") {
                if(!state.Contains("red"))Say("רומי","רגע, לפי הכתובת צריך קודם את האדום.");
                else {
                    state.Add("blue");doorOpen=true;
                    Say("רומי","הצלחנו ביחד! השער נפתח!");
                }
            } else if(id=="gate") {
                if(doorOpen)Say("ג'יימס","יש אור מעבר לשער. אולי זה חדר המעקב?");
                else Say("איציק","נעול. מצאו איך מפעילים את שני המתגים.");
            }
        }
        void OnGUI() {
            if(title==null) {
                title=new GUIStyle(GUI.skin.label){fontSize=25,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,wordWrap=true};
                title.normal.textColor=new Color(1f,.75f,.35f);
                words=new GUIStyle(GUI.skin.label){fontSize=18,alignment=TextAnchor.MiddleCenter,wordWrap=true};
                words.normal.textColor=Color.white;
                button=new GUIStyle(GUI.skin.button){fontSize=17,fixedHeight=39};
            }
            float w=Mathf.Min(900,Screen.width-16);
            GUILayout.BeginArea(new Rect((Screen.width-w)/2,6,w,60));
            GUILayout.Label("SHON ADVENTURE | המעבר הסודי",title);
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect((Screen.width-w)/2,Screen.height-169,w,163),GUI.skin.box);
            GUILayout.Label(speaker+": "+line,words);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("רמז",button))Say("רומי","קרא את לוח האבן. לחץ על המתג האדום ואחריו על המתג הכחול.");
            if(GUILayout.Button("חזרה לספרייה",button))
                SceneManager.LoadScene("HauntedHouse_Library3D");
            GUILayout.EndHorizontal();
            if(doorOpen)GUILayout.Label("המעבר נפתח! המשך החדרים בפיתוח.",title);
            GUILayout.EndArea();
        }
    }
}
