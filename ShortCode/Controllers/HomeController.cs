using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShortCode.Models;

namespace ShortCode.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ValidateAccessToken validate;
    private readonly HttpClient httpClient;
    private readonly string API_KEY = "uJtei8zWaD5Yo5eEONdw92fvCYZYeQEN93ey8wEv";
    private readonly string api_base = "https://0eoz1atq1l.execute-api.us-east-1.amazonaws.com";
    private readonly string api_Stage = "beta";
    private readonly string endpoint;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
        validate = new ValidateAccessToken();
        httpClient = new HttpClient();
        endpoint = api_base + "/" + api_Stage;
    }

    public async Task<IActionResult> Index()
    {
        var authenticated = await HttpContext.AuthenticateAsync();

        if (authenticated.Succeeded)
        {
            Console.WriteLine("User authenticated");
        } else Console.WriteLine("User NOT authenticated");


        return View();
    }

    [Route("s/{shortcode}")]
    public async Task<IActionResult> RedirectToURL(string shortcode)
    {

        var url_list = new List<Url>();

        string api_resource = "/url/record";
        string requestUri = endpoint + api_resource;


        var data = new
        {
            shortcode = shortcode
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


                    var urlId = responseBody.body.items[0].url_id.S.ToString();
                    var userId = responseBody.body.items[0].user_id.S.ToString();
                    var url = responseBody.body.items[0].url.S.ToString();
                    var shortCode = responseBody.body.items[0].short_code.S.ToString();
                    var createdon = responseBody.body.items[0].createdon.S.ToString();
                    var views = responseBody.body.items[0].views.N;

                    var urlModel = new Url
                    {
                        url_id = urlId,
                        user_id = userId,
                        url = url,
                        shortcode = shortCode,
                        createdon = DateTime.Parse(createdon),
                        views = views
                    };


                    var fullURl = urlModel.url;

                    return Redirect(fullURl);

                }

            }

            return Error();
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

