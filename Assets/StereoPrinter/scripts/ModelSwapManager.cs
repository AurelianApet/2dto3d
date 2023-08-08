using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSwapManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        onGenerateDog();
    }
	
	// Update is called once per frame
	void Update () {
        
	}

    public void onGenerateDog()
    {
        Destroy(GameObject.Find("dog"));
        Destroy(GameObject.Find("clock"));
        Destroy(GameObject.Find("cutDog"));
        GameObject m_dog = Instantiate(dog);
        m_dog.name = "dog";
    }

    public void onGenerateClock()
    {
        Destroy(GameObject.Find("dog"));
        Destroy(GameObject.Find("clock"));
        Destroy(GameObject.Find("cutDog"));
        GameObject m_clock = Instantiate(clock);
        m_clock.name = "clock";
    }

    public void onGenerateCutDog()
    {
        Destroy(GameObject.Find("dog"));
        Destroy(GameObject.Find("clock"));
        Destroy(GameObject.Find("cutDog"));
        GameObject m_cutDog = Instantiate(cutDog);
        m_cutDog.name = "cutDog";
    }

    public GameObject dog;
    public GameObject clock;
    public GameObject cutDog;
}
