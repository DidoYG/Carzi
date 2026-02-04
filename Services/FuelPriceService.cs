using System.Net.Http.Json;
using System.Text.Json.Serialization;

public class FuelPriceService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    private readonly string[] _fuels =
    {
        "gasoline",
        "diesel",
        "lpg"
    };

    // Constructor with HttpClient and IConfiguration parameters
    public FuelPriceService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    // Method to get fuel prices asynchronously
    public async Task<List<FuelDto>> GetFuelPricesAsync()
    {
        var apiKey = _config["Fuelo:ApiKey"];
        var result = new List<FuelDto>();

        foreach (var fuel in _fuels)
        {
            var url =
                $"https://fuelo.net/api/price?key={apiKey}&fuel={fuel}";

            FueloFuelResponse? response;

            try
            {
                response = await _http.GetFromJsonAsync<FueloFuelResponse>(url);
            }
            catch
            {
                continue;
            }

            if (response == null)
                continue;

            var price = response.Price;

            result.Add(new FuelDto
            {
                Name = RenameFuels(response.Fuel),
                Price = response.Price
            });
        }

        return result;
    }

    // Helper method to rename fuel API values to more user-friendly names
    private static string RenameFuels(string fuel)
    {
        return fuel switch
        {
            "gasoline" => "Petrol",
            "diesel" => "Diesel",
            "lpg" => "LPG",
            _ => fuel
        };
    }
}

// DTO class for fuel data transfer
public class FuelDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Class to represent the response from the Fuelo API
public class FueloFuelResponse
{
    [JsonPropertyName("fuel")]
    public string Fuel { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
}