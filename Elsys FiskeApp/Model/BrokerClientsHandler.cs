using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Elsys_FiskeApp
{
    
    public class BrokerClientsHandler // handles all the clients
    {
        List<BrokerClient> BrokerClients;
        public static BrokerClientsHandler Instance;
        Dictionary<string, Queue<updateData>> inputsDataQueues; // input : name of merd. Output : the input data given from the hydrophone.


        public ObservableCollection<ClientConnectionSettings> ClientsConnectionSettings;
        public BrokerClientsHandler()
        {
            if (Instance == null)
            {
                Instance = this;
                BrokerClients = new List<BrokerClient>();
                inputsDataQueues = new Dictionary<string, Queue<updateData>>();
                LoadClientsConnectionSettings();
                ClientsConnectionSettings.CollectionChanged += (sender, e) => SaveClientsConnectionSettings();

                foreach (var clientConnectionSettings in ClientsConnectionSettings) // Creates and setups brokers and ties input data to the dict.
                {
                    var newClient = new BrokerClient();
                    newClient.setBrokerSetttings(clientConnectionSettings);
                    BrokerClients.Add(newClient);
                    inputsDataQueues.Add(clientConnectionSettings.merdName, newClient.inputData);
                }

                ConnectAllBrokerClients();

            }

        }

        public async Task ConnectAllBrokerClients()
        {
            List<Task> tasks = new List<Task>();
            foreach (var client in BrokerClients)
            {
                tasks.Add(client.ConnectToBroker(CancellationToken.None));
            }
            await Task.WhenAll(tasks);
        }
            
        void LoadClientsConnectionSettings()
        {
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName; // Appdomain.Currentdomain.Basedirectory gives the basedirectory inside the bin. Since the bin is 4 steps inside the main directory, by parenting 4 times, we can access the original .json file.
            string filePath = Path.Combine(projectDirectory, "Model", "ClientsConnectionSettings.json");


            if (File.Exists(filePath))
            {
                string input = File.ReadAllText(filePath);
                ClientsConnectionSettings = JsonSerializer.Deserialize<ObservableCollection<ClientConnectionSettings>>(input) ?? new ObservableCollection<ClientConnectionSettings>();
            }
            else
            {
                ClientsConnectionSettings = new ObservableCollection<ClientConnectionSettings>(); // Just make one if the filepath doesnt exist. We will later save it so 
                //the next time it tries to access filePath, filePath exists. 
            }
        }
        public void SaveClientsConnectionSettings()
        {
            string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string filePath = Path.Combine(projectDirectory, "Model", "ClientsConnectionSettings.json");
            var options = new JsonSerializerOptions { WriteIndented = true };

            string serializedJson = JsonSerializer.Serialize(ClientsConnectionSettings, options);
            File.WriteAllText(filePath, serializedJson);
        }




    }
}



