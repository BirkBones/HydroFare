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
using System.Data;
using System.Windows.Input;
using Elsys_FiskeApp.Model;

namespace MQTTnet;

public class BrokerClient : ViewModelBase
{

    public IMqttClient brokerClient;
    public string brokerName;
    public string ip { get; set; }
    int port;

    public event Action? ConnectionStateChanged;

    public MqttClientConnectionStatus _connectionStatus { set;  get; } = MqttClientConnectionStatus.Disconnected;

    public MqttClientConnectionStatus ConnectionStatus
    {
        set { _connectionStatus = value; ConnectionStateChanged?.Invoke(); }
        get { return _connectionStatus; }
    } 

    public CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public Queue<Elsys_FiskeApp.updateData> inputData; // Contains all input for the given brokerclient.

    public BrokerClient(MerdSettings settings)
    {
        setBrokerSetttings(settings);
        ConnectToBroker();
        inputData = new Queue<updateData>();
        
    }
    ~BrokerClient() // should fix this to implement idisposable, so it actually waits for it to disconnect.
    {
        DisconnectFromBroker();
    }

    public void setBrokerSetttings(MerdSettings settings)
    {
        brokerName = settings.MerdName; 
        ip = settings.Ip;
        port = settings.Port;

    }

    public async Task AbortConnectionAttempt()
    {
        await Task.Run(()=>cancellationTokenSource.Cancel()); // if there is one, cancels the ongoing connection attempt.
        cancellationTokenSource = new CancellationTokenSource();
    }
    public async Task ConnectToBroker() // Returns the result of the connection
    {

        if (ConnectionStatus == MqttClientConnectionStatus.Connecting) await AbortConnectionAttempt(); // if there is one, cancels the ongoing connection attempt.
        var token = cancellationTokenSource.Token;
        MqttClientFactory mqttFactory = new MqttClientFactory();
        brokerClient = mqttFactory.CreateMqttClient();
        brokerClient.DisconnectedAsync += async e => { ConnectionStatus = MqttClientConnectionStatus.Disconnected; };
        brokerClient.ConnectedAsync += async e => { ConnectionStatus = MqttClientConnectionStatus.Connected; };
        brokerClient.ConnectingAsync += async e => { ConnectionStatus = MqttClientConnectionStatus.Connecting; };

        var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(ip, port).Build(); // idk really know for sure right now that the ongoing connection attempt has had the time to be canceled.


        try
        {
            MqttClientConnectResult result = await brokerClient.ConnectAsync(mqttClientOptions, token);
            if (result.ResultCode == MqttClientConnectResultCode.Success)
            {
                brokerClient.ApplicationMessageReceivedAsync -= HandleMessageReceived;
                brokerClient.ApplicationMessageReceivedAsync += HandleMessageReceived;
                brokerClient.DisconnectedAsync += HandleDisconnected;
                Debug.WriteLine("Connection of broker: " + brokerName + " to IP " + ip + " on port " + port + " was successful!");
                return;

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
            brokerClient.DisconnectedAsync -= HandleDisconnected;


        }
        catch (Exception ex)
        {
            Debug.WriteLine("Connection of broker: " + brokerName + " to IP " + ip + " on port " + port + " was unsuccessful. Reason: " + ex.Message);
            brokerClient.DisconnectedAsync -= HandleDisconnected;

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
        if (ConnectionStatus == MqttClientConnectionStatus.Connected)
        {
            var subOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(topic).WithQualityOfServiceLevel(qos))
                .Build();

            await brokerClient.SubscribeAsync(subOptions, token);
            Debug.WriteLine($"MQTT client '{brokerName}' subscribed to topic '{topic}' with QoS {qos}.");
        }
        else
        {
            Debug.WriteLine("BrokerClient cannot subscribe to a topic when its disconnected.");
            //throw new Exception("BrokerClient cannot subscribe to a topic when its disconnected.");
        }
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
        Debug.WriteLine("BrokerClient named: " + brokerName + " on IP " + ip + " and port " + port + " was disconnected. Reason : " + e.Reason);
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


