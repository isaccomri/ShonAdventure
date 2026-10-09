using System.Collections.Generic;
using UnityEngine;

namespace ShonAdventure
{
    // Graphical standalone vertical slice. Primitive meshes deliberately serve as placeholders.
    public sealed class DollSalon3D : MonoBehaviour
    {
        readonly Dictionary<GameObject,string> hotspots = new Dictionary<GameObject,string>();
        readonly HashSet<string> inventory = new HashSet<string>();
        readonly HashSet<string> completed = new HashSet<string>();
        readonly Dictionary<string,string> names = new Dictionary<string,string> {
            {"photo","חצי תמונה"}, {"eye","עין זכוכית"}, {"tinykey","מפתח לתיבת נגינה"}, {"rustkey","מפתח חלוד"}
        };
        Camera cam;
        Transform dollHead;
        Light flicker;
        Transform hero;
        Transform[] friends;
        Vector3 walkTarget;
        bool walking;
        string selected = "";
        string speaker = "שון";
        string dialogue = "זה הסלון? אפילו הבובות כאן צריכות טיפול.";
        bool jumpScare, finished;
        float scareEnd;
        GUIStyle captionStyle, textStyle, buttonStyle;
        Material MakeMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            var material = new Material(shader);
            material.color = color;
            return material;
        }
        GameObject Cube(string name, Vector3 pos, Vector3 size, Color color, string id = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name=name; go.transform.position=pos; go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=MakeMaterial(color);
            if (id != null) hotspots.Add(go,id);
            return go;
        }
        GameObject Sphere(string name, Vector3 pos, Vector3 size, Color color, string id = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name=name; go.transform.position=pos; go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=MakeMaterial(color);
            if (id != null) hotspots.Add(go,id);
            return go;
        }
        void Start()
        {
            cam=Camera.main;
            if (cam == null)
            {
                var go=new GameObject("Doll Salon Camera"); cam=go.AddComponent<Camera>(); go.tag="MainCamera";
                go.AddComponent<AudioListener>();
            }
            cam.transform.position=new Vector3(0,5.2f,-12f);
            cam.transform.LookAt(new Vector3(0,1.5f,0));
            cam.fieldOfView=56;
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.035f,.04f,.075f);
            RenderSettings.ambientLight=new Color(.29f,.29f,.4f);
            BuildRoom();
            BuildCast();
        }
        void BuildRoom()
        {
            Cube("Wooden floor",new Vector3(0,-.2f,0),new Vector3(13,.4f,10),new Color(.29f,.16f,.10f));
            Cube("Back wall",new Vector3(0,3.8f,5),new Vector3(13,7.6f,.25f),new Color(.17f,.20f,.26f));
            Cube("Left wall",new Vector3(-6.5f,3,0),new Vector3(.2f,6,10),new Color(.12f,.15f,.21f));
            Cube("Right wall",new Vector3(6.5f,3,0),new Vector3(.2f,6,10),new Color(.12f,.15f,.21f));
            Cube("Carpet",new Vector3(0,.015f,-1.3f),new Vector3(7,.035f,3.3f),new Color(.38f,.055f,.12f));
            for(int i=0;i<7;i++) Cube("Floorboard "+i,new Vector3(-5.1f+i*1.7f,.018f,3),new Vector3(.04f,.03f,9),new Color(.15f,.085f,.06f));
            // Painting and missing photo.
            Cube("Family portrait frame",new Vector3(-3.5f,3.8f,4.73f),new Vector3(2.3f,1.8f,.22f),new Color(.54f,.38f,.14f));
            Cube("Family portrait",new Vector3(-3.5f,3.8f,4.56f),new Vector3(1.95f,1.48f,.12f),new Color(.17f,.22f,.22f),"painting");
            Cube("Torn photograph",new Vector3(-3f,.07f,-1.2f),new Vector3(.55f,.04f,.46f),new Color(.92f,.85f,.63f),"photo");
            // Doll and chair.
            Cube("Rocking chair seat",new Vector3(2.5f,1.0f,2.0f),new Vector3(1.4f,.25f,1.3f),new Color(.36f,.19f,.12f));
            Cube("Rocking chair back",new Vector3(2.5f,2.05f,2.54f),new Vector3(1.4f,2.1f,.2f),new Color(.32f,.16f,.08f));
            for(int i=-1;i<=1;i+=2) Cube("Chair leg",new Vector3(2.5f+i*.54f,.45f,1.9f),new Vector3(.14f,.9f,.14f),new Color(.22f,.12f,.08f));
            Sphere("Doll torso",new Vector3(2.5f,1.75f,1.95f),new Vector3(.85f,1.05f,.6f),new Color(.44f,.25f,.42f),"doll");
            var head=Sphere("Doll head",new Vector3(2.5f,2.65f,1.78f),new Vector3(.78f,.8f,.7f),new Color(.9f,.8f,.64f),"doll");
            dollHead=head.transform;
            Sphere("Doll right eye",new Vector3(2.35f,2.74f,1.43f),new Vector3(.11f,.11f,.11f),Color.black,"doll");
            // Shelf and eye.
            Cube("Shelf",new Vector3(4.8f,2.2f,4.1f),new Vector3(2,.18f,.8f),new Color(.4f,.21f,.09f));
            Sphere("Glass eye",new Vector3(4.8f,2.39f,3.95f),new Vector3(.28f,.28f,.28f),new Color(.56f,.89f,1f),"eye");
            // Music box near foreground.
            Cube("Music box table",new Vector3(-2.5f,.8f,1f),new Vector3(1.7f,1.6f,1.1f),new Color(.27f,.15f,.09f));
            Cube("Locked music box",new Vector3(-2.5f,1.78f,.95f),new Vector3(1.1f,.45f,.8f),new Color(.72f,.51f,.18f),"box");
            // Curtain, hidden key and locked door.
            Cube("Velvet curtain",new Vector3(1f,3.15f,4.68f),new Vector3(1.8f,4.8f,.16f),new Color(.35f,.035f,.085f),"curtain");
            Cube("Library door",new Vector3(5.3f,1.9f,4.7f),new Vector3(1.9f,3.8f,.2f),new Color(.35f,.2f,.1f),"door");
            Sphere("Door knob",new Vector3(4.65f,1.8f,4.47f),new Vector3(.18f,.18f,.18f),new Color(.93f,.75f,.2f),"door");
            Cube("Creaking floorboard",new Vector3(.6f,.07f,.5f),new Vector3(1.5f,.10f,.42f),new Color(.5f,.31f,.16f),"floor");
            var lamp=new GameObject("Flickering moonlight");
            flicker=lamp.AddComponent<Light>(); flicker.type=LightType.Point;
            flicker.range=17; flicker.intensity=4.5f;
            flicker.color=new Color(.55f,.68f,1f);
            lamp.transform.position=new Vector3(-1,4.2f,-1.5f);
        }
        GameObject ActorPart(string name, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 scale, Color color)
        {
            var obj=GameObject.CreatePrimitive(type);
            obj.name=name; obj.transform.SetParent(parent,false);
            obj.transform.localPosition=localPosition;obj.transform.localScale=scale;
            obj.GetComponent<Renderer>().material=MakeMaterial(color);
            // Actors must not cover the interactive objects behind them.
            var collider=obj.GetComponent<Collider>();
            if(collider!=null) Destroy(collider);
            return obj;
        }
        Transform CreateActor(string name, Vector3 position, Color shirt)
        {
            var root=new GameObject(name).transform;
            root.position=position;
            ActorPart("Body",PrimitiveType.Capsule,root,new Vector3(0,.93f,0),new Vector3(.48f,.65f,.4f),shirt);
            ActorPart("Head",PrimitiveType.Sphere,root,new Vector3(0,1.72f,0),new Vector3(.54f,.59f,.53f),new Color(.93f,.72f,.55f));
            ActorPart("Hair",PrimitiveType.Sphere,root,new Vector3(0,1.99f,.04f),new Vector3(.57f,.22f,.57f),new Color(.19f,.12f,.09f));
            ActorPart("Left leg",PrimitiveType.Cube,root,new Vector3(-.15f,.37f,0),new Vector3(.16f,.7f,.25f),new Color(.13f,.15f,.22f));
            ActorPart("Right leg",PrimitiveType.Cube,root,new Vector3(.15f,.37f,0),new Vector3(.16f,.7f,.25f),new Color(.13f,.15f,.22f));
            return root;
        }
        void BuildCast()
        {
            hero=CreateActor("Shon - temporary 3D character",new Vector3(-1.4f,.05f,-2.9f),new Color(.12f,.47f,.88f));
            walkTarget=hero.position;
            friends=new Transform[] {
                CreateActor("Romi - temporary 3D character",new Vector3(-2.6f,.05f,-3.1f),new Color(.82f,.22f,.62f)),
                CreateActor("James - temporary 3D character",new Vector3(.8f,.05f,-3.3f),new Color(.19f,.72f,.44f)),
                CreateActor("Gregory - temporary 3D character",new Vector3(2f,.05f,-3.0f),new Color(.94f,.57f,.19f))
            };
        }
        // Supports both Input System and legacy Input Manager project configurations.
        bool MouseClicked(out Vector2 position)
        {
            position = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame) return false;
            position = mouse.position.ReadValue();
            return true;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (!Input.GetMouseButtonDown(0)) return false;
            position = Input.mousePosition;
            return true;
#else
            return false;
#endif
        }
        void Update()
        {
            if (MouseClicked(out Vector2 mousePosition) && mousePosition.y > 190 && cam != null)
            {
                RaycastHit hit;
                if(Physics.Raycast(cam.ScreenPointToRay(mousePosition),out hit,100f))
                {
                    string id;
                    if(hotspots.TryGetValue(hit.collider.gameObject,out id))
                    {
                        Interact(id);
                    }
                    else if(hit.collider.gameObject.name == "Wooden floor" || hit.collider.gameObject.name == "Carpet")
                    {
                        walkTarget=new Vector3(Mathf.Clamp(hit.point.x,-5.7f,5.7f),.05f,Mathf.Clamp(hit.point.z,-4.2f,4.0f));
                        walking=true;
                    }
                }
            }
            if(hero!=null && walking)
            {
                Vector3 previous=hero.position;
                hero.position=Vector3.MoveTowards(previous,walkTarget,Time.deltaTime*2.7f);
                Vector3 dir=walkTarget-previous;
                if(dir.sqrMagnitude>.005f)
                    hero.rotation=Quaternion.Slerp(hero.rotation,Quaternion.LookRotation(dir),Time.deltaTime*12f);
                if((hero.position-walkTarget).sqrMagnitude<.003f) walking=false;
            }
            if(hero!=null && friends!=null)
            {
                for(int i=0;i<friends.Length;i++)
                {
                    Vector3 follow=hero.position+new Vector3(-1.2f+i*1.1f,0,-.6f-(i%2)*.5f);
                    follow.x=Mathf.Clamp(follow.x,-5.7f,5.7f);
                    follow.z=Mathf.Clamp(follow.z,-4.2f,4.0f);
                    friends[i].position=Vector3.MoveTowards(friends[i].position,follow,Time.deltaTime*2.2f);
                }
            }
            if (dollHead != null && jumpScare && Time.time < scareEnd)
                dollHead.localRotation=Quaternion.Euler(0,Mathf.Sin(Time.time*9)*38f,0);
            else if (dollHead != null) dollHead.localRotation=Quaternion.identity;
            if (flicker != null) flicker.intensity=3.5f+Mathf.PerlinNoise(Time.time*9,.2f)*1.8f;
        }
        void Say(string who,string text) {speaker=who; dialogue=text;}
        void Interact(string id)
        {
            if (selected != "")
            {
                string item=selected;selected="";
                if (item=="photo" && id=="painting")
                {
                    inventory.Remove("photo");completed.Add("portrait");
                    Say("רומי","התמונה הושלמה. חסרה לבובה עין!");
                }
                else if(item=="eye" && id=="doll")
                {
                    inventory.Remove("eye");completed.Add("eye");
                    jumpScare=true; scareEnd=Time.time+2f;
                    Say("הבובה","תודה... עכשיו אפשר לנגן.");
                }
                else if(item=="tinykey" && id=="box" && completed.Contains("eye"))
                {
                    inventory.Remove("tinykey");completed.Add("music");
                    Say("ג'יימס","המנגינה עובדת! הווילון זוהר. זה אף פעם לא סימן טוב.");
                }
                else if(item=="rustkey" && id=="door")
                {
                    inventory.Remove("rustkey"); finished=true;
                    Say("שון","הספרייה נפתחה! מה כבר יכול להשתבש בחדר הבא?");
                }
                else Say("גרגורי","ניסינו לשלב אותם. זה לא עבד. מדהים.");
                return;
            }
            switch(id)
            {
                case "photo":inventory.Add("photo");Say("ג'יימס","מצאתי חצי תמונה מתחת לשטיח.");break;
                case "eye":inventory.Add("eye");Say("רומי","עין מזכוכית. ננסה להחזיר אותה לבובה.");break;
                case "painting":Say("רומי","בתמונה חסר חלק. מה מסתתר בו?");break;
                case "doll":
                    jumpScare=true;scareEnd=Time.time+2f;
                    Say("גרגורי","היא הזיזה את הראש! אני דורש לצאת!");
                    break;
                case "floor":
                    if(completed.Contains("eye")) {inventory.Add("tinykey");Say("שון","מצאנו מפתח קטן בין הקרשים!");}
                    else Say("רומי","משהו תקוע מתחת לקרש. אולי נבין מה אחרי שנתקן את הבובה.");
                    break;
                case "box":Say("ג'יימס","תיבת נגינה נעולה. צריך מפתח קטן.");break;
                case "curtain":
                    if(completed.Contains("music")) {inventory.Add("rustkey");Say("רומי","מצאנו מפתח חלוד מאחורי הווילון!");}
                    else Say("איציק","הווילון לא זז. אולי תיבת הנגינה תשחרר את המנגנון.");
                    break;
                case "door":Say("איציק","הדלת נעולה. חפשו מפתח.");break;
            }
        }
        void OnGUI()
        {
            if (captionStyle==null)
            {
                captionStyle=new GUIStyle(GUI.skin.label){fontSize=24,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,wordWrap=true};
                captionStyle.normal.textColor=new Color(1f,.79f,.38f);
                textStyle=new GUIStyle(GUI.skin.label){fontSize=18,alignment=TextAnchor.MiddleCenter,wordWrap=true};
                textStyle.normal.textColor=Color.white;
                buttonStyle=new GUIStyle(GUI.skin.button){fontSize=17,fixedHeight=40};
            }
            float w=Mathf.Min(950,Screen.width-16);
            GUILayout.BeginArea(new Rect((Screen.width-w)/2,6,w,58));
            GUILayout.Label("SHON ADVENTURE • סלון הבובות בתלת־ממד",captionStyle);
            GUILayout.EndArea();
            GUILayout.BeginArea(new Rect((Screen.width-w)/2,Screen.height-182,w,175),GUI.skin.box);
            GUILayout.Label(speaker+": "+dialogue,textStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("לחץ על הרצפה כדי ללכת | תיק:",textStyle,GUILayout.Width(45));
            foreach(string item in new List<string>(inventory))
            {
                GUI.backgroundColor=selected==item?Color.yellow:Color.white;
                if(GUILayout.Button(names[item],buttonStyle,GUILayout.MaxWidth(185)))
                    selected=selected==item?"":item;
                GUI.backgroundColor=Color.white;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("רמז",buttonStyle)) Say("רומי","אספו חצי תמונה ועין. תקנו את הבובה, חפשו ברצפה, נגנו ופתחו את הווילון.");
            if(GUILayout.Button("אפס בחירת חפץ",buttonStyle)) selected="";
            GUILayout.EndHorizontal();
            if(finished) GUILayout.Label("הפרק הושלם! השלב הבא: הספרייה.",captionStyle);
            GUILayout.EndArea();
        }
    }
}
