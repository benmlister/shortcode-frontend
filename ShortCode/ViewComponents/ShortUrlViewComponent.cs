using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShortCode.Models;

namespace ShortCode.ViewComponents
{
	public class ShortUrlViewComponent : ViewComponent
	{
        private readonly HttpClient httpClient;
        private readonly string API_KEY = "uJtei8zWaD5Yo5eEONdw92fvCYZYeQEN93ey8wEv";
        private readonly string api_base = "https://0eoz1atq1l.execute-api.us-east-1.amazonaws.com";
        private readonly string api_Stage = "beta";
        private readonly string endpoint;

        public ShortUrlViewComponent()
        {
            httpClient = new HttpClient();
            endpoint = api_base + "/" + api_Stage;
        }

        public async Task<IViewComponentResult> InvokeAsync(string user_id)
        {
            var url_list = new List<Url>();

            string api_resource = "/url/records";
            string requestUri = endpoint + api_resource;

            var baserequest = HttpContext.Request;
            string baseurl = $"{baserequest.Scheme}://{baserequest.Host}/s/";

            ViewBag.BaseUrl = baseurl;

            var data = new
            {
                user_id = user_id 
            };

            Console.WriteLine(data);

            var jsonUrlData = JsonConvert.SerializeObject(data);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(requestUri),
                Headers =
                {
                    {"x-api-key",API_KEY},
                },
                Content = new StringContent(jsonUrlData)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            using (var response = await httpClient.SendAsync(request))
            {
                Console.WriteLine(response);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Reach External API");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                    var responseBody = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    Console.WriteLine(responseBody);
                    if (responseBody.statusCode == 200)
                    {
                        // Get items and add to list
                        Console.WriteLine("Get items and add to list");
                        Console.WriteLine(responseBody.body.items);

                        foreach (var item in responseBody.body.items)
                        {
                            var urlId = item.url_id.S.ToString();
                            var userId = item.user_id.S.ToString();
                            var url = item.url.S.ToString();
                            var shortCode = item.short_code.S.ToString();
                            var createdon = item.createdon.S.ToString();
                            var views = item.views.N;

                            var urlModel = new Url
                            {
                                url_id = urlId,
                                user_id = userId,
                                url = url,
                                shortcode = shortCode,
                                createdon = DateTime.Parse(createdon),
                                views = views
                            };

                            url_list.Add(urlModel);
                        }

                        
                    }

                }

                return View(url_list);
            }
        }
    }

}


