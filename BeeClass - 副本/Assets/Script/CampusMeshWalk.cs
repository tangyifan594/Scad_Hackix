using UnityEngine;
public class CampusMeshWalk : MonoBehaviour
{
    Renderer targetRenderer;
    MaterialPropertyBlock properties;
    Vector3 previous;
    float gait;
    void Awake() { targetRenderer=GetComponent<Renderer>(); properties=new MaterialPropertyBlock(); previous=transform.position; }
    void LateUpdate()
    {
        float speed=Vector3.Distance(transform.position,previous)/Mathf.Max(Time.deltaTime,.001f); previous=transform.position;
        gait=Mathf.MoveTowards(gait,Mathf.Clamp01(speed/2.8f),Time.deltaTime*5);
        targetRenderer.GetPropertyBlock(properties); properties.SetFloat("_Gait",gait);targetRenderer.SetPropertyBlock(properties);
    }
}
