using Splat;
using Splat.Serilog;
using Serilog;
using System.Reactive.Linq;
using System.IO;
using System.Security.Cryptography;
using utils;
using CanGPSLogger;
using System.Windows.Forms;
using ZedGraph;
using System;
using System.Collections.Generic;
using System.Reactive.Threading.Tasks;

namespace antena_tester
{
    public partial class Form1 : Form, IEnableLogger
    {
        List<(double, UInt16)> fileData = new();
        double[] response;
        double timeStep = 0;

        public delegate void MyDelegate();
        RTUtransport rTUtransport = new RTUtransport();
        LoggerSetups setups;

        LineItem lineCurve = new LineItem("lineCurve");
        LineItem responseCurve = new LineItem("line");
        double[] x = new double[2];
        double[] y = new double[2];
        private int _record_now = 0;

        IDisposable? disposableTimerVisu;
        IDisposable? disposableTimerCom;

        IObservable<long> timer = null;
        IObservable<long> timer_visu = null;

        public int record_now
        {
            get { return _record_now; }
            set
            {
                _record_now = value;

                BeginInvoke(new MyDelegate(() =>
                {
                    if (fileData.Count == 0) return;
                    if (record_now >= fileData.Count) return;

                    x[0] = fileData[record_now].Item1;
                    x[1] = fileData[record_now].Item1;
                }));

            }
        }

        bool _connected = false;
        public bool connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                BeginInvoke(new MyDelegate(() =>
                {
                    if (value)
                    {
                        button_connect.BackColor = Color.Green;
                        button_stop.Enabled = true;
                    }
                    else
                    {
                        button_connect.BackColor = Color.Gray;
                        button_start.Enabled = false;

                        button_connect.Enabled = true;
                        button_start.Enabled = false;
                        button_stop.Enabled = false;
                        this.Log().Warn($"Disconnected from device");
                    }
                }));

            }

        }

        bool _playing = false;
        public bool playing
        {
            get { return _playing; }
            set
            {
                _playing = value;
                BeginInvoke(new MyDelegate(() =>
                {
                    if (_playing)
                    {

                    }
                    else
                    {
                        this.Log().Warn($"Playing stoped");
                    }

                }));
            }
        }



        public Form1()
        {
            InitializeComponent();



            Locator.CurrentMutable.RegisterConstant(new utils.LogProvider(), typeof(utils.ILogProvider));

            Log.Logger = new LoggerConfiguration()
                 .WriteTo.Observers(events => events.Do(evt =>
                 {
                     Locator.Current.GetService<utils.ILogProvider>()?.Post(
                     $"   {evt.Timestamp.LocalDateTime} :   [ {evt.Level} ]  {evt.MessageTemplate.Text} \n"
                         );
                 }).Subscribe())
            .CreateLogger();

            Locator.CurrentMutable.UseSerilogFullLogger();

            Locator.Current.GetService<ILogProvider>()?.GetObservable.Subscribe(observer =>
            {
                try
                {

                    BeginInvoke(new MyDelegate(() => { textBox_log.AppendText($"{observer}{Environment.NewLine}"); }));

                }
                catch { };

            });

            this.Log().Info($"Start application");
            setups = LoggerSetups.read();

            GraphPane pane = zedGraph1.GraphPane;
            pane.XAxis.Title.Text = null;
            pane.YAxis.Title.Text = null;
            pane.Title.Text = string.Empty;

            button_connect.Enabled = false;
            button_start.Enabled = false;
            button_stop.Enabled = false;

            timer_visu = Observable.Interval(TimeSpan.FromMilliseconds(250));
            timer_visu.Subscribe(observer =>
            {

                BeginInvoke(new MyDelegate(() =>
                {
                    lineCurve.Clear();
                    lineCurve.AddPoint(x[0], y[0]);
                    lineCurve.AddPoint(x[1], y[1]);

                    if (pane.CurveList.Count > 2)
                    {
                        pane.CurveList.RemoveAt(2);
                        responseCurve = pane.AddCurve("Responce", fileData.Select(x => x.Item1).ToArray(), response,
                             System.Drawing.Color.Green,
                             SymbolType.Circle
                        );

                    }

                    zedGraph1.Refresh();
                    zedGraph1.AxisChange();
                    zedGraph1.Invalidate();
                }));

            });

        }

        private void button_start_Click(object sender, EventArgs e)
        {
            if (timeStep == 0) { return; };

            if (connected & !playing)
            {
                playing = true;
                this.Log().Info($"Start playing");
            }

            if (disposableTimerVisu == null)
            {
                disposableTimerVisu = timer.Subscribe(_ =>
                {
                    response[record_now] = rTUtransport.speed_now;
                    record_now++;
                });
            }
        }

        bool ch()
        {
            return true;
        }

        private async void button1_fopen_Click(object sender, EventArgs e)
        {
            disposableTimerCom?.Dispose();
            FileDialog fd = new OpenFileDialog();
            fd.Filter = "Log (*.log)|*.log|All (*.*)|*.*";
            var res = fd.ShowDialog();
            if (res == DialogResult.OK)
            {
                this.Log().Info($"Reading file data");
                fileData.Clear();
                string path = fd.FileName;
                this.Log().Info($"File selected {path}");
                textBox_path.Text = path;
                // read data file
                using (StreamReader reader = new StreamReader(path))
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {


                        var point = line.Split(" ");
                        if (point.Length < 2)
                        {
                            this.Log().Fatal($"Wrong file record length [{line}]");
                            return;
                        }
                        point[0] = point[0].Replace(".", ",");
                        bool con1_res = double.TryParse(point[0], out var time);
                        bool con2_res = Int16.TryParse(point[1], out Int16 freq);
                        if (!con1_res | !con2_res)
                        {
                            this.Log().Fatal($"Wrong file record data [{line}]");
                            return;
                        }
                        var tmp = (time, (UInt16)Math.Abs(freq));
                        fileData.Add(tmp);
                    }
                    this.Log().Info($"File read, total {fileData.Count} records found");

                    timeStep = 0;


                    for (int i = 1; i < fileData.Count; i++)
                    {
                        timeStep += fileData[i].Item1 - fileData[i - 1].Item1;
                    }

                    timeStep /= fileData.Count;

                    this.Log().Info($"Data record time step {timeStep}");


                    if (timer != null)
                    {
                        var ts = new CancellationTokenSource(TimeSpan.FromSeconds(0));
                        ts.Token.Register(timer.Subscribe((_) => {; }).Dispose);
                    }
                    timer = Observable.Interval(TimeSpan.FromSeconds(timeStep));

                    button_connect.Enabled = true;

                    // drawing points

                    GraphPane pane = zedGraph1.GraphPane;
                    pane.CurveList.Clear();
                    var myCurve = pane.AddCurve("Setpoint", fileData.Select(x => x.Item1).ToArray(), fileData.Select(x => (double)x.Item2).ToArray(),
                       System.Drawing.Color.Blue,
                           SymbolType.Circle
                           );



                    myCurve.Symbol.Size = 2;
                    pane.XAxis.Scale.Min = 0;
                    pane.XAxis.Scale.Max = fileData[fileData.Count - 1].Item1;


                    zedGraph1.Refresh();
                    zedGraph1.AxisChange();
                    zedGraph1.Invalidate();


                    record_now = 0;
                    y[0] = pane.YAxis.Scale.Max; y[1] = pane.YAxis.Scale.Min;

                    lineCurve = pane.AddCurve(" ", x, y, Color.Red, SymbolType.None);

                    response = new double[fileData.Count];


                    responseCurve = pane.AddCurve("Responce", fileData.Select(x => x.Item1).ToArray(), response,
                         System.Drawing.Color.Green,
                         SymbolType.Circle
                    );

                    zedGraph1.Refresh();
                    zedGraph1.AxisChange();
                    zedGraph1.Invalidate();
                }


            }
        }

        private void button_stop_Click(object sender, EventArgs e)
        {


            if (playing)
            {
                this.Log().Warn($"Process interrupted by user at {fileData[record_now].Item1} sec");
            }

            stopPlaying();

        }

        private void stopPlaying()
        {

            if (disposableTimerCom != null)
            {
                disposableTimerCom?.Dispose();
                disposableTimerVisu?.Dispose();
                record_now = 0;
            }

            disposableTimerCom = null;
            disposableTimerVisu = null;

            playing = false;
            connected = false;
            rTUtransport.Disconnect();

        }

        private void button_reset_Click(object sender, EventArgs e)
        {

        }

        int timeStepFaultCntr = 0;
        private void button_connect_Click(object sender, EventArgs e)
        {

            if (connected) { return; }
            if (disposableTimerCom != null) { return; }

            rTUtransport.Connect(setups.comName, setups.comSpeed);


            disposableTimerCom = timer.Subscribe(_ =>
            {
                // here modbus send / receive should be
                if (record_now == fileData.Count)
                {
                    stopPlaying();
                    return;
                }

                if (rTUtransport.connected && !connected)
                {
                    connected = true;
                }
                else if (!rTUtransport.connected && connected)
                {
                    stopPlaying();
                }
                else if (connected && rTUtransport.receiveTimeout)
                {
                    stopPlaying();

                }
                else if (connected)
                {
                    UInt16 ref_val = 0;
                    if (playing)
                    {
                        if (record_now >= fileData.Count)
                        {
                            stopPlaying();
                        }
                        else
                        {
                            ref_val = fileData[record_now].Item2;
                        }

                    }
                    rTUtransport.SendSpeed(ref_val);

                    if ((double)rTUtransport.mesuaredTimeStepMs / 1000.0 > timeStep * 1.2)
                    {
                        timeStepFaultCntr++;
                        if (timeStepFaultCntr > 1)
                        {
                            this.Log().Warn($"Time step too big ({rTUtransport.mesuaredTimeStepMs} ms)");
                        }
                    }
                    else
                    {
                        timeStepFaultCntr = 0;
                    }

                    BeginInvoke(new MyDelegate(() =>
                    {
                        switch (rTUtransport.slaveState)
                        {
                            case RTUtransport.SlaveState.onFault:
                                if (button_connect.BackColor != Color.Red)
                                {
                                    button_connect.BackColor = Color.Red;
                                    this.Log().Info("Device is on Fault state");
                                }
                                break;
                            case RTUtransport.SlaveState.onRdy:
                                if (button_connect.BackColor != Color.Blue)
                                {
                                    button_connect.BackColor = Color.Blue;
                                    this.Log().Info("Device is on Ready state");
                                }
                                break;
                            case RTUtransport.SlaveState.onRun:
                                if (button_connect.BackColor != Color.Green)
                                {
                                    button_connect.BackColor = Color.Green;
                                    this.Log().Info("Device is on Run state");
                                }
                                button_start.Enabled = true;
                                break;
                        }
                    }));
                }

            });

        }
    }
}