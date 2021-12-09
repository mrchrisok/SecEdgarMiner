using SecEdgarMiner.Data.Entities;
using System.Collections.Generic;

namespace SecEdgarMiner.Domain.Models
{
    public class Form4FilingsModel
    {
        public RssFeed FeedState { get; set; }
        public List<Form4InfoModel> Filings { get; set; }
    }
}
