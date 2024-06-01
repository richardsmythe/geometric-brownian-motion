using System;
using System.Collections.Generic;

class GBM
{
    /// <summary>
    /// Simulation showing how prices will evolve over time using GBM
    /// </summary>
    static void Main()
    {
        int numberOfSimulations = 1000;
        int numberOfSteps = 252; // 1 year of prices
        double initialPrice = 100.0;

        double[][] monteCarloSimulations = MonteCarloSimulation(numberOfSimulations, numberOfSteps, initialPrice);

        foreach (var simulation in monteCarloSimulations)
        {
            foreach (double price in simulation)
            {
                Console.Write(price + " ");
            }
            Console.WriteLine();
        }
        string filePath = "C:\\Users\\rsmythe\\source\\repos\\gbm\\montecarlo.csv";
        SaveToCsv(monteCarloSimulations, filePath);

        double[] finalPrices = new double[numberOfSimulations];
        for (int i = 0; i < numberOfSimulations; i++)
        {
            finalPrices[i] = monteCarloSimulations[i][numberOfSteps - 1];
        }

        double meanFinalPrice = CalcMean(finalPrices);
        double varianceFinalPrice = CalcVariance(finalPrices);
        double stdDevFinalPrice = Math.Sqrt(varianceFinalPrice);

        Console.WriteLine($"\nMean Final Price: {meanFinalPrice}");
        Console.WriteLine($"Standard Deviation of Final Price: {stdDevFinalPrice}");
    }

    public static double[][] MonteCarloSimulation(int numberOfSimulations, int numberOfSteps, double initialPrice)
    {
        double[][] simulations = new double[numberOfSimulations][];

        for (int i = 0; i < numberOfSimulations; i++)
        {
            double[] prices = GeneratePrices(initialPrice, numberOfSteps);
            simulations[i] = GeometricBrownianMotion(prices);
        }

        return simulations;
    }

    public static double[] GeneratePrices(double startingPrice, int n)
    {
        double volatility = 0.05;
        Random rand = new Random();

        List<double> stockPrices = new List<double>();
        stockPrices.Add(startingPrice);

        for (int i = 1; i < n; i++)
        {
            // add random fluctuation
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
        double volatility = Math.Sqrt(CalcLogReturnsVariance(prices));
        double drift = CalcLogReturnDrift(prices); // expected return of the asset over time
        double driftAdjustment = 0.5; // this is the 1/2 x volatility^2 which appears in the GBM formula that accounts for any risk adjustment

        Random rand = new Random();
        for (int i = 1; i < n; i++)
        {
            double dt = 1.0 / (n - 1); // normalise timestep
            double r = rand.NextDouble() * volatility * Math.Sqrt(dt);
            gbm[i] = gbm[i - 1] * Math.Exp(drift - driftAdjustment * (volatility * volatility * dt) + r);
        }
        return gbm;
    }

    /// <summary>
    /// Compute mean of natural logarithmic return.
    /// Measure % of change in asset over time using natural logarithm (base e) of price ratio.
    /// Essentially averages out all up and downs to get a trend line
    /// </summary>
    /// <param name="prices"></param>
    /// <returns></returns>
    private static double CalcLogReturnDrift(double[] prices)
    {
        List<double> logReturns = new List<double>();
        for (int i = 1; i < prices.Length; i++)
        {
            double logReturn = Math.Log(prices[i] / prices[i - 1]);
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
        for (int i = 1; i < prices.Length; i++)
        {
            double logReturn = Math.Log(prices[i] / prices[i - 1]);
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

    private static double CalcVariance(double[] data)
    {
        double mean = CalcMean(data);
        double sumOfSquaredDeviations = 0;

        foreach (double value in data)
        {
            double deviation = value - mean;
            sumOfSquaredDeviations += deviation * deviation;
        }
        return sumOfSquaredDeviations / data.Length;
    }
    private static void SaveToCsv(double[][] simulations, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            int numberOfSteps = simulations[0].Length;
            int numberOfSimulations = simulations.Length;
         
            writer.Write("Time Step");
            for (int i = 0; i < numberOfSimulations; i++)
            {
                writer.Write($",Simulation {i + 1}");
            }
            writer.WriteLine();

            for (int step = 0; step < numberOfSteps; step++)
            {
                writer.Write(step);
                for (int sim = 0; sim < numberOfSimulations; sim++)
                {
                    writer.Write($",{simulations[sim][step]}");
                }
                writer.WriteLine();
            }
        }
    }
}