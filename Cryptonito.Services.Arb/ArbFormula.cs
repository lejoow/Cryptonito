using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptonito.Services.Arb
{
    public class ArbFormula
    {
        private static decimal FEE = new Decimal(0.0025);

        public static decimal CalculateExpectedReturn(decimal drkToLtcRate, decimal ltcToBtcRate, decimal btcToDrkRate)
        {
            if (drkToLtcRate <= 0M || ltcToBtcRate <= 0M || btcToDrkRate <= 0M) return -999M;

            decimal initialCost = new decimal(1);

            decimal totalLtcWeCanBuy = (initialCost * drkToLtcRate);
            totalLtcWeCanBuy = (totalLtcWeCanBuy) - (totalLtcWeCanBuy * FEE);

            decimal totalBtcWeCanBuyWithLtc = (totalLtcWeCanBuy * ltcToBtcRate);
            totalBtcWeCanBuyWithLtc = totalBtcWeCanBuyWithLtc - (totalBtcWeCanBuyWithLtc * FEE);

            decimal totalDrkWeCanBuyWithBtc = (totalBtcWeCanBuyWithLtc / btcToDrkRate);
            totalDrkWeCanBuyWithBtc = totalDrkWeCanBuyWithBtc - (totalDrkWeCanBuyWithBtc * FEE);

            decimal profit = totalDrkWeCanBuyWithBtc - initialCost;

            decimal totalReturn = profit / initialCost;

            return totalReturn;
        }
    }
}
