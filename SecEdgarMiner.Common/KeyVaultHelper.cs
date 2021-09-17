using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;

namespace SecEdgarMiner.Common
{
   public class KeyVaultHelper
   {
	  public static KeyVaultClient GetKeyVaultClient(string keyVaultName)
	  {
		 var keyVaultEndpoint = $"https://{keyVaultName.ToLowerInvariant()}.vault.azure.net/";

		 var azureServiceTokenProvider = new AzureServiceTokenProvider();
		 var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

		 return keyVaultClient;
	  }

	  public static async Task<string> GetSecretValueAsync(string keyVaultName, string secretName)
	  {
		 var keyVaultClient = GetKeyVaultClient(keyVaultName);
		 var secretValue = await keyVaultClient.GetSecretAsync(secretName);
		 return secretValue.Value;
	  }
   }
}
