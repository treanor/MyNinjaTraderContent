#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class SuperTrendUnlocked : Strategy
	{
        private double atrValue;
        private double superTrendUpper;
        private double superTrendLower;
        private double superTrend;
        private bool isUpTrend;
        private bool isDownTrend;
        private Series<double> superTrendSeries;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Super Trend strategy based on ATR.";
				Name = "SuperTrendUnlocked";
				Calculate = Calculate.OnBarClose;
				EntriesPerDirection = 1;
				EntryHandling = EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy = true;
				ExitOnSessionCloseSeconds = 30;
				IsFillLimitOnTouch = false;
				MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution = OrderFillResolution.Standard;
				Slippage = 1;
				StartBehavior = StartBehavior.WaitUntilFlat;
				TimeInForce = TimeInForce.Gtc;
				TraceOrders = false;
				RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling = StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade = 20;
				IsInstantiatedOnEachOptimizationIteration = true;
				ATRLength = 5;
				ATRFactor = 3;
			}
			else if (State == State.Configure)
			{
				// AddDataSeries(Data.BarsPeriodType, 1); // 1-minute bars for calculations
				superTrendSeries = new Series<double>(this);
			}
			else if (State == State.DataLoaded)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < ATRLength)
				return;

			// Calculate ATR
			atrValue = ATR(ATRLength)[0];

			// Calculate SuperTrend levels
			superTrendUpper = High[0] - ATRFactor * atrValue;
			superTrendLower = Low[0] + ATRFactor * atrValue;

			// Determine trend
			if (Close[0] > superTrendUpper)
			{
				if (superTrendLower > superTrendSeries[1])
					superTrend = superTrendLower;
				else
					superTrend = superTrendSeries[1];
				// superTrend = superTrendLower;
				isUpTrend = true;
				isDownTrend = false;
			}
			else if (Close[0] < superTrendLower)
			{
				if (superTrendUpper < superTrendSeries[1])
					superTrend = superTrendLower;
				else
					superTrend = superTrendSeries[1];
				// superTrend = superTrendUpper;
				isUpTrend = false;
				isDownTrend = true;
			}

			Print("SuperTrend: " + superTrend);
			Print("Last SuperTrend: " + superTrendSeries[1]);
			Print("Close: " + Close[0]);
			Print("isUpTrend: " + isUpTrend);
			Print("isDownTrend: " + isDownTrend);
			

			// Store SuperTrend value
			superTrendSeries[0] = superTrend;

			// Plotting SuperTrend
			PlotSuperTrend();
		}

		private void PlotSuperTrend()
		{
			// Plot uptrend in green
			if (isUpTrend)
			{
				Draw.Line(this, "UpTrend_" + CurrentBar, false, 1, superTrendSeries[1], 0, superTrend, Brushes.Green, DashStyleHelper.Solid, 2);
			}
			// Plot downtrend in red
			else if (isDownTrend)
			{
				Draw.Line(this, "DownTrend_" + CurrentBar, false, 1, superTrendSeries[1], 0, superTrend, Brushes.Red, DashStyleHelper.Solid, 2);
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "ATR Length", Order = 1, GroupName = "Parameters")]
		public int ATRLength { get; set; }

		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(Name = "ATR Factor", Order = 2, GroupName = "Parameters")]
		public double ATRFactor { get; set; }
		#endregion
	}
}