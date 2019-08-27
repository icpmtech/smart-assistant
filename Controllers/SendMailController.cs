using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QnABot.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QnABot.Controllers
{
    [Route("api/sendmail")]
    public class SendMailController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;

        public SendMailController(IConfiguration configuration,IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }
        /// <summary>
        /// Api to Send Mail to Flow
        /// </summary>
        /// <param name="ticket">The message object to create one Ticket in Flow</param>
        // POST api/sendmail
        [HttpPost]
        public void Post([FromBody]Ticket ticket)
        {
            if (!string.IsNullOrEmpty(ticket.Question)&& !string.IsNullOrEmpty(ticket.UserTeamsMail))
            {
                try
                {
                    var flowUrl = configuration["FlowUrl"];
                    var ticketFlow = new TicketFlow(ticket.UserTeamsMail, ticket.UserName, ticket.Question);
                    var httpClient = httpClientFactory.CreateClient();
                    var client = new HttpClient { BaseAddress = new Uri(flowUrl) };
                    string output = Newtonsoft.Json.JsonConvert.SerializeObject(ticketFlow);
                    var response = client.PostAsync(flowUrl, new StringContent(output,Encoding.UTF8, "application/json")).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return;
                    }
                    else
                    {
                        throw new Exception(response.ToString());
                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
              

               
            }
        }

      
       
    }
}
