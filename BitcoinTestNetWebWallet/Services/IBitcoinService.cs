using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitcoinTestNetWebWallet.Services
{
    public interface IBitcoinService
    {
        public string GenerateNewAddress();
        public Dictionary<string, string> GetDepositsByAddress();
        public Dictionary<string, List<string>> GetTransactionsByAddress();
        public bool ValidateBitcoinAddress(string address);
        public string SendTransaction(string address, decimal amount);
        public decimal GetWalletBalance(string user);
    }
}
