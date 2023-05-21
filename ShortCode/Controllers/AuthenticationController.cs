using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;

namespace ShortCode.Controllers
{
	public class AuthenticationController : Controller
	{
        private readonly ILogger<AuthenticationController> _logger;
        private readonly HttpClient httpClient;
        private readonly string API_KEY = "uJtei8zWaD5Yo5eEONdw92fvCYZYeQEN93ey8wEv";
        private readonly string api_base= "https://0eoz1atq1l.execute-api.us-east-1.amazonaws.com";
        private readonly string api_Stage = "beta";
        private readonly string endpoint;
        TokenValidator tv;

        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            _logger = logger;
            httpClient = new HttpClient();
            endpoint = api_base + "/" + api_Stage;
            tv = new TokenValidator();

        }

        public async Task<IActionResult> Login()
        {
            var authenticated = await HttpContext.AuthenticateAsync();

            if (authenticated.Succeeded)
            {
                return RedirectToAction("Index", "App");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            string api_resource = "/login";
            string requestUri = endpoint + api_resource;
            

            var loginData = new
            {
                username,
                password
            };

            var jsonLoginData = Newtonsoft.Json.JsonConvert.SerializeObject(loginData);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(requestUri),
                Headers =
                {
                    {"x-api-key",API_KEY},
                },
                Content = new StringContent(jsonLoginData)
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

                        
                        string accessToken = responseBody.body.idToken;
                        Console.WriteLine(accessToken);
                        var userid = await tv.ValidateAndDecodeIdTokenAsync(accessToken);




                        foreach (var item in userid.Claims)
                        {
                            Console.WriteLine("Type {0}, Value {1}, ValueType {2}, Properties {3}", item.Type, item.Value, item.ValueType, item.Properties);
                        }

                        var userobject = new
                        {
                            Email = userid.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value,
                            Username = userid.FindFirst("cognito:username").Value,
                            UUID = userid.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value
                        };

                        Console.WriteLine("Email: {0}, Username: {1}, Sub: {2}", userobject.Email, userobject.Username, userobject.UUID);


                        // Create session cookie
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Email, userobject.Email),
                            new Claim("username", userobject.Username),
                            new Claim("UUID", userobject.UUID),
                            new Claim(ClaimTypes.Role, "Administrator"),
                        };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                        };



                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);



                        


                        Console.WriteLine("Successfully logged in");
                        return RedirectToAction("Index", "App");
                    } else
                    {
                        return View("Login", responseBody.body.message);
                    }
                }
                else
                {
                    // The login failed, you can display an error message or handle the failure accordingly
                    // You can also parse the response content to extract the error message if applicable
                    var errorMessage = "Login failed";
                    Console.WriteLine(errorMessage);
                    return View("Login", errorMessage);
                }
            }           
        }


        public async Task<IActionResult> Register()
        {
            var authenticated = await HttpContext.AuthenticateAsync();

            if (authenticated.Succeeded)
            {
                return RedirectToAction("Index", "App");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {

            string api_resource = "/register";
            string requestUri = endpoint + api_resource;


            var registerData = new
            {
                username,
                email,
                password
            };

            var jsonLoginData = Newtonsoft.Json.JsonConvert.SerializeObject(registerData);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(requestUri),
                Headers =
                {
                    {"x-api-key",API_KEY},
                },
                Content = new StringContent(jsonLoginData)
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

                        return RedirectToAction("Login", "Authentication");
                    }
                    else
                    {
                        return View("Register", responseBody.body.message);
                    }
                }
                else
                {
                    // The login failed, you can display an error message or handle the failure accordingly
                    // You can also parse the response content to extract the error message if applicable
                    var errorMessage = "Registration failed";
                    Console.WriteLine(errorMessage);
                    return View("Login", errorMessage);
                }
            }
        }

        public async Task<IActionResult> Logout()
        {
            // Delete session cookie
            await HttpContext.SignOutAsync(
             CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Message"] = ViewBag.Message;
            TempData["MessageType"] = ViewBag.MessageType;
            return Redirect("/");
        }
    }
}

