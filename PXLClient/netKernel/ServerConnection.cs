using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PXLClient.netKernel {
    public class ServerConnection {
        public const int POINTER_SIZE = sizeof(int);
        private Socket socket;
        public int ConnectTimeout = 10;


        


        //端口通过地址中冒号后的数字提供,如果没有冒号，则参考port参数
        public bool connect(string address, int port = -1) {
            string[] ips = address.Split(':');
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPAddress addr = NetUtils.isIP(ips[0]) ? IPAddress.Parse(ips[0]) : Dns.GetHostEntry(ips[0]).AddressList[0];
            var res = socket.BeginConnect(addr, ips.Length == 1 ? port : int.Parse(ips[1]), null, null);
            var success = res.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(ConnectTimeout));
            socket.EndConnect(res);
            if (success) {
                Thread th = new Thread(receiver);
                th.Start();
                return true;
            } else {
                return false;
            }
        }


        
        public void disconnect() {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        protected virtual void onReceived(Package pak) {

        }
        protected virtual void onDisconnect() {

        }
        protected virtual void onLostConnection() {
            if (socket.Connected) {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            onDisconnect();
        }


        private void receiver() {
            Package pak;
            while (true) {
                try {
                    pak = NetUtils.getPack(socket,POINTER_SIZE);
                    onReceived(pak);
                }
                catch {
                    onLostConnection();
                    break;
                }
            }
        }

        public void sendPackage(Package pak) {
            byte[] data, dataWithHead;
            data = pak.data;
            dataWithHead = new byte[POINTER_SIZE + data.Length];
            Array.Copy(NetUtils.integer2bin(data.Length, POINTER_SIZE), dataWithHead, POINTER_SIZE);
            Array.Copy(data, 0, dataWithHead, POINTER_SIZE, data.Length);
            socket.Send(dataWithHead);
        }

}


    class NetUtils {
        //true表示是ip,false表示可能是域名
        public static bool isIP(string addr) {
            string[] seg = addr.Split('.');
            if (seg.Length != 4) {
                return false;
            }
            else {
                for (int i = 0; i < 4; ++i) {
                    bool isn = int.TryParse(seg[i], out int res);
                    if (!isn) return false;
                    if (!(res >= 0 && res <= 255)) return false;
                }
            }
            return true;
        }

        public static byte[] integer2bin(long n, int size) {
            byte[] bin = new byte[size];
            for (int i = 0; i < size; ++i) {
                bin[i] = (byte)(n & 0xFF);
                n >>= 8;
            }
            return bin;
        }
        //函数返回long，数值上是unsigned的int或short，但使用强制转换可以自动转换成signed的
        public static long bin2integer(byte[] bin, int off, int len) {
            long n = 0;
            for (int i = off; i < off + len; ++i)
                n |= ((bin[i] & 0xFFL) << ((i - off) * 8));

            return n;
        }
        public static byte[] i2b(int n) {
            return integer2bin(n, 4);
        }
        public static byte[] l2b(long n) {
            return integer2bin(n, 8);
        }
        public static byte[] s2b(short n) {
            return integer2bin(n, 2);
        }
        public static byte[] d2b(double v) {
            return BitConverter.GetBytes(v);
        }

        public static byte[] f2b(float v) {
            return BitConverter.GetBytes(v);
        }
        public static float b2f(byte[] bin, int off) {
            return BitConverter.ToSingle(bin, off);
        }
        public static double b2d(byte[] bin, int off) {
            return BitConverter.ToDouble(bin, off);
        }

        public static Package getPack(Socket socket, int headSize) {
            byte[] sizeb = new byte[headSize];
            byte[] data = null;
            int res = socket.Receive(sizeb, headSize, SocketFlags.None);
            if (res <= 0) throw new SocketException();
            else if (res > 0) {
                int size = (int)NetUtils.bin2integer(sizeb, 0, headSize);
                data = new byte[size];
                socket.Receive(data, size, SocketFlags.None);
            }
            return new Package(data);
        }

    }
}
