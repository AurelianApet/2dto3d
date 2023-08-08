using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;

public class MeshCreatorManager1 : MonoBehaviour {

	// Use this for initialization
	void Start () {

        CreateMesh();
        /*
        for (int i = 0; i < 4; i++)
        {


            GameObject cloneobj = Instantiate(temp) as GameObject;
            cloneobj.transform.parent = parent;
        }*/
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void CreateMesh()
    {
        
        MeshCreatorData meshData = this.GetComponent<MeshCreatorData>();
        MeshCreator.UpdateMesh(meshData.gameObject,baseMat,transMat,0,null,null);
    }

    public Material baseMat;
    public Material transMat;

    public GameObject temp;
    public Transform parent;

    public Texture2D cutDog;
}
