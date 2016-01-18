using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SInnovations.Azure.ResourceManager.CryptoHelper
{
    public class CryptoUtilities
    {
        public static IEnumerable<string> GetKeys(int length = 32, int count = 2)
        {
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                var key = new byte[length];
                foreach (var i in Enumerable.Range(0, count))
                {
                    rngCsp.GetBytes(key);
                    yield return Convert.ToBase64String(key);
                }
            }
        }
    }
}
