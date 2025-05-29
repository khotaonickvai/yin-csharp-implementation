using NAudio.Wave;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using System.Diagnostics;

namespace Yin_CSharp
{
    public partial class MainForm : Form
    {
        private PlotModel plotModel;
        public MainForm()
        {
            InitializeComponent();
            plotModel = new PlotModel();
            plotModel.Title = "Test";
            var plotView = new OxyPlot.WindowsForms.PlotView
            {
                Dock = DockStyle.Fill,
                Model = plotModel
            };
            this.Controls.Add(plotView);
        }


        private List<DataPoint> GetAudioData(string filePath)
        {
            var points = new List<DataPoint>();

            // Mở tệp âm thanh WAV
            using (var reader = new WaveFileReader(filePath))
            {
                int sampleRate = reader.WaveFormat.SampleRate;
                int length = (int)reader.Length / 2; // 16-bit stereo

                // Đọc dữ liệu âm thanh (2 byte mỗi mẫu)
                byte[] buffer = new byte[reader.Length];
                reader.Read(buffer, 0, buffer.Length);

                for (int i = 0; i < length; i++)
                {
                    // Chuyển đổi 2 byte thành giá trị âm thanh (16-bit)
                    short sample = BitConverter.ToInt16(buffer, i * 2);
                    // Chuyển đổi giá trị mẫu thành giá trị từ -1 đến 1
                    double normalizedSample = sample / 32768.0;

                    // Thêm điểm vào danh sách
                    points.Add(new DataPoint(i / (double)sampleRate, normalizedSample));
                }
            }

            return points;
        }

        private void btnInput_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set dialog properties (optional)
            openFileDialog.Title = "Select a File";
            //openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*"; // Set file filter
            
            openFileDialog.Multiselect = false; // Allow multiple file selection (optional)

            // Show the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the file path
                string filePath = openFileDialog.FileName;
                txtInput.Text = filePath;
                //MessageBox.Show("File Selected: " + filePath);
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            PreprocessFile();
            var yin = new YinAgorithm();
            var f0 = yin.Main("outTest.wav", w_len: 2048,w_step:256);
            labelF0.Text = f0.ToString();
        }

        private void DrawAudioWave()
        {
            List<DataPoint> points = GetAudioData(txtInput.Text);
            var series = new LineSeries
            {
                Title = "Waveform",
                MarkerType = MarkerType.None
            };
            foreach (var point in points)
            {
                series.Points.Add(point);
            }
            plotModel.Series.Clear();
            plotModel.Series.Add(series);
            plotModel.PlotView.InvalidatePlot();
        }

        private void PreprocessFile()
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.UseShellExecute = false;
          
            processStartInfo.CreateNoWindow = true;
            processStartInfo.Arguments = $" -i \"{txtInput.Text}\" -vn -acodec pcm_s16le -ar 44100 -ac 1 -y outTest.wav";
            processStartInfo.FileName = "ffmpeg_new.exe";

            Process process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();
        }
    }
}
