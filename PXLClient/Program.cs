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
                Package pak = new Package(1);
                string s = Console.ReadLine();
                if (s=="disconnect") {
                    c.disconnect();
                } else {
                    pak.putString(s);
                    pak.pack();
                    c.sendPackage(pak);
                }
                
            }
        }
    }


    class Con : ServerConnection {
        protected override void onReceived(Package pak) {
            Console.WriteLine(pak.readString(4));
        }

        protected override void onDisconnect() {
            Console.WriteLine("DISCONNECTED");
        }
    }
}
