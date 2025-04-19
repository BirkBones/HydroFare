using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using MQTTnet.Server;
using MQTTnet.Server.EnhancedAuthentication;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Net;
using System.Text.Json;
using System.Numerics;
using Elsys_FiskeApp;
using System.Diagnostics;
using MQTTnet.Exceptions;

namespace MQTTnet;

public class BrokerClient
{

    public IMqttClient brokerClient;
    public string brokerName;
    string ip;
    int port;
    public bool isConnected { get { return brokerClient.IsConnected; } }
    public Queue<Elsys_FiskeApp.updateData> inputData; // Contains all input for the given brokerclient.

    public BrokerClient(ClientConnectionSettings settings)
    {
        setBrokerSetttings(settings);
        ConnectToBroker(CancellationToken.None);
        inputData = new Queue<updateData>();
    }
    ~BrokerClient()
    {
        DisconnectFromBroker();
    }

    public void setBrokerSetttings(ClientConnectionSettings settings)
    {
        brokerName = settings.merdName;
        ip = settings.ip;
        port = settings.port;
    }
    public async Task ConnectToBroker(CancellationToken token) // Returns the result of the connection
    {

        MqttClientFactory mqttFactory = new MqttClientFactory();
        brokerClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(ip, port).Build();


        try
        {
            MqttClientConnectResult result = await brokerClient.ConnectAsync(mqttClientOptions, token);
            if (result.ResultCode == MqttClientConnectResultCode.Success)
            {
                brokerClient.ApplicationMessageReceivedAsync -= HandleMessageReceived;
                brokerClient.ApplicationMessageReceivedAsync += HandleMessageReceived;
                brokerClient.DisconnectedAsync += HandleDisconnected;
                Debug.WriteLine("Connection of broker: " + brokerName + " to IP " + ip + " on port " + port + " was successful!");
            }
            else
            {
                brokerClient.DisconnectedAsync -= HandleDisconnected;

                Debug.WriteLine("Connection of broker: " + brokerName + " to IP " + ip + " on port " + port + " was unsuccessful. Reason: " + result.ReasonString);
            }
        }
        catch (MqttCommunicationException ex)
        {
            Debug.WriteLine("Connection of broker: " + brokerName + " to IP " + ip + " on port " + port + " was unsuccessful. Reason: " + ex.Message);

        }
        catch (Exception ex)
        {
            Debug.WriteLine("Connection of broker: " + brokerName + " to IP " + ip + " on port " + port + " was unsuccessful. Reason: " + ex.Message);
        }

    }
    
    public async Task DisconnectFromBroker()
    {
        await brokerClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder()
        .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
        .Build());
        brokerClient.Dispose();

    }

    public async Task Subscribe(string topic, MqttQualityOfServiceLevel qos, CancellationToken token)
    {
        var subOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic(topic).WithQualityOfServiceLevel(qos)) 
            .Build();

        await brokerClient.SubscribeAsync(subOptions, token);
        Debug.WriteLine($"MQTT client '{brokerName}' subscribed to topic '{topic}' with QoS {qos}.");
    }

    public async Task UnSubscribe(string topic, CancellationToken token)
    {
        MqttClientUnsubscribeOptions unSubOptions = new MqttClientUnsubscribeOptionsBuilder().
        WithTopicFilter(topic).Build();
        
        await brokerClient.UnsubscribeAsync(unSubOptions, token);
        Debug.WriteLine($"MQTT client '{brokerName}' unsubscribed from topic '{topic}'.");
    }
    public async Task PublishMessage(string topic, string Message, MqttQualityOfServiceLevel QOSL)
    {
        var applicationMessage = new MqttApplicationMessageBuilder()
            .WithTopic(topic).WithPayload(Message).WithQualityOfServiceLevel(QOSL)
            .Build();

        await brokerClient.PublishAsync(applicationMessage);
    }

    public async Task PublishFloatArray(string topic, float[] values, MqttQualityOfServiceLevel QOSL)
    {
        string payload = JsonSerializer.Serialize(values);
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic).WithPayload(payload).WithQualityOfServiceLevel(QOSL)
            .Build();
        await brokerClient.PublishAsync(message);
    }


    private Task HandleMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
        string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        Debug.WriteLine($"Received message on topic '{e.ApplicationMessage.Topic}': {payload}");
        if (e.ApplicationMessage.Topic == "rawData") // Uploads the data to the inputdata queue.
        {
            var inputList = interpretInput(payload);
            inputList.ForEach(input => inputData.Enqueue(input));
        }


        return Task.CompletedTask;
    }

    private async Task HandleDisconnected(MqttClientDisconnectedEventArgs e)
    {
        Debug.WriteLine("Broker named: " + brokerName + " on IP " + ip + " and port " + port + " was disconnected. Reason : " + e.Reason);
        await Task.CompletedTask;
    }

    List<updateData> interpretInput(string input) // currently assumes format "(x1,y1), (x2, y2), ..."
    {
        var totalInput = input // totalInput consists of multiple updatedata gathered over time.
            .Split(new[] { "), " }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p =>
            {
                var trimmed = p.Trim('(', ')');
                var parts = trimmed.Split(',');
                var inputData = new updateData { Time = float.Parse(parts[0]), RawData = float.Parse(parts[1]) };
                return inputData;
            })
            .ToList();
        return totalInput;
      
    }


}


