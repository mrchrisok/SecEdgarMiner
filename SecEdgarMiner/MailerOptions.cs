namespace SecEdgarMiner
{
   public class MailerOptions
   {
	  public string AlertFilePath { get; set; }
	  public string DerivativeAlertFileName { get; set; }
	  public string NonDerivativeAlertFileName { get; set; }
	  public string SendGridApiKey { get; set; }

	  public string DerivativeAlertFileUri => $"{AlertFilePath}{DerivativeAlertFileName}";
	  public string NonDerivativeAlertFileUri => $"{AlertFilePath}{NonDerivativeAlertFileName}";
   }
}
