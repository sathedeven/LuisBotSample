using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    //[LuisModel(ConfigurationManager.AppSettings["LuisAppId"], "e37a5aa2ca7d4c34b790ae6dd4635400", domain: "westus.api.cognitive.microsoft.com", Staging = true)]
    public class BasicLuisDialog : LuisDialog<object>
    {
        private string _LuisAppId;
        private string _LuisApiKey;
        private string _LuisApiHostName;

        public BasicLuisDialog() : base(new LuisService(new LuisModelAttribute(
            ConfigurationManager.AppSettings["LuisAppId"],
            ConfigurationManager.AppSettings["LuisAPIKey"],
            domain: ConfigurationManager.AppSettings["LuisAPIHostName"])))
        {
            _LuisAppId = ConfigurationManager.AppSettings["LuisAppId"];
            _LuisApiKey = ConfigurationManager.AppSettings["LuisAPIKey"];
            _LuisApiHostName = ConfigurationManager.AppSettings["LuisAPIHostName"];
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        // Go to https://luis.ai and create a new intent, then train/publish your luis app.
        // Finally replace "Greeting" with the name of your newly created intent in the following handler
        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        [LuisIntent("About")]
        public async Task AboutIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            string returnMsg = $"You have reached {result.Intents[0].Intent}. You said: {result.Query}";

            if (result.Entities.Count > 0)
            {
                if (!string.IsNullOrEmpty(result.Entities[0].Entity))
                {

                    var client = new HttpClient();
                    var path = "rest/v2/name/" + result.Entities[0].Entity;
                    CountryInfo countryInfo = new CountryInfo();

                    client.BaseAddress = new Uri("https://restcountries.eu/");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(path);

                    if (response.IsSuccessStatusCode)
                    {
                        countryInfo = await response.Content.ReadAsAsync<CountryInfo>();
                    }

                    returnMsg += $" Capital is {countryInfo.Capital} \n";
                    returnMsg += $" Region is {countryInfo.Region} \n";
                    returnMsg += $" Sub Region is {countryInfo.SubRegion} \n";
                }
                
            }



            await context.PostAsync(returnMsg);
            context.Wait(MessageReceived);
        }
    }

    public class CountryInfo
    {
        public string Name { get; set; }
        public string Capital { get; set; }

        public string Region { get; set; }

        public string SubRegion { get; set; }
    }
}