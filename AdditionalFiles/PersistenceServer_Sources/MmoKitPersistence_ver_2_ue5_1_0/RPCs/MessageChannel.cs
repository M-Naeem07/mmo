﻿namespace PersistenceServer.RPCs
{
    public class MessageChannel : BaseRpc
    {
        public MessageChannel()
        {
            RpcType = RpcType.RpcMessageChannel; // set it to the RpcType you want to catch
        }

        // Read message from the reader, then enqueue an Action on the concurrent queue server.Processor.ConQ
        // For example: Server!.Processor.ConQ.Enqueue(() => Console.WriteLine("like this"));
        // Look at other RPCs for more examples.
        protected override void ReadRpc(UserConnection connection, BinaryReader reader)
        {
            int channel = reader.ReadInt32();
            string message = reader.ReadMmoString();            
            int maxLength = 255;
            // Trim message by maxLength (255 characters)
            message = message.Length <= maxLength ? message : message[..maxLength]; // .. is a C# 8.0 Range Operator https://www.codeguru.com/csharp/c-8-0-ranges-and-indices-types/
            Server!.Processor.ConQ.Enqueue(() => ProcessMessage(channel, message, connection));
        }

        private void ProcessMessage(int channel, string message, UserConnection connection)
        {
            var charName = Server!.GameLogic.GetPlayerName(connection);
            if (charName == "") return;

            // Say channel 0
            // "Say" must only be displayed in vicinity of the person who says it, so we'll tell the game server
            // to multicast it on character, whose netcull distance will be used as vicinity range
            if (channel == 0)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm} [Say] {charName}: \"{message}\"");
                byte[] msg = MergeByteArrays(ToBytes(RpcType.RpcMessageChannel), WriteMmoString(charName), WriteMmoString(message));
                //@TODO: optimize by sending only to the right server
                foreach(var serverConn in Server!.GameLogic.GetAllServerConnections())
                {
                    serverConn.Send(msg);
                }
            }

            // Global channel 1
            if (channel == 1)
            {                
                Console.WriteLine($"{DateTime.Now:HH:mm} [Global] {charName}: \"{message}\"");
                byte[] msg = MergeByteArrays(ToBytes(RpcType.RpcMessageChannel), ToBytes(channel), WriteMmoString(charName), WriteMmoString(message));
                var players = Server!.GameLogic.GetAllPlayerConnections();
                foreach(var player in players)
                {
                    player.Send(msg);
                }
            }
        }
    }
}