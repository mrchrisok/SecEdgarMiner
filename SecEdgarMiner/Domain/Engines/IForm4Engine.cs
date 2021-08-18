using SecEdgarMiner.Domain.Models;
using SecuritiesExchangeCommission.Edgar;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecEdgarMiner.Domain.Engines
{
   public interface IForm4Engine
   {
	  Task<StatementOfBeneficialOwnership> GetForm4StatementAsync(Form4Info form4Info);
	  Task<Form4Info> GetInsiderBuyingForm4InfoAsync(Form4Info form4Info);
	  Task<IEnumerable<SecurityTransaction>> GetInsiderBuyingTransactionsAsync(Form4Info form4Info);
   }
}