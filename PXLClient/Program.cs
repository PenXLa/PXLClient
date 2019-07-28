using Google.Protobuf;
using PXLClient.netKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLClient {
    class Program {
        static void Main(string[] args) {
            Con c = new Con();
            c.connect("127.0.0.1:6667");

            while(true) {
                Package.ChatMessage cm = new Package.ChatMessage {
                    Txt = Console.ReadLine(),
                    Tag = DateTime.Now.Ticks
                };
                c.sendPackage(MessageExtensions.ToByteArray(cm));
            }
        }
    }


    class Con : ServerConnection {
        protected override void onReceived(byte[] pak) {
            Package.ChatMessage cm = Package.ChatMessage.Parser.ParseFrom(pak);
            Console.WriteLine(cm.Txt + " " + cm.Tag);
        }

        protected override void onDisconnect() {
            Console.WriteLine("DISCONNECTED");
        }
    }
}
