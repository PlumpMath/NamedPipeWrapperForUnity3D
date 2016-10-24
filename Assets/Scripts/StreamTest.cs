using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;

[Serializable]
public class DataTest
{
    public int type;
    public string infomation;
}

public class StreamTest : MonoBehaviour
{
    StreamWrapper<DataTest> testWarp;
    FileStream stream;
    DataTest testData;
    void Start()
    {
        testData = new DataTest();
        testData.type = 7;
        testData.infomation = "hhhhhhhhhh";

        stream = new FileStream(Application.dataPath + "/test.bin", FileMode.OpenOrCreate);
        testWarp = new StreamWrapper<DataTest>(stream);
    }


    void OnGUI()
    {
        if (GUILayout.Button("Write"))
        {
            if (testWarp.CanWrite)
            {
                testWarp.WriteObject(testData);
                Debug.Log("写入你好");
            }
        }
        if (GUILayout.Button("Read"))
        {
            if (testWarp.CanRead)
            {
                DataTest dga = (DataTest)testWarp.ReadObject();
                if (dga != null)
                {
                    Debug.Log(dga.type);
                    Debug.Log(dga.infomation);
                }
            }
        }
    }
}
