using System;
using System.Collections.Generic;

namespace SecEdgarMiner.Domain.Models
{
   public class Form4Info
   {

	  public DateTime? PeriodOfReport { get; set; }
	  public string OwnerName { get; set; }
	  public string OwnerCik { get; set; }
	  public string OwnerOfficerTitle { get; set; }
	  public bool OwnerIsOfficer { get; set; }
	  public string IssuerName { get; set; }
	  public string IssuerTradingSymbol { get; set; }
	  public string IssuerCik { get; set; }
	  public string HtmlUrl { get; set; }
	  public string XmlUrl { get; set; }
	  public List<InsiderBuyingAlertType> InsiderBuyingAlertTypes { get; set; }

	  public void AddInsiderBuyingAlertType(InsiderBuyingAlertType alertType)
	  {
		 InsiderBuyingAlertTypes ??= new List<InsiderBuyingAlertType>();
		 if (!InsiderBuyingAlertTypes.Contains(alertType))
		 {
			InsiderBuyingAlertTypes.Add(alertType);
		 }
	  }
   }

   public enum InsiderBuyingAlertType
   {
	  Derivative,
	  NonDerivative
   }
}
