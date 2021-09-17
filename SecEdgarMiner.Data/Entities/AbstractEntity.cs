using System;

namespace SecEdgarMiner.Data.Entities
{
   public abstract class AbstractEntity : IEntity
   {
	  public long? Id { get; set; }
	  public DateTime DateCreated { get; set; }
	  public DateTime? DateModified { get; set; }
   }
}
