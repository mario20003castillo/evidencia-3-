using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using menus.Models;
using System.Text.Json;
using System.Net.Http;
using menus.Services;


namespace menus.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient httpClient;

    private readonly IAuthService authService;




    public HomeController(HttpClient httpClient)
    {
        this.httpClient = httpClient;
        this.authService = new SimpleAuthService();
    }

    [HttpGet("Home/GetPokemon/{name}")]
    public async Task<IActionResult> GetPokemon(string name)
    {
        String url = $"https://pokeapi.co/api/v2/pokemon/{name}";
        HttpResponseMessage response =
             await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }
        String json = await response
                           .Content
                           .ReadAsStringAsync();
        JsonDocument doc = JsonDocument
                          .Parse(json);
        Pokemon pokemon = new Pokemon
        {
            Name = doc.RootElement.GetProperty("name").GetString(),
            ImageUrl = doc
                   .RootElement
                   .GetProperty("sprites")
                   .GetProperty("front_default")
                   .GetString(),
            Types = doc.RootElement.GetProperty("types")
           .EnumerateArray()
           .Select(t => t.GetProperty("type")
                       .GetProperty("name")
                       .GetString())
                  .ToList(),
            Abilities = doc.RootElement.GetProperty("abilities")
                .EnumerateArray()
                .Select(a => a.GetProperty("ability")
                              .GetProperty("name")
                              .GetString())
                 .ToList(),
            Weight = doc.RootElement.GetProperty("weight").GetInt32(),
        };

        return View(pokemon);

    }

    [HttpGet("Home/GetJoke/{lang}")]
    public async Task<IActionResult> GetJoke(string lang = "en")
    {
        // Validar que el idioma sea válido (solo 'en' o 'es')
        lang = lang.ToLower() == "es" ? "es" : "en";

        string url = $"https://v2.jokeapi.dev/joke/Programming?lang={lang}";

        HttpResponseMessage response = await httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        string json = await response.Content.ReadAsStringAsync();
        JsonDocument doc = JsonDocument.Parse(json);

        // La API puede devolver chistes de una o dos partes
        var joke = new Joke
        {
            Category = doc.RootElement.GetProperty("category").GetString(),
            Type = doc.RootElement.GetProperty("type").GetString(),
            Language = lang
        };

        if (joke.Type == "single")
        {
            joke.Content = doc.RootElement.GetProperty("joke").GetString();
        }
        else // two-part joke
        {
            joke.Setup = doc.RootElement.GetProperty("setup").GetString();
            joke.Delivery = doc.RootElement.GetProperty("delivery").GetString();
        }

        return View(joke);
    }


    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Photos()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    ///holaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }
    [HttpPost]
    public ActionResult Login(User user)
    {
        if (authService.Authenticate(user.Username, user.Password))
        {
            HttpContext.Session.SetString("UserAuthenticated", "true");
            return RedirectToAction("Welcome");
        }
        else
        {
            ViewBag.Error = "Usuario o contraseña incorrectos";
            return View();
        }
    }




    public IActionResult Welcome()
    {
        var isAuthenticated = HttpContext.Session.GetString("UserAuthenticated");
        if (isAuthenticated != "true")
        {
            return RedirectToAction("Index");
        }
        return View();
    }

// La vista de Welcome (Welcome.cshtml)






    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
