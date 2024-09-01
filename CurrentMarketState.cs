using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;

namespace NinjaTrader.NinjaScript.Indicators
{
    public class CurrentMarketState : Indicator
    {
        private enum MarketState
        {
            Ranging,
            UpTrend,
            DownTrend
        }

        private MarketState currentState = MarketState.Ranging;
        private double rangeHigh;
        private double rangeLow;
        private double lastTrendExtreme;
        private double previousLowerHigh = double.MaxValue;
        private double previousHigherLow = double.MinValue;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Analyzes market state to determine ranging or trending conditions";
                Name = "Current Market State";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;

                // Default property values
                LookbackPeriod = 20;
                RangeThreshold = 0.5; // Percentage of ATR

                AddPlot(Brushes.Orange, "State");
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < LookbackPeriod)
                return;

            double atr = ATR(14)[0];
            double rangeSize = High[0] - Low[0];

            // Remove previous lines
            RemoveDrawObjects();

            // Determine market state
            if (rangeSize <= atr * RangeThreshold)
            {
                // Ranging market
                currentState = MarketState.Ranging;
                rangeHigh = High[0];
                rangeLow = Low[0];

                for (int i = 1; i < LookbackPeriod; i++)
                { 
                    rangeHigh = Math.Max(rangeHigh, High[i]);
                    rangeLow = Math.Min(rangeLow, Low[i]);
                }

                // Draw range lines
                Draw.Line(this, "RangeHigh", false, LookbackPeriod, rangeHigh, 0, rangeHigh, Brushes.White, DashStyleHelper.Solid, 2);
                Draw.Line(this, "RangeLow", false, LookbackPeriod, rangeLow, 0, rangeLow, Brushes.White, DashStyleHelper.Solid, 2);
            }
            else
            {
                // Trending market
                if (Close[0] > Close[1])
                {
                    currentState = MarketState.DownTrend;
                    lastTrendExtreme = High[0];
                    for (int i = 1; i < LookbackPeriod; i++)
                    {
                        if (High[i] > lastTrendExtreme && Close[i] > Close[i + 1])
                        {
                            // Draw Lower high line
                            Draw.Line(this, "LowerHigh" + i, false, i, High[i], 0, High[i], Brushes.Red, DashStyleHelper.Solid, 2);
                            lastTrendExtreme = High[i];
                        }
                    }
                }
                else
                {
                    currentState = MarketState.UpTrend;
                    lastTrendExtreme = Low[0];
                    for (int i = 1; i < LookbackPeriod; i++)
                    {
                        if (Low[i] < lastTrendExtreme && Close[i] > Close[i + 1])
                        {
                            // Draw Higher Low line
                            Draw.Line(this, "HigherLow" + i, false, i, Low[i], 0, Low[i], Brushes.Green, DashStyleHelper.Solid, 2);
                            lastTrendExtreme = Low[i];
                        }
                    }
                }
            }

            // Set indicator value
            Value[0] = (int)currentState;

            // Draw state text
            Draw.TextFixed(this, "StateText", GetMarketStateText(), TextPosition.TopRight);
        }

        private string GetMarketStateText()
        {
            switch (currentState)
            {
                case MarketState.Ranging:
                    return string.Format("Ranging: High {0}, Low {1}", rangeHigh.ToString("N2"), rangeLow.ToString("N2"));
                case MarketState.UpTrend:
                    return string.Format("Up Trend: HL {0}", lastTrendExtreme.ToString("N2"));
                case MarketState.DownTrend:
                    return string.Format("Down Trend: LH {0}", lastTrendExtreme.ToString("N2"));
                default:
                    return "Unknown State";
            }
        }

        private void RemoveDrawObjects()
        {
            RemoveDrawObject("RangeHigh");
            RemoveDrawObject("RangeLow");
            for (int i = 0; i < LookbackPeriod; i++)
            {
                RemoveDrawObject("HigherHigh" + i);
                RemoveDrawObject("LowerLow" + i);
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Lookback Period", Description = "Number of bars to analyze", Order = 1, GroupName = "Parameters")]
        public int LookbackPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name = "Range Threshold", Description = "Percentage of ATR to determine ranging market", Order = 2, GroupName = "Parameters")]
        public double RangeThreshold { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CurrentMarketState[] cacheCurrentMarketState;
		public CurrentMarketState CurrentMarketState(int lookbackPeriod, double rangeThreshold)
		{
			return CurrentMarketState(Input, lookbackPeriod, rangeThreshold);
		}

		public CurrentMarketState CurrentMarketState(ISeries<double> input, int lookbackPeriod, double rangeThreshold)
		{
			if (cacheCurrentMarketState != null)
				for (int idx = 0; idx < cacheCurrentMarketState.Length; idx++)
					if (cacheCurrentMarketState[idx] != null && cacheCurrentMarketState[idx].LookbackPeriod == lookbackPeriod && cacheCurrentMarketState[idx].RangeThreshold == rangeThreshold && cacheCurrentMarketState[idx].EqualsInput(input))
						return cacheCurrentMarketState[idx];
			return CacheIndicator<CurrentMarketState>(new CurrentMarketState(){ LookbackPeriod = lookbackPeriod, RangeThreshold = rangeThreshold }, input, ref cacheCurrentMarketState);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CurrentMarketState CurrentMarketState(int lookbackPeriod, double rangeThreshold)
		{
			return indicator.CurrentMarketState(Input, lookbackPeriod, rangeThreshold);
		}

		public Indicators.CurrentMarketState CurrentMarketState(ISeries<double> input , int lookbackPeriod, double rangeThreshold)
		{
			return indicator.CurrentMarketState(input, lookbackPeriod, rangeThreshold);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CurrentMarketState CurrentMarketState(int lookbackPeriod, double rangeThreshold)
		{
			return indicator.CurrentMarketState(Input, lookbackPeriod, rangeThreshold);
		}

		public Indicators.CurrentMarketState CurrentMarketState(ISeries<double> input , int lookbackPeriod, double rangeThreshold)
		{
			return indicator.CurrentMarketState(input, lookbackPeriod, rangeThreshold);
		}
	}
}

#endregion
