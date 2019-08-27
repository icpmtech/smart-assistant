using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace QnABot.Controllers
{
    [Route("api/updatekb")]
    [ApiController]
    public class UpdateKBController : ControllerBase
    {
      
        private static string QnAKnowledgebaseId;
        private static string KeyQnA;
        public UpdateKBController(IConfiguration configuration)
        {
          
           QnAKnowledgebaseId = configuration["QnAKnowledgebaseId"];
           KeyQnA = configuration["KeyQnA"];
        }
        // Represents the various elements used to create HTTP request URIs
        // for QnA Maker operations.
        static string host = "https://westus.api.cognitive.microsoft.com";
        static string service = "/qnamaker/v4.0";
        static string method = "/knowledgebases/";
        static string new_kb = @"
{
  'add': {
    'qnaList': [
      {
        'id': 1,
        'answer': 'You can change the default message if you use the QnAMakerDialog. See this for details: https://docs.botframework.com/azure-bot-service/templates/qnamaker/#navtitle',
        'source': 'Custom Editorial',
        'questions': [
          'How can I change the default message from QnA Maker?'
        ],
        'metadata': []
      }
    ],
    'urls': [
      'https://docs.microsoft.com/azure/cognitive-services/Emotion/FAQ'
    ]
  },
  'update' : {
    'name' : 'New KB Name'
  },
  'delete': {
    'ids': [
      0
    ]
  }
}
";
        /// <summary>
        /// Asynchronously sends a PUT HTTP request.
        /// </summary>
        /// <param name="uri">The URI of the HTTP request.</param>
        /// <param name="body">The body of the HTTP request.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task{TResult}(QnAMaker.Program.Response)"/> 
        /// object that represents the HTTP response."</returns>
        async static Task<string> Put(string uri, String body)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Put;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", KeyQnA);

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return "{'result' : 'Success.'}";
                }
                else
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Replaces a knowledge base.
        /// </summary>
        async static void ReplaceKB()
        {
            var uri = host + service + method + QnAKnowledgebaseId;
            Console.WriteLine("Calling " + uri + ".");
            var response = await Put(uri, new_kb);
            Console.WriteLine(PrettyPrint(response));
            Console.WriteLine("Press any key to continue.");
        }
        /// <summary>
        /// Asynchronously sends a GET HTTP request.
        /// </summary>
        /// <param name="uri">The URI of the HTTP request.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task{TResult}(QnAMaker.Program.Response)"/> 
        /// object that represents the HTTP response."</returns>
        async static Task<string> GetKb(string uri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", KeyQnA);

                var response = await client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
        }

        async static void PublishKB()
        {
            string responseText;

            var uri = host + service + method + QnAKnowledgebaseId;
            Console.WriteLine("Calling " + uri + ".");
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", KeyQnA);

                var response = await client.SendAsync(request);

                // successful status doesn't return an JSON so create one
                if (response.IsSuccessStatusCode)
                {
                    responseText = "{'result' : 'Success.'}";
                }
                else
                {
                    responseText = await response.Content.ReadAsStringAsync();
                }
            }
            Console.WriteLine(PrettyPrint(responseText));
            Console.WriteLine("Press any key to continue.");
        }

        // GET: api/UpdateKB
        /// <summary>
        /// Downloads a knowledge base, then prints it.
        /// </summary>
        [HttpGet]
        public  string Get()
        {
            var method_with_id = String.Format(method, QnAKnowledgebaseId, "test");
            var uri = host + service + method_with_id;
            Console.WriteLine("Calling " + uri + ".");
            var response =  GetKb(uri).Result;
            return PrettyPrint(response);
        }

        // GET: api/UpdateKB/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(string id)
        {
            var method_with_id = String.Concat(method, id);
            var uri = host + service + method_with_id;
            Console.WriteLine("Calling " + uri + ".");
            var response = GetKb(uri).Result;
            return PrettyPrint(response);
        }
        // POST: api/UpdateKB/Publish
        [HttpPost]
        public void Publish()
        {
            PublishKB();

        }
        // POST: api/UpdateKB
        [HttpPost]
        public void Post()
        {
            UpdateKB(QnAKnowledgebaseId, new_kb);
        }
        public struct Response
        {
            public HttpResponseHeaders headers;
            public string response;

            public Response(HttpResponseHeaders headers, string response)
            {
                this.headers = headers;
                this.response = response;
            }
        }

        static string PrettyPrint(string s)
        {
            return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(s), Formatting.Indented);
        }

        async static Task<Response> Patch(string uri, string body)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = new HttpMethod("PATCH");
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", KeyQnA);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                return new Response(response.Headers, responseBody);
            }
        }

        async static Task<Response> GetAsync(string uri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", KeyQnA);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                return new Response(response.Headers, responseBody);
            }
        }

        async static Task<Response> PostUpdateKB(string kb, string new_kb)
        {
            string uri = host + service + method + kb;
            Console.WriteLine("Calling " + uri + ".");
            return await Patch(uri, new_kb);
        }

        async static Task<Response> GetStatus(string operation)
        {
            string uri = host + service + operation;
            Console.WriteLine("Calling " + uri + ".");
            return await GetAsync(uri);
        }
        async static void UpdateKB(string kb, string new_kb)
        {
            var response = await PostUpdateKB(kb, new_kb);
            var operation = response.headers.GetValues("Location").First();
            Console.WriteLine(PrettyPrint(response.response));

            var done = false;
            while (true != done)
            {
                response = await GetStatus(operation);
                Console.WriteLine(PrettyPrint(response.response));

                var fields = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.response);

                String state = fields["operationState"];
                if (state.CompareTo("Running") == 0 || state.CompareTo("NotStarted") == 0)
                {
                    var wait = response.headers.GetValues("Retry-After").First();
                    Console.WriteLine("Waiting " + wait + " seconds...");
                    Thread.Sleep(Int32.Parse(wait) * 1000);
                }
                else
                {
                    Console.WriteLine("Press any key to continue.");
                    done = true;
                }
            }
        }
        // PUT: api/UpdateKB/5
        [HttpPut("{id}")]
        public void Put()
        {
            ReplaceKB();
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
