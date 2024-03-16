﻿namespace PersistenceServer.RPCs
{
    public class GetCharacters : BaseRpc
    {
        public GetCharacters()
        {
            RpcType = RpcType.RpcGetCharacters; // set it to the RpcType you want to catch
        }

        // Read message from the reader, then enqueue an Action on the concurrent queue server.Processor.ConQ
        // For example: Server!.Processor.ConQ.Enqueue(() => Console.WriteLine("like this"));
        // Look at other RPCs for more examples.
        protected override void ReadRpc(UserConnection connection, BinaryReader reader)
        {
            Server!.Processor.ConQ.Enqueue(async () => await ProcessGetCharacters(connection));
        }

        private async Task ProcessGetCharacters(UserConnection connection)
        {
            int accountId = Server!.GameLogic.GetAccountId(connection);
            if (accountId == -1)
            {
                Console.WriteLine("Get characters failed: user not logged in. This must never happen.");
                connection.Disconnect();
                return;
            }
            var characters = await Server!.Database.GetCharacters(accountId);

            // The message is structured as follows:
            // An int to signify the number of characters on this account
            // Then for each character: id (int), name (string), json (string)            

            byte[] numCharacters = ToBytes(characters.Count);
            List<byte> allCharacters = new();
            foreach (var toon in characters)
            {
                allCharacters.AddRange(ToBytes(toon.Item1)); // id
                allCharacters.AddRange(WriteMmoString(toon.Item2)); // name
                allCharacters.AddRange(WriteMmoString(toon.Item3)); // json
            }

            byte[] msg = MergeByteArrays(ToBytes(RpcType.RpcGetCharacters), numCharacters, allCharacters.ToArray());
            connection.Send(msg);
        }
    }
}