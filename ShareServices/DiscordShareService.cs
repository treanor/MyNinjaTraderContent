#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
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

#endregion

//This namespace holds Share adapters in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.ShareServices
{
	public class DiscordShareService : ShareService
	{
		private static readonly HttpClient client = new HttpClient();

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Share service for sending messages and images to Discord via a webhook.";
				Name = "DiscordShareService";
				DiscordWebhook = string.Empty;
				CharacterLimit = 2000; // Discord has a character limit of 2000
				CharactersReservedPerMedia = 0;    // Discord doesn’t reserve characters for media
				IsImageAttachmentSupported = true; // Allow image attachments
				IsConfigured = false; // Initial configuration status
				UseOAuth = true;
			}
			else if (State == State.Configure)
			{
				// IsConfigured = !string.IsNullOrEmpty(DiscordWebhook);
				// Print("DiscordShareService is " + (IsConfigured ? "configured" : "not configured"));
				IsConfigured = true;
			}
		}

		public override async Task OnAuthorizeAccount()
		{
			// Assuming DiscordWebhook is a class member or can be accessed here
			if (string.IsNullOrEmpty(DiscordWebhook))
			{
				IsConfigured = false;
				throw new InvalidOperationException("Webhook URL is not configured.");
			}

			using (HttpClient client = new HttpClient())
			{
				try
				{
					HttpResponseMessage response = await client.GetAsync(DiscordWebhook);
					if (response.IsSuccessStatusCode)
					{
						IsConfigured = true;
					}
					else
					{
						IsConfigured = false;
						throw new InvalidOperationException("Invalid webhook URL.");
					}
				}
				catch (HttpRequestException)
				{
					IsConfigured = false;
					throw new InvalidOperationException("Error validating webhook URL.");
				}
			}
		}

		public override async Task OnShare(string text, string imgFilePath)
		{
			Print("DiscordShareService: " + text);

			if (string.IsNullOrWhiteSpace(DiscordWebhook))
			{
				Print("DiscordWebhook URL is not set.");
				return;
			}

			// Construct the payload for Discord
			var payload = new Dictionary<string, string>
	  {
		{ "content", text }
	  };

			// Create multipart content if an image is included
			var content = new MultipartFormDataContent();

			// Add the message
			content.Add(new StringContent(text), "content");

			// Add the image if provided
			if (!string.IsNullOrEmpty(imgFilePath) && File.Exists(imgFilePath))
			{
				var imgContent = new ByteArrayContent(File.ReadAllBytes(imgFilePath));
				imgContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
				content.Add(imgContent, "file", Path.GetFileName(imgFilePath));
			}

			try
			{
				// Send the payload to the Discord webhook
				var response = await client.PostAsync(DiscordWebhook, content);

				if (response.IsSuccessStatusCode)
				{
					Print("Message successfully sent to Discord.");
				}
				else
				{
					Print("Failed to send message to Discord. Status code: " + response.StatusCode);
				}
			}
			catch (Exception ex)
			{
				Print("Error sending message to Discord: " + ex.Message);
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "Discord Webhook URL", Order = 1, GroupName = "Parameters")]
		public string DiscordWebhook
		{ get; set; }
		#endregion
	}
}
