using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public record struct ClientConnectionSettings
    {
        public int port { get; set; }

        public string ip { get; set; }
        public string merdName { get; set; }

        public ClientConnectionSettings(string ip, int port, string merdName) { this.ip = ip; this.port = port; this.merdName = merdName; }
    }
}
