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
	public class HMAcrossover : Strategy
	{
		private HMA fastHMA;
		private HMA slowHMA;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "HMAcrossover";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= false;
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
				Fast 										= 15;
				Slow										= 38;
				Stop										= 300;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
				AddDataSeries("ES 06-21", Data.BarsPeriodType.Minute, 3, Data.MarketDataType.Last);
				
				fastHMA = HMA(Fast);
				slowHMA = HMA(Slow);
				
				
				//Anytime we are in a live position, take profit at $50 profit
				//SetProfitTarget(CalculationMode.Currency, 500);
				
				
				//Anytime we are in a live position take a loss at $25
				SetStopLoss(CalculationMode.Currency, Stop);
				
				
			} else if (State == State.DataLoaded) {
				//Sets the SMA colors
			fastHMA.Plots[0].Brush = Brushes.Goldenrod;
			slowHMA.Plots[0].Brush = Brushes.SeaGreen;
			
			//Draw SMA values on chart
			AddChartIndicator(fastHMA);
			AddChartIndicator(slowHMA);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;
			
			//Enter a long trade when the 13 HMA crosses above the 30 HMA
			if (CrossAbove(fastHMA, slowHMA, 1))
				EnterLongLimit(GetCurrentBid());
			
			//Enter a shoprt trade when 13 HMA cross below the 30 HMA
			if (CrossBelow(fastHMA, slowHMA, 1))
				EnterShortLimit(GetCurrentAsk());
			
			//If cross from below while long, take profit/close pos
			if (Position.MarketPosition == MarketPosition.Long) {
				if (CrossBelow(fastHMA, slowHMA, 1))
					ExitLong();
			}
			
				//If HMA cross above while short, close out the position
			if (Position.MarketPosition == MarketPosition.Short) {
				if (CrossAbove(fastHMA, slowHMA, 1))
					ExitShort();	
			}
				
		
			
			
			
		
		}
		
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
		public int Fast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int Slow
		{ get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Stop", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
		public int Stop
		{ get; set; }
		#endregion
		
	}
}
