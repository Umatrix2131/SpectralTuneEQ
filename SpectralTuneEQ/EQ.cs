using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SpectralTuneEQ
{
    public class EQ
    {
        [DllImport("EQCore.dll")]
        private static extern void FFT_HM(IntPtr Harmonics_Real, IntPtr Harmonics_Imaginary, int HarmonicsLength, bool forward);
        [DllImport("EQCore.dll")]

        private static extern void EQProcessGain(IntPtr Real, IntPtr Imaginary, IntPtr BezierWeights, int DataLen, int BezierLen, double SampleRate, double dBGain, double MaxFrequencyControl);
        [DllImport("EQCore.dll")]
        private static extern double BezierWeightsFunction(IntPtr Y, int Len, double T);
        public class EQAttribute
        {
            public double Frequency, Weight;
        }
        static List<EQAttribute> EQControlPointData = new List<EQAttribute>();
        static bool MouseDown = false; 
        static double dBGain = 12;
        static double[] BezierWeights = null;
        static EQAttribute[] EQViewData = null;
        static object locker = new object();
        static double MaxFrequencySet = 0;
        static List<double> AudioDataList_L = new List<double>();
        static List<double> AudioDataList_R = new List<double>();
        static List<double> Calculated_L = new List<double>();
        static List<double> Calculated_R = new List<double>();
        public static void EQInit(PictureBox PB, double ControlPoints = 100, double MaxFrequency = 10000)
        {
            EQControlPointData.Clear();
            MaxFrequencySet = MaxFrequency; 
            for (int i = 0; i < ControlPoints; i++) EQControlPointData.Add(new EQAttribute() { Frequency = i * (MaxFrequency / ControlPoints), Weight = 1 });
            BezierWeights = new double[1000].Select(s => 1d).ToArray();
            int BlockHeight = 6;

            Bitmap SaveMap = null;
            PB.Image = new Bitmap(PB.Width, PB.Height);
            using (Graphics graphics = Graphics.FromImage(PB.Image))
            {
                graphics.Clear(Color.FromArgb(55, 55, 222));
                double XStep = PB.Width / (double)EQControlPointData.Count;

                Pen p1 = new Pen(Color.Black);
                Pen p2 = new Pen(Color.FromArgb(177, 177, 177));
                Brush brush2 = new SolidBrush((Color.FromArgb(222, 111, 55)));

                for (int i = 0; i < 10; i++)
                {
                    int y = (int)(PB.Height / 10d * i);
                    graphics.DrawLine(p1, 0, y, PB.Width, y);
                }



                for (int i = 0; i < EQControlPointData.Count; i++)
                {
                    int x = (int)(i * XStep);
                    graphics.DrawLine(p1, x, 0, x, PB.Height);
                }
                double ILEN = 20;
                for (int i = 0; i < ILEN; i++)
                {
                    int x = (int)(PB.Width / ILEN * i);
                    graphics.DrawLine(p2, x, 0, x, PB.Height);
                }
                for (int i = 0; i < ILEN; i++)
                {
                    double EveryIOnFrequency = MaxFrequency / ILEN;
                    double X = PB.Width / ILEN * i + 2;
                    graphics.DrawString((EveryIOnFrequency * i).ToString("f0") + "Hz", new Font("Arial", 10), new SolidBrush((Color.FromArgb(177, 177, 177))), new PointF((int)X, 5));
                    graphics.DrawString("~" + (EveryIOnFrequency * (i + 1)).ToString("f0") + "Hz", new Font("Arial", 10), new SolidBrush((Color.FromArgb(177, 177, 177))), new PointF((int)X, 20));
                }

                int DBLEN = (int)dBGain;
                for (int i = 0; i <= DBLEN; i++)
                {
                    double EveryDB = PB.Height / 2d / DBLEN;
                    double X = PB.Width - 35d;
                    graphics.DrawString((i != 0 ? "-" : "") + i.ToString("f0") + "dB", new Font("Arial", 8), new SolidBrush((Color.FromArgb(177, 177, 177))), new PointF((int)X, (int)(PB.Height / 2d + EveryDB * i - 7)));
                    graphics.DrawString((i != 0 ? "+" : "") + i.ToString("f0") + "dB", new Font("Arial", 8), new SolidBrush((Color.FromArgb(177, 177, 177))), new PointF((int)X, (int)(PB.Height / 2d - EveryDB * i - 7)));
                }


                SaveMap = (PB.Image as Bitmap).Clone() as Bitmap;

                for (int i = 0; i < EQControlPointData.Count; i++)
                {
                    int x1 = (int)(i * XStep);
                    int x2 = (int)((i + 1) * XStep);
                    graphics.FillRectangle(brush2, x1, PB.Height / 2 - BlockHeight / 2, x2 - x1, BlockHeight);
                }
            }

            new Thread(() =>
            {
                object MapLocker = new object();
                Pen p1 = new Pen(Color.Black);
                Pen p2 = new Pen(Color.FromArgb(177, 177, 177));
                Brush brush2 = new SolidBrush((Color.FromArgb(222, 111, 55)));
                while (true)
                {
                    Thread.Sleep(33);

                    Bitmap Temp = SaveMap.Clone() as Bitmap;
                    double XStep = PB.Width / (double)EQControlPointData.Count;
                    double XStepBezier = PB.Width / (double)BezierWeights.Length;

                    using (Graphics graphics = Graphics.FromImage(Temp))
                    {
                        if (EQViewData != null)
                        {
                            for (int i = 0; i < EQViewData.Length; i++)
                            {
                                int X = (int)(EQViewData[i].Frequency / MaxFrequency * SaveMap.Width);
                                X = X >= SaveMap.Width ? SaveMap.Width - 1 : X;
                                X = X < 0 ? 0 : X;
                                double WAMP = (EQViewData[i].Weight - 80d) / 100d;
                                int Y = (int)((WAMP < 0 ? 0 : WAMP) * SaveMap.Height);
                                Y = Y >= SaveMap.Height ? SaveMap.Height : Y;
                                var Colors =Color.FromArgb((int)(Y / (double)SaveMap.Height*255d));
                                graphics.DrawLine(new Pen(Colors), X, SaveMap.Height, X, SaveMap.Height - Y);
                            }
                        }

                        for (int i = 0; i < EQControlPointData.Count; i++)
                        {
                            int x1 = (int)(i * XStep);
                            int x2 = (int)((i + 1) * XStep);
                            int y = (int)(PB.Height - PB.Height * (EQControlPointData[i].Weight / 2d));
                            graphics.FillRectangle(brush2, x1, y - BlockHeight / 2, x2 - x1, BlockHeight);
                        }
                        var BeziersPoints = new PointF[BezierWeights.Length].Select((s, i) => new PointF((float)(i * XStepBezier), (float)(PB.Height - PB.Height * (BezierWeights[i] / 2d)))).ToArray();
                        graphics.DrawLines(new Pen(Color.Red), BeziersPoints);
                    }

                    PB.Invoke(new Action(() =>
                    {
                        PB.Image.Dispose();
                        PB.Image = Temp;
                        PB.Invalidate();
                        PB.Refresh();
                    }));


                }
            }).Start();



            PB.MouseDown += (sender1, e1) =>
            {
                MouseDown = true;
            };
            PB.MouseUp += (sender1, e1) =>
            {
                MouseDown = false;
            };
            PB.MouseMove += (sender1, e1) =>
            {
                if (MouseDown == false) return;
                double Y = e1.Location.Y;
                double X = e1.Location.X;

                int DataIndex = (int)(X / (double)PB.Width * EQControlPointData.Count);
                DataIndex = DataIndex < 0 ? 0 : DataIndex >= EQControlPointData.Count ? EQControlPointData.Count - 1 : DataIndex;
                EQControlPointData[DataIndex].Weight = (PB.Height - Y) / PB.Height * 2d;

                for (int i = 0; i < EQControlPointData.Count; i++)
                    if (i != DataIndex)
                    {
                        double DistWeight = 1d / Math.Pow(Math.Abs(DataIndex - i), 2);
                        EQControlPointData[i].Weight += (EQControlPointData[DataIndex].Weight - EQControlPointData[i].Weight) * DistWeight;
                    }


                IntPtr EQControlPointData_PTR = Marshal.AllocHGlobal(Marshal.SizeOf(new double()) * EQControlPointData.Count);
                Marshal.Copy(EQControlPointData.Select(s => s.Weight).ToArray(), 0, EQControlPointData_PTR, EQControlPointData.Count);
                Parallel.For(0, BezierWeights.Length, (i) =>
                {
                    BezierWeights[i] = BezierWeightsFunction(EQControlPointData_PTR, EQControlPointData.Count, (double)i / (double)BezierWeights.Length); 
                    BezierWeights[i] = BezierWeights[i] < 0 ? 0 : BezierWeights[i] > 2 ? 2 : BezierWeights[i];
                });
                Marshal.FreeHGlobal(EQControlPointData_PTR);

            };

        }

        public static (double[] L, double[] R) EQProcess((double[] L, double[] R) GetData, double SampleRate)
        {
            int DataLen = GetData.L.Length;
            int BezierLen = BezierWeights.Length;
            double[] Local_L_X = GetData.L;
            double[] Local_R_X = GetData.R;
            double[] Local_L_Y = new double[GetData.L.Length];
            double[] Local_R_Y = new double[GetData.R.Length];

            IntPtr LX_PTR = Marshal.AllocHGlobal(Marshal.SizeOf(new double()) * DataLen);
            IntPtr RX_PTR = Marshal.AllocHGlobal(Marshal.SizeOf(new double()) * DataLen);
            IntPtr LY_PTR = Marshal.AllocHGlobal(Marshal.SizeOf(new double()) * DataLen);
            IntPtr RY_PTR = Marshal.AllocHGlobal(Marshal.SizeOf(new double()) * DataLen);
            IntPtr Bezier_PTR = Marshal.AllocHGlobal(Marshal.SizeOf(new double()) * BezierLen);
            Marshal.Copy(Local_L_X, 0, LX_PTR, DataLen);
            Marshal.Copy(Local_R_X, 0, RX_PTR, DataLen);
            Marshal.Copy(Local_L_Y, 0, LY_PTR, DataLen);
            Marshal.Copy(Local_R_Y, 0, RY_PTR, DataLen);
            Marshal.Copy(BezierWeights, 0, Bezier_PTR, BezierLen);

            FFT_HM(LX_PTR, LY_PTR, GetData.L.Length, forward: true);
            FFT_HM(RX_PTR, RY_PTR, GetData.L.Length, forward: true);


            EQProcessGain(LX_PTR, LY_PTR, Bezier_PTR, DataLen, BezierLen, SampleRate, dBGain, MaxFrequencySet);
            EQProcessGain(RX_PTR, RY_PTR, Bezier_PTR, DataLen, BezierLen, SampleRate, dBGain, MaxFrequencySet);
              
            Marshal.Copy(LX_PTR, Local_L_X, 0, DataLen);
            Marshal.Copy(RX_PTR, Local_R_X, 0, DataLen);
            Marshal.Copy(LY_PTR, Local_L_Y, 0, DataLen);
            Marshal.Copy(RY_PTR, Local_R_Y, 0, DataLen);
            Task.Run(() =>
            {
                EQView(Local_L_X, Local_R_X, Local_L_Y, Local_R_Y, SampleRate);
            }); 

            FFT_HM(LX_PTR, LY_PTR, GetData.L.Length, forward: false);
            FFT_HM(RX_PTR, RY_PTR, GetData.L.Length, forward: false);

            Marshal.Copy(LX_PTR, Local_L_X, 0, GetData.L.Length);
            Marshal.Copy(RX_PTR, Local_R_X, 0, GetData.L.Length);


            Marshal.FreeHGlobal(LX_PTR);
            Marshal.FreeHGlobal(RX_PTR);
            Marshal.FreeHGlobal(LY_PTR);
            Marshal.FreeHGlobal(RY_PTR);
            Marshal.FreeHGlobal(Bezier_PTR);
            return (Local_L_X, Local_R_X);
        }
        public static byte[] EqualizeAudioData(byte[] data, double BitsPerSampl, double SampleRate)
        { 
            if (BitsPerSampl != 32) throw new Exception("EqualizeAudioData,waveFormat.BitsPerSample != 32");
            var Data32 = others.ToData<int>(data);
            if (!others.IsPowerOfTwo(Data32.Length)) throw new Exception("The array length is not a power function of 2");
            if (Data32.Length < 131072) throw new Exception("The array length is not sufficient");

            var Data64 = others.ConvertDataTypesToDouble(Data32);
            int DataLength = Data64.L.Length;

            AudioDataList_L.AddRange(Data64.L);
            AudioDataList_R.AddRange(Data64.R);
            if (AudioDataList_L.Count < DataLength * 2 || AudioDataList_R.Count < DataLength * 2) return data;

            for (int i = 0; i < 2; i++)
            {
                (var Local_L_X, var Local_R_X) = EQProcess((AudioDataList_L.GetRange(0, DataLength).ToArray(), AudioDataList_R.GetRange(0, DataLength).ToArray()), SampleRate);
                if (Calculated_L.Count == 0 || Calculated_R.Count == 0)
                {
                    Calculated_L.AddRange(Local_L_X);
                    Calculated_R.AddRange(Local_R_X);
                }
                else
                {
                    for (int j = 0; j < DataLength; j++)
                    {
                        int Calindex = Calculated_R.Count - DataLength / 2 + j;
                        if (Calindex < Calculated_R.Count)
                        {
                            Calculated_L[Calindex] += Local_L_X[j];
                            Calculated_R[Calindex] += Local_R_X[j];
                        }
                        else
                        {
                            Calculated_L.Add(Local_L_X[j]);
                            Calculated_R.Add(Local_R_X[j]);
                        }
                    }
                }
                AudioDataList_L.RemoveRange(0, DataLength / 2);
                AudioDataList_R.RemoveRange(0, DataLength / 2);
            }
            if (Calculated_L.Count < DataLength * 2 || Calculated_R.Count < DataLength * 2) return data;


            var FinalData = others.Combind(Calculated_L.GetRange(0, DataLength).ToArray(), Calculated_R.GetRange(0, DataLength).ToArray()).Select(s => (int)s).ToArray();
            var FinalBytes = others.ToBytes(FinalData);

            Calculated_L.RemoveRange(0, DataLength);
            Calculated_R.RemoveRange(0, DataLength);

            return FinalBytes;
        }
          
        public static void EQView(double[] Local_L_X, double[] Local_R_X, double[] Local_L_Y, double[] Local_R_Y, double sampleRate)
        {
            int Length = Local_L_X.Length;
            if (EQViewData == null || EQViewData.Length != Length / 2) EQViewData = new EQAttribute[Length / 2].Select((s, i) => new EQAttribute() { Frequency = i * sampleRate / Length }).ToArray();
            for (int k = 0; k < Length / 2; k++)
            {
                double frequency = k * sampleRate / Length;
                float amplitude1 = (float)Math.Sqrt(Local_L_X[k] * Local_L_X[k] + Local_L_Y[k] * Local_L_Y[k]);
                float amplitude2 = (float)Math.Sqrt(Local_R_X[k] * Local_R_X[k] + Local_R_Y[k] * Local_R_Y[k]);
                float dBFS = (float)(20f * (float)Math.Log10((amplitude1 + amplitude2) * 0.5d));

                lock (locker)
                {
                    EQViewData[k].Weight = EQViewData[k].Weight * 0.75 + dBFS * 0.25f;
                }
            }

        }
    }
}

