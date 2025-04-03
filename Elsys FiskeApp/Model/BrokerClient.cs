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

namespace MQTTnet;

public class BrokerClient
{

    public IMqttClient brokerClient;
    public string brokerName;
    string ip;
    int port;
    public Queue<Elsys_FiskeApp.updateData> inputData; // Contains all input for the given brokerclient.


    ~BrokerClient()
    {
        DisconnectFromBroker();
    }

    public void setBrokerSetttings(Elsys_FiskeApp.ClientConnectionSettings settings)
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
        MqttClientConnectResult result = await brokerClient.ConnectAsync(mqttClientOptions, token);
        if (result.ResultCode == MqttClientConnectResultCode.Success)
        {
            brokerClient.ApplicationMessageReceivedAsync -= HandleMessageReceived;
            brokerClient.ApplicationMessageReceivedAsync += HandleMessageReceived;
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
        Console.WriteLine($"MQTT client subscribed to topic '{topic}' with QoS {qos}.");
    }

    public async Task UnSubscribe(string topic, CancellationToken token)
    {
        MqttClientUnsubscribeOptions unSubOptions = new MqttClientUnsubscribeOptionsBuilder().
        WithTopicFilter(topic).Build();

        await brokerClient.UnsubscribeAsync(unSubOptions, token);
        Console.WriteLine($"MQTT client unsubscribed from topic '{topic}'.");
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
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        Console.WriteLine($"Received message on topic '{e.ApplicationMessage.Topic}': {payload}");
        return Task.CompletedTask;
    }


}


