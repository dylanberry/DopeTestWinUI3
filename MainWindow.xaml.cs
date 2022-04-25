using Azure;
using Azure.Storage.Blobs;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using Saplin.xOPS.UI.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;


namespace DopeTestWinUI3
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        volatile bool breakTest = false;
        const int maxItemsOnScreen = 600;

        public static Brush ConvertColor(Color c) => new SolidColorBrush(c);

        void StartTestST()
        {
            var rand = new Random2(0);

            breakTest = false;

            var width = absolute.ActualWidth;
            var height = absolute.ActualHeight;

            var processed = 0;

            long prevLoopTicks = 0;
            long prevLoopFinishMarker = 0;
            int prevProcessedCount = 0;
            double totalCount = 0;
            int avgN = 0;
            var sw = new Stopwatch();

            Action loop = null;

            loop = () =>
            {
                var loopStartMarker = sw.ElapsedMilliseconds;

                if (breakTest)
                {
                    sw.Stop();
                    //var avg = totalCount / avgN;
                    var avg = totalCount / sw.Elapsed.TotalSeconds;
                    dopes.Text = string.Format("{0:0.00} Dopes/s (AVG)", avg).PadLeft(21);
                    return;
                }

                //60hz, 16ms to build the frame
                while (sw.ElapsedMilliseconds - loopStartMarker < 16 && !breakTest)
                {
                    TextBlock label = GetRandomDope(rand);

                    Canvas.SetLeft(label, rand.NextDouble() * width);
                    Canvas.SetTop(label, rand.NextDouble() * height);

                    if (processed > maxItemsOnScreen)
                    {
                        absolute.Children.RemoveAt(0);
                    }

                    absolute.Children.Add(label);

                    processed++;

                    if (sw.ElapsedMilliseconds - prevLoopFinishMarker > 500)
                    {

                        var rate = (double)(processed - prevProcessedCount) / ((double)(sw.ElapsedTicks - prevLoopTicks) / Stopwatch.Frequency);
                        prevLoopTicks = sw.ElapsedTicks;
                        prevProcessedCount = processed;

                        if (processed > maxItemsOnScreen)
                        {
                            dopes.Text = string.Format("{0:0.00} Dopes/s", rate).PadLeft(15);
                            totalCount += rate;
                            //avgN++;
                        }

                        prevLoopFinishMarker = sw.ElapsedMilliseconds;
                    }
                }

                _ = DispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() => loop()));
            };

            sw.Start();

            _ = DispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() => loop()));
        }

        private static TextBlock GetRandomDope(Random2 rand)
        {
            var label = new TextBlock()
            {
                Text = "Dope",
                Foreground = new SolidColorBrush(Color.FromArgb(0xFF, (byte)(rand.NextDouble() * 255), (byte)(rand.NextDouble() * 255), (byte)(rand.NextDouble() * 255)))
            };

            label.RenderTransform = new RotateTransform() { Angle = rand.NextDouble() * 360 };
            return label;
        }

        void StartTestReuseST()
        {
            var rand = new Random2(0);

            breakTest = false;

            var width = absolute.ActualWidth;
            var height = absolute.ActualHeight;

            const int step = 20;
            var labels = new TextBlock[step * 2];

            var processed = 0;

            long prevTicks = 0;
            long prevMs = 0;
            int prevProcessed = 0;
            double avgSum = 0;
            int avgN = 0;
            var sw = new Stopwatch();

            Action loop = null;

            Stack<TextBlock> _cache = new Stack<TextBlock>();

            loop = () =>
            {
                var now = sw.ElapsedMilliseconds;

                if (breakTest)
                {
                    var avg = avgSum / avgN;
                    dopes.Text = string.Format("{0:0.00} Dopes/s (AVG)", avg).PadLeft(21);
                    return;
                }

                //60hz, 16ms to build the frame
                while (sw.ElapsedMilliseconds - now < 16 && !breakTest)
                {
                    var label = _cache.Count == 0 ? new TextBlock() { Foreground = new SolidColorBrush() } : _cache.Pop();

                    label.Text = "Dope";
                    (label.Foreground as SolidColorBrush).Color = Color.FromArgb(0xFF, (byte)(rand.NextDouble() * 255), (byte)(rand.NextDouble() * 255), (byte)(rand.NextDouble() * 255));

                    label.RenderTransform = new RotateTransform() { Angle = rand.NextDouble() * 360 };

                    Canvas.SetLeft(label, rand.NextDouble() * width);
                    Canvas.SetTop(label, rand.NextDouble() * height);

                    if (processed > maxItemsOnScreen)
                    {
                        _cache.Push(absolute.Children[0] as TextBlock);
                        absolute.Children.RemoveAt(0);
                    }

                    absolute.Children.Add(label);

                    processed++;

                    if (sw.ElapsedMilliseconds - prevMs > 500)
                    {

                        var r = (double)(processed - prevProcessed) / ((double)(sw.ElapsedTicks - prevTicks) / Stopwatch.Frequency);
                        prevTicks = sw.ElapsedTicks;
                        prevProcessed = processed;

                        if (processed > maxItemsOnScreen)
                        {
                            dopes.Text = string.Format("{0:0.00} Dopes/s", r).PadLeft(15);
                            avgSum += r;
                            avgN++;
                        }

                        prevMs = sw.ElapsedMilliseconds;
                    }
                }

                _ = DispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() => loop()));
            };

            sw.Start();

            _ = DispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() => loop()));
        }

        void StartTestBindings()
        {
            var rand = new Random2(0);

            breakTest = false;

            var width = absolute.ActualWidth;
            var height = absolute.ActualHeight;

            const int step = 20;
            var labels = new TextBlock[step * 2];

            var processed = 0;

            long prevTicks = 0;
            long prevMs = 0;
            int prevProcessed = 0;
            double avgSum = 0;
            int avgN = 0;
            var sw = new Stopwatch();

            var source = Enumerable.Range(0, maxItemsOnScreen).Select(i => new BindingItem() { Color = Colors.Red }).ToArray();
            items.ItemsSource = source;

            Action loop = null;
            var current = 0;

            loop = () =>
            {
                var now = sw.ElapsedMilliseconds;

                if (breakTest)
                {
                    var avg = avgSum / avgN;
                    dopes.Text = string.Format("{0:0.00} Dopes/s (AVG)", avg).PadLeft(21);
                    return;
                }

                //60hz, 16ms to build the frame
                while (sw.ElapsedMilliseconds - now < 16 && !breakTest)
                {
                    var index = current++ % source.Length;

                    source[index].Color = Color.FromArgb(0xFF, (byte)(rand.NextDouble() * 255), (byte)(rand.NextDouble() * 255), (byte)(rand.NextDouble() * 255));
                    source[index].Rotation = rand.NextDouble() * 360;
                    source[index].Top = rand.NextDouble() * height;
                    source[index].Left = rand.NextDouble() * width;

                    processed++;

                    if (sw.ElapsedMilliseconds - prevMs > 500)
                    {

                        var r = (double)(processed - prevProcessed) / ((double)(sw.ElapsedTicks - prevTicks) / Stopwatch.Frequency);
                        prevTicks = sw.ElapsedTicks;
                        prevProcessed = processed;

                        if (processed > maxItemsOnScreen)
                        {
                            dopes.Text = string.Format("{0:0.00} Dopes/s", r).PadLeft(15);
                            avgSum += r;
                            avgN++;
                        }

                        prevMs = sw.ElapsedMilliseconds;
                    }
                }

                _ = DispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() => loop()));
            };

            sw.Start();

            _ = DispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() => loop()));
        }

        public void StartTestChangeST()
        {
            var rand = new Random2(0);

            breakTest = false;

            var width = grid.ActualWidth;
            var height = grid.ActualHeight;

            const int step = 20;
            var labels = new TextBlock[step * 2];

            var processed = 0;

            long prevTicks = 0;
            long prevMs = 0;
            int prevProcessed = 0;
            double avgSum = 0;
            int avgN = 0;
            var sw = new Stopwatch();

            var texts = new string[] { "dOpe", "Dope", "doPe", "dopE" };

            Action loop = null;

            loop = () =>
            {
                if (breakTest)
                {
                    var avg = avgSum / avgN;
                    dopes.Text = string.Format("{0:0.00} Dopes/s (AVG)", avg).PadLeft(21);
                    return;
                }

                var now = sw.ElapsedMilliseconds;

                //60hz, 16ms to build the frame
                while (sw.ElapsedMilliseconds - now < 16 && !breakTest)
                {
                    if (processed > maxItemsOnScreen)
                    {
                        (absolute.Children[processed % maxItemsOnScreen] as TextBlock).Text = texts[(int)Math.Floor(rand.NextDouble() * 4)];
                    }
                    else
                    {
                        var label = new TextBlock()
                        {
                            Text = "Dope",
                            Foreground = new SolidColorBrush(Color.FromArgb(0xFF, (byte)(rand.NextDouble() * 255), (byte)(rand.NextDouble() * 255), (byte)(rand.NextDouble() * 255)))
                        };

                        label.RenderTransform = new RotateTransform() { Angle = rand.NextDouble() * 360 };

                        Canvas.SetLeft(label, rand.NextDouble() * width);
                        Canvas.SetTop(label, rand.NextDouble() * height);

                        absolute.Children.Add(label);
                    }

                    processed++;

                    if (sw.ElapsedMilliseconds - prevMs > 500)
                    {

                        var r = (double)(processed - prevProcessed) / ((double)(sw.ElapsedTicks - prevTicks) / Stopwatch.Frequency);
                        prevTicks = sw.ElapsedTicks;
                        prevProcessed = processed;

                        if (processed > maxItemsOnScreen)
                        {
                            dopes.Text = string.Format("{0:0.00} Dopes/s", r).PadLeft(15);
                            avgSum += r;
                            avgN++;
                        }

                        prevMs = sw.ElapsedMilliseconds;
                    }
                }

                _ = DispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() => loop()));
            };

            sw.Start();

            _ = DispatcherQueue.TryEnqueue(new Microsoft.UI.Dispatching.DispatcherQueueHandler(() => loop()));
        }

        private void SetControlsAtStart()
        {
            startChangeST.Visibility = startST.Visibility = startGridST.Visibility = Visibility.Collapsed;
            stop.Visibility = dopes.Visibility = Visibility.Visible;
            absolute.Children.Clear();
            grid.Children.Clear();
            dopes.Text = "Warming up..";
        }

        void startST_Clicked(System.Object sender, object e)
        {
            SetControlsAtStart();
            StartTestST();
        }

        void startGridST_Clicked(System.Object sender, object e)
        {
            SetControlsAtStart();
            StartTestBindings();
        }

        void startChangeST_Clicked(System.Object sender, object e)
        {
            SetControlsAtStart();
            StartTestChangeST();
        }

        void startChangeReuse_Clicked(System.Object sender, object e)
        {
            SetControlsAtStart();
            StartTestReuseST();
        }

        void Stop_Clicked(System.Object sender, object e)
        {
            breakTest = true;
            stop.Visibility = Visibility.Collapsed;
            startChangeST.Visibility = startST.Visibility = startGridST.Visibility = Visibility.Visible;
        }

        async void startAll_Clicked(System.Object sender, object e)
        {
            var deviceInfo = new
            {
                OS = "Windows",
                OSVersion = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                DeviceModel = "",
                DeviceManufacturer = "",
                DeviceName = "",
                DeviceIdiom = "Desktop",
                DeviceType = ""
            };

#if DEBUG
            int testLengthMs = 5000;
#else
            int testLengthMs = 60000;
#endif
            int pauseLengthMs = 100;

            startST_Clicked(default, default);
            await Task.Delay(testLengthMs);
            Stop_Clicked(default, default);
            await Task.Delay(pauseLengthMs);
            _ = Decimal.TryParse(dopes.Text.Replace(" Dopes/s (AVG)", "").Trim(), out var resultST);

            startChangeST_Clicked(default, default);
            await Task.Delay(testLengthMs);
            Stop_Clicked(default, default);
            await Task.Delay(pauseLengthMs);
            _ = Decimal.TryParse(dopes.Text.Replace(" Dopes/s (AVG)", "").Trim(), out var resultChangeST);

            //startChangeReuse_Clicked(default, default);
            //await Task.Delay(testLengthMs);
            //Stop_Clicked(default, default);
            //await Task.Delay(pauseLengthMs);
            //_ = Decimal.TryParse(dopes.Text.Replace(" Dopes/s (AVG)", "").Trim(), out var resultReuseST);

            //startGridST_Clicked(default, default);
            //await Task.Delay(testLengthMs);
            //Stop_Clicked(default, default);
            //await Task.Delay(pauseLengthMs);
            //_ = Decimal.TryParse(dopes.Text.Replace(" Dopes/s (AVG)", "").Trim(), out var resultGridST);

            var platformVersion = "WinUI 3 (SDK 10.0.19041.23)";

            var results = new
            {
                DeviceInfo = deviceInfo,
                Platform = platformVersion,
                Build = resultST,
                Change = resultChangeST,
                Reuse = 0,
                Grid = 0
            };
            string jsonString = JsonConvert.SerializeObject(results);
            dopes.Text = $"Build: {results.Build}; Change: {results.Change}";

            Console.WriteLine(jsonString);
#if !DEBUG
            try
            {
                var client = new BlobServiceClient(new Uri(Config.StorageUrl), new AzureSasCredential(Config.StorageSasToken));
                var blobContainerClient = client.GetBlobContainerClient("results");
                await blobContainerClient.CreateIfNotExistsAsync();

                var filename = $"{deviceInfo.OS}-{platformVersion}-{DateTime.UtcNow.ToString("yyyy-MM-dd-hh-mm-ss")}.json";

                using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
                    await blobContainerClient.UploadBlobAsync(filename, memoryStream);

                Console.WriteLine("Uploaded.");
            }
            catch (Exception ex)
            {
            }
#endif
        }
    }

    public class BindingItem : INotifyPropertyChanged
    {
        private double top;
        private double left;
        private double rotation;
        private Color color;

        public double Top
        {
            get => top; set
            {
                top = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Top)));
            }
        }
        public double Left
        {
            get => left; set
            {
                left = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Left)));
            }
        }
        public double Rotation
        {
            get => rotation; set
            {
                rotation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rotation)));
            }
        }
        public Color Color
        {
            get => color; set
            {
                color = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
