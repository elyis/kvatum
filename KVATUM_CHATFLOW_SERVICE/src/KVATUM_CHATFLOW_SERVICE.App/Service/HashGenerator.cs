using System.Security.Cryptography;
using System.Text;
using KVATUM_CHATFLOW_SERVICE.Core.IService;

namespace KVATUM_CHATFLOW_SERVICE.App.Service
{
    public class HashGenerator : IHashGenerator
    {
        public string Compute()
        {
            string uuid = Guid.NewGuid().ToString();

            using SHA256 sha256 = SHA256.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(uuid);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            StringBuilder hashString = new();
            foreach (byte b in hashBytes)
            {
                hashString.Append(b.ToString("x2"));
            }

            return hashString.ToString();
        }
    }
}