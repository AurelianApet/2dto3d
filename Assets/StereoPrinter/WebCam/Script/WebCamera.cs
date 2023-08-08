using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebCamera : MonoBehaviour
{
    public Material _mWebCamMaterial = null;

    private WebCamTexture _mTexture = null;

    private bool _mSelected = false;

    // Use this for initialization
    void Start()
    {
        initWebCam();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void initWebCam()
    {
        if (_mSelected)
        {
            return;
        }
        if (null == _mWebCamMaterial)
        {
            return;
        }
        WebCamDevice[] devices = WebCamTexture.devices;
        foreach (WebCamDevice device in devices)
        {
            
                _mSelected = true;
                _mTexture = new WebCamTexture(device.name, 640, 480, 30);
                _mTexture.Play();
                _mWebCamMaterial.mainTexture = _mTexture;
            
        }
    }

    private void OnGUI()
    {
        
    }
}
