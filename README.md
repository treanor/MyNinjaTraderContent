# MyNinjaTraderContent

A collection of custom NinjaTrader 8 strategies, indicators, and share services for advanced trading automation and analytics.

## Contents

- **Indicators/**
  - `ATRTrailingStop.cs`: Implements an Average True Range (ATR) based trailing stop indicator, as described in Sylvain Vervoort's S&C article. Used for dynamic stop placement based on market volatility.
- **ShareServices/**
  - `DiscordShareService.cs`: Enables automated sharing of trade signals and images to Discord channels via webhooks, supporting both text and image payloads.
- **Strategies/**
  - `CaptBacktestUnlocked.cs`: A flexible backtesting strategy with configurable time windows, bias detection, and risk/reward management. Designed for session-based range and breakout trading.
  - `MACDStratUnlocked.cs`: A MACD-based strategy with SMA filtering, volume confirmation, cooldowns, and Discord notifications. Limits trades per day and supports trailing stops.
  - `SuperTrendUnlocked.cs`: Implements a SuperTrend strategy using the custom ATRTrailingStop indicator, with optional take-profit logic and market session filtering.
  - `TrendFollowUnlocked.cs`: A trend-following strategy using dual moving averages and ADX for trend strength, with profit/stop management and account balance tracking.

## Features

- **Custom Indicators**: Advanced trailing stop logic for dynamic risk management.
- **Automated Strategies**: Multiple strategies for trend, breakout, and indicator-based trading.
- **Discord Integration**: Share trade signals and images directly to Discord for real-time alerts.
- **Highly Configurable**: All scripts expose parameters for easy optimization and customization within NinjaTrader.

## Usage

1. **Import into NinjaTrader 8**: Copy `.cs` files into the appropriate NinjaScript folders.
2. **Compile**: Open NinjaTrader 8, go to NinjaScript Editor, and compile.
3. **Configure**: Add indicators/strategies to your charts and set parameters as desired.
4. **Discord Setup**: For `DiscordShareService`, set your Discord webhook URL in the strategy or indicator properties.

## Requirements

- NinjaTrader 8
- .NET Framework (as required by NinjaTrader)
- Discord webhook (for share service)

## File Overview

| File | Type | Description |
|------|------|-------------|
| `Indicators/ATRTrailingStop.cs` | Indicator | ATR-based trailing stop for dynamic exits |
| `ShareServices/DiscordShareService.cs` | Share Service | Sends messages/images to Discord |
| `Strategies/CaptBacktestUnlocked.cs` | Strategy | Session-based range/breakout backtesting |
| `Strategies/MACDStratUnlocked.cs` | Strategy | MACD + SMA + volume, Discord alerts |
| `Strategies/SuperTrendUnlocked.cs` | Strategy | SuperTrend using ATR trailing stop |
| `Strategies/TrendFollowUnlocked.cs` | Strategy | Dual MA + ADX trend following |

## Customization

All scripts are designed for easy parameter tuning within the NinjaTrader UI. Refer to each script's `[NinjaScriptProperty]` attributes for available settings.

## License

This repository is provided for educational and personal use. Please review and test all scripts in a simulation environment before live trading.

## Trading Disclaimer

> **Disclaimer:** Trading financial instruments, including futures, stocks, and forex, involves significant risk and is not suitable for every investor. The scripts and strategies provided in this repository are for educational and informational purposes only and do not constitute financial advice. Past performance is not indicative of future results. You are solely responsible for any trading decisions and for evaluating the suitability and risks associated with the use of these scripts. Always test thoroughly in a simulation environment before live trading. Consult with a licensed financial advisor before making any investment decisions.
