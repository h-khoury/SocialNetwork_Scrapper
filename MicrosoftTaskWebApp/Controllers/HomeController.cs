using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MicrosoftTaskWebApp.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Data.SqlClient;


namespace MicrosoftTaskWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
             WebPage wp = new WebPage();
            wp.DBresult = DBConnection("SELECT nameSearch FROM[dbo].[SearchResults];");
            ViewBag.Message = wp;
            return View();
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
        public IActionResult SearchBing(string query)
        {
            if (query == null)
            {
                WebPage wp = new WebPage();
                wp.name = "Error! No Search Entry Was given";
                ViewBag.Message = wp;
                return View();

            }
            DBConnection("INSERT INTO [dbo].[SearchResults] (nameSearch) VALUES ('" + query + "');");
            var subscriptionKey = "1f924aba83fe4e64975eac8a997ca515";
            var customConfigId = "091fc9a5-5130-4417-9a1b-20dc46458976";
            var searchTerm = query;
            var url = "https://api.cognitive.microsoft.com/bingcustomsearch/v7.0/search?" +
            "q=" + searchTerm + "&" +
            "customconfig=" + customConfigId;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            var httpResponseMessage = client.GetAsync(url).Result;
            var responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
            BingCustomSearchResponse response = JsonConvert.DeserializeObject<BingCustomSearchResponse>(responseContent);
            // for (int i = 0; i < 5; i++){
          
            var webPage = response.webPages.value[0];
            //  Console.WriteLine("\n \n");
            // Console.WriteLine("Name: " + webPage.name);
            // Console.WriteLine("URL: " + webPage.url);
            // Console.WriteLine("Info: " + webPage.snippet);


            //}
            //   Console.WriteLine("\n \n Search History has been saved to DB");
            //   Console.ReadLine();
            webPage.DBresult = DBConnection("SELECT nameSearch FROM[dbo].[SearchResults];");
            ViewBag.Message = webPage;
            return View();
        }
        public static string[] DBConnection(string query)
        {
            string[] results= new string[20];
            int i = 0;
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

                builder.DataSource = "microsofttask20200109010627dbserver.database.windows.net";
                builder.UserID = "Hanakh";
                builder.Password = "1Q2w3e4r";
                builder.InitialCatalog = "WebJobsApp20200111120814_db";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {


                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append(query);
                    // sb.Append("INSERT INTO [dbo].[SearchResults] (nameSearch) VALUES ('" + val+"')");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine(reader.GetString(0));
                                results[i] = reader.GetString(0);
                                i++;
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            return results;
        }
    }
}
