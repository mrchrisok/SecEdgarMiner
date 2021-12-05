using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;

namespace SecEdgarMiner.Common
{
    public class KeyVaultHelper
    {
        public static KeyVaultClient GetKeyVaultClient()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            return keyVaultClient;
        }

        public static async Task<string> GetSecretValueAsync(string keyVaultUri, string secretName)
        {
            var keyVaultClient = GetKeyVaultClient();
            var secretValue = await keyVaultClient.GetSecretAsync(keyVaultUri, secretName);
            return secretValue.Value;
        }
    }
}
