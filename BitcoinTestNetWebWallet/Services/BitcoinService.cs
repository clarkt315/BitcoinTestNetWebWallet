using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BitcoinTestNetWebWallet.Data;
using Microsoft.Extensions.Configuration;

namespace BitcoinTestNetWebWallet.Services
{
    public class BitcoinService : IBitcoinService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public BitcoinService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public bool ValidateBitcoinAddress(string address)
        {
            var inputParameters = new JArray();
            inputParameters.Add(address);
            JObject response = CallBitcoinServerAPI("validateaddress", inputParameters);
            JToken result = response["result"];
            string isValid = result["isvalid"].ToString();
            return Convert.ToBoolean(isValid);
        }
        public string GenerateNewAddress()
        {
            JObject response = CallBitcoinServerAPI("getnewaddress", new JArray());
            string newAddress = response["result"].Value<string>();
            return newAddress;
        }
        
        // This returns all the addresses in the wallet (for all users), along with the total amount deposited into each.
        // Filtering out addresses that don't belong to the logged in user will be done in the controller.
        public Dictionary<string, string> GetDepositsByAddress()
        {
            JObject response = CallBitcoinServerAPI("listreceivedbyaddress", new JArray());
            JArray addresses = (JArray)response["result"];
            var amountPerAddress = new Dictionary<string, string>();
            foreach (var addressObject in addresses)
            {
                string address = addressObject["address"].ToString();
                // Small amounts like 0.00001 will be returned as scientific notation by the bitcoin API.
                // We need to switch to decimal and back to string again to fix the format.
                string amountScientificNotationString = addressObject["amount"].ToString();
                decimal amountDecimal = Decimal.Parse(amountScientificNotationString, System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowDecimalPoint);
                string amountDecimalString = string.Format("{0:0.00000000}", amountDecimal);
                amountPerAddress.Add(address, amountDecimalString);
            }
            return amountPerAddress;
        }

        // This returns all the addresses in the wallet (for all users), along with the transactions IDs of
        // deposits sent into each address. Each address can have 0 to many deposits.
        // Filtering out addresses that don't belong to the logged in user will be done in the controller.
        public Dictionary<string, List<string>> GetTransactionsByAddress()
        {
            JObject response = CallBitcoinServerAPI("listreceivedbyaddress", new JArray());
            JArray addresses = (JArray)response["result"];
            var transactionsByAddress = new Dictionary<string, List<string>>();
            foreach (var address in addresses)
            {
                var txidStringList = new List<string>();
                var txidJTokenList = address["txids"].ToList();
                foreach (JToken jToken in txidJTokenList)
                {
                    txidStringList.Add(jToken.ToString());
                }
                transactionsByAddress.Add(address["address"].ToString(), txidStringList);
            }
            return transactionsByAddress;
        }

        public string SendTransaction(string address, decimal amount)
        {
            var inputParameters = new JArray();
            inputParameters.Add(address);
            inputParameters.Add(amount);
            JObject response = CallBitcoinServerAPI("sendtoaddress", inputParameters);
            string txid = response["result"].ToString();
            return txid;
        }

        public decimal GetWalletBalance(string user)
        {
            // Sum all withdrawals in the table for current user
            decimal withdrawalSum = _context.Withdrawal.Where(w => w.UserId == user).Sum(s => s.Amount);

            // Get a list of all addresses and deposit amounts from the Bitcoin API
            decimal depositSum = 0;
            JObject response = CallBitcoinServerAPI("listreceivedbyaddress", new JArray());
            JArray addresses = (JArray)response["result"];

            // Go through each address returned by the Bitcoin API
            foreach (var addressObject in addresses)
            {
                string address = addressObject["address"].ToString();
                // If the address belongs to the current user, add its deposit amount to the running total.
                if (_context.DepositAddress.Any(s => s.Address == address && s.UserId == user))
                {
                    string amountScientificNotationString = addressObject["amount"].ToString();
                    decimal amountDecimal = Decimal.Parse(amountScientificNotationString, System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowDecimalPoint);
                    depositSum = depositSum + amountDecimal;
                }
            }
            // Balance equals total deposits minus total withdrawals
            return depositSum - withdrawalSum;
        }

        // This is function is used to call the bitcoin core "RPC" json server.
        // It is listen on the local server at a given port and requires a certain
        // user name and password to access it.
        private JObject CallBitcoinServerAPI(string method, JArray parameters)
        {
            var bitcoinServerConfiguration = _configuration.GetSection("BitcoinServer");
            string serverAndPort = bitcoinServerConfiguration.GetValue<string>("ServerAndPort");
            string userName = bitcoinServerConfiguration.GetValue<string>("UserName");
            string password = bitcoinServerConfiguration.GetValue<string>("Password");

            // Web request settings
            HttpWebRequest webRequest = HttpWebRequest.CreateHttp(serverAndPort);
            webRequest.Credentials = new NetworkCredential(userName, password);
            webRequest.ContentType = "application/json-rpc";
            webRequest.Method = "POST";
            
            // Set up JSON object with request details.
            JObject request = new JObject();
            request.Add(new JProperty("jsonrpc", "1.0"));
            request.Add(new JProperty("id", "1"));
            request.Add(new JProperty("method", method));
            request.Add(new JProperty("params", parameters));

            // Set up the web request in proper format
            string s = JsonConvert.SerializeObject(request);
            byte[] byteArray = Encoding.UTF8.GetBytes(s);
            webRequest.ContentLength = byteArray.Length;
            Stream dataStream = webRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            // Get the response
            StreamReader streamReader = null;
            WebResponse webResponse = webRequest.GetResponse();
            streamReader = new StreamReader(webResponse.GetResponseStream(), true);
            string responseValue = streamReader.ReadToEnd();
            JObject responseObject = (JObject)JsonConvert.DeserializeObject(responseValue);
            return responseObject;
        }
    }
}
