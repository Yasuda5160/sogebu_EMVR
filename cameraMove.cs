﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMove : MonoBehaviour
{
    private float unitAccel = 0.1f;

    Vector3 playraccelplayrrestframe3;//ACCEL3
    Vector4 playraccelplayrrestframe4;//ACCEL4

    Vector3 playraccelworldframe3;//accela3
    Vector4 playraccelworldframe4;//accela4

    public Vector3 playrvelworldframe3;//u3
    public Vector4 playrvelworldframe4;//u4
    Vector3 u3hat;
    public Vector4 playrposworldframe4;//xx4
    public Vector3 playrposworldframe3;
    public Vector3 playrposplayrrestframe3;
    public Vector4 playrposplayrrestframe4;
    public Matrix4x4 Lplayer;
    public Matrix4x4 Lplayerinverse;
    private float qom;
    public GameObject targetObject;
    public Matrix4x4 metrictensor;
    private Matrix4x4 aq;
    ArrowDirection ad;
    public Matrix4x4 R;
    public Vector4 LorentzForceworldframe;
    public List<Vector4> ppoL = new List<Vector4>();
    public Vector4 xint;//intersection of worldline with PLC

    // Use this for initialization
    void Start()
    {
        ad = targetObject.GetComponent<ArrowDirection>();
        R = Matrix4x4.identity;
        playrposworldframe4 = new Vector4(transform.position.x, transform.position.y, transform.position.z, 0f);
        playrvelworldframe3 = new Vector3(0.0f, 0.0f, 0.0f);
        playrvelworldframe4 = new Vector4(playrvelworldframe3.x, playrvelworldframe3.y, playrvelworldframe3.z, 1f);
        u3hat = playrvelworldframe3.normalized;
        Lplayer = Matrix4x4.identity;
        metrictensor = Matrix4x4.identity;
        metrictensor.m33 = -1;
        qom = -0.01f;
        aq = ad.q;
        Debug.Log($"Q0={ad.q}");
        ppoL.Add(playrposworldframe4);
    }

    // Update is called once per frame
    void Update()
    {
        aq = ad.q;
        Quaternion q = transform.rotation.normalized;
        //Defining Rotation Matrix by using Quartanion
        R.m00 = q.x * q.x - q.y * q.y - q.z * q.z + q.w * q.w;
        R.m01 = 2 * (q.x * q.y - q.z * q.w);
        R.m02 = 2 * (q.x * q.z + q.y * q.w);
        R.m10 = 2 * (q.x * q.z + q.y * q.w);
        R.m11 = -q.x * q.x + q.y * q.y - q.z * q.z + q.w * q.w;
        R.m12 = 2 * (q.y * q.z - q.x * q.w);
        R.m20 = 2 * (q.x * q.z - q.y * q.w);
        R.m21 = 2 * (q.y * q.z + q.x * q.w);
        R.m22 = -q.x * q.x - q.y * q.y + q.z * q.z + q.w * q.w;

        //Player's Input
        if (Input.GetKey(KeyCode.RightArrow))
        {
            playraccelplayrrestframe3 = R * Vector3.up * unitAccel;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            playraccelplayrrestframe3 = R * Vector3.down * unitAccel;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            playraccelplayrrestframe3 = R * Vector3.forward * unitAccel;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            playraccelplayrrestframe3 = R * Vector3.back * unitAccel;
        }
        else
        {
            playraccelplayrrestframe3 = Vector3.zero;
        }

        //L has upper and lower indices: (0,1,2,3) is (x,y,z,w), where w is t.
        Lplayer = LTrans(playrvelworldframe3);

        Lplayerinverse = Lplayer.inverse;

        //LForce is a vector in world coordinate space
        LorentzForceworldframe = qom * (metrictensor * ad.f * playrvelworldframe4);

        playraccelplayrrestframe4 = new Vector4(playraccelplayrrestframe3.x, playraccelplayrrestframe3.y, playraccelplayrrestframe3.z, 0);
        playraccelworldframe4 = Lplayerinverse * playraccelplayrrestframe4 + LorentzForceworldframe - playrvelworldframe4.normalized * 0.1f;// 0.15f
        playraccelworldframe3 = new Vector3(playraccelworldframe4.x, playraccelworldframe4.y, playraccelworldframe4.z);

        playrvelworldframe3 += playraccelworldframe3 * Time.deltaTime;
        //u3 *= 0.98f;
        playrvelworldframe4 = new Vector4(playrvelworldframe3.x, playrvelworldframe3.y, playrvelworldframe3.z, Mathf.Sqrt(1f + playrvelworldframe3.sqrMagnitude));
        u3hat = playrvelworldframe3.normalized;

        playrposworldframe4 += playrvelworldframe4 * Time.deltaTime;
        playrposworldframe3 = playrposworldframe4;
        playrposplayrrestframe4 = Lplayer * playrposworldframe4;
        playrposplayrrestframe3 = new Vector3(playrposplayrrestframe4.x, playrposplayrrestframe4.y, playrposplayrrestframe4.z);
        transform.position = playrposplayrrestframe3;

        //add a latest position to position list
        ppoL.Add(playrposworldframe4);

        //debugging functions
        Debug.Log($"L={Lplayer}");
        Debug.Log($"AD.Q={aq}");
        Debug.Log($"addf={ad.f}");
        Debug.Log($"addq={ad.q}");
        Debug.Log($"LForce={LorentzForceworldframe}");
        Debug.Log($"Linv={Lplayerinverse}");
        Debug.Log($"ACCEL4={playraccelplayrrestframe4}");
        Debug.Log($"accel4={playraccelworldframe4}");
        Debug.Log($"u4={playrvelworldframe4}");
        Debug.Log($"x4 ={playrposworldframe4}");
        Debug.Log($"u4={playrvelworldframe4}");
    }
    public Vector3 vp(Vector3 v1, Vector3 v2) //calculate vectorproduct
    {
        //private Vector3 v3;
        return new Vector3(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
    }

    public Vector4 cont(Matrix4x4 m1, Vector4 v1)　//contraction between rank2 tensor and vector
    {
        return new Vector4(m1.m00 * v1.x + m1.m01 * v1.y + m1.m02 * v1.z + m1.m03 * v1.w, m1.m10 * v1.x + m1.m11 * v1.y + m1.m12 * v1.z + m1.m13 * v1.w, m1.m20 * v1.x + m1.m21 * v1.y + m1.m22 * v1.z + m1.m23 * v1.w, 0);
    }
    public Vector4 cor(Matrix4x4 m1, Vector4 v1)
    {
        return new Vector4(m1.m03 * v1.w, m1.m13 * v1.w, m1.m23 * v1.w, 0);
    }
    public float lip(Vector4 v1, Vector4 v2) //Lorentzian inner product
    {
        return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z - v1.w * v2.w;
    }

    public float lSqN(Vector4 v) //Lorentzian squared norm
    {
        return v.x * v.x + v.y * v.y + v.z * v.z - v.w * v.w;
    }

    public Matrix4x4 LTrans(Vector3 u3)
    {
        Vector3 u3hat = u3.normalized;
        Vector4 u4 = new Vector4(u3.x, u3.y, u3.z, Mathf.Sqrt(1f + u3.sqrMagnitude));
        Matrix4x4 L = Matrix4x4.identity;
        //Lply has upper and lower indices: (0,1,2,3) is (x,y,z,w), where w is t.
        L.m00 = 1f + (u4.w - 1f) * u3hat.x * u3hat.x;
        L.m11 = 1f + (u4.w - 1f) * u3hat.y * u3hat.y;
        L.m22 = 1f + (u4.w - 1f) * u3hat.z * u3hat.z;

        L.m01 = (u4.w - 1f) * u3hat.x * u3hat.y;
        L.m02 = (u4.w - 1f) * u3hat.x * u3hat.z;
        L.m10 = (u4.w - 1f) * u3hat.y * u3hat.x;
        L.m12 = (u4.w - 1f) * u3hat.y * u3hat.z;
        L.m20 = (u4.w - 1f) * u3hat.z * u3hat.x;
        L.m21 = (u4.w - 1f) * u3hat.z * u3hat.y;
        L.m03 = (-1) * u4.x;
        L.m13 = (-1) * u4.y;
        L.m23 = (-1) * u4.z;
        L.m30 = (-1) * u4.x;
        L.m31 = (-1) * u4.y;
        L.m32 = (-1) * u4.z;

        L.m33 = u4.w;
        return L;
    }

}