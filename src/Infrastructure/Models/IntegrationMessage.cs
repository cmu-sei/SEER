using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Seer.Hubs;
using Seer.Infrastructure.Data;
using Seer.Infrastructure.Services;

namespace Seer.Infrastructure.Models;

public class IntegrationMessage
{
    public IntegrationDocument Document;
    public IEnumerable<KeyValuePair<string, string>> Updates { get; set; }
    public EventDetailHistory Detail { get; set; }

    public IntegrationMessage(object payload)
    {
        this.Updates = new List<KeyValuePair<string, string>>();
        this.Detail = new EventDetailHistory();
        
        var t = payload.ToString().Replace("ValueKind = Object : \"", "");
        t = t.Substring(0, t.Length - 2) + "]}";
        var integrationDocument = JsonConvert.DeserializeObject<IntegrationDocument>(t);
        //Console.Write($"{integrationDocument.Embeds[0].Title} : {integrationDocument.Embeds[0].Description}");

        this.Document = integrationDocument;
    }

    //TODO I don't love this, but this is what dependency injection gives you sometimes
    public async Task Process(ApplicationDbContext db, IHubContext<ExecutionHub> executionHubContext, UserManager<User> userManager)
    {
        this.Parse();
        await IntegrationMessageService.Process(db, executionHubContext, userManager, this);
    }

    private void Parse()
    {
        this.Detail.Message = this.Document.Embeds[0].Description;
        
    }
            
    public class IntegrationDocumentEmbeds
    {
        [JsonProperty("embed.description")]
        public string Description { get; set; }

        [JsonProperty("embed.title")]
        public string Title { get; set; }
    }

    public class IntegrationDocument
    {
        [JsonProperty("embeds.0")]
        public List<IntegrationDocumentEmbeds> Embeds { get; set; }
    }
}