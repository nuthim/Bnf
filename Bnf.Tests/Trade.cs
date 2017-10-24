using Bnf.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnf.Tests
{
    public class DateTimeObj
    {
        [Bnf(Key= "trade_date")]
        public DateTime TradeDate { get; set; }

        [Bnf(Key = "trade_time")]
        public TimeSpan TradeTime { get; set; }
    }
}
