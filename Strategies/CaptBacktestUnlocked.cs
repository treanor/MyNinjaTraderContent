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
	public class CaptBacktestUnlocked : Strategy
	{
		private Series<double> _rangeHigh;
		private Series<double> _rangeLow;
		
		private Series<int> _bias;
		
		private Series<bool> _oppositeClose;
		private Series<bool> _tookHiLow;
		private Series<bool> _isLong;
		private Series<bool> _isShort;

		private Series<bool> _t_prev;
		private Series<bool> _t_take;
		private Series<bool> _t_trade;

		private int _lastTrades = 0;
		private int _priorNumberTrades = 0;
		private int _priorSessionTrades = 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "CaptBacktestUnlocked";
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
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;

				PrevStart = 060000;
				PrevEnd = 100000;
				TakeStart = 100000;
				TakeEnd = 111500;
				TradeStart = 100000;
				TradeEnd = 160000;

				WaitRetrace1 = true;
				WaitRetrace2 = true;
				UseStopOrders = false;
				UseFixedRR = true;
				RiskPoints = 25;
				RewardPoints = 75;

			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 3)
				return;
			
			_t_prev[0] = CheckTime(PrevStart, PrevEnd);
			_t_take[0] = CheckTime(TakeStart, TakeEnd);
			_t_trade[0] = CheckTime(TradeStart, TradeEnd);

			_bias[0] = _bias[1];

			bool canTrade = TookTrade();
			
			SetProfitTarget("", CalculationMode.Ticks, RewardPoints / TickSize);
			SetStopLoss("", CalculationMode.Ticks, RiskPoints / TickSize);
		}

		private bool TookTrade()
		{
			bool trade = false;

			if (Bars.IsFirstBarOfSession && IsFirstTickOfBar)
			{
				_priorSessionTrades = SystemPerformance.AllTrades.Count;
			}

			if ((SystemPerformance.AllTrades.Count - _priorSessionTrades) > 0)
			{
				trade = true;
			}

			return trade;
		}

		private bool CheckTime(int T1, int T2)
		{
			bool result = false;
			int T = ToTime(Time[0]);
			if (T1 > T2)
			{
				result = T >= T1 || T <= T2;
			}
			else
			{
				result = T >= T1 && T <= T2;
			}

			return result;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Price Range Start", Order=101, GroupName="Time")]
		public int PrevStart
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Price Range End", Order=102, GroupName="Time")]
		public int PrevEnd
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Bias Window Start", Order=103, GroupName="Time")]
		public int TakeStart
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Bias Window End", Order=104, GroupName="Time")]
		public int TakeEnd
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Trade Window Start", Order=105, GroupName="Time")]
		public int TradeStart
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Trade Window End", Order=106, GroupName="Time")]
		public int TradeEnd
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Wait for Retracement", Order=201, GroupName="Strategy")]
		public bool WaitRetrace1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Wait for Retracement", Order=202, GroupName="Strategy")]
		public bool WaitRetrace2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use Stop Orders", Order=203, GroupName="Strategy")]
		public bool UseStopOrders
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Use Fixed R:R", Order=204, GroupName="Strategy")]
		public bool UseFixedRR
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Risk (Points)", Order=301, GroupName="Risk")]
		public double RiskPoints
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Reward (Points)", Order=302, GroupName="Risk")]
		public double RewardPoints
		{ get; set; }
		#endregion
	}
}
