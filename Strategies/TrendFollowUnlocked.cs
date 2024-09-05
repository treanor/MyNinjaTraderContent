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
	public class TrendFollowUnlocked : Strategy
	{
		private SMA shortMAIndicator;
		private SMA longMAIndicator;
		private double shortMA;
		private double longMA;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Trend-following strategy using moving averages.";
				Name = "TrendFollowUnlocked";
				Calculate = Calculate.OnBarClose;
				EntriesPerDirection = 1;
				EntryHandling = EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy = true;
				ExitOnSessionCloseSeconds = 30;
				Slippage = 0;
				StartBehavior = StartBehavior.WaitUntilFlat;
				TimeInForce = TimeInForce.Gtc;
				BarsRequiredToTrade = 20;

				// Parameters for moving averages
				ShortMAPeriod = 50;
				LongMAPeriod = 200;

				IsInstantiatedOnEachOptimizationIteration = true;
			}
			else if (State == State.Configure)
			{
				shortMAIndicator = SMA(ShortMAPeriod);
				longMAIndicator = SMA(LongMAPeriod);

				// Add the moving average plots
				AddChartIndicator(shortMAIndicator);
				AddChartIndicator(longMAIndicator);

			}
			else if (State == State.DataLoaded)
			{
				longMAIndicator.Plots[0].Brush = Brushes.Blue;
				longMAIndicator.Plots[0].Width = 2;
				shortMAIndicator.Plots[0].Brush = Brushes.Red;
				shortMAIndicator.Plots[0].Width = 2;
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0 || CurrentBars[0] < BarsRequiredToTrade)
				return;

			bool MarketOpen = ToTime(Time[0]) >= 090000 && ToTime(Time[0]) <= 143000;

			// Get current moving average values
			shortMA = SMA(ShortMAPeriod)[0];
			longMA = SMA(LongMAPeriod)[0];


			if (MarketOpen)
			{
				// Detect bullish crossover (short MA crosses above long MA)
				if (CrossAbove(SMA(ShortMAPeriod), SMA(LongMAPeriod), 1))
				{
					if (Position.MarketPosition == MarketPosition.Flat)
					{
						EnterLong();
					}
					else if (Position.MarketPosition == MarketPosition.Short)
					{
						ExitShort();
						EnterLong();
					}
				}
				// Detect bearish crossover (short MA crosses below long MA)
				else if (CrossBelow(SMA(ShortMAPeriod), SMA(LongMAPeriod), 1))
				{
					if (Position.MarketPosition == MarketPosition.Flat)
					{
						EnterShort();
					}
					else if (Position.MarketPosition == MarketPosition.Long)
					{
						ExitLong();
						EnterShort();
					}
				}
			}
			if (!MarketOpen)
			{
				ExitLong();
				ExitShort();
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "ShortMA Period", Order = 1, GroupName = "Parameters")]
		public int ShortMAPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "LongMA Period", Order = 2, GroupName = "Parameters")]
		public int LongMAPeriod { get; set; }

		#endregion
	}
}