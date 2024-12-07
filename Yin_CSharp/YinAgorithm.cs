using System;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;  // Đảm bảo thêm namespace này
using System.Collections.Generic;
using System.Numerics;
using NAudio.Wave;
using OxyPlot;
using System.Diagnostics;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace Yin_CSharp
{
    public class YinAgorithm
    {
        public void PlotResults(double[] sig, double[] pitches, double[] harmonicRates, double[] argmins, double[] avgFrames, double duration, double harmoThresh)
        {
            // Tạo đối tượng PlotModel
            var plotModel = new PlotModel { Title = "Yin Algorithm Results" };

            

            // Vẽ biểu đồ F0 (Fundamental Frequency)
            
            var f0Series = new LineSeries
            {
                Title = "F0",
                Color = OxyColor.FromRgb(255, 0, 0) // Màu đỏ
            };
            var maxF0 = pitches.Max();
            f0Series.Points.AddRange(pitches.Select((p, i) => new DataPoint(i * duration / pitches.Length, p)));

            plotModel.Series.Add(f0Series);

            // Vẽ biểu đồ Audio data
            var audioDataSeries = new LineSeries
            {
                Title = "Audio data",
                Color = OxyColor.FromRgb(0, 0, 255) // Màu xanh dương
            };
            audioDataSeries.Points.AddRange(sig.Select((s, i) => new DataPoint(i * duration / sig.Length, s * maxF0)));
            plotModel.Series.Add(audioDataSeries);

            //Ve bieu do average frames

            var avgSeries = new LineSeries
            {
                Title = "Avg Sig",
                Color = OxyColor.FromRgb(0, 0, 0)

            };

            avgSeries.Points.AddRange(
                avgFrames.SelectMany(
                    (f,i)=> new DataPoint[] 
                    { 
                        new DataPoint (i * duration/avgFrames.Length - duration/avgFrames.Length/2,f * maxF0),
                        new DataPoint (i * duration/avgFrames.Length + duration/avgFrames.Length/2,f * maxF0)})
                );
            plotModel.Series.Add(avgSeries);

            // Vẽ biểu đồ Harmonic Rate
            var harmonicRateSeries = new LineSeries
            {
                Title = "Harmonic rate",
                Color = OxyColor.FromRgb(0, 255, 0) // Màu xanh lá
            };

            var hMax = harmonicRates.Max();
            
            harmonicRateSeries.Points.AddRange(harmonicRates.Select((hr, i) => new DataPoint(i * duration / harmonicRates.Length, hr * maxF0)));
            plotModel.Series.Add(harmonicRateSeries);

            // Vẽ biểu đồ Harmonic Threshold (vạch đỏ)
            var harmonicThreshSeries = new LineSeries
            {
                Title = "Harmonic Threshold",
                Color = OxyColor.FromRgb(255, 0, 0), // Màu đỏ
                LineStyle = LineStyle.Dash
            };
            harmonicThreshSeries.Points.AddRange(harmonicRates.Select((_, i) => new DataPoint(i * duration / harmonicRates.Length, harmoThresh * maxF0)));
            plotModel.Series.Add(harmonicThreshSeries);

            // Vẽ biểu đồ Index of minimums of CMND
            var minIndexSeries = new LineSeries
            {
                Title = "Index of minimums of CMND",
                Color = OxyColor.FromRgb(255, 165, 0) // Màu cam
            };
            minIndexSeries.Points.AddRange(argmins.Select((am, i) => new DataPoint(i * duration / argmins.Length, am)));
            plotModel.Series.Add(minIndexSeries);

            // Tạo các trục (Axes)
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time (seconds)",
                Minimum = 0,
                Maximum = duration
            });

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Amplitude / Frequency / Rate",
                Minimum = 0
            });

            // Hiển thị kết quả trên một Windows Form
            var plotView = new PlotView
            {
                Model = plotModel,
                Dock = DockStyle.Fill
            };

            var form = new Form
            {
                Text = "Yin Algorithm Visualization",
                Width = 800,
                Height = 600
            };
            form.Controls.Add(plotView);
            form.Show();
        }

        public double Main(string audioFileName = "whereIam.wav", int w_len = 1024, int w_step = 256,
                            int f0_min = 70, int f0_max = 5000, double harmo_thresh = 0.6,
                            string dataFileName = null, int verbose = 4)
        {
            // Tạo đường dẫn tệp âm thanh
            string audioFilePath = audioFileName;
            double f0 = 0;
            // Đọc tệp âm thanh và lấy dữ liệu
            var (sr, sig) = AudioRead(audioFilePath);

            // Khởi tạo stopwatch để đo thời gian
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Tính toán Yin
            double duration = sig.Length / (double)sr;
            var result = ComputeYin(sig, sr, dataFileName, w_len, w_step, f0_min, f0_max, harmo_thresh);
            PlotResults(sig,result.Item1, result.Item2, result.Item3,result.Item5, duration,harmo_thresh);
            f0= GetMainFreq(result.Item5,result.Item1,0.9f);
            stopwatch.Stop();
            Console.WriteLine("Yin computed in: " + stopwatch.Elapsed.TotalSeconds + " seconds");

            // Tính thời gian của tín hiệu
            
            Console.WriteLine("Duration: " + duration + " seconds");
            return f0;
        }

        public double GetMainFreq(double[] avgFrames, double[] pitches, float threadHold)
        {
            var avg = avgFrames.Max();
            var prioPitches = pitches.Where((p, i) => avgFrames[i]/avg > threadHold).ToArray();
            return prioPitches.Min();
        }
        public static (int, double[]) AudioRead(string filePath)
        {
            using (var reader = new WaveFileReader(filePath))
            {
                int sampleRate = reader.WaveFormat.SampleRate;
                int length = (int)reader.Length / 2; // 16-bit stereo

                // Đọc dữ liệu âm thanh (2 byte mỗi mẫu)
                byte[] buffer = new byte[reader.Length];
                reader.Read(buffer, 0, buffer.Length);
                var res = new List<double>();
                for (int i = 0; i < length; i++)
                {
                    // Chuyển đổi 2 byte thành giá trị âm thanh (16-bit)
                    short sample = BitConverter.ToInt16(buffer, i * 2);
                    // Chuyển đổi giá trị mẫu thành giá trị từ -1 đến 1
                    double normalizedSample = sample / 32768.0;

                    // Thêm điểm vào danh sách
                    res.Add(normalizedSample);
                }
                return (sampleRate, res.ToArray());
            }
        }
        public static double[] DifferenceFunction(double[] x, int N, int tauMax)
        {
            int w = x.Length;
            tauMax = Math.Min(tauMax, w);

            // Tính tổng tích lũy của x^2
            double[] xSquared = x.Select(val => val * val).ToArray();
            double[] xCumsum = new double[xSquared.Length + 1];
            xCumsum[0] = 0;
            for (int i = 0; i < xSquared.Length; i++)
            {
                xCumsum[i + 1] = xCumsum[i] + xSquared[i];
            }

            // Tính kích thước cho FFT
            int size = w + tauMax;
            int p2 = (int)Math.Ceiling(Math.Log2(size / 32.0));

            // Các kích thước padding "nice"
            int[] niceNumbers = { 16, 18, 20, 24, 25, 27, 30, 32 };
            int sizePad = niceNumbers
                .Where(x => x * (int)Math.Pow(2, p2) >= size)
                .Min() * (int)Math.Pow(2, p2);

            // Thực hiện biến đổi Fourier
            Complex[] xComplex = x.Select(i => new Complex(i, 0)).ToArray();
            Fourier.Forward(xComplex, FourierOptions.Matlab);

            // Tính conjugate của fc
            Complex[] fcConjugate = xComplex.Select(c => Complex.Conjugate(c)).ToArray();

            // Tính conv = inverse FFT của fc * fc conjugate
            Complex[] fcMultiplied = xComplex.Zip(fcConjugate, (f, fConj) => f * fConj).ToArray();
            Fourier.Inverse(fcMultiplied, FourierOptions.Matlab);

            // Lấy các phần tử đầu của kết quả sau khi inverse
            Complex[] conv = fcMultiplied.Take(tauMax).ToArray();

            // Tính kết quả
            var result = new System.Collections.Generic.List<double>();
            var xs_n = new List<double>();
            for (int i = w; i >= w - tauMax + 1; i--)
            {
                xs_n.Add(xCumsum[i]);
            } 
            for (int i = 0; i < tauMax; i++)
            {
                var value = xs_n[i] + xCumsum[^1] - xCumsum[i] -2 * conv[i].Real;
                result.Add(value);
                Console.WriteLine(value.ToString());
            }

            return result.ToArray();
        }

        public static double[] CumulativeMeanNormalizedDifferenceFunction(double[] df, int N)
        {
            // Bước 1: Tính cumulative sum của df[1:]
            double[] cumsum = new double[df.Length - 1];
            cumsum[0] = df[1];  // Khởi tạo phần tử đầu tiên

            for (int i = 1; i < df.Length - 1; i++)
            {
                cumsum[i] = cumsum[i - 1] + df[i + 1];
            }

            // Bước 2: Tính CMND
            double[] cmndf = new double[df.Length - 1];
            for (int i = 0; i < df.Length - 1; i++)
            {
                cmndf[i] = (df[i + 1] * (i + 1)) / cumsum[i];
            }

            // Bước 3: Thêm phần tử 1 vào đầu mảng
            double[] result = new double[cmndf.Length + 1];
            result[0] = 1;
            Array.Copy(cmndf, 0, result, 1, cmndf.Length);

            return result;
        }

        static int GetPitch(double[] cmdf, int tau_min, int tau_max, double harmo_th = 0.1)
        {
            int tau = tau_min;

            // Vòng lặp chính tìm kiếm pitch
            while (tau < tau_max)
            {
                // Kiểm tra nếu cmdf[tau] nhỏ hơn ngưỡng
                if (cmdf[tau] < harmo_th)
                {
                    // Nếu điều kiện trên thỏa, kiểm tra tiếp các phần tử tiếp theo
                    while (tau + 1 < tau_max && cmdf[tau + 1] < cmdf[tau])
                    {
                        tau++;
                    }
                    return tau;
                }
                tau++;
            }

            // Trả về 0 nếu không tìm thấy giá trị dưới ngưỡng
            return 0;
        }

        public  Tuple<double[], double[], double[], double[], double[]> ComputeYin(double[] sig, int sr, string dataFileName = null,
        int w_len = 512, int w_step = 256, int f0_min = 100, int f0_max = 500, double harmo_thresh = 0.1)
        {
            Console.WriteLine("Yin: compute yin algorithm");

            int tau_min = sr / f0_max;
            int tau_max = sr / f0_min;

            var avgSig = sig.Average();
            // Tính toán scale thời gian cho mỗi cửa sổ phân tích
            var timeScale = Enumerable.Range(0, (sig.Length - w_len) / w_step + 1).Select(i => i * w_step).ToArray();
            var times = timeScale.Select(t => t / (double)sr).ToArray();
            var frames = timeScale.Select(t => sig.Skip(t).Take(w_len).ToArray()).ToArray();
            var avgFrames = frames.Select(f => f.Select(s=>Math.Abs(s)).ToArray().Average()).ToArray(); 
            // Các mảng chứa kết quả
            var pitches = new double[timeScale.Length];
            var harmonicRates = new double[timeScale.Length];
            var argmins = new double[timeScale.Length];

            // Vòng lặp tính YIN cho từng frame
            for (int i = 0; i < frames.Length; i++)
            {
                var frame = frames[i];

                // Tính YIN
                var df = DifferenceFunction(frame, w_len, tau_max);
                var cmdf = CumulativeMeanNormalizedDifferenceFunction(df, tau_max);
                var p = GetPitch(cmdf, tau_min, tau_max, harmo_thresh);

                // Lưu kết quả
                int minIndex = cmdf.ToList().IndexOf(cmdf.Min());
                if (minIndex > tau_min)
                {
                    argmins[i] = sr / (double)minIndex;
                }

                if (p != 0) // Nếu tìm thấy pitch
                {
                    pitches[i] = sr / (double)p;
                    harmonicRates[i] = cmdf[p];
                }
                else // Không có pitch, tính giá trị harmonic rate
                {
                    harmonicRates[i] = cmdf.Min();
                }
            }

            // Lưu dữ liệu vào file (nếu cần)
            if (dataFileName != null)
            {
                SaveData(dataFileName, times, sr, w_len, w_step, f0_min, f0_max, harmo_thresh, pitches, harmonicRates, argmins);
                Console.WriteLine("\t- Data file written in: " + dataFileName);
            }

            return Tuple.Create(pitches, harmonicRates, argmins, times,avgFrames);
        }
        public static void SaveData(string dataFileName, double[] times, int sr, int w_len, int w_step,
        int f0_min, int f0_max, double harmo_thresh, double[] pitches, double[] harmonicRates, double[] argmins)
        {
            // Ví dụ lưu dữ liệu thành file CSV hoặc các định dạng khác nếu cần
            using (var writer = new StreamWriter(dataFileName))
            {
                for (int i = 0; i < times.Length; i++)
                {
                    writer.WriteLine($"{times[i]},{pitches[i]},{harmonicRates[i]},{argmins[i]}");
                }
            }
        }
    }
}

    

