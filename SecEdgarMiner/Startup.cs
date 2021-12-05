using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecEdgarMiner.Common;
using SecEdgarMiner.Data;
using SecEdgarMiner.Domain.Engines;
using SecEdgarMiner.Domain.Workers;

[assembly: FunctionsStartup(typeof(SecEdgarMiner.Startup))]

namespace SecEdgarMiner
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Get the azure function application directory. 'C:\whatever' for local and 'd:\home\whatever' for Azure
            var executionContextOptions = builder.Services.BuildServiceProvider()
                .GetService<IOptions<ExecutionContextOptions>>().Value;

            // Get the directory & configuration provider from the Azure Function
            var currentDirectory = executionContextOptions.AppDirectory;
            var configuration = builder.Services.BuildServiceProvider().GetService<IConfiguration>();

            // Create a new IConfigurationRoot and add our configuration along with Azure's original configuration 
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddConfiguration(configuration) // Add the original function configuration
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            GetKeyVaultConfigurations(configBuilder);

            var config = configBuilder.Build();

            // Replace the Azure Function configuration with our new one
            builder.Services.AddSingleton<IConfiguration>(config);

            builder.Services.AddLogging(options =>
            {
             // this will enable all loggers to write to the console
             options.AddFilter(nameof(SecEdgarMiner), LogLevel.Information);
            });

            builder.Services.AddHttpClient("SecEdgarMinerClient", client =>
            {
                var userAgent = "Chris Okonkwo mrchrisok@hotmail.com";
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            });

            builder.Services.AddScoped<IForm4RssWorker, Form4RssWorker>();
            builder.Services.AddScoped<IForm4Engine, Form4Engine>();
            //builder.Services.AddScoped<ISendGridClient, SendGridClient>();

            string connectionString = config["ConnectionStrings:SqlConnectionString"];
            builder.Services.AddDbContext<MarketMinerContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddOptions<MailerOptions>()
               .Configure<IConfiguration>((settings, configuration) =>
               {
                   configuration.GetSection("MailerOptions").Bind(settings);
               });
        }

        private IConfigurationBuilder GetKeyVaultConfigurations(IConfigurationBuilder configBuilder)
        {
            var config = configBuilder.Build();
            var keyVaultEndpoint = $"https://{config["KeyVaultName"]}.vault.azure.net/";
            var keyVaultClient = KeyVaultHelper.GetKeyVaultClient(config["KeyVaultName"]);

            //var azureServiceTokenProvider = new AzureServiceTokenProvider();
            //var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            configBuilder.AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());

            return configBuilder;
        }
    }
}