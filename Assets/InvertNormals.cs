using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertNormals : MonoBehaviour {

    public void InvertSphere()
    {
        //string me = this.gameObject.name;
        // to invert the normals, we need to update the mesh normals and update the triangles
        Vector3[] normals = GetComponent<MeshFilter>().mesh.normals; // This creates a copy of the mesh normals, not a reference.
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        GetComponent<MeshFilter>().mesh.normals = normals;

        // Now the triangles
        int[] triangles = GetComponent<MeshFilter>().mesh.triangles; // this creates a copy of the mesh triangles, not a reference.
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int t = triangles[i];
            triangles[i] = triangles[i + 2];
            triangles[i + 2] = t;
        }

        GetComponent<MeshFilter>().mesh.triangles = triangles;

    }

	// Use this for initialization
	void Start () {
        InvertSphere();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
