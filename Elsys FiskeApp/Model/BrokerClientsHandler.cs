using MQTTnet;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace Elsys_FiskeApp
{
    
    public class BrokerClientsHandler // handles all the clients
    {
        public Dictionary<string, BrokerClient> BrokerClients;
        public static BrokerClientsHandler Instance;
        public Dictionary<string, Queue<updateData>> inputsDataQueues; // input : name of merd. Output : the raw input data given from the hydrophone, along with the time the data was recorded.

        public ObservableCollection<ClientConnectionSettings> ClientsConnectionSettings;
        public BrokerClientsHandler()
        {
            if (Instance == null)
            {
                Instance = this;
                BrokerClients = new Dictionary<string, BrokerClient>();
                inputsDataQueues = new Dictionary<string, Queue<updateData>>();
                LoadClientsConnectionSettings();
                ClientsConnectionSettings.CollectionChanged += (sender, e) => SaveClientsConnectionSettings();

                foreach (var clientConnectionSettings in ClientsConnectionSettings) // Creates and setups brokers and ties input data to the dict.
                {
                    var newClient = new BrokerClient(clientConnectionSettings); // Initializes connectionsettings of broker, and attempts to connect.
                    BrokerClients[clientConnectionSettings.merdName] = newClient;
                    inputsDataQueues.Add(clientConnectionSettings.merdName, newClient.inputData);
                }

                //ConnectAllBrokerClients();

            }

        }

        public async Task ConnectAllBrokerClients()
        {
            List<Task> tasks = new List<Task>();
            foreach (var client in BrokerClients.Values)
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



