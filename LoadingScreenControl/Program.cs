using System;
using System.Linq;
using System.Timers;
using LeagueSharp;
using LeagueSharp.Common;

namespace LoadingScreenControl
{
    class Program
    {
        private static byte[] _packetData;

        static void Main(string[] args)
        {
            // Connect stuff during the loading screen
            Game.OnSendPacket += Game_OnSendPacket;
            Drawing.OnDraw += Drawing_OnDraw;

            // Disconnect stuff after the loading screen
            CustomEvents.Game.OnGameLoad += gameArgs =>
            {
                Game.OnSendPacket -= Game_OnSendPacket;
                Drawing.OnDraw -= Drawing_OnDraw;
            };
        }

        private static void Game_OnSendPacket(GamePacketEventArgs args) // 0x34
        {
            //* Used for finding which packet is the packet we want when an update comes out.
            // Simply put a "/" before the "*" on the previous line to get this to run.
            Console.WriteLine("Packet Recieved: {0} ({1})",
                args.PacketData.Aggregate("", (str, b) => str + " " + b.ToString("X2")).TrimStart(),
                args.PacketData[0] == 0x83);
            //*/

            // The way this method works is as follows:
            // 1. Check all outgoing packets to see if they're a certain packet, which starts with
            //    0x34. If this method is called on a packet which isn't what we're looking for,
            //    immediately return.
            // 2. If they're the packet we're looking for, stop that packet from going out and
            //    store the packet data in a variable so we can send the data later.
            //    - The data is stored because it can't be recreated without knowledge of what the
            //      packet contains, how it stores the data, and how it's formatted.
            // 3. Wait some time (30 seconds).
            // 4. Send the packet data that we stored earlier.
            
            if (_packetData != null || args.PacketData[0] != 0x83)
                return;

            Console.WriteLine("Found packet.");

            args.Process = false;
            _packetData = args.PacketData;

            Console.WriteLine("Waiting 30 seconds.");

            var timer = new Timer(30 * 1000)
            {
                AutoReset = false,
                Enabled = true
            };
            timer.Elapsed += (sender, elapsedArgs) =>
            {
                Console.WriteLine("Sending packet.");
                Game.SendPacket(_packetData, PacketChannel.C2S, PacketProtocolFlags.Reliable);
            };

            timer.Start();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            // TODO
        }
    }
}
