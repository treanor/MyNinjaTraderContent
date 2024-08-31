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
        private MACD macd;
        private SMA sma200;
        private DateTime lastPositionExitTime;
		

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Strategy here.";
                Name = "MACDStratUnlocked";
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
                IsInstantiatedOnEachOptimizationIteration = true;

                FastPeriod = 12;
                SlowPeriod = 26;
                SignalPeriod = 9;
                SmaPeriod = 200;
                StopLossTicks = 20;
                TakeProfitTicks = 40;
                CooldownMinutes = 10;

                // Initialize lastPositionExitTime to the first bar's time
                lastPositionExitTime = DateTime.MinValue;
                Print("Initialized lastPositionExitTime: " + lastPositionExitTime);
            }
            else if (State == State.Configure)
            {
                macd = MACD(FastPeriod, SlowPeriod, SignalPeriod);
                AddChartIndicator(macd);

                sma200 = SMA(SmaPeriod);
                AddChartIndicator(sma200);

                SetStopLoss(CalculationMode.Ticks, StopLossTicks);
                SetProfitTarget(CalculationMode.Ticks, TakeProfitTicks);
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0)
                return;

            bool MarketOpen = ToTime(Time[0]) >= 090000 && ToTime(Time[0]) <= 145000;

            bool HasCrossedAbove = CrossAbove(macd.Default, macd.Avg, 1);
            bool HasCrossedBelow = CrossBelow(macd.Default, macd.Avg, 1);

            bool PriceAboveSMA = Close[0] > sma200[0];
            bool PriceBelowSMA = Close[0] < sma200[0];

            bool cooldownElapsed = Time[0] >= lastPositionExitTime.AddMinutes(CooldownMinutes);

            if (MarketOpen)
            {
                if (cooldownElapsed)
                {
					Print("Trading resumed at " + Time[0]);
                    if (HasCrossedAbove && PriceAboveSMA)
                    {
                        EnterLong(Convert.ToInt32(DefaultQuantity), "");
						Print("Entered long at " + Time[0]);
                    }
                    else if (HasCrossedBelow && PriceBelowSMA)
                    {
                        EnterShort(Convert.ToInt32(DefaultQuantity), "");
						Print("Entered short at " + Time[0]);
                    }
                }
                else
                {
                    // Log message to check cooldown status
                    Print("Cooldown period active. Trading will resume at: " + (lastPositionExitTime.AddMinutes(CooldownMinutes)));
                }
            }

            if (!MarketOpen)
            {
                ExitLong();
                ExitShort();
            }
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            if (execution.Order.OrderState == OrderState.Filled)
            {
                if (Position.MarketPosition == MarketPosition.Flat)
                {
                    Draw.ArrowUp(this, "Exit" + orderId, true, 0, price + 20, Brushes.Yellow);
                    lastPositionExitTime = Time[0]; // Update the last position exit time to the current bar's time
                    Print("Updated lastPositionExitTime to: " + lastPositionExitTime);
                }
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "FastPeriod", Order = 1, GroupName = "Parameters")]
        public int FastPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SlowPeriod", Order = 2, GroupName = "Parameters")]
        public int SlowPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SignalPeriod", Order = 3, GroupName = "Parameters")]
        public int SignalPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SmaPeriod", Order = 4, GroupName = "Parameters")]
        public int SmaPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, double.MaxValue)]
        [Display(Name = "Stop Loss (ticks)", Order = 5, GroupName = "Parameters")]
        public double StopLossTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, double.MaxValue)]
        [Display(Name = "Take Profit (ticks)", Order = 6, GroupName = "Parameters")]
        public double TakeProfitTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Cooldown (minutes)", Order = 7, GroupName = "Parameters")]
        public int CooldownMinutes { get; set; }
        #endregion
    }
}
