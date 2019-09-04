using officebleams_lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using static System.Text.Encoding;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace officebleams_gui
{
    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
        ACCENT_INVALID_STATE = 5
    }

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public uint AccentFlags;
        public uint GradientColor;
        public uint AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    public partial class Root : Window
    {
        private uint _blurOpacity;
        private readonly uint _blurBackgroundColor = 0x990000;

        public double BlurOpacity { get => _blurOpacity; set { _blurOpacity = (uint)value; EnableBlur(); } }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                GradientColor = (_blurOpacity << 24) | (_blurBackgroundColor & 0xFFFFFF)
            };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);

            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        //internal void ClearSelection(object sender)
        //{
        //    foreach (SideButton button in navigationButtonStack.Children)
        //    {
        //        button.IsSelected = button.Equals(sender as SideButton) ? true : false;
        //    }
        //}

        private int invokes = 0;
        private readonly double scaleFactor = 150 / 4;
        private (double x, double y) beacon1 = (double.NegativeInfinity, double.NegativeInfinity);
        private (double x, double y) beacon2 = (double.NegativeInfinity, double.NegativeInfinity);
        private (double x, double y) beacon3 = (double.NegativeInfinity, double.NegativeInfinity);
        private List<Node> batchNodes = new List<Node>();
        private delegate void Coordinates(int beacon, (double x, double y) coordinates);
        private delegate void Connection(Network network);
        private delegate void Log(int port, List<Node> log);
        private delegate void Map(int beacon);
        private delegate void Status(int port, string status);
        private delegate void UI();

        private void UpdateCanvas(int beacon, (double x, double y) coordinates)
        {
            switch (beacon)
            {
                case 1:
                    Canvas.SetLeft(beaconMap1, coordinates.x);
                    Canvas.SetBottom(beaconMap1, coordinates.y);
                    beaconMap1.Visibility = Visibility.Visible;
                    break;

                case 2:
                    Canvas.SetLeft(beaconMap2, coordinates.x);
                    Canvas.SetBottom(beaconMap2, coordinates.y);
                    beaconMap2.Visibility = Visibility.Visible;
                    break;

                case 3:
                    Canvas.SetLeft(beaconMap3, coordinates.x);
                    Canvas.SetBottom(beaconMap3, coordinates.y);
                    beaconMap3.Visibility = Visibility.Visible;
                    break;

                default:
                    return;
            }
        }

        private void UpdateStatusBar(int port, string status)
        {
            statusCaption.Text = status;

            switch (port)
            {
                case 55555:
                    proximityStatus.Text = status;
                    break;

                case 55556:
                    node1Status.Text = status;
                    break;

                case 55557:
                    node2Status.Text = status;
                    break;

                case 55558:
                    node3Status.Text = status;
                    break;

                default:
                    break;
            }
        }

        private void UpdateUI()
        {
            logList.Children.Clear();
        }

        private void MapBeacon(int beacon)
        {
            while (true)
            {
                switch (beacon)
                {
                    case 1:
                        if (beacon1.x == double.PositiveInfinity && beacon1.y == double.PositiveInfinity)
                        {
                            // OUTSIDE
                            continue;
                        }
                        else if (beacon1.x == double.NegativeInfinity && beacon1.y == double.NegativeInfinity)
                        {
                            // MISSING
                            continue;
                        }
                        else
                        {
                            Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Normal,
                                new Coordinates(UpdateCanvas),
                                1,
                                (beacon1.x * scaleFactor, beacon1.y * scaleFactor));
                        }

                        break;

                    case 2:
                        if (beacon2.x == double.PositiveInfinity && beacon2.y == double.PositiveInfinity)
                        {
                            // OUTSIDE
                            continue;
                        }
                        else if (beacon2.x == double.NegativeInfinity && beacon2.y == double.NegativeInfinity)
                        {
                            // MISSING
                            continue;
                        }
                        else
                        {
                            Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Normal,
                                new Coordinates(UpdateCanvas),
                                2,
                                (beacon2.x * scaleFactor, beacon2.y * scaleFactor));
                        }

                        break;

                    case 3:
                        if (beacon3.x == double.PositiveInfinity && beacon3.y == double.PositiveInfinity)
                        {
                            // OUTSIDE
                            continue;
                        }
                        else if (beacon3.x == double.NegativeInfinity && beacon3.y == double.NegativeInfinity)
                        {
                            // MISSING
                            continue;
                        }
                        else
                        {
                            Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Normal,
                                new Coordinates(UpdateCanvas),
                                3,
                                (beacon3.x * scaleFactor, beacon3.y * scaleFactor));
                        }

                        break;

                    default:
                        continue;
                }

                Thread.Sleep(5000);
            }                     
        }

        private void TimeConnection(object s)
        {
            AutoResetEvent state = (AutoResetEvent)s;
            Connection node1 = new Connection(StartConnection);
            Connection node2 = new Connection(StartConnection);
            Connection node3 = new Connection(StartConnection);

            if (batchNodes.Count == 9)
            {
                List<Node> beaconData1 = batchNodes.FindAll(delegate (Node node)
                {
                    return node.Beacon.Address == "D9:51:6B:6E:B1:92";
                });

                List<Node> beaconData2 = batchNodes.FindAll(delegate (Node node)
                {
                    return node.Beacon.Address == "F2:DE:EF:09:9A:68";
                });

                List<Node> beaconData3 = batchNodes.FindAll(delegate (Node node)
                {
                    return node.Beacon.Address == "FE:DF:1B:48:FC:5A";
                });

                beacon1 = new Triangulation((8, 4), (6, 4)).GetCoordinates(
                    beaconData1.Find(delegate (Node node) { return node.Id == "PN1"; })?.Beacon.Distance ?? double.PositiveInfinity,
                    beaconData1.Find(delegate (Node node) { return node.Id == "PN2"; })?.Beacon.Distance ?? double.PositiveInfinity,
                    beaconData1.Find(delegate (Node node) { return node.Id == "PN3"; })?.Beacon.Distance ?? double.PositiveInfinity);

                beacon2 = new Triangulation((8, 4), (6, 4)).GetCoordinates(
                    beaconData2.Find(delegate (Node node) { return node.Id == "PN1"; })?.Beacon.Distance ?? double.PositiveInfinity,
                    beaconData2.Find(delegate (Node node) { return node.Id == "PN2"; })?.Beacon.Distance ?? double.PositiveInfinity,
                    beaconData2.Find(delegate (Node node) { return node.Id == "PN3"; })?.Beacon.Distance ?? double.PositiveInfinity);

                beacon3 = new Triangulation((8, 4), (6, 4)).GetCoordinates(
                    beaconData3.Find(delegate (Node node) { return node.Id == "PN1"; })?.Beacon.Distance ?? double.PositiveInfinity,
                    beaconData3.Find(delegate (Node node) { return node.Id == "PN2"; })?.Beacon.Distance ?? double.PositiveInfinity,
                    beaconData3.Find(delegate (Node node) { return node.Id == "PN3"; })?.Beacon.Distance ?? double.PositiveInfinity);

                batchNodes.Clear();                
            }

            if (invokes == 5)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Status(UpdateStatusBar),
                    55555,
                    "Scanning finally stopped!");

                invokes = 0;
                state.Set();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Status(UpdateStatusBar),
                    55555,
                    string.Format("Scanning started {0} time(s).", ++invokes));

                node1.BeginInvoke(new Network(55556), null, null);
                node2.BeginInvoke(new Network(55557), null, null);
                node3.BeginInvoke(new Network(55558), null, null);
            }
        }

        private void GetProximityStatus(object s)
        {
            Network server = (Network)s;

            while (true)
            {
                try
                {
                    server.Connection.Start();
                }
                catch (SocketException)
                {
                    server.Connection.Stop();
                    return;
                }

                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Status(UpdateStatusBar),
                    server.Port,
                    string.Format("Waiting for connection @ {0}...", server.Port));

                server.Client = server.Connection.AcceptTcpClient();

                using (server.Stream = server.Client.GetStream())
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new Status(UpdateStatusBar),
                        server.Port,
                        string.Format("Connected to {0}!", server.Port));

                    byte[] buffer = new byte[1];
                    server.Stream.ReadTimeout = 10000;

                    try
                    {
                        while (server.Stream.Read(buffer, 0, buffer.Length) != 0)
                        {
                            Application.Current.Dispatcher.BeginInvoke(
                                DispatcherPriority.Normal,
                                new Status(UpdateStatusBar),
                                server.Port,
                                "Waiting for proximity...");


                            if (int.Parse(ASCII.GetString(buffer, 0, buffer.Length)) != 0)
                            {
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new UI(UpdateUI));


                                Application.Current.Dispatcher.BeginInvoke(
                                    DispatcherPriority.Normal,
                                    new Status(UpdateStatusBar),
                                    server.Port,
                                    "The proximity has been triggered!");

                                var state = new AutoResetEvent(false);
                                var timer = new Timer(TimeConnection, state, 0, 60000);

                                state.WaitOne();
                                timer.Dispose();

                                Application.Current.Dispatcher.BeginInvoke(
                                    DispatcherPriority.Normal,
                                    new Status(UpdateStatusBar),
                                    server.Port,
                                    "Connections ended.");

                                break;
                            }
                        }
                    }
                    catch (IOException)
                    {
                        server.Client.Close();
                        server.Connection.Stop();
                    }
                    catch (NullReferenceException)
                    {
                        Environment.Exit(1);
                    }
                }
            }
        }

        private void StartConnection(object s)
        {
            Network server = (Network)s;

            try
            {
                server.Connection.Start();
            }
            catch (SocketException)
            {
                server.Connection.Stop();
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Status(UpdateStatusBar),
                server.Port,
                string.Format("Waiting for connection @ {0}...", server.Port));

            server.Client = server.Connection.AcceptTcpClient();

            using (server.Stream = server.Client.GetStream())
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Status(UpdateStatusBar),
                    server.Port,
                    string.Format("Connected to {0}!", server.Port));

                Data info = new Data();
                server.Stream.ReadTimeout = 10000;

                try
                {
                    while ((info.Length = server.Stream.Read(info.Buffer, 0, info.Buffer.Length)) != 0)
                    {
                        Data.Fields = ASCII.GetString(info.Buffer, 0, info.Length).Split(' ');
                        Data.Nodes.Add(new Node(id: Data.Fields[0],
                                                txPower: int.Parse(Data.Fields[1]),
                                                rssi: double.Parse(Data.Fields[2]),
                                                address: Data.Fields[3]));

                        if (!Data.References.Exists(Data.IsReference))
                        {
                            Data.References.Add((Data.Fields[0], Data.Fields[3]));
                        }
                    }
                }
                catch (IOException)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new Status(UpdateStatusBar),
                        server.Port,
                        string.Format("Creating nodes for {0}...", server.Port));

                    server.Client.Close();
                    server.Connection.Stop();

                    foreach ((string id, string address) reference in Data.References)
                    {
                        List<Node> results = Data.Nodes.FindAll(delegate (Node node)
                        {
                            return node.Id == reference.id && node.Beacon.Address == reference.address;
                        });

                        if (results.Count > 1)
                        {
                            List<double> rssis = new List<double>();
                            rssis.AddRange(from Node result in results select result.Beacon.Rssi);
                            Node mean = new Node(id: reference.id,
                                                 txPower: results[0].Beacon.TxPower,
                                                 rssi: rssis.Average(),
                                                 address: reference.address);

                            foreach (Node result in results)
                            {
                                Data.Nodes.Remove(result);
                            }

                            Data.Nodes.Add(mean);
                        }
                    }

                    List<Node> specificNodes;

                    switch (server.Port)
                    {
                        case 55556:
                            specificNodes = Data.Nodes.FindAll(delegate (Node node) { return node.Id == "PN1"; });
                            break;

                        case 55557:
                            specificNodes = Data.Nodes.FindAll(delegate (Node node) { return node.Id == "PN2"; });
                            break;

                        case 55558:
                            specificNodes = Data.Nodes.FindAll(delegate (Node node) { return node.Id == "PN3"; });
                            break;

                        default:
                            specificNodes = new List<Node>();
                            break;
                    }

                    batchNodes.AddRange(specificNodes);

                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new Log(UpdateLogs),
                        server.Port,
                        specificNodes);

                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new Status(UpdateStatusBar),
                        server.Port,
                        string.Format("Connection to {0} has ended.", server.Port));
                }
                catch (NullReferenceException)
                {
                    Environment.Exit(1);
                }
            }
        }

        public void UpdateLogs(int port, List<Node> updatedNodes)
        {
            foreach (Node node in updatedNodes)
            {
                var resources = new ResourceDictionary { Source = new Uri("/officebleams-lib;component/Themes/Resources.xaml", UriKind.RelativeOrAbsolute) };

                Grid g = new Grid() { Margin = new Thickness(20, 0, 20, 0) };

                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.50, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.50, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.50, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.75, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.00, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.75, GridUnitType.Star) });

                TextBlock t1 = new TextBlock() { FontSize = 13, Style = resources["textListStyle"] as Style, Text = port.ToString() };
                TextBlock t2 = new TextBlock() { FontSize = 13, Style = resources["textListStyle"] as Style, Text = node.Id };
                TextBlock t3 = new TextBlock() { FontSize = 13, Style = resources["textListStyle"] as Style, Text = node.Beacon.Address };
                TextBlock t4 = new TextBlock() { FontSize = 13, Style = resources["textListStyle"] as Style, Text = node.Beacon.Rssi.ToString("F2") };
                TextBlock t5 = new TextBlock() { FontSize = 13, Style = resources["textListStyle"] as Style, Text = node.Beacon.TxPower.ToString("F2") };
                TextBlock t6 = new TextBlock() { FontSize = 13, Style = resources["textListStyle"] as Style, Text = node.Beacon.Distance.ToString("F2") };

                Grid.SetColumn(t1, 0);
                Grid.SetColumn(t2, 1);
                Grid.SetColumn(t3, 2);
                Grid.SetColumn(t4, 3);
                Grid.SetColumn(t5, 4);
                Grid.SetColumn(t6, 5);

                g.Children.Add(t1);
                g.Children.Add(t2);
                g.Children.Add(t3);
                g.Children.Add(t4);
                g.Children.Add(t5);
                g.Children.Add(t6);

                logList.Children.Add(g);
            }
        }

        public Root()
        {
            DataContext = this;
            InitializeComponent();
            profilePopup.OnMouseClickSwitchButtonEvent += new EventHandler(OnMouseClickSwitchButton);
            profilePopup.OnMouseClickSignOutButtonEvent += new EventHandler(OnMouseClickSignOutButton);

            Connection proximity = new Connection(GetProximityStatus);
            Map beacon = new Map(MapBeacon);

            proximity.BeginInvoke(new Network(55555), null, null);
            beacon.BeginInvoke(1, null, null);
            beacon.BeginInvoke(2, null, null);
            beacon.BeginInvoke(3, null, null);
        }

        private void OnMouseClickSwitchButton(object sender, EventArgs e)
        {
            profileButton.Username = "Administrator";
            profileButton.Company = "Partex BSI";
            profilePopup.UpdateContent();
        }

        private void OnMouseClickSignOutButton(object sender, EventArgs e)
        {
            profileButton.Username = "Guest";
            profileButton.Company = "N/A";
            profilePopup.UpdateContent();
        }

        private void OnLoadRootWindow(object sender, RoutedEventArgs e)
        {
            EnableBlur();

            //dashboardButton.IsSelected = true;
            profileButton.Username = "Guest";
            profileButton.Company = "N/A";
            profilePopup.UpdateContent();

            dashboardContentGrid.Visibility = Visibility.Visible;
            mapContentGrid.Visibility = Visibility.Visible;

            nodeRangeMap1.Visibility = Visibility.Collapsed;
            nodeRangeMap2.Visibility = Visibility.Collapsed;
            nodeRangeMap3.Visibility = Visibility.Collapsed;

            beaconMap1.Visibility = Visibility.Collapsed;
            beaconMap2.Visibility = Visibility.Collapsed;
            beaconMap3.Visibility = Visibility.Collapsed;
        }

        private void OnMouseDownRootWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void OnMouseClickMinimizeButton(object sender, MouseButtonEventArgs e) => WindowState = WindowState.Minimized;

        private void OnMouseClickCloseButton(object sender, MouseButtonEventArgs e) => Close();

        private void OnMouseClickProfileButton(object sender, MouseButtonEventArgs e) => profilePopupContainer.IsOpen = true;

        private void OnMouseClickToggleNodeButton(object sender, MouseButtonEventArgs e)
        {
            nodeMap1.Visibility = nodeMap1.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            nodeMap2.Visibility = nodeMap2.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            nodeMap3.Visibility = nodeMap3.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnMouseClickToggleRangeButton(object sender, MouseButtonEventArgs e)
        {
            nodeRangeMap1.Visibility = nodeRangeMap1.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            nodeRangeMap2.Visibility = nodeRangeMap2.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            nodeRangeMap3.Visibility = nodeRangeMap3.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnMouseClickToggleBeaconButton(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
