using SecEdgarMiner.Data.Entities;
using System.Collections.Generic;

namespace SecEdgarMiner.Domain.Models
{
   public class Form4InfoModel : Form4Info
   {
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
