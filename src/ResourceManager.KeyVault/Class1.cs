using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SInnovations.Azure.ResourceManager;

namespace ResourceManager.KeyVault
{
    public static class KeyVaultHelper
    {

        public static async Task<Tuple<string, string>> CreateSSHKeyPair(string keyvaultUri, string name)
        {
            var keyVaultClient = new KeyVaultClient((r, c, s) =>
            {
                //   return Task.FromResult(options.AccessToken);
                var context = new AuthenticationContext(r, new FileCache());
                //var result = context.AcquireTokenByRefreshToken(options.RefreshToken,options.CliendId,c);
                var result = context.AcquireToken(c, "1950a258-227b-4e31-a9cf-717495945fc2", new Uri("urn:ietf:wg:oauth:2.0:oob"));
                return Task.FromResult(result.AccessToken);
            });
            var exists = await keyVaultClient.GetSecretsAsync(keyvaultUri);
            string pubKey = "", priKey = "";
            if (exists.Value == null || !exists.Value.Any(v => v.Identifier.Name == name))
            {
                Chilkat.SshKey key = new Chilkat.SshKey();

                bool success;
                int numBits;
                int exponent;
                numBits = 2048;
                exponent = 65537;
                success = key.GenerateRsaKey(numBits, exponent);
                var pub = Convert.ToBase64String(Encoding.UTF8.GetBytes(pubKey = key.ToOpenSshPublicKey()));
                var pri = Convert.ToBase64String(Encoding.UTF8.GetBytes(priKey = key.ToOpenSshPrivateKey(false)));

                await keyVaultClient.SetSecretAsync(keyvaultUri,
                    name, $"{pub}.{pri}", new Dictionary<string, string>
                    {

                    });
                return new Tuple<string, string>(pubKey, priKey);

            }
            var secret = await keyVaultClient.GetSecretAsync(keyvaultUri, name);

            return new Tuple<string, string>(Encoding.UTF8.GetString(Convert.FromBase64String(secret.Value.Split('.')[0])), Encoding.UTF8.GetString(Convert.FromBase64String(secret.Value.Split('.')[1])));
        }
    }
}
