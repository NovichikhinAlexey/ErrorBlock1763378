using System;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.WebSocketClient;
using Nethereum.Web3;

namespace ReproduceWebSocketError
{
    class Program
    {
        private static Web3 _https;
        private static Web3 _wss;

        /// Problem with block #1763378. If we try to get GetBlockWithTransactionsHash by HTTPS then all good.
        /// but if we try to get by WSS then exception with timeout
        ///
        /// Base on this we have an error into our transaction scanner, we stopped at this block and cannot go next
        /// We switch to HTTPS to solve this problem.But https works in general slowly.
        ///
        /// In this example we want to illustrate problem.
        static void Main(string[] args)
        {
            _https = CreateWeb3("https://bnode-emaartestblockchain.blockchain.azure.com:3200/eLEayJDUJ1_Tv-r9GmBpxwit");
            _wss = CreateWeb3("wss://bnode-emaartestblockchain.blockchain.azure.com:3300/eLEayJDUJ1_Tv-r9GmBpxwit");
            
            var blockNumber = new HexBigInteger(21);
            CheckTransactionsInBlock(blockNumber);

            blockNumber = new HexBigInteger(1763378);
            CheckTransactionsInBlock(blockNumber);

            blockNumber = new HexBigInteger(1763379);
            CheckTransactionsInBlock(blockNumber);

            blockNumber = new HexBigInteger(1763382);
            CheckTransactionsInBlock(blockNumber);
            
        }

        private static void CheckTransactionsInBlock(HexBigInteger blockNumber)
        {
            Console.WriteLine();
            Console.WriteLine("======================");
            Console.WriteLine($"Block number: {blockNumber}");

            try
            {
                var httpsResult = _https.Eth.Blocks.GetBlockWithTransactionsHashesByNumber
                    .SendRequestAsync(blockNumber)
                    .GetAwaiter()
                    .GetResult();

                //if (httpsResult.TransactionHashes?.Length > 0)
                Console.WriteLine($"Https result: {httpsResult.TransactionHashes?.Length}. Block number: {blockNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error by https request:");
                Console.WriteLine(ex);
            }

            try
            {
                var wssResult = _wss.Eth.Blocks.GetBlockWithTransactionsHashesByNumber
                    .SendRequestAsync(blockNumber)
                    .GetAwaiter()
                    .GetResult();

                if (wssResult.TransactionHashes?.Length > 0)
                    Console.WriteLine($"WebSocket result: {wssResult.TransactionHashes?.Length}. Block number: {blockNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error by wss request:");
                Console.WriteLine(ex);
            }
        }

        private static Web3 CreateWeb3(string connString)
        {
            if (connString.StartsWith("ws"))
            {
                var wsClient = new WebSocketClient(connString);
                return new Web3(wsClient);
            }

            return new Web3(connString);
        }
    }
}
