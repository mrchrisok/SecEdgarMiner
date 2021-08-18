﻿using SecEdgarMiner.Domain.Models;

namespace SecEdgarMiner.Logging
{
   public class Information
   {
	  public static string Form4InsiderBuyingFoundInIssuer(Form4Info form4Info)
	  {
		 var ownerType = form4Info.OwnerIsOfficer ? "Officer" : "NonOfficer";
		 var periodOfReport = form4Info.PeriodOfReport.GetValueOrDefault().ToShortDateString();

		 return $"Form4 insider buying found in Issuer: '{form4Info.IssuerTradingSymbol},{form4Info.IssuerCik}' by '{ownerType},{form4Info.OwnerCik}' with PeriodOfReport: {periodOfReport}";
	  }

	  public static string Form4InsiderBuyingShortAlert(Form4Info form4Info)
	  {
		 return $"ALERT: {form4Info.IssuerTradingSymbol}, {form4Info.HtmlUrl}";
	  }
	  public static string Form4InsiderBuyingShortAlertFile(Form4Info form4Info)
	  {
		 return $"{form4Info.IssuerTradingSymbol}, {form4Info.HtmlUrl}";
	  }
	  public static string Form4InsiderBuyingShortAlertEmail(Form4Info form4Info)
	  {
		 return $"{form4Info.IssuerTradingSymbol} \n{form4Info.HtmlUrl}";
	  }
   }
}
