using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using static System.Net.Dns;
using System.Net.Sockets;
using static System.Text.Encoding;
using System.Threading;
using System.Linq;

namespace AssetManagementSystem
{
    class Beacon
    {
        private double distance;
        private double rssi;
        private int txPower;
        private string address;
        public double Distance { get => distance; set => distance = value; }
        public double Rssi { get => rssi; }
        public int TxPower { get => txPower; }
        public string Address { get => address; }
        public Beacon(string address, int txPower, double rssi)
        {
            this.address = address;
            this.distance = 0.89976 * Math.Pow(rssi/txPower, 7.7095) + 0.111;
            this.txPower = txPower;
            this.rssi = rssi;
        }
    }
    class Node
    {
        private string id;
        private Beacon beacon;
        public string Id { get => id; }
        public Beacon Beacon { get => beacon; }
        public Node(string id, string address, int txPower, double rssi)
        {
            this.id = id;
            beacon = new Beacon(address, txPower, rssi);
        }
    }
    class Data
    {
        private int length;
        private byte[] buffer;
        private static string[] fields;
        private static List<Node> nodes;
        private static List<(string, string)> references;
        public int Length { get => length; set => length = value; }
        public byte[] Buffer { get => buffer; }
        public static string[] Fields { get => fields; set => fields = value; }
        public static List<Node> Nodes { get => nodes; set => nodes = value; }
        public static List<(string, string)> References { get => references; set => references = value; }
        public Data()
        {
            length = 0;
            buffer = new byte[30];
            fields = null;
            nodes = new List<Node>();
            references = new List<(string, string)>();
        }
        public static bool IsReference((string id, string address) reference)
        {
            return reference.id == Fields[0] && reference.address == Fields[3];
        }
    }
    class Network
    {
        private int port;
        private NetworkStream stream;
        private TcpClient client;
        private TcpListener connection;
        public int Port { get => port; }
        public NetworkStream Stream { get => stream; set => stream = value; }
        public TcpClient Client { get => client; set => client = value; }
        public TcpListener Connection { get => connection; }
        public Network(int port)
        {
            this.port = port;
            IPAddress address = GetHostEntry(GetHostName()).AddressList[1];
            connection = new TcpListener(address, port);
        }
    }
    static class IndoorLocalization
    {
        private static double GetCosAngle(double adjacentSide1, double adjacentSide2, double oppositeSide) => Math.Acos(GetAngleRatio(adjacentSide1, adjacentSide2, oppositeSide));
        private static double GetSinAngle(double adjacentSide1, double adjacentSide2, double oppositeSide) => Math.Asin(GetAngleRatio(adjacentSide1, adjacentSide2, oppositeSide));
        private static double GetAngleRatio(double adjacentSide1, double adjacentSide2, double oppositeSide) => (Math.Pow(adjacentSide1, 2) - Math.Pow(oppositeSide, 2) + Math.Pow(adjacentSide2, 2)) / (2 * adjacentSide1 * adjacentSide2);
        private static (double, double) GetBeaconCoordinates()
        {
            return (1, 2);
        }
    }
    static class Program
    {
        private static int invokes = 0;
        private static void TimeConnection(object s)
        {
            AutoResetEvent state = (AutoResetEvent)s;
            Thread node1 = new Thread(StartConnection);
            Thread node2 = new Thread(StartConnection);
            Thread node3 = new Thread(StartConnection);
            if (node1.IsAlive || node2.IsAlive || node3.IsAlive)
            {
                Console.Write("\rWaiting for scan threads to finish");
            }
            else if (invokes == 5)
            {
                Console.WriteLine("Scanning finally stopped!");
                invokes = 0;
                state.Set();
            }
            else
            {
                Console.WriteLine("Scanning started {0}x!", ++invokes);
                node1.Start(new Network(55556));
                node2.Start(new Network(55557));
                node3.Start(new Network(55558));
            }
        }
        private static void GetProximityStatus(object s)
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
                Console.WriteLine("Waiting for connection @ {0}", server.Port);
                server.Client = server.Connection.AcceptTcpClient();
                using (server.Stream = server.Client.GetStream())
                {
                    Console.WriteLine("Connected to {0}!", server.Port);
                    byte[] buffer = new byte[1];
                    server.Stream.ReadTimeout = 10000;
                    try
                    {
                        while (server.Stream.Read(buffer, 0, buffer.Length) != 0)
                        {
                            Console.Write("\rWaiting for proximity...");
                            if (int.Parse(ASCII.GetString(buffer, 0, buffer.Length)) != 0)
                            {
                                Console.WriteLine("\rThe proximity is triggered!");
                                var state = new AutoResetEvent(false);
                                var timer = new Timer(TimeConnection, state, 0, 60000);
                                state.WaitOne();
                                timer.Dispose();
                                Console.WriteLine("Connections ended!");
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
        private static void StartConnection(object s)
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
            Console.WriteLine("Waiting for connection @ {0}", server.Port);
            server.Client = server.Connection.AcceptTcpClient();
            using (server.Stream = server.Client.GetStream())
            {
                Console.WriteLine("Connected to {0}!", server.Port);
                Data info = new Data();
                server.Stream.ReadTimeout = 10000;
                try
                {
                    while ((info.Length = server.Stream.Read(info.Buffer, 0, info.Buffer.Length)) != 0)
                    {
                        Data.Fields = ASCII.GetString(info.Buffer, 0, info.Length).Split(" ");
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
                    Console.WriteLine("Creating nodes for {0}", server.Port);
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
                    Console.WriteLine("Connection to {0} ended!", server.Port);
                }
            }

        }
        public static void DisplayNodes(List<Node> updatedNodes)
        {
            foreach (Node node in updatedNodes)
            {
                Console.WriteLine("Node: {0}\tAddress: {1}\tRSSI: {2:F2}\tTX Power: {3}\tDistance: {4:F4} m",
                                  node.Id,
                                  node.Beacon.Address,
                                  node.Beacon.Rssi,
                                  node.Beacon.TxPower,
                                  node.Beacon.Distance);
            }
        }
        static void Main(string[] args)
        {
            GetProximityStatus(new Network(55555));
        }
    }
}
