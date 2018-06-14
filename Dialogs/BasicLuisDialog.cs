using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;

namespace Microsoft.Bot.Sample.LuisBot
{
    // For more information about this template visit http://aka.ms/azurebots-csharp-luis
    [Serializable]
    //[LuisModel("76419b31-47b7-4402-b23b-588c103f3446", "6d430284cf3640d08b4e4ff950d5c0b5", domain: "westus.api.cognitive.microsoft.com")]
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
            var messageToUser = "Hello, I am chatbot that provides some basic information about European countries. Type some country name eg. About France";
            await context.PostAsync(messageToUser);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Cancel")]
        public async Task CancelIntent(IDialogContext context, LuisResult result)
        {
            var messageToUser = "Thanks for interacting with me. Have a good day.";
            await context.PostAsync(messageToUser);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task HelpIntent(IDialogContext context, LuisResult result)
        {
            var messageToUser = "Sure, I will try to find something for you. Type some country name eg. About France";
            await context.PostAsync(messageToUser);
            context.Wait(MessageReceived);

        }

        [LuisIntent("About")]
        public async Task AboutIntent(IDialogContext context, LuisResult result)
        {
            await this.ShowLuisResult(context, result);
        }

        private async Task ShowLuisResult(IDialogContext context, LuisResult result)
        {
            StringBuilder sb = new StringBuilder();
            bool notFound = true;
            //sb.Append($"You have reached {result.Intents[0].Intent}. You said: {result.Query}");
            try
            {
                if (result.Entities.Count > 0)
                {
                    if (!string.IsNullOrEmpty(result.Entities[0].Entity))
                    {
                        var client = new HttpClient();
                        var path = "rest/v2/name/" + result.Entities[0].Entity;
                        List<CountryInfo> lstCountryInfo = new List<CountryInfo>();

                        client.BaseAddress = new Uri("https://restcountries.eu/");
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response = await client.GetAsync(path);

                        if (response.IsSuccessStatusCode)
                        {
                            var jsonString = await response.Content.ReadAsStringAsync();
                            //var resp = await response.Content.ReadAsAsync(typeof(CountryInfo));

                            lstCountryInfo = JsonConvert.DeserializeObject<List<CountryInfo>>(jsonString);

                            sb.Append($" Capital is {lstCountryInfo[0].Capital} \n");
                            sb.Append($" Region is {lstCountryInfo[0].Region} \n");
                            sb.Append($" Sub Region is {lstCountryInfo[0].SubRegion} \n");
                            //sb.Append($" Currency is {lstCountryInfo[0].Currencies[0].Name } ( { lstCountryInfo[0].Currencies[0].Code} )  \n");
                        }
                        else
                        {
                            sb.Append("There was some problem in fetching the data. Please try after sometime.");
                        }
                        notFound = false;
                    }
                }
                if (notFound)
                    sb.Append("Sorry I don't know that. \n");
            }

            catch (Exception ex)
            {
                sb.Append(ex.Message + " , " + ex.InnerException);
            }
            finally
            {
                await context.PostAsync(sb.ToString());
                context.Wait(MessageReceived);
            }
        }

    }

    //[JsonArray]
    [Serializable]
    public class CountryInfo
    {
        public string Name { get; set; }
        public string Capital { get; set; }

        public string Region { get; set; }

        public string SubRegion { get; set; }

        //public List<Currency> Currencies { get; set; }
    }

    //[Serializable]
    //public class Currency
    //{
    //    public string Code { get; set; }
    //    public string Name { get; set; }
    //    public string Symbol { get; set; }
    //}
}