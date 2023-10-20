#region StandardUsing
using System;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.OPCUAServer;

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


#endregion
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.IO;
using FTOptix.SQLiteStore;
using FTOptix.Store;
using FTOptix.ODBCStore;
using FTOptix.DataLogger;
using FTOptix.Recipe;
using Newtonsoft.Json.Linq;
using System.Linq;
using FTOptix.WebUI;

public class PublicLogic : BaseNetLogic
{
    private PeriodicTask MiTask;
    public override void Start()
    {
        // Configure MQTT credentials
        var broker = LogicObject.GetVariable("Broker");
        string brokerIpAddress = broker.Value;
        int port = LogicObject.GetVariable("Port").Value;
        string clientID = LogicObject.GetVariable("ClientID").Value;
        string username = LogicObject.GetVariable("Username").Value;
        string password = LogicObject.GetVariable("Password").Value;

        publishClient = new MqttClient(brokerIpAddress, port, true, null, null, MqttSslProtocols.TLSv1_2);
        //publishClient = new MqttClient(brokerIpAddress);

        // Connect to the broker
        publishClient.Connect(clientID, username, password, false, 60);
        // Assign a callback to be executed when a message is published to the broker
        publishClient.MqttMsgPublished += PublishClientMqttMsgPublished;


        MiTask = new PeriodicTask(PublishMessage, 250, LogicObject);
        MiTask.Start();

    }

    public override void Stop()
    {
        publishClient.Disconnect();
        publishClient.MqttMsgPublished -= PublishClientMqttMsgPublished;
    }

    private void PublishClientMqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
    {
        //Log.Info("Message " + e.MessageId + " - published = " + e.IsPublished);
    }

    public void PublishMessage()
    {
        var dataLoggerStore = LogicObject.GetVariable("DataLogger").Value;
        var dataInfo = InformationModel.Get<DataLogger>(dataLoggerStore);

        // Log.Info(dataInfo.TableName);

        // foreach (var variableToLog in dataInfo.VariablesToLog.ToList())
        // {
        //     Log.Info(variableToLog.BrowseName);
        //     Log.Info(variableToLog.Value);
        // }

        var dataToSerialize = new
        {
            TableName = dataInfo.TableName,
            VariablesToLog = dataInfo.VariablesToLog.Select(v => new {
                BrowseName = v.BrowseName,
                Value = v.Value
            })
        };

        var json = JsonConvert.SerializeObject(dataToSerialize);
        var topic = LogicObject.GetVariable("Topic");

        ushort msgId = publishClient.Publish(topic.Value, // topic
            System.Text.Encoding.UTF8.GetBytes(json), // message body
            MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, // QoS level
            false); // retained

    }


    private MqttClient publishClient;


}
