using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitcoinTestNetWebWallet.Models
{
    public class Withdrawal
    {
        public int Id { get; set; }
        public string DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public string Txid { get; set; }
        public DateTime DatetimeSent { get; set; }
        public string UserId { get; set; }
        public string Comment { get; set; }
        public Withdrawal()
        {
            DatetimeSent = DateTime.Now;
        }
    }
}
