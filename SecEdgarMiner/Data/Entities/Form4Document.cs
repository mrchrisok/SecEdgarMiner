using System;

namespace SecEdgarMiner.Data.Entities
{
   public class Form4Document
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
   }
}
