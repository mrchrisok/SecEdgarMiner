using Microsoft.Extensions.Logging;
using SecEdgarMiner.Data;
using SecEdgarMiner.Domain.Models;
using SecEdgarMiner.Logging;
using SecEdgarMiner.Utilities;
using SecuritiesExchangeCommission.Edgar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecEdgarMiner.Domain.Engines
{
   public class Form4Engine : IForm4Engine
   {
	  public Form4Engine(IHttpClientFactory httpClientFactory, MarketMinerDbContext dbContext, ILogger<Form4Engine> logger)
	  {
		 _client = httpClientFactory.CreateClient("SecEdgarMinerClient");
		 _dbContext = dbContext;
		 _logger = logger;
	  }

	  private readonly MarketMinerDbContext _dbContext;
	  private readonly HttpClient _client;
	  private readonly ILogger<Form4Engine> _logger;

	  private static readonly string[] _stockIndications = new string[] { "STOCK", "COMMON", "SHARES" };

	  public async Task<Form4Info> GetInsiderBuyingForm4InfoAsync(Form4Info form4Info)
	  {
		 var form4Statement = await GetForm4StatementAsync(form4Info);

		 if (form4Statement == null)
		 {
			return null;
		 }

		 var derivativeInsiderTransactions = GetDerivativeInsiderBuyingTransactions(form4Info, form4Statement);
		 var nonDerivativeInsiderBuyingTransactions = GetNonDerivativeInsiderBuyingTransactions(form4Info, form4Statement);

		 if (derivativeInsiderTransactions.Count() == 0 && nonDerivativeInsiderBuyingTransactions.Count() == 0)
		 {
			return null;
		 }

		 var insiderBuyingform4Info = new Form4Info
		 {
			PeriodOfReport = form4Statement.PeriodOfReport,
			OwnerName = form4Statement.OwnerName,
			OwnerCik = form4Statement.OwnerCik,
			OwnerIsOfficer = form4Statement.OwnerIsOfficer,
			OwnerOfficerTitle = form4Statement.OwnerOfficerTitle,
			IssuerName = form4Statement.IssuerName,
			IssuerTradingSymbol = form4Statement.IssuerTradingSymbol,
			IssuerCik = form4Statement.IssuerCik,
			//
			XmlUrl = form4Info.XmlUrl,
			HtmlUrl = form4Info.HtmlUrl
		 };

		 if (derivativeInsiderTransactions.Count() != 0)
		 {
			insiderBuyingform4Info.AddInsiderBuyingAlertType(InsiderBuyingAlertType.Derivative);
		 }
		 if (nonDerivativeInsiderBuyingTransactions.Count() != 0)
		 {
			insiderBuyingform4Info.AddInsiderBuyingAlertType(InsiderBuyingAlertType.NonDerivative);
		 }

		 _logger.LogInformation(Information.Form4InsiderBuyingFoundInIssuer(insiderBuyingform4Info));

		 await SaveStatementOfBeneficialOwnershipAsync(form4Statement);
		 await SaveForm4InfoAsync(insiderBuyingform4Info);

		 return insiderBuyingform4Info;
	  }

	  public async Task<StatementOfBeneficialOwnership> GetForm4StatementAsync(Form4Info form4Info)
	  {
		 string form4XmlString;
		 try
		 {
			var response = await _client.GetAsync(form4Info.XmlUrl);
			var form4XmlResponse = await HttpHelper.GetResponseMessageAsync(response);
			form4XmlString = Uri.UnescapeDataString(form4XmlResponse);
		 }
		 catch (Exception ex)
		 {
			_logger.LogError($"HttpGet error occured for statement: {form4Info.XmlUrl}. \nMessage: {ex.Message}");
			return null;
		 }

		 StatementOfBeneficialOwnership form4Statement;
		 try
		 {
			form4Statement = StatementOfBeneficialOwnership.ParseXml(form4XmlString);
		 }
		 catch (Exception ex)
		 {
			_logger.LogError($"XML error occured for statement: {form4Info.XmlUrl}. \nMessage: {ex.Message}");
			return null;
		 }

		 return form4Statement;
	  }

	  /// <summary>
	  /// 
	  /// </summary>
	  /// <param name="form4Info"></param>
	  /// <param name="form4Statement"></param>
	  /// <returns>Returns a list of NonDerivativeTransaction objects</returns>
	  private IEnumerable<NonDerivativeTransaction> GetNonDerivativeInsiderBuyingTransactions(Form4Info form4Info, StatementOfBeneficialOwnership form4Statement)
	  {
		 static bool transactionPriceNearLastPrice(float? transactionPrice)
		 {
			return true;

			//var stockTicker = form4Statement.IssuerTradingSymbol;
			//var stockLastTradingPrice = await GetStockLastTradingPrice(stockTicker);

			//var lastMarketPricePlus10Percent = stockLastTradingPrice * 1.10;
			//var lastMarketPriceMinus10Percent = stockLastTradingPrice * 0.90;

			//return transactionPrice >= lastMarketPriceMinus10Percent
			//   && transactionPrice <= lastMarketPricePlus10Percent;
		 }

		 IEnumerable<NonDerivativeTransaction> insiderBuyingTransactions = null;
		 try
		 {
			insiderBuyingTransactions = form4Statement?.NonDerivativeTransactions?
			   .Where(transaction =>
				  _stockIndications.Any(indication => transaction.SecurityTitle.ToUpperInvariant().Contains(indication))
					 && IsOpenMarketOrPrivatePurchase(transaction)
					 && transaction.TransactionQuantity.GetValueOrDefault() > 1000.0
					 && transaction.AcquiredOrDisposed.GetValueOrDefault(AcquiredDisposed.Disposed) == AcquiredDisposed.Acquired
					 && transaction.TransactionPricePerSecurity.GetValueOrDefault() > 5.00
					 && transactionPriceNearLastPrice(transaction.TransactionPricePerSecurity)
				  //&& transaction.DirectOrIndirectOwnership == OwnershipNature.Direct
				  );
		 }
		 catch (Exception ex)
		 {
			_logger.LogError($"LINQ error occured for statement: {form4Info.XmlUrl}");
		 }

		 return insiderBuyingTransactions ?? new List<NonDerivativeTransaction>();
	  }

	  /// <summary>
	  /// 
	  /// </summary>
	  /// <param name="form4Info"></param>
	  /// <param name="form4Statement"></param>
	  /// <returns>Returns a list of DerivativeTransaction objects</returns>
	  private IEnumerable<DerivativeTransaction> GetDerivativeInsiderBuyingTransactions(Form4Info form4Info, StatementOfBeneficialOwnership form4Statement)
	  {
		 static bool transactionPriceNearLastPrice(float? transactionPrice)
		 {
			return true;
		 }

		 IEnumerable<DerivativeTransaction> insiderBuyingTransactions = null;
		 try
		 {
			insiderBuyingTransactions = form4Statement?.DerivativeTransactions?
			   .Where(transaction =>
					 //_stockIndications.Any(indication => transaction.SecurityTitle.ToUpperInvariant().Contains(indication))
					 IsOpenMarketOrPrivatePurchase(transaction)
					 && transaction.TransactionQuantity.GetValueOrDefault() > 1000.0
					 && transaction.AcquiredOrDisposed.GetValueOrDefault(AcquiredDisposed.Disposed) == AcquiredDisposed.Acquired
					 && transaction.TransactionPricePerSecurity.GetValueOrDefault() > 0
					 && transactionPriceNearLastPrice(transaction.TransactionPricePerSecurity)
				  //&& transaction.DirectOrIndirectOwnership == OwnershipNature.Direct
				  );
		 }
		 catch (Exception ex)
		 {
			_logger.LogError($"LINQ error occured for statement: {form4Info.XmlUrl}");
		 }

		 return insiderBuyingTransactions ?? new List<DerivativeTransaction>();
	  }

	  private bool IsOpenMarketOrPrivatePurchase(SecurityTransaction transaction)
	  {
		 return transaction.TransactionCode.GetValueOrDefault(TransactionType.OpenMarketOrPrivateSale) == TransactionType.OpenMarketOrPrivatePurchase;
	  }

	  private Task<double> GetStockLastTradingPriceAsync(string tradingSymbol)
	  {
		 return Task.FromResult(5.00);
	  }

	  public Task<IEnumerable<SecurityTransaction>> GetInsiderBuyingTransactionsAsync(Form4Info form4Info)
	  {
		 throw new NotImplementedException();
	  }

	  private async Task SaveStatementOfBeneficialOwnershipAsync(StatementOfBeneficialOwnership form4)
	  {
		 // todo
		 // write code to do this

		 //await _dbContext.StatementsOfBeneficialOwnership.AddAsync(form4);
		 //await _dbContext.SaveChangesAsync();
	  }
	  private async Task SaveForm4InfoAsync(Form4Info form4Info)
	  {
		 // todo
		 // write code to do this

		 //await _dbContext.Form4Info.AddAsync(form4);
		 //await _dbContext.SaveChangesAsync();
	  }
   }
}
