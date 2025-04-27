using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using OxyPlot;
using Newtonsoft.Json.Linq;

namespace Elsys_FiskeApp.Model
{
    public class FakeSender
    {
        public updateData currentUpdate;
        public Action currentUpdateChanged;

        float samplingPeriod;
        float sampleRate;
        float fourierPeriod;
        float startTime;
        float fourierTime; // the time for the fourier part. Easier to do it this way.
        bool shouldUpdate = true;
        bool isHealthGood;

        List<DataPoint> currentFourierPoints;
        string filepathSignal;
        string filepathFourier;
        string filepathWellbeing;
        CsvReader signalReader;
        CsvReader fourierReader;
        CsvReader healthReader;
        StreamReader _signalStreamReader;
        StreamReader _fourierStreamReader;
        StreamReader _healthStreamReader;
        public FakeSender(string _filepathsignal, string _filepathfourier, string _filepathwellbeing, float _period = 1 / 4f, float _samplerate = 4, float _fourierperiod = 1, float _startTime = 0)
        {
            samplingPeriod = _period;
            sampleRate = _samplerate;
            startTime = _startTime;
            fourierTime = _startTime;
            filepathSignal = _filepathsignal;
            filepathFourier = _filepathfourier;
            fourierPeriod = _fourierperiod;
            filepathWellbeing = _filepathwellbeing;

            InitializeReader(filepathSignal, out signalReader, out _signalStreamReader);
            InitializeReader(filepathFourier, out fourierReader, out _fourierStreamReader);
            InitializeReader(filepathWellbeing, out healthReader, out _healthStreamReader);
            DataHolder.Instance.GlobalUpdateTimer.Tick += (sender, e) => updateCurrentOutput();


        }

        void InitializeReader(string filepath, out CsvReader outputReader, out StreamReader outputStreamReader)
        {
            outputStreamReader = new StreamReader(filepath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = (outputStreamReader != _fourierStreamReader),//So Hasheaderrecord is set true for signal, but not for fourier.
                MissingFieldFound = null,
                HeaderValidated = null

            };
            outputReader = new CsvReader(outputStreamReader, config);
            
            
        }
        bool isUpdating = false;
        void updateCurrentOutput()
        {
            
            if (shouldUpdate)
            {
                var temp = GetNextSignal();
                if (fourierTime == startTime)
                {
                    fourierTime += fourierPeriod;
                    currentFourierPoints = GetNextFourierTransform();
                    isHealthGood = GetIsFishDoingGood();
                }

                if (temp.Time - fourierTime >= fourierPeriod) // Since fourierplot is to be changed only once each second, it needs to be ensured that xs - 1s <= 1
                {
                    currentFourierPoints = GetNextFourierTransform(); // update which fourierpoints we be using.
                    isHealthGood = GetIsFishDoingGood();
                    fourierTime += fourierPeriod;

                }
                temp.FourierData = currentFourierPoints;
                temp.IsHealthGood = isHealthGood;
                currentUpdate = temp;
                currentUpdateChanged?.Invoke();
            }

        }

        bool GetIsFishDoingGood()
        {
            if (!healthReader.Read()) { shouldUpdate = false; return isHealthGood; }
            var temp = healthReader.GetRecord<updateData>();
            return temp.IsHealthGood;
        }
        updateData GetNextSignal()
        {
            if (!signalReader.Read()) { shouldUpdate = false; return currentUpdate; }
            updateData nextVal = signalReader.GetRecord<updateData>();
            int batchNumber = signalReader.Parser.Row - 1; //batchnumber = 1 is the first one.
            float time = startTime + (batchNumber - 1) * samplingPeriod; // increases with samplingperiod for each batch, and starts at starttime. 
            var nextValWithTime = new updateData() 
            { RawData = nextVal.RawData, TreatedSignal = nextVal.TreatedSignal, Time = time };
            return nextValWithTime;

            
        }
        //gives the next row of fourier points.
       List<DataPoint> GetNextFourierTransform()
        {
            string line = _fourierStreamReader.ReadLine();
            if (line == null)
            {
                shouldUpdate = false;
            }
            List<float> row = line.Split(',')
                .Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToList();

            return makeFourierPoints(row);

        }

        // makes the fourier values into points thats to be plotted.
        List<DataPoint> makeFourierPoints(List<float> fourierVals)
        {
            fourierVals = DownsampleLogarithmically(fourierVals);
            List<DataPoint> fourierPoints = new(48_000);
            int freq = 0;
            int freqdiff = 1;
            foreach (var val in fourierVals) // fouriervals holds fourier vals over time.
            {
                var point = new DataPoint(freq, val);
                fourierPoints.Add(point);
                freq += freqdiff;
            }
            
            fourierPoints = convertTodB(fourierPoints);
            fourierPoints = Normalize(fourierPoints);
            return fourierPoints;
        }

        bool skip = false;
        List<float> DownsampleLogarithmically(List<float> points)
        {
            if (skip) return points;
            List<float> reduced = new();
            int n = points.Count;
            double i = 1;

            while (i < n)
            {
                reduced.Add(points[(int)i]);
                i *= 1.05; // slower growth: 10% more each time
            }

            return reduced;
        }


        List<DataPoint> convertTodB(List<DataPoint> points)
        {
            List<DataPoint> convertedPoints = new(48_000);
            for (int i = 0; i < points.Count; i++)
            {
                var NewPoint = new DataPoint(points[i].X, 20 * Math.Log10(points[i].Y));
                convertedPoints.Add(NewPoint);
            }
            return convertedPoints;
        }
        List<DataPoint> Normalize(List<DataPoint> points)
        {
            List<DataPoint> normalizedPoints = new(48_000);
            float div = (float)points.Max(point => point.Y);
            for (int i = 0; i < points.Count; i++)
            {
                var newPoint = new DataPoint(points[i].X, points[i].Y / div);
                normalizedPoints.Add(newPoint);
            }
            return normalizedPoints;
        }


    }
}