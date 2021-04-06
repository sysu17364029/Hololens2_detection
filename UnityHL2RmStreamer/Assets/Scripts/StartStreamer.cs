﻿using System.Runtime.InteropServices;
using UnityEngine;

public class StartStreamer : MonoBehaviour
{

#if ENABLE_WINMD_SUPPORT
    [DllImport("HL2RmStreamUnityPlugin", EntryPoint = "StartStreaming", CallingConvention = CallingConvention.StdCall)]
    public static extern void StartStreaming();
#endif

    // Start is called before the first frame update
    void Start()
    {
#if ENABLE_WINMD_SUPPORT
        StartStreaming();
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
}