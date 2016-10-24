using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using NamedPipeWrapper;

public class Client : MonoBehaviour {
    private readonly NamedPipeClient<string,string> _client = new NamedPipeClient<string, string>(Constants.PIPE_NAME,".");
    public string infomation;
    void Start() { 
        _client.ServerMessage += OnServerMessage;
        _client.Disconnected += OnDisconnected;
        _client.Error += (x) => { AddLine(x.ToString()); };
        _client.Start();
    }

    private void OnServerMessage(NamedPipeConnection<string, string> connection, string message)
    {
        AddLine("<b>Server</b>: " + message);
    }

    private void OnDisconnected(NamedPipeConnection<string, string> connection)
    {
        AddLine("<b>Disconnected from server</b>");
    }

    private void AddLine(string html)
    {
        infomation += html + "\n";
    }
    void OnGUI()
    {
        if (GUILayout.Button("clean"))
        {
            infomation = "";
        }
        GUILayout.Label(infomation);
    }

    public void buttonSend_Click()
    {
        _client.PushMessage("hellw server");
    }

    void OnDistroy()
    {
        _client.Stop();
    }
}
