using System;

namespace SecEdgarMiner.Data.Entities
{
    public class RssFeed
    {
        public DateTimeOffset LastUpdatedTime { get; set; }
        public string LastETag { get; set; }
    }
}
