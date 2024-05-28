class gbm
{
    /// <summary>
    /// Simulation showing how prices will evolve over time using GBM
    /// </summary>
    static void Main()
    {
        double[] prices = GeneratePrices();
        double[] gbmPrices = GeometricBrownianMotion(prices);

        Console.WriteLine("GBM prices:");
        foreach (double price in gbmPrices)
        {
            Console.WriteLine(price);
        }
    }

    public static double[] GeneratePrices()
    {
        double startingPrice = 140.0;
        int n = 500;
        double volatility = 0.02;
        Random rand = new Random();

        List<double> stockPrices = new List<double>();
        stockPrices.Add(startingPrice);

        for (int i = 1; i < n; i++)
        {
            // Generate random price fluctuation
            double fluctuation = rand.NextDouble() * volatility - (volatility / 2);
            double newPrice = stockPrices[i - 1] * (1 + fluctuation);
            stockPrices.Add(newPrice);
        }
        return stockPrices.ToArray();
    }

    private static double[] GeometricBrownianMotion(double[] prices)
    {
        int n = prices.Length;
        double[] gbm = new double[n];

        gbm[0] = prices[0];
        double volitility = Math.Sqrt(CalcLogReturnsVariance(prices));
        double drift = CalcLogReturnDrift(prices); // expected return of the asset over time
        double driftAdjustment = 0.5; // this is the 1/2 x volatility^2 which appears in the GBM formula that accounts for any risk adjustment

        Random rand = new Random();
        for (int i = 1; i < n; i++)
        {
            double dt = 1.0 / (n - 1); // normalise timestep
            double r = rand.NextDouble() * volitility * Math.Sqrt(dt);
            gbm[i] = gbm[i - 1] * Math.Exp(drift - driftAdjustment * (volitility * volitility * dt) + r);

        }
        return gbm;

    }

    /// <summary>
    /// Compute mean of natural logarithmic return.
    /// Measure % of change in asset over time using natural logarithm (base e) of price ratio
    /// </summary>
    /// <param name="prices"></param>
    /// <returns></returns>
    private static double CalcLogReturnDrift(double[] prices)
    {
        List<double> logReturns = new List<double>();
        for (int i = 1; i < prices.Length; i++)
        {
            double logReturn = Math.Log(prices[i] / prices[i-1]);
            logReturns.Add(logReturn);
        }
        var meanLogReturn = CalcMean(logReturns.ToArray());
        return meanLogReturn;
    }

    public static double CalcMean(double[] data)
    {
        double sum = 0;
        foreach (double v in data)
        {
            sum += v;
        }
        return sum / data.Length;
    }

    private static double CalcLogReturnsVariance(double[] prices)
    {
        List<double> logReturns = new List<double>();
        for (int i = 1  ; i < prices.Length; i++)
        {
            double logReturn = Math.Log(prices[i]/ prices[i-1]);
            logReturns.Add(logReturn);
        }

        double mean = CalcMean(logReturns.ToArray());
        double sumOfSquaredDeviations = 0;

        foreach (double r in logReturns)
        {
            double deviation = r - mean;
            sumOfSquaredDeviations += deviation * deviation;

        }
        return sumOfSquaredDeviations / logReturns.Count;
    }

}
