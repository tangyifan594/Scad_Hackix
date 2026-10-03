using UnityEngine;
using UnityEngine.SceneManagement;

// Adds decorative animals without changing classroom furniture or inventory.
public class ClassroomAnimals : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Populate(SceneManager.GetActiveScene());
    }
    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) { Populate(scene); }
    static void Populate(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Transform room in root.GetComponentsInChildren<Transform>(true))
        {
            if (!room.name.StartsWith("Classroom ") || room.Find("Classroom animals")) continue;
            int number;
            if (!int.TryParse(room.name.Substring(10), out number)) continue;
            int floor = number / 100 - 1, column = number % 100 - 1;
            if (floor < 0 || floor > 2 || column < 0 || column > 2) continue;
            Transform group = new GameObject("Classroom animals").transform;
            group.SetParent(room, false);
            for (int i = 0; i < 2; i++)
            {
                int kind = (column + floor + i) % 3;
                Transform animal = Make(kind);
                animal.SetParent(group, false);
                animal.position = room.TransformPoint(new Vector3(-8 + column * 8 + (i == 0 ? -2.8f : 2.8f), floor * 4 + .03f, 10));
                animal.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }
    static Material Material(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) shader = Shader.Find("Standard");
        Material m = new Material(shader); m.color = color;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", .25f);
        return m;
    }
    static Transform Ball(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        part.name = name; part.transform.SetParent(parent, false);
        part.transform.localPosition = position; part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        Destroy(part.GetComponent<Collider>());
        return part.transform;
    }
    static Transform Make(int kind)
    {
        Transform a = new GameObject(new[] { "Rabbit", "Cat", "Bear" }[kind]).transform;
        Material fur = Material(kind == 0 ? new Color(.94f,.88f,.76f) : kind == 1 ? new Color(.88f,.58f,.29f) : new Color(.56f,.36f,.23f));
        Material cream = Material(new Color(1f,.91f,.73f));
        Material dark = Material(new Color(.15f,.10f,.08f));
        Material pink = Material(new Color(.92f,.55f,.51f));
        Ball(a,"Body",new Vector3(0,.40f,0),new Vector3(.56f,.65f,.46f),fur);
        Transform head = Ball(a,"Head",new Vector3(0,.86f,.03f),new Vector3(.68f,.59f,.56f),fur);
        Ball(a,"Belly",new Vector3(0,.40f,.20f),new Vector3(.35f,.40f,.08f),cream);
        for(int side=-1;side<=1;side+=2)
        {
            Ball(a,"Foot",new Vector3(side*.18f,.10f,.09f),new Vector3(.25f,.18f,.31f),fur);
            Ball(a,"Paw",new Vector3(side*.30f,.48f,.04f),new Vector3(.19f,.32f,.19f),fur).localRotation=Quaternion.Euler(0,0,side*20);
            Ball(head,"Eye",new Vector3(side*.17f,.04f,.47f),new Vector3(.095f,.13f,.065f),dark);
            Ball(head,"Eye sparkle",new Vector3(side*.17f-.013f,.061f,.50f),Vector3.one*.026f,cream);
            Ball(head,"Cheek",new Vector3(side*.29f,-.10f,.43f),new Vector3(.15f,.11f,.04f),pink);
            Vector3 earScale = kind==0 ? new Vector3(.16f,.62f,.18f) : kind==1 ? new Vector3(.26f,.33f,.20f) : new Vector3(.27f,.27f,.19f);
            Transform ear=Ball(a,"Ear",new Vector3(side*.22f,kind==0?1.36f:1.13f,.015f),earScale,fur);
            ear.localRotation=Quaternion.Euler(0,0,-side*12);
            Ball(ear,"Inner ear",new Vector3(0,0,.42f),new Vector3(.58f,.73f,.19f),pink);
        }
        Ball(head,"Muzzle",new Vector3(0,-.14f,.45f),new Vector3(.28f,.21f,.10f),cream);
        Ball(head,"Nose",new Vector3(0,-.08f,.515f),new Vector3(.085f,.06f,.045f),dark);
        Ball(a,"Tail",new Vector3(0,.33f,-.25f),kind==1?new Vector3(.14f,.50f,.14f):Vector3.one*.21f,fur);
        a.gameObject.AddComponent<ClassroomAnimals>();
        return a;
    }
    void Update()
    {
        Transform head=transform.Find("Head");
        if(head) head.localRotation=Quaternion.Euler(0, Mathf.Sin(Time.time*.8f+transform.position.x)*7f, Mathf.Sin(Time.time*1.1f)*2f);
    }
}
