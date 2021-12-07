namespace SecEdgarMiner.Options
{
    public class MailerOptions
    {
        public string AlertFilePath { get; set; }
        public string DerivativeAlertFileName { get; set; }
        public string NonDerivativeAlertFileName { get; set; }
        public string SendGridApiUri { get; set; }
        public string SendGridApiKey { get; set; }
        public bool SendDerivativeInsiderBuyingAlertEmail { get; set; }
        public bool SendNonDerivativeInsiderBuyingAlertEmail { get; set; }
        public bool SendOfficerInsiderBuyingAlertEmail { get; set; }
        public bool SendNonOfficerInsiderBuyingAlertEmail { get; set; }
        public string DerivativeAlertFileUri => GetAlertFileUri(DerivativeAlertFileName);
        public string NonDerivativeAlertFileUri => GetAlertFileUri(NonDerivativeAlertFileName);

        private string GetAlertFileUri(string fileName)
        {
            if (string.IsNullOrWhiteSpace(AlertFilePath))
            {
                AlertFilePath = System.IO.Directory.GetCurrentDirectory();
            }

            return $"{AlertFilePath}\\{fileName}";
        }
    }
}
