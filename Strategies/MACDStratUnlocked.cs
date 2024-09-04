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
        private SMA volumeSMA;
        private DateTime lastPositionExitTime;
        private int numberOfTradesToday;
        private DateTime currentTradingDay;


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
                VolumeSMA_Period = 50;
                VolumeMultiplier = 1.0;
                MaxTradesPerDay = 3;
                UseTrailingStop = true;


                lastPositionExitTime = DateTime.MinValue;
                currentTradingDay = DateTime.MinValue;
                numberOfTradesToday = 0;


            }
            else if (State == State.Configure)
            {
                macd = MACD(FastPeriod, SlowPeriod, SignalPeriod);
                AddChartIndicator(macd);

                sma200 = SMA(SmaPeriod);
                AddChartIndicator(sma200);

                if (UseTrailingStop)
                {
                    SetTrailStop(CalculationMode.Ticks, StopLossTicks);
                }
                else
                {
                    SetStopLoss(CalculationMode.Ticks, StopLossTicks);
                    SetProfitTarget(CalculationMode.Ticks, TakeProfitTicks);
                }

                // Volume SMA setup
                volumeSMA = SMA(Volume, VolumeSMA_Period);
                volumeSMA.Panel = 1;  // Assuming the volume indicator is in Panel 1
                volumeSMA.Plots[0].Brush = Brushes.Blue; // Change color as needed
                AddChartIndicator(volumeSMA);
            }
            else if (State == State.Realtime)
            {
                Share("DiscordShareService", "Strategy started at " + Time[0]);
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0)
                return;

            // Check if the trading day has changed
            if (currentTradingDay.Date != Time[0].Date)
            {
                currentTradingDay = Time[0].Date;
                numberOfTradesToday = 0; // Reset trade count for the new day
            }

            if (numberOfTradesToday >= MaxTradesPerDay)
            {
                Print("Max trades per day limit reached. No more trades will be executed today.");
                return; // Exit the method if the max trade limit has been reached
            }

            bool MarketOpen = ToTime(Time[0]) >= 090000 && ToTime(Time[0]) <= 140000;

            bool HasCrossedAbove = CrossAbove(macd.Default, macd.Avg, 1);
            bool HasCrossedBelow = CrossBelow(macd.Default, macd.Avg, 1);

            bool PriceAboveSMA = Close[0] > sma200[0];
            bool PriceBelowSMA = Close[0] < sma200[0];

            bool cooldownElapsed = Time[0] >= lastPositionExitTime.AddMinutes(CooldownMinutes);

            bool volumeConditionMet = Volume[0] > volumeSMA[0] * VolumeMultiplier;

            if (MarketOpen)
            {
                if (cooldownElapsed)
                {
                    Print("Trading resumed at " + Time[0]);
                    if (HasCrossedAbove && PriceAboveSMA && volumeConditionMet)
                    {
                        EnterLong(Convert.ToInt32(DefaultQuantity), "");
                        Print("Entered long at " + Time[0]);
                        Share("DiscordShareService", "Entered long at " + Time[0]);
                    }
                    else if (HasCrossedBelow && PriceBelowSMA && volumeConditionMet)
                    {
                        EnterShort(Convert.ToInt32(DefaultQuantity), "");
                        Print("Entered short at " + Time[0]);
                        Share("DiscordShareService", "Entered short at " + Time[0]);
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
                    numberOfTradesToday++;
                    Print("Updated lastPositionExitTime to: " + lastPositionExitTime);
                    Share("DiscordShareService", "Exited position at " + Time[0]);
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

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Volume SMA Period", Order = 8, GroupName = "Parameters")]
        public int VolumeSMA_Period { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "Volume Multiplier", Order = 9, GroupName = "Parameters")]
        public double VolumeMultiplier { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Max Trades Per Day", Order = 10, GroupName = "Parameters")]
        public int MaxTradesPerDay { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use Trailing Stop", Order = 11, GroupName = "Parameters")]
        public bool UseTrailingStop { get; set; }

        #endregion
    }
}
