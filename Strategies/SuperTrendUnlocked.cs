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
		private ATRTrailingStop atrTrailingStop;

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "ATR Period", Order = 1, GroupName = "Parameters")]
		public int AtrPeriod { get; set; }

		[Range(1.0, double.MaxValue), NinjaScriptProperty]
		[Display(Name = "ATR Multiplier", Order = 2, GroupName = "Parameters")]
		public double AtrMultiplier { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Use Take Profit", Order = 1, GroupName = "Parameters")]
		public bool UseTakeProfit { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Take Profit in Dollars", Order = 2, GroupName = "Parameters")]
		public double TakeProfit { get; set; }

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Strategy based on ATR Trailing Stop";
				Name = "SuperTrendUnlocked";
				Calculate = Calculate.OnBarClose;
				EntriesPerDirection = 1;
				EntryHandling = EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy = true;
				ExitOnSessionCloseSeconds = 30;
				IsFillLimitOnTouch = false;
				MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution = OrderFillResolution.Standard;
				Slippage = 0;
				StartBehavior = StartBehavior.WaitUntilFlat;
				TimeInForce = TimeInForce.Gtc;
				TraceOrders = false;
				RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling = StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade = 20;
				IsInstantiatedOnEachOptimizationIteration = false;
				AtrPeriod = 10;
				AtrMultiplier = 3;
				UseTakeProfit = false;
				TakeProfit = 10;
			}
			else if (State == State.Configure)
			{
				// Instantiate the ATRTrailingStop indicator
				atrTrailingStop = ATRTrailingStop(AtrPeriod, AtrMultiplier);

				if (UseTakeProfit)
				{
					SetProfitTarget(CalculationMode.Currency, TakeProfit);
				}
			}
		}

		protected override void OnBarUpdate()
		{
			// Check if BarsInProgress is the primary data series
			if (BarsInProgress != 0)
				return;

			// Ensure we have enough bars to calculate ATR
			if (CurrentBar < BarsRequiredToTrade)
				return;

			bool MarketOpen = ToTime(Time[0]) >= 083000 && ToTime(Time[0]) <= 150000;

			if (MarketOpen)
			{
				// Buy logic: if price crosses above the trailing stop, enter long
				if (Close[0] > atrTrailingStop.TrailingStop[0])
				{
					if (Position.MarketPosition == MarketPosition.Short)
					{
						ExitShort();
					}
					if (Position.MarketPosition == MarketPosition.Flat)
					{
						EnterLong();
					}
				}
				// Sell logic: if price crosses below the trailing stop, enter short
				else if (Close[0] < atrTrailingStop.TrailingStop[0])
				{
					if (Position.MarketPosition == MarketPosition.Long)
					{
						ExitLong();
					}
					if (Position.MarketPosition == MarketPosition.Flat)
					{
						EnterShort();
					}
				}
			}

			if (!MarketOpen)
			{
				if (Position.MarketPosition == MarketPosition.Short)
				{
					ExitShort();
				}
				if (Position.MarketPosition == MarketPosition.Long)
				{
					ExitLong();
				}
			}
		}

		private ATRTrailingStop ATRTrailingStop(int period, double multiplier)
		{
			// Get the ATRTrailingStop indicator
			return ATRTrailingStop(Input, period, multiplier);
		}
	}
}