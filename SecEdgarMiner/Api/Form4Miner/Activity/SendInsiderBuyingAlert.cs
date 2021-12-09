using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecEdgarMiner.Contracts;
using SecEdgarMiner.Domain.Models;
using SecEdgarMiner.Logging;
using SecEdgarMiner.Options;
using SendGrid;
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
        //private readonly ISendGridClient _mailClient;

        [FunctionName(nameof(SendInsiderBuyingAlert))]
        public async Task Run([ActivityTrigger] IDurableActivityContext context)
        {
            var insiderBuyingForm4InfoList = context.GetInput<IEnumerable<Form4InfoModel>>();

            // important:
            // a Form4 can contain derivative and non-derivative transactions so
            // evaluation will be divided into 2 channels

            await SendAlertAsync(insiderBuyingForm4InfoList, InsiderBuyingAlertType.Derivative, _options.SendDerivativeInsiderBuyingAlertEmail);
            await SendAlertAsync(insiderBuyingForm4InfoList, InsiderBuyingAlertType.NonDerivative, _options.SendNonDerivativeInsiderBuyingAlertEmail);
        }

        private async Task SendAlertAsync(IEnumerable<Form4InfoModel> insiderBuyingForm4InfoList, InsiderBuyingAlertType alertType, bool sendAlertEmail = false)
        {
            var insiderBuyingList = insiderBuyingForm4InfoList.Where(form4Info => form4Info.InsiderBuyingAlertTypes.Contains(alertType));

            if (insiderBuyingList.Count() == 0)
            {
                return;
            }

            var messageParts = BuildAlertMessageParts(insiderBuyingList, alertType);

            //await SendMessageToFileAsync(messageParts.FileMessage.ToString(), alertType);

            if (sendAlertEmail && messageParts.EmailSubject.Count > 0)
            {
                await SendEmailMessageAsync(string.Join(',', messageParts.EmailSubject), messageParts.EmailMessage.ToString());
            }
        }

        private class AlertMessageParts
        {
            public List<string> EmailSubject { get; set; } = new List<string>();
            public StringBuilder EmailMessage { get; set; } = new StringBuilder();
            public StringBuilder FileMessage { get; set; } = new StringBuilder();
        }

        private AlertMessageParts BuildAlertMessageParts(IEnumerable<Form4InfoModel> form4InfoList, InsiderBuyingAlertType alertType)
        {
            var messageParts = new AlertMessageParts();

            foreach (var form4Info in form4InfoList)
            {
                var shortAlertMessage = Information.Form4InsiderBuyingShortAlert(form4Info, alertType);

                _logger.LogInformation(shortAlertMessage);

                if (SendEmailAlert(form4Info))
                {
                    if (!messageParts.EmailSubject.Contains(form4Info.IssuerTradingSymbol))
                    {
                        messageParts.EmailSubject.Add(form4Info.IssuerTradingSymbol);
                    }

                    var emailMessage = Information.Form4InsiderBuyingShortAlertEmail(form4Info);
                    messageParts.EmailMessage.AppendLine($"{emailMessage}\n");
                }

                var shortAlertFileMessage = Information.Form4InsiderBuyingShortAlertFile(form4Info);
                messageParts.FileMessage.AppendLine($"{shortAlertFileMessage}");
            }

            return messageParts;
        }

        private bool SendEmailAlert(Form4InfoModel form4Info)
        {
            if (form4Info.OwnerIsOfficer)
            {
                return _options.SendOfficerInsiderBuyingAlertEmail;
            }

            return _options.SendNonOfficerInsiderBuyingAlertEmail;
        }

        private async Task SendMessageToFileAsync(string message, InsiderBuyingAlertType alertType)
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
                    From = new EmailAddress("secedgar@mrktminr.com"),
                    Subject = $"Form4 Insider Buying Alert: {subject}"
                };
                mail.AddTo(new EmailAddress("mrchrisok@hotmail.com"));
                mail.AddContent(MimeType.Text, message);

                string apiKey = _options.SendGridApiKey;

                var sgClient = new SendGrid.SendGridClient(apiKey, _options.SendGridApiUri);

                response = await sgClient.SendEmailAsync(mail);

                // need to DI this in Startup with the factory pattern
                // info here: https://github.com/sendgrid/sendgrid-csharp
                //response = await _mailClient.SendEmailAsync(mail);

                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Body.ReadAsStringAsync();
                    throw new OperationErrorException(responseBody);
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
