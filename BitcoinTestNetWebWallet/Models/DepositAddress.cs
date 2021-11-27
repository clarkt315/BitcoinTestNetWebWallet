using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BitcoinTestNetWebWallet.Models
{
    public class DepositAddress
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Comment { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDatetime { get; set; }
        public DepositAddress()
        {
            CreatedDatetime = DateTime.Now;
        }
    }
}
