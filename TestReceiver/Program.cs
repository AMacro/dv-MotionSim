using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace TestReceiver
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct telemetryPacket
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public char[] apiMode;
        public uint version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] game;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] vehicleName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] location;
        public float speed;
        public float rpm;
        public float maxRpm;
        public int gear;
        public float pitch;
        public float roll;
        public float yaw;
        public float lateralVelocity;
        public float lateralAcceleration;
        public float verticalAcceleration;
        public float longitudinalAcceleration;
        public float suspensionTravelFrontLeft;
        public float suspensionTravelFrontRight;
        public float suspensionTravelRearLeft;
        public float suspensionTravelRearRight;
        public uint wheelTerrainFrontLeft;
        public uint wheelTerrainFrontRight;
        public uint wheelTerrainRearLeft;
        public uint wheelTerrainRearRight;
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("UDP Listener started. Listening on port 33001...");

            using (UdpClient udpClient = new UdpClient(33001))
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                try
                {
                    while (true)
                    {
                        byte[] receiveBytes = udpClient.Receive(ref remoteEndPoint);
                        telemetryPacket packet = ByteArrayToStructure<telemetryPacket>(receiveBytes);

                        Console.WriteLine($"Received packet from {remoteEndPoint}:");
                        Console.WriteLine($"Game: {new string(packet.game).Trim('\0')}");
                        Console.WriteLine($"Location: {new string(packet.location).Trim('\0')}");
                        Console.WriteLine($"Vehicle: {new string(packet.vehicleName).Trim('\0')}");

                        Console.WriteLine($"Speed: {packet.speed}");
                        Console.WriteLine($"RPM: {packet.rpm}");
                        Console.WriteLine("------------------------------");
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"SocketException: {e}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception: {e}");
                }
            }
        }

        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
