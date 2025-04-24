using Elsys_FiskeApp.Model;
using MQTTnet;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace Elsys_FiskeApp
{
    
    public class MerdsHandler // handles all the clients
    {
        public Dictionary<string, SingleMerdModel> Merds;
        public static MerdsHandler Instance;
        public Dictionary<string, Queue<updateData>> inputsDataQueues; // input : name of merd. Output : the raw input data given from the hydrophone, along with the time the data was recorded.

        public List<MerdSettings> MerdsSettings;
        ~ MerdsHandler()
        {
            SaveMerdsSettings();
        }
        public MerdsHandler()
        {
            if (Instance == null)
            {
                Instance = this;
                Merds = new Dictionary<string, SingleMerdModel>();
                inputsDataQueues = new Dictionary<string, Queue<updateData>>();
                LoadMerdsSettings();

                foreach (var Settings in MerdsSettings) // Creates and setups all the merdmodels and ties input data to the dict.
                {
                    var newMerd = new SingleMerdModel(Settings);

                    Merds[Settings.MerdName] = newMerd;
                    inputsDataQueues.Add(Settings.MerdName, newMerd.brokerClient.inputData);
                }
            }

        }

        public async Task ConnectAllMerds()
        {
            List<Task> tasks = new List<Task>();
            foreach (var Merd in Merds.Values)
            {
                tasks.Add(Merd.brokerClient.ConnectToBroker());
            }
            await Task.WhenAll(tasks);
        }
            
        void LoadMerdsSettings()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MerdsSettings.json");

            if (File.Exists(filePath))
            {
                string input = File.ReadAllText(filePath);
                MerdsSettings = JsonSerializer.Deserialize<List<MerdSettings>>(input) ?? new List<MerdSettings>();
            }
            else
            {
                MerdsSettings = new List<MerdSettings>(); // Just make one if the filepath doesnt exist. We will later save it so 
            }
            //MerdsSettings.CollectionChanged += (sender, e) => SaveMerdsSettings();

        }
        public void SaveMerdsSettings()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MerdsSettings.json");

            var options = new JsonSerializerOptions { WriteIndented = true };

            string serializedJson = JsonSerializer.Serialize(MerdsSettings, options);
            File.WriteAllText(filePath, serializedJson);
        }




    }
}



