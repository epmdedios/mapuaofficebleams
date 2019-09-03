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

        private delegate void Connection(Network network);
        private delegate void Status(string status);
        private int invokes = 0;

        private void UpdateStatusBar(string status)
        {
            statusCaption.Text = status;
        }

        private void TimeConnection(object s)
        {
            AutoResetEvent state = (AutoResetEvent)s;
            Connection node1 = new Connection(StartConnection);
            Connection node2 = new Connection(StartConnection);
            Connection node3 = new Connection(StartConnection);

            if (invokes == 5)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Status(UpdateStatusBar),
                    "Scanning finally stopped!");

                invokes = 0;
                state.Set();
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Status(UpdateStatusBar),
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
                    string.Format("Waiting for connection @ {0}...", server.Port));

                server.Client = server.Connection.AcceptTcpClient();

                using (server.Stream = server.Client.GetStream())
                {
                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new Status(UpdateStatusBar),
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
                                "Waiting for proximity...");


                            if (int.Parse(ASCII.GetString(buffer, 0, buffer.Length)) != 0)
                            {
                                Application.Current.Dispatcher.BeginInvoke(
                                    DispatcherPriority.Normal,
                                    new Status(UpdateStatusBar),
                                    "The proximity has been triggered!");

                                var state = new AutoResetEvent(false);
                                var timer = new Timer(TimeConnection, state, 0, 60000);

                                state.WaitOne();
                                timer.Dispose();

                                Application.Current.Dispatcher.BeginInvoke(
                                    DispatcherPriority.Normal,
                                    new Status(UpdateStatusBar),
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
                string.Format("Waiting for connection @ {0}...", server.Port));

            server.Client = server.Connection.AcceptTcpClient();

            using (server.Stream = server.Client.GetStream())
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Status(UpdateStatusBar),
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

                    DisplayNodes(new List<Node>(Data.Nodes));

                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new Status(UpdateStatusBar),
                        string.Format("Connection to {0} has ended.", server.Port));
                }
            }
        }

        public void DisplayNodes(List<Node> updatedNodes)
        {
            foreach (Node node in updatedNodes)
            {
                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Status(UpdateStatusBar),
                    string.Format(
                        "Node: {0}\tAddress: {1}\tRSSI: {2:F2}\tTX Power: {3}\tDistance: {4:F4} m",
                        node.Id,
                        node.Beacon.Address,
                        node.Beacon.Rssi,
                        node.Beacon.TxPower,
                        node.Beacon.Distance));
            }
        }

        public Root()
        {
            DataContext = this;
            InitializeComponent();
            profilePopup.OnMouseClickSwitchButtonEvent += new EventHandler(OnMouseClickSwitchButton);
            profilePopup.OnMouseClickSignOutButtonEvent += new EventHandler(OnMouseClickSignOutButton);
            Connection proximity = new Connection(GetProximityStatus);
            proximity.BeginInvoke(new Network(55555), null, null);
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
            nodeRangeMap1.Visibility = Visibility.Collapsed;
            nodeRangeMap2.Visibility = Visibility.Collapsed;
            nodeRangeMap3.Visibility = Visibility.Collapsed;
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
