﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox : MonoBehaviour
{
    public Material[] sky;
    int num = 0;
    private Matrix4x4 Linv;
    public Vector3 xn3;
    Camera cam;
    cameraMove2 c;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        c = cam.GetComponent<cameraMove2>();
        Linv = c.Lplayer.inverse;
        /*Output of this Lorentz matrix is on debug console*/
        Debug.Log($"lmat = {Linv}");
        RenderSettings.skybox = sky[num];
    }

    // Update is called once per frame
    void Update()
    {
        Linv = c.Lplayer.inverse;
        Shader.SetGlobalMatrix("L", Linv);
        Debug.Log($"lmat = {Linv}");
    }
}
