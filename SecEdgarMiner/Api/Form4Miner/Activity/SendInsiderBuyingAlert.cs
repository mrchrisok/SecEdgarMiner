using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecEdgarMiner.Domain.Engines;
using SecEdgarMiner.Domain.Models;
using SecEdgarMiner.Logging;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecEdgarMiner.Api.Form4Miner.Activity
{
   public class SendInsiderBuyingAlert
   {
	  public SendInsiderBuyingAlert(IForm4Engine form4Engine, ILogger<Form4MinerTimer> logger, IOptions<MailerOptions> options)
	  {
		 _form4Engine = form4Engine;
		 _logger = logger;
		 _options = options.Value;
	  }

	  private readonly IForm4Engine _form4Engine;
	  private readonly ILogger<Form4MinerTimer> _logger;
	  private readonly MailerOptions _options;

	  [FunctionName(nameof(SendInsiderBuyingAlert))]
	  public async Task Run([ActivityTrigger] IDurableActivityContext context)
	  {
		 var insiderBuyingForm4InfoList = context.GetInput<IEnumerable<Form4Info>>();

		 await SendDerivativeAlertAsync(insiderBuyingForm4InfoList);
		 await SendNonDerivativeAlertAsync(insiderBuyingForm4InfoList);
	  }

	  private async Task SendDerivativeAlertAsync(IEnumerable<Form4Info> insiderBuyingForm4InfoList)
	  {
		 var derivativeAlertForm4InfoList = insiderBuyingForm4InfoList.Where(form4Info => form4Info.InsiderBuyingAlertTypes.Contains(InsiderBuyingAlertType.Derivative));

		 if (derivativeAlertForm4InfoList.Count() == 0)
		 {
			return;
		 }

		 var alertEmailSubject = new List<string>();
		 var alertEmailBuilder = new StringBuilder();
		 var alertFileBuilder = new StringBuilder();

		 foreach (var form4Info in derivativeAlertForm4InfoList)
		 {
			var shortAlertMessage = Information.Form4InsiderBuyingShortAlert(form4Info);
			//_logger.LogInformation(shortAlertMessage);

			var shortAlertEmailMessage = Information.Form4InsiderBuyingShortAlertEmail(form4Info);

			if (!alertEmailSubject.Contains(form4Info.IssuerTradingSymbol))
			{
			   alertEmailSubject.Add(form4Info.IssuerTradingSymbol);
			}

			alertEmailBuilder.AppendLine($"{shortAlertEmailMessage}\n");

			var shortAlertFileMessage = Information.Form4InsiderBuyingShortAlertFile(form4Info);
			alertFileBuilder.AppendLine($"{shortAlertFileMessage}");
		 }

		 await SendMessageToFileAsync(alertFileBuilder.ToString(), InsiderBuyingAlertType.Derivative);
		 //await SendEmailMessageAsync(string.Join(',', alertEmailSubject), alertEmailBuilder.ToString());
	  }

	  private async Task SendNonDerivativeAlertAsync(IEnumerable<Form4Info> insiderBuyingForm4InfoList)
	  {
		 var nonDerivativeAlertForm4InfoList = insiderBuyingForm4InfoList.Where(form4Info => form4Info.InsiderBuyingAlertTypes.Contains(InsiderBuyingAlertType.NonDerivative));

		 if (nonDerivativeAlertForm4InfoList.Count() == 0)
		 {
			return;
		 }

		 var alertEmailSubject = new List<string>();
		 var alertEmailBuilder = new StringBuilder();
		 var alertFileBuilder = new StringBuilder();

		 foreach (var form4Info in nonDerivativeAlertForm4InfoList)
		 {
			var shortAlertMessage = Information.Form4InsiderBuyingShortAlert(form4Info);
			_logger.LogInformation(shortAlertMessage);

			var shortAlertEmailMessage = Information.Form4InsiderBuyingShortAlertEmail(form4Info);

			if (!alertEmailSubject.Contains(form4Info.IssuerTradingSymbol))
			{
			   alertEmailSubject.Add(form4Info.IssuerTradingSymbol);
			}

			alertEmailBuilder.AppendLine($"{shortAlertEmailMessage}\n");

			var shortAlertFileMessage = Information.Form4InsiderBuyingShortAlertFile(form4Info);
			alertFileBuilder.AppendLine($"{shortAlertFileMessage}");
		 }

		 await SendMessageToFileAsync(alertFileBuilder.ToString(), InsiderBuyingAlertType.NonDerivative);
		 await SendEmailMessageAsync(string.Join(',', alertEmailSubject), alertEmailBuilder.ToString());
	  }

	  public async Task SendMessageToFileAsync(string message, InsiderBuyingAlertType alertType)
	  {
		 try
		 {
			var alertFileUri = alertType == InsiderBuyingAlertType.NonDerivative
			   ? _options.NonDerivativeAlertFileUri : _options.DerivativeAlertFileUri;

			using (var file = new StreamWriter(alertFileUri, append: true))
			{
			   await file.WriteLineAsync(message);
			}
		 }
		 catch (Exception ex)
		 {
			_logger.LogError(Error.OperationFailed(nameof(SendMessageToFileAsync), ex));
		 }
	  }

	  private async Task<string> SendEmailMessageAsync(string subject, string message)
	  {
		 string result = "success";
		 SendGrid.Response response = null;

		 try
		 {
			var mail = new SendGridMessage
			{
			   From = new EmailAddress("noreply@secedgarminer.com"),
			   Subject = $"Form4 Insider Buying Alert: {subject}"
			};
			mail.AddTo("mrchrisok@hotmail.com");
			mail.AddContent("text/plain", message);

			string apiKey = _options.SendGridApiKey;

			var sgClient = new SendGrid.SendGridClient(apiKey, _options.SendGridApiUri);

			response = await sgClient.SendEmailAsync(mail);

			if (!response.IsSuccessStatusCode)
			{
			   throw new OperationErrorException(response.ToString());
			}
		 }
		 catch (Exception ex)
		 {
			_logger.LogError(Error.OperationFailed(nameof(SendEmailMessageAsync), ex));
			result = ex.ToString();
		 }

		 return result;
	  }
   }
}
