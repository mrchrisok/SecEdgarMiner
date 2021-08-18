using System;

namespace SecEdgarMiner.Logging
{
   public class Error
   {
	  public static string GetInsiderBuyingFailed(Exception ex)
	  {
		 return $"GetInsiderBuying failed. Message: { ex.Message}";
	  }

	  public static string OperationFailed(string operation, Exception ex)
	  {
		 return $"{operation} failed. Message: { ex.Message}";
	  }
   }
}
