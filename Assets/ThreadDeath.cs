using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ThreadDeath : MonoBehaviour {
    Thread thread;
    void Start()
    {
        thread = new Thread(Run);
        thread.Start();
    }
    private int i;
    void Run()
    {
        while(true)
        {
            i++;
        }
    }
    void OnDisable()
    {
        thread.Abort();
        Debug.Log("quit");
    }
    //void OnGUI()
    //{
    //    GUILayout.Label(i.ToString());
    //}
}
