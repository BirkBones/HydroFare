using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Elsys_FiskeApp.Model
{
    public class DataHolder
    {
        public static DataHolder Instance;

        public Dictionary<string, Queue<updateData>> newProcessedData; // input : name of merd. Output : the (new) raw input data, the fourier transform and the treated signal.
        public Dictionary<string, List<updateData>> totalProcessedData; // the same, but holds all accumulated data during runtime.
        
        public DispatcherTimer GlobalUpdateTimer { get; private set; } =
                    new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };// Global timer
        public DataHolder() 
        {
            if (Instance == null)
            {
                Instance = this;
                newProcessedData = new Dictionary<string, Queue<updateData>>();
                totalProcessedData = new Dictionary<string, List<updateData>>();
                GlobalUpdateTimer.Start();
                GlobalUpdateTimer.Tick += (sender, e) => { UpdateProcessedData(); }; // make sure the processed data is regularly updated.
            }
        
        
        }

        void UpdateProcessedData() // transfers data from the raw input queue in brokerclientshandler, to the processed data here.
        {
            foreach (var key in MerdsHandler.Instance.inputsDataQueues.Keys)
            {
                var inputQueue = MerdsHandler.Instance.inputsDataQueues[key];
                if (!newProcessedData.ContainsKey(key) || !totalProcessedData.ContainsKey(key))
                {
                    newProcessedData[key] = new Queue<updateData>();
                    totalProcessedData[key] = new List<updateData>();
                }

                var outputQueue = newProcessedData[key];


                while (inputQueue.Count > 0)
                {
                    var rawInput = inputQueue.Dequeue();
                    var processedData = processData(rawInput);

                    outputQueue.Enqueue(processedData); // update the queue
                    totalProcessedData[key].Add(processedData); // save the information.
                }
            }
        }


        private updateData processData(updateData rawInput)
        {
            // Do things with the data here.
            //rawInput.FourierData = rawInput.RawData*rawInput.RawData*rawInput.RawData; // temporary solution so they're not null.
            //rawInput.TreatedSignal = -rawInput.RawData * rawInput.RawData;
            return rawInput;
        }
    }
}
