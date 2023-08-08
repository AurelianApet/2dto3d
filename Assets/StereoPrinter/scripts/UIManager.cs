using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {
	public GameObject loadingBar;
	// Use this for initialization
	void Start () {
		StartCoroutine (LoadingBar ());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	float smooth = 5.0f;
	float angle = 0.0f;

	IEnumerator LoadingBar(){
		for (;;) {
			if (Global.starting && !Global.processing) {
				loadingBar.SetActive (true);
				angle += 5.0f;
				loadingBar.transform.rotation = Quaternion.Slerp (loadingBar.transform.rotation, Quaternion.Euler (0, 0, angle), Time.deltaTime * smooth);
			} else if (Global.starting && Global.processing) {
				loadingBar.SetActive (false);
				angle = 0.0f;
			}
			yield return new WaitForSeconds (0.01f);
		}
	}
}
