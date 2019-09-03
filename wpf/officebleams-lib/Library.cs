using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using static System.Net.Dns;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace officebleams_lib
{
    public class Beacon
    {
        public double Distance { get; set; }

        public double Rssi { get; }

        public int TxPower { get; }

        public string Address { get; }

        public Beacon(string address, int txPower, double rssi)
        {
            Address = address;
            Distance = (0.89976 * Math.Pow(rssi / txPower, 7.7095)) + 0.111;
            TxPower = txPower;
            Rssi = rssi;
        }
    }

    public class Node
    {
        public string Id { get; }

        public Beacon Beacon { get; }

        public Node(string id, string address, int txPower, double rssi)
        {
            Id = id;
            Beacon = new Beacon(address, txPower, rssi);
        }
    }

    public class Data
    {
        public int Length { get; set; } = 0;

        public byte[] Buffer { get; } = new byte[30];

        public static string[] Fields { get; set; } = null;

        public static List<Node> Nodes { get; set; } = new List<Node>();

        public static List<(string, string)> References { get; set; } = new List<(string, string)>();

        public static bool IsReference((string id, string address) reference)
        {
            return reference.id == Fields[0] && reference.address == Fields[3];
        }
    }

    public class Network
    {
        public int Port { get; }

        public NetworkStream Stream { get; set; }

        public TcpClient Client { get; set; }

        public TcpListener Connection { get; }

        public Network(int port)
        {
            Port = port;
            IPAddress address = GetHostEntry(GetHostName()).AddressList[1];
            Connection = new TcpListener(address, port);
        }
    }

    public class Triangulation
    {
        public (double length, double width) ISize { get; set; }

        public (double length, double width) JSize { get; set; }

        public Triangulation((double, double) iSize, (double, double) jSize)
        {
            ISize = iSize;
            JSize = jSize;
        }

        public double GetCosFactor(double adjacentSide1, double adjacentSide2, double oppositeSide)
            => Math.Acos(GetAngleRatio(adjacentSide1, adjacentSide2, oppositeSide));

        public double GetSinFactor(double adjacentSide1, double adjacentSide2, double oppositeSide)
            => Math.Asin(GetAngleRatio(adjacentSide1, adjacentSide2, oppositeSide));

        public double GetAngleRatio(double adjacentSide1, double adjacentSide2, double oppositeSide)
            => (Math.Pow(adjacentSide1, 2) + Math.Pow(adjacentSide2, 2) - Math.Pow(oppositeSide, 2)) / (2 * adjacentSide1 * adjacentSide2);

        public (double, double) GetCoordinates(double nodeDistance1, double nodeDistance2, double nodeDistance3)
        {
            double x, y;

            if (nodeDistance1 != 0 && nodeDistance2 != 0 && nodeDistance3 != 0)
            {
                x = nodeDistance1 * GetCosFactor(nodeDistance1, JSize.width, nodeDistance2);
                y = nodeDistance1 * GetSinFactor(nodeDistance1, JSize.width, nodeDistance2);
            }
            else if (nodeDistance1 != 0 && nodeDistance2 != 0 && nodeDistance3 == 0)
            {
                x = nodeDistance1 * GetCosFactor(nodeDistance1, JSize.width, nodeDistance2);
                y = nodeDistance1 * GetSinFactor(nodeDistance1, JSize.width, nodeDistance2) * -1;
            }
            else if (nodeDistance1 != 0 && nodeDistance2 == 0 && nodeDistance3 != 0)
            {
                x = nodeDistance1 * GetSinFactor(nodeDistance1, JSize.length / 2 + (ISize.length - JSize.length), nodeDistance2) * -1;
                y = nodeDistance1 * GetCosFactor(nodeDistance1, JSize.length / 2 + (ISize.length - JSize.length), nodeDistance2);
            }
            else if (nodeDistance1 == 0 && nodeDistance2 != 0 && nodeDistance3 != 0)
            {
                x = 0;
                y = 0;
            }
            else if ((nodeDistance1 != 0 && nodeDistance2 == 0 && nodeDistance3 == 0)
                  || (nodeDistance1 == 0 && nodeDistance2 != 0 && nodeDistance3 == 0)
                  || (nodeDistance1 == 0 && nodeDistance2 == 0 && nodeDistance3 != 0)
                  || (nodeDistance1 == 0 && nodeDistance2 == 0 && nodeDistance3 == 0))
            {
                x = double.PositiveInfinity;
                y = double.PositiveInfinity;
            }
            else
            {
                x = double.NegativeInfinity;
                y = double.NegativeInfinity;
            }

            return (x, y);
        }
    }

    /// <summary>
    /// Contains static methods for making <see cref="Window"/>s glow.
    /// </summary>
    public static class GlowManager
    {
        static readonly DependencyProperty GlowInfoProperty =
            DependencyProperty.RegisterAttached("GlowInfo",
                                                typeof(GlowInfo),
                                                typeof(GlowManager));

        /// <summary>
        /// Identifies the ActiveGlowBrush attached property.
        /// </summary>
        public static readonly DependencyProperty ActiveGlowBrushProperty =
            DependencyProperty.RegisterAttached("ActiveGlowBrush",
                                                typeof(Brush),
                                                typeof(GlowManager),
                                                new FrameworkPropertyMetadata(
                                                    new SolidColorBrush(
                                                        Color.FromArgb(0xff, 0x00, 0x7a, 0xcc))));

        /// <summary>
        /// Identifies the InactiveGlowBrush attached property.
        /// </summary>
        public static readonly DependencyProperty InactiveGlowBrushProperty =
            DependencyProperty.RegisterAttached("InactiveGlowBrush",
                                                typeof(Brush),
                                                typeof(GlowManager),
                                                new FrameworkPropertyMetadata(
                                                    new SolidColorBrush(Colors.Black) { Opacity = 0.25 }));

        /// <summary>
        /// Identifies the EnableGlow attached property
        /// </summary>
        public static readonly DependencyProperty EnableGlowProperty =
            DependencyProperty.RegisterAttached("EnableGlow",
                                                typeof(bool),
                                                typeof(GlowManager),
                                                new FrameworkPropertyMetadata((d, e) =>
       {
           if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
           {
               if ((bool)e.NewValue == true)
                   Assign((Window)d);
               else
                   Unassign((Window)d);
           }
       }));

        static Action<Window> assignGlows = window =>
        {
            if (GetGlowInfo(window) != null)
                throw new InvalidOperationException("Glows have already been assigned.");
            GlowInfo glowInfo = new GlowInfo();
            glowInfo.glows.Add(new GlowWindow { Location = Location.Left });
            glowInfo.glows.Add(new GlowWindow { Location = Location.Top });
            glowInfo.glows.Add(new GlowWindow { Location = Location.Right });
            glowInfo.glows.Add(new GlowWindow { Location = Location.Bottom });
            foreach (GlowWindow glow in glowInfo.glows)
            {
                glow.Owner = window;
                glow.OwnerChanged();
            }
            SetGlowInfo(window, glowInfo);
        };

        static bool Assign(Window window)
        {
            if (window == null)
                throw new ArgumentNullException("window");
            if (GetGlowInfo(window) != null)
                return false;
            else if (!window.IsLoaded)
                window.SourceInitialized += delegate
                {
                    assignGlows(window);
                };
            else
            {
                assignGlows(window);
            }
            return true;
        }

        static bool Unassign(Window window)
        {
            GlowInfo info = GetGlowInfo(window);
            if (info == null)
                return false;
            else
            {
                foreach (GlowWindow glow in info.glows)
                {
                    try
                    {
                        glow.Close();
                    }
                    catch
                    {
                        // Do nothing
                    }
                }
                SetGlowInfo(window, null);
            }
            return true;
        }

        /// <summary>
        /// Gets the <see cref="Brush"/> for the glow when the <see cref="Window"/> is active.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <returns>The brush for the glow when the <see cref="Window"/> is active.</returns>
        [Category("Brush")]
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static Brush GetActiveGlowBrush(Window window)
        {
            return (Brush)window.GetValue(ActiveGlowBrushProperty);
        }

        /// <summary>
        /// Sets the <see cref="Brush"/> for the glow when the <see cref="Window"/> is active.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <param name="value">The <see cref="Brush"/> for the glow when the <see cref="Window"/> is active.</param>
        public static void SetActiveGlowBrush(Window window, Brush value)
        {
            window.SetValue(ActiveGlowBrushProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="Brush"/> for the glow when the <see cref="Window"/> is inactive.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <returns>The <see cref="Brush"/> for the glow when the <see cref="Window"/> is inactive.</returns>
        [Category("Brush")]
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static Brush GetInactiveGlowBrush(Window window)
        {
            return (Brush)window.GetValue(InactiveGlowBrushProperty);
        }

        /// <summary>
        /// Sets the <see cref="Brush"/> for the glow when the <see cref="Window"/> is inactive.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <param name="value">The <see cref="Brush"/> for the glow when the <see cref="Window"/> is inactive.</param>
        public static void SetInactiveGlowBrush(Window window, Brush value)
        {
            window.SetValue(InactiveGlowBrushProperty, value);
        }

        /// <summary>
        /// Gets whether glows are enabled for the <see cref="Window"/>.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <returns>Whether glows are enabled for the <see cref="Window"/>.</returns>
        [Category("Appearance")]
        [AttachedPropertyBrowsableForType(typeof(Window))]
        public static bool GetEnableGlow(Window window)
        {
            return (bool)window.GetValue(EnableGlowProperty);
        }

        /// <summary>
        /// Sets whether glows are enabled for the <see cref="Window"/>.
        /// </summary>
        /// <param name="window">The <see cref="Window"/>.</param>
        /// <param name="value">Whether glows are enabled for the <see cref="Window"/>.</param>
        public static void SetEnableGlow(Window window, bool value)
        {
            window.SetValue(EnableGlowProperty, value);
        }

        internal static GlowInfo GetGlowInfo(Window window)
        {
            return (GlowInfo)window.GetValue(GlowInfoProperty);
        }

        internal static void SetGlowInfo(Window window, GlowInfo info)
        {
            window.SetValue(GlowInfoProperty, info);
        }
    }

    class GlowInfo
    {
        public readonly Collection<GlowWindow> glows = new Collection<GlowWindow>();
    }

    enum Location
    {
        Left, Top, Right, Bottom
    }

    [TemplatePart(Name = "PART_Glow", Type = typeof(Border))]
    class GlowWindow : Window
    {
        static GlowWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GlowWindow), new FrameworkPropertyMetadata(typeof(GlowWindow)));
        }

        public double glowThickness = 10d, tolerance = 72d;

        public Location Location;
        public Action Update;
        public Func<Point, Cursor> GetCursor;
        public Func<Point, HT> GetHT;
        Border Glow;
        Func<bool> canResize;

        public void NotifyResize(HT ht)
        {
            NativeMethods.SendNotifyMessage(new WindowInteropHelper(Owner).Handle, (int)WM.NCLBUTTONDOWN, (IntPtr)ht, IntPtr.Zero);
        }

        public GlowWindow()
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = true;
            SnapsToDevicePixels = true;
            ShowInTaskbar = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Glow = (Border)GetTemplateChild("PART_Glow");
            if (Glow == null)
                throw new Exception("PART_Glow not found.");
            Update();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int ws_ex = NativeMethods.GetWindowLong(hwnd, (int)GWL.EXSTYLE);
            ws_ex |= (int)WS_EX.TOOLWINDOW;
            NativeMethods.SetWindowLong(hwnd, (int)GWL.EXSTYLE, ws_ex);
            HwndSource.FromHwnd(hwnd).AddHook(WinMain);
        }

        public virtual IntPtr WinMain(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WM)msg)
            {
                case WM.MOUSEACTIVATE:
                    {
                        handled = true;
                        return new IntPtr(3); // MA_NOACTIVATE
                    }
                case WM.NCHITTEST:
                    {
                        if (canResize())
                        {
                            Point ptScreen = NativeMethods.LPARAMTOPOINT(lParam);
                            Point ptClient = PointFromScreen(ptScreen);
                            Cursor = GetCursor(ptClient);
                        }
                        break;
                    }
                case WM.LBUTTONDOWN:
                    {
                        POINT ptScreenWin32;
                        NativeMethods.GetCursorPos(out ptScreenWin32);
                        Point ptScreen = new Point(ptScreenWin32.x, ptScreenWin32.y);
                        Point ptClient = PointFromScreen(ptScreen);
                        HT result = GetHT(ptClient);
                        IntPtr ownerHwnd = new WindowInteropHelper(Owner).Handle;
                        NativeMethods.SendNotifyMessage(ownerHwnd, (int)WM.NCLBUTTONDOWN, (IntPtr)result, IntPtr.Zero);
                        break;
                    }
                case WM.GETMINMAXINFO:
                    {
                        MINMAXINFO info = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
                        info.ptMaxSize = info.ptMaxTrakSize = new POINT { x = int.MaxValue, y = int.MaxValue };
                        Marshal.StructureToPtr(info, lParam, true);
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        public void OwnerChanged()
        {
            canResize = () => Owner.ResizeMode == ResizeMode.CanResize ? true :
            Owner.ResizeMode == ResizeMode.CanResizeWithGrip ? true : false;

            switch (Location)
            {
                case Location.Left:
                    {
                        GetCursor = pt =>
                            new Rect(new Point(0, 0), new Size(ActualWidth, tolerance)).Contains(pt) ?
                            Cursors.SizeNWSE :
                            new Rect(new Point(0, ActualHeight - tolerance), new Size(ActualWidth, tolerance)).Contains(pt) ?
                            Cursors.SizeNESW :
                            Cursors.SizeWE;

                        GetHT = pt => new Rect(new Point(0, 0), new Size(ActualWidth, tolerance)).Contains(pt) ?
                            HT.TOPLEFT :
                            new Rect(new Point(0, ActualHeight - tolerance), new Size(ActualWidth, tolerance)).Contains(pt) ?
                            HT.BOTTOMLEFT :
                            HT.LEFT;

                        Update = delegate
                        {
                            if (Glow != null)
                                Glow.Margin = new Thickness(glowThickness, glowThickness, -glowThickness, glowThickness);
                            Left = Owner.Left - glowThickness;
                            Top = Owner.Top - glowThickness;
                            Width = glowThickness;
                            Height = Owner.ActualHeight + glowThickness * 2;
                        };
                        break;
                    }

                case Location.Top:
                    {
                        GetCursor = pt =>
                            new Rect(new Point(0, 0), new Size(tolerance, glowThickness)).Contains(pt) ?
                            Cursors.SizeNWSE :
                            new Rect(new Point(ActualWidth - tolerance, 0), new Size(tolerance, ActualHeight)).Contains(pt) ?
                            Cursors.SizeNESW :
                            Cursors.SizeNS;

                        GetHT = pt =>
                            new Rect(new Point(0, 0), new Size(tolerance, glowThickness)).Contains(pt) ?
                            HT.TOPLEFT :
                            new Rect(new Point(ActualWidth - tolerance, 0), new Size(tolerance, ActualHeight)).Contains(pt) ?
                            HT.TOPRIGHT :
                            HT.TOP;

                        Update = delegate
                        {
                            if (Glow != null)
                                Glow.Margin = new Thickness(glowThickness, glowThickness, glowThickness, -glowThickness);
                            Left = Owner.Left - glowThickness;
                            Top = Owner.Top - glowThickness;
                            Width = Owner.ActualWidth + glowThickness * 2;
                            Height = glowThickness;
                        };
                        break;
                    }

                case Location.Right:
                    {
                        GetCursor = pt =>
                            new Rect(new Point(0, 0), new Size(ActualWidth, tolerance)).Contains(pt) ?
                            Cursors.SizeNESW :
                            new Rect(new Point(0, ActualHeight - tolerance), new Size(ActualWidth, tolerance)).Contains(pt) ?
                            Cursors.SizeNWSE :
                            Cursors.SizeWE;

                        GetHT = pt => new Rect(new Point(0, 0), new Size(ActualWidth, tolerance)).Contains(pt) ?
                            HT.TOPRIGHT :
                            new Rect(new Point(0, ActualHeight - tolerance), new Size(ActualWidth, tolerance)).Contains(pt) ?
                            HT.BOTTOMRIGHT :
                            HT.RIGHT;


                        Update = delegate
                        {
                            if (Glow != null)
                                Glow.Margin = new Thickness(-glowThickness, glowThickness, glowThickness, glowThickness);
                            Left = Owner.Left + Owner.ActualWidth;
                            Top = Owner.Top - glowThickness;
                            Width = glowThickness;
                            Height = Owner.ActualHeight + glowThickness * 2;
                        };
                        break;
                    }

                case Location.Bottom:
                    {
                        GetCursor = pt =>
                            new Rect(new Point(0, 0), new Size(tolerance, glowThickness)).Contains(pt) ?
                            Cursors.SizeNESW :
                            new Rect(new Point(ActualWidth - tolerance, 0), new Size(tolerance, ActualHeight)).Contains(pt) ?
                            Cursors.SizeNWSE :
                            Cursors.SizeNS;

                        GetHT = pt =>
                            new Rect(new Point(0, 0), new Size(tolerance, glowThickness)).Contains(pt) ?
                            HT.BOTTOMLEFT :
                            new Rect(new Point(ActualWidth - tolerance, 0), new Size(tolerance, ActualHeight)).Contains(pt) ?
                            HT.BOTTOMRIGHT :
                            HT.BOTTOM;

                        Update = delegate
                        {
                            if (Glow != null)
                                Glow.Margin = new Thickness(glowThickness, -glowThickness, glowThickness, glowThickness);
                            Left = Owner.Left - glowThickness;
                            Top = Owner.Top + Owner.ActualHeight;
                            Width = Owner.ActualWidth + glowThickness * 2;
                            Height = glowThickness;
                        };
                        break;
                    }
            }
            Owner.LocationChanged += delegate
            {
                Update();
            };
            Owner.SizeChanged += delegate
            {
                Update();
            };
            Owner.StateChanged += delegate
            {
                switch (Owner.WindowState)
                {
                    case WindowState.Maximized:
                        Hide();
                        break;
                    default:
                        Show();
                        Owner.Activate();
                        break;
                }
            };
            Owner.Activated += delegate
            {
                Binding activeBrushBinding = new Binding();
                activeBrushBinding.Path = new PropertyPath(GlowManager.ActiveGlowBrushProperty);
                activeBrushBinding.Source = Owner;
                SetBinding(ForegroundProperty, activeBrushBinding);
            };
            Owner.Deactivated += delegate
            {
                Binding activeBrushBinding = new Binding();
                activeBrushBinding.Path = new PropertyPath(GlowManager.InactiveGlowBrushProperty);
                activeBrushBinding.Source = Owner;
                SetBinding(ForegroundProperty, activeBrushBinding);
            };
            Update();
            Show();
        }
    }
    
    enum HT
    {
        BORDER = 18,
        BOTTOM = 15,
        BOTTOMLEFT = 16,
        BOTTOMRIGHT = 17,
        LEFT = 10,
        RIGHT = 11,
        TOP = 12,
        TOPLEFT = 13,
        TOPRIGHT = 14
    }

    enum WM
    {
        NCLBUTTONDOWN = 0x00A1,
        MOUSEACTIVATE = 0x0021,
        GETMINMAXINFO = 0x0024,
        LBUTTONDOWN = 0x0201,
        NCHITTEST = 0x0084
    }

    enum GWL
    {
        EXSTYLE = -20
    }

    enum WS_EX
    {
        TOOLWINDOW = 0x00000080
    }

    class NativeMethods
    {
        public static Func<short, short, IntPtr> MAKELPARAM = (wLow, wHigh) =>
        {
            return new IntPtr((wHigh << 16) | (wLow & 0xFFFF));
        };

        public static Func<IntPtr, Point> LPARAMTOPOINT = lParam =>
        {
            return new Point((int)lParam & 0xFFFF, ((int)lParam >> 16) & 0xFFFF);
        };

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SendNotifyMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrakSize;
    }
}
