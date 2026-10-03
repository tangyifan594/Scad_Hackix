using UnityEngine;
public class CampusBeeMotion : MonoBehaviour
{
    public Transform leftWing, rightWing;
    Vector3 rest;Quaternion leftRest,rightRest;
    void Awake(){rest=transform.localPosition;if(leftWing)leftRest=leftWing.localRotation;if(rightWing)rightRest=rightWing.localRotation;}
    void Update(){transform.localPosition=rest+Vector3.up*(Mathf.Sin(Time.time*2)*.03f);float flap=Mathf.Sin(Time.time*20)*12;if(leftWing)leftWing.localRotation=leftRest*Quaternion.Euler(0,0,-flap);if(rightWing)rightWing.localRotation=rightRest*Quaternion.Euler(0,0,flap);}
}
