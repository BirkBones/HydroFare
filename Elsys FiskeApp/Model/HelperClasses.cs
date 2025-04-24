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
        public float TreatedSignal;
        public float FourierData;
        public float RawData;
        public float Time;
        public updateData(float treatedSignal, float fourierData, float rawData, float time) { TreatedSignal = treatedSignal; FourierData = fourierData; RawData = rawData; Time = time; }
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

}
