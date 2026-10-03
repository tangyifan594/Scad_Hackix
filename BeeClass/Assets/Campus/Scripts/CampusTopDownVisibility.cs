using System.Collections.Generic;
using UnityEngine;
public class CampusTopDownVisibility : MonoBehaviour
{
    class Entry { public Renderer renderer;public int floor=-1;public bool overhead,wall;public MeshFilter filter;public Mesh original,cutaway; }
    List<Entry> entries=new List<Entry>();CampusWalker player;
    void Awake()
    {
        player=FindFirstObjectByType<CampusWalker>();
        foreach(var renderer in GetComponentsInChildren<Renderer>())
        {
            var e=new Entry{renderer=renderer};
            for(Transform p=renderer.transform;p && p!=transform;p=p.parent)
                if(p.name.StartsWith("Floor ") && int.TryParse(p.name.Substring(6),out int number)){e.floor=number-1;break;}
            string name=renderer.name;
            e.overhead=name=="Roof" || name=="School sign" || name=="School name" || name=="Door lintel" || name.Contains("window") || name=="Elevator landing sign" || name=="Elevator label" || name.StartsWith("Room ") || name.StartsWith("Chalkboard");
            e.wall=name.Contains("wall") || name=="Classroom partition" || name.Contains("shaft") || name.StartsWith("Cabin ");
            if(e.wall)
            {
                e.filter=renderer.GetComponent<MeshFilter>();
                if(e.filter && e.filter.sharedMesh)
                {
                    e.original=e.filter.sharedMesh;e.cutaway=Instantiate(e.original);e.cutaway.name="Low wall visual";
                    var vertices=e.cutaway.vertices;for(int i=0;i<vertices.Length;i++)vertices[i].y=(vertices[i].y+.5f)*.23f-.5f;
                    e.cutaway.vertices=vertices;e.cutaway.RecalculateBounds();
                }
            }
            entries.Add(e);
        }
        Refresh();
    }
    void LateUpdate(){Refresh();}
    void Refresh()
    {
        if(!player)return;
        Vector3 p=player.transform.position;
        bool inside=p.x>-12.6f && p.x<12.6f && p.z>-4.3f && p.z<14.4f;
        int level=Mathf.Clamp(Mathf.FloorToInt((p.y+.5f)/4),0,2);
        foreach(var e in entries)
        {
            if(!e.renderer)continue;
            e.renderer.enabled=!inside || ((!e.overhead) && (e.floor<0 || e.floor==level));
            if(e.filter && e.cutaway)e.filter.sharedMesh=inside?e.cutaway:e.original;
        }
    }
    void OnDestroy(){foreach(var e in entries){if(e.filter && e.original)e.filter.sharedMesh=e.original;if(e.cutaway)Destroy(e.cutaway);}}
}
