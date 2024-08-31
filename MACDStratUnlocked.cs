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
	public class MACDStratUnlocked : Strategy
	{
		// Declare the MACD and SMA variables
		private MACD macd;
		private SMA sma200;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "MACDStratUnlocked";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				FastPeriod					= 12;
				SlowPeriod					= 26;
				SignalPeriod				= 9;
				SmaPeriod					= 200; // Added SMA period
			}
			else if (State == State.Configure)
			{
				// Initialize the MACD indicator with parameters
				macd = MACD(FastPeriod, SlowPeriod, SignalPeriod);
				AddChartIndicator(macd);
				
				// Initialize the 200-period SMA
				sma200 = SMA(SmaPeriod);
				AddChartIndicator(sma200);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;
			
			// Check if RTH
			bool MarketOpen = ToTime(Time[0]) >= 090000 && ToTime(Time[0]) <= 145000;

			// MACD Cross
			bool HasCrossedAbove = CrossAbove(macd.Default, macd.Avg, 1);
			bool HasCrossedBelow = CrossBelow(macd.Default, macd.Avg, 1);
			
			// Check if price is above or below the 200-period SMA
			bool PriceAboveSMA = Close[0] > sma200[0];
			bool PriceBelowSMA = Close[0] < sma200[0];
			
			// Enter Positions
			if (MarketOpen)
			{
				if (HasCrossedAbove && PriceAboveSMA)
				{
					EnterLong(Convert.ToInt32(DefaultQuantity), "");
				}
				else if (HasCrossedBelow && PriceBelowSMA)
				{
					EnterShort(Convert.ToInt32(DefaultQuantity), "");
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
		[Display(Name="FastPeriod", Order=1, GroupName="Parameters")]
		public int FastPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowPeriod", Order=2, GroupName="Parameters")]
		public int SlowPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SignalPeriod", Order=3, GroupName="Parameters")]
		public int SignalPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SmaPeriod", Order=4, GroupName="Parameters")]
		public int SmaPeriod
		{ get; set; }
		#endregion
	}
}
