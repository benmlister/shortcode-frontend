using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;
using static System.Net.WebRequestMethods;

namespace ShortCode.Controllers
{
	public class AppController : Controller
    {


        private readonly ILogger<AppController> _logger;
        private readonly HttpClient httpClient;
        private readonly string API_KEY = "uJtei8zWaD5Yo5eEONdw92fvCYZYeQEN93ey8wEv";
        private readonly string api_base = "https://0eoz1atq1l.execute-api.us-east-1.amazonaws.com";
        private readonly string api_Stage = "beta";
        private readonly string endpoint;


        public AppController(ILogger<AppController> logger)
        {
            _logger = logger;
            httpClient = new HttpClient();
            endpoint = api_base + "/" + api_Stage;
        }

        public async Task<IActionResult> Index()
        {

            var authenticated = await HttpContext.AuthenticateAsync();

            if (!authenticated.Succeeded)
            {
                return RedirectToAction("Login", "Authentication");
            }

            foreach (var item in User.Claims)
            {
                Console.WriteLine("Type: {0}, Value: {1}", item.Type, item.Value);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string url)
        {
            string api_resource = "/url";
            string requestUri = endpoint + api_resource;

            var user_id = User.Claims.FirstOrDefault(c => c.Type == "UUID").Value;


            var urlData = new
            {
                user_id,
                url
            };

            var jsonUrlData = Newtonsoft.Json.JsonConvert.SerializeObject(urlData);


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
                    var responseBody = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

                    if (responseBody.statusCode == 200)
                    {

                        return RedirectToAction("Index", "App");
                    }
                    else
                    {
                        return View("Index", responseBody.body.message.toString());
                    }
                }
                else
                {
                    // The login failed, you can display an error message or handle the failure accordingly
                    // You can also parse the response content to extract the error message if applicable
                    var errorMessage = "Creation failed";
                    Console.WriteLine(errorMessage);
                    return View("Index", errorMessage);
                }
            }


            return View();
        }


        public async Task<String> Generate(string url)
        {
            string api_resource = "/caption";
            string requestUri = endpoint + api_resource;

            


            var data = new
            {
                
                url = url
            };

            var jsonUrlData = Newtonsoft.Json.JsonConvert.SerializeObject(data);


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
                    var responseBody = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

                    if (responseBody.statusCode == 200)
                    {
                        Console.WriteLine(responseBody.body.caption);

                        return responseBody.body.caption;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    // The login failed, you can display an error message or handle the failure accordingly
                    // You can also parse the response content to extract the error message if applicable
                    var errorMessage = "Creation failed";
                    Console.WriteLine(errorMessage);
                    return "";
                }
            }
        }

        public async Task<String> qrcode(string shortcode)
        {
            string api_resource = "/qrcode";
            string requestUri = endpoint + api_resource;

            var baserequest = HttpContext.Request;
            string baseurl = $"{baserequest.Scheme}://{baserequest.Host}/s/";

            Console.WriteLine(baseurl);

            var data = new
            {

                shortcode = shortcode,
                baseurl = baseurl
            };

            var jsonUrlData = Newtonsoft.Json.JsonConvert.SerializeObject(data);


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
                    var responseBody = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(responseContent);

                    if (responseBody.statusCode == 200)
                    {
                        Console.WriteLine(responseBody.body.qr_code);

                        return responseBody.body.qr_code;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    // The login failed, you can display an error message or handle the failure accordingly
                    // You can also parse the response content to extract the error message if applicable
                    var errorMessage = "Creation failed";
                    Console.WriteLine(errorMessage);
                    return "";
                }
            }
        }

    }
}

