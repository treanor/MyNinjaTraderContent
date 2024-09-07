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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ATRTrailingStop : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Indicator as described in Sylvain Vervoort's 'Average True Range Trailing Stops' June 2009 S&C article.";
				Name										= "ATRTrailingStop";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Period					= 5;
				Multi					= 3.5;
				AddPlot(Brushes.Blue, "TrailingStop");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;

			// Trailing stop
			double trail;
			double loss = ATR(Input, Period)[0] * Multi;
			
			if (Close[0] > Value[1] && Close[1] > Value[1])
				trail = Math.Max(Value[1], Close[0] - loss);
			
			else if (Close[0] < Value[1] && Close[1] < Value[1])
				trail = Math.Min(Value[1], Close[0] + loss);
				
			else if (Close[0] > Value[1])
			{
				trail = Close[0] - loss;
				Draw.ArrowDown(this, CurrentBar.ToString(), false, 1, Value[1], Brushes.Orange);
			}
			
			else
			{
				trail = Close[0] + loss;
				Draw.ArrowUp(this, CurrentBar.ToString(), false, 1, Value[1], Brushes.Orange);
			}

			Value[0] = trail;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Description="ATR period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Multi", Description="ATR multiplication", Order=2, GroupName="Parameters")]
		public double Multi
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrailingStop
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ATRTrailingStop[] cacheATRTrailingStop;
		public ATRTrailingStop ATRTrailingStop(int period, double multi)
		{
			return ATRTrailingStop(Input, period, multi);
		}

		public ATRTrailingStop ATRTrailingStop(ISeries<double> input, int period, double multi)
		{
			if (cacheATRTrailingStop != null)
				for (int idx = 0; idx < cacheATRTrailingStop.Length; idx++)
					if (cacheATRTrailingStop[idx] != null && cacheATRTrailingStop[idx].Period == period && cacheATRTrailingStop[idx].Multi == multi && cacheATRTrailingStop[idx].EqualsInput(input))
						return cacheATRTrailingStop[idx];
			return CacheIndicator<ATRTrailingStop>(new ATRTrailingStop(){ Period = period, Multi = multi }, input, ref cacheATRTrailingStop);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ATRTrailingStop ATRTrailingStop(int period, double multi)
		{
			return indicator.ATRTrailingStop(Input, period, multi);
		}

		public Indicators.ATRTrailingStop ATRTrailingStop(ISeries<double> input , int period, double multi)
		{
			return indicator.ATRTrailingStop(input, period, multi);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ATRTrailingStop ATRTrailingStop(int period, double multi)
		{
			return indicator.ATRTrailingStop(Input, period, multi);
		}

		public Indicators.ATRTrailingStop ATRTrailingStop(ISeries<double> input , int period, double multi)
		{
			return indicator.ATRTrailingStop(input, period, multi);
		}
	}
}

#endregion
