using System;
using MQTTnet;

namespace DataCollection
{ 
public class DataGathering
	{
		public DataGathering()
		{
			



		}

		public void ConnectToHost()
		{
            string broker = "MainBroker";
            int port = 8883;
            string clientId = Guid.NewGuid().ToString();
            string topic = "Control";
            string username = "emqxtest";
            string password = "******";

            // Create a MQTT client factory
            var factory = new MqttClientFactory();

            // Create a MQTT client instance
            var mqttClient = factory.CreateMqttClient();
           

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port) // MQTT broker address and port
                .WithCredentials(username, password) // Set username and password
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();
        }

	}

}