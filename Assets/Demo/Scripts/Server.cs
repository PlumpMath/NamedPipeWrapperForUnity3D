using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using NamedPipeWrapper;

public class Server : MonoBehaviour {
    private readonly NamedPipeServer<string,string> _server = new NamedPipeServer<string,string>(Constants.PIPE_NAME,null);
    private readonly HashSet<string> _clients = new HashSet<string>();
    private string infoamtion;
    void Start()
    {
        _server.ClientConnected += OnClientConnected;
        _server.ClientDisconnected += OnClientDisconnected;
        _server.ClientMessage += (client, message) => AddLine("<b>" + client.Name + "</b>: " + message);
        _server.Error += (x) => { AddLine(x.ToString()); };
        _server.Start();
    }

    private void OnClientConnected(NamedPipeConnection<string, string> connection)
    {
        _clients.Add(connection.Name);
        AddLine("<b>" + connection.Name + "</b> connected!");
        UpdateClientList();
        connection.PushMessage("Welcome!  You are now connected to the server.");
    }

    private void OnClientDisconnected(NamedPipeConnection<string, string> connection)
    {
        _clients.Remove(connection.Name);
        AddLine("<b>" + connection.Name + "</b> disconnected!");
        UpdateClientList();
    }
    void OnGUI()
    {
        if (GUILayout.Button("clear")){
            infoamtion = "";
        }

        GUILayout.Label(infoamtion);
    }
    private void AddLine(string html)
    {
        infoamtion += html.Substring(0,20) + "\n";
    }

    private void UpdateClientList()
    {
    //    //listBoxClients.Invoke(new Action(UpdateClientListImpl));
    ////}

    //private void UpdateClientListImpl()
    //{
        //listBoxClients.Items.Clear();
        foreach (var client in _clients)
        {
            //listBoxClients.Items.Add(client);
            Debug.Log(client);
        }
    }

    public void buttonSend_Click()
    {
        //if (string.IsNullOrWhiteSpace(textBoxMessage.Text))
        //    return;

        //_server.PushMessage(textBoxMessage.Text);
        //textBoxMessage.Text = "";
        _server.PushMessage("hellow");
    }
    void OnDisable()
    {
        _server.Stop();
    }
}
