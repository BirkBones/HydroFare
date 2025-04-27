using CsvHelper.Configuration.Attributes;
using OxyPlot;

namespace Elsys_FiskeApp
{
    public struct Tuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;
        public Tuple(T1 item1, T2 item2) { Item1 = item1; Item2 = item2; }
    }

    public struct updateData
    {
        [Name("filtered_data")] 
        public float TreatedSignal { get; set; }
        [Name("ffts")]
        public List<DataPoint> FourierData { get; set; }
        [Name("raw_data")]
        public float RawData {  get; set; }    
        public float Time {  get; set; }
        [Name("stress")]
        public bool IsHealthGood { get; set; }
        public updateData(float treatedSignal, List<DataPoint> fourierData, float rawData, float time) { TreatedSignal = treatedSignal; FourierData = fourierData; RawData = rawData; Time = time; }
        
    
    }

public struct MerdSettings
    {
        public string Ip { set; get; }
        public int Port { set; get; }
        public string MerdName { set; get; }
        public float Height { set; get; }
        public float Radius { set; get; }

        public MerdSettings(string ip, int port, string merdName, float radius, float height)
        {
            Ip = ip; Port = port; MerdName = merdName; Radius = radius; Height = height; 

        }

        public MerdSettings() { } // Needed for deserialization
    }
public enum Wellfare
    {
        Good = 0, Bad = 1, Suspicious = 2, UnKnown = 3
        
    }
}
