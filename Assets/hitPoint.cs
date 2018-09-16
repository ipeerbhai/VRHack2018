using UnityEngine;
using System.Collections;

public class hitPoint : MonoBehaviour
{
    public GameObject redDot;


    void Update()
    {
        /*
        RaycastHit hit;
        Ray ray = new Ray(this.transform.position, this.transform.forward);

        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("did hit");
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

            redDot.transform.position = hit.point;
            redDot.transform.rotation = transform.rotation;
        }

        else Debug.Log("didn't hit");
        */
        redDot.transform.position = new Vector3(0f, 0f, 0f);
        redDot.transform.Translate(0, 0, 50f, transform);


    }
}