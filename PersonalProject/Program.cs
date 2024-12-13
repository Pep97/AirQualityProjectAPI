using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.IO;
using System.Globalization;

namespace PersonalProject
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private const string apiKey = ""; // Insert your API key here
        private static AirQualityResponse? storedCityData = null;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Welcome to the program. \n\nHere you can check for air quality in the mayor city of world and download a csv file with the data.");

        MainMenu:
            while (true)
            {
                Console.WriteLine("\nWhat would you like to do?");
                Console.WriteLine("\n[1] -> Look for air quality in your city");
                Console.WriteLine("[2] -> Exit");

                int choice = Convert.ToInt32(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        while (true)
                        {
                            Console.WriteLine("\nWhat country is the city located in?");
                            string? countryInput = Console.ReadLine();

                            // This code converts the countryInput to lowercase and then to title case, ensuring that the first letter of each word is capitalized.
                            if (!string.IsNullOrEmpty(countryInput))
                            {
                                countryInput = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(countryInput.ToLower());
                            }

                            if (string.IsNullOrEmpty(countryInput))
                            {
                                Console.WriteLine("\nInvalid input. Please try again.");
                                continue;
                            }

                            bool isCountryAvailable = await CheckAvailableCountry(countryInput);

                            if (!isCountryAvailable)
                            {
                                Console.WriteLine("\n[1] -> Search another country?");
                                Console.WriteLine("[2] -> Go back to the main menu.");
                                int subChoice = Convert.ToInt32(Console.ReadLine());

                                if (subChoice == 2)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                await PrintStates(countryInput);
                                Console.WriteLine("\n\nIn what state is located your city?");

                                string? stateInput = Console.ReadLine();

                                if (!string.IsNullOrEmpty(stateInput))
                                {
                                    stateInput = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(stateInput.ToLower());
                                }

                                if (string.IsNullOrEmpty(stateInput))
                                {
                                    Console.WriteLine("\nInvalid input. Please try again.");
                                    continue;
                                }

                                bool isStateAvailable = await CheckAvailableState(countryInput, stateInput);

                                if (!isStateAvailable)
                                {
                                    Console.WriteLine("\n[1] -> Search again?");
                                    Console.WriteLine("[2] -> Go back to the main menu.");
                                    int subChoice = Convert.ToInt32(Console.ReadLine());

                                    if (subChoice == 2)
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    await PrintCities(countryInput, stateInput);
                                    Console.WriteLine("\n\nInput your city: ");

                                    string? cityInput = Console.ReadLine();

                                    if (!string.IsNullOrEmpty(cityInput))
                                    {
                                        cityInput = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cityInput.ToLower());
                                    }

                                    if (string.IsNullOrEmpty(cityInput))
                                    {
                                        Console.WriteLine("\nInvalid input. Please try again.");
                                        continue;
                                    }

                                    bool displayCityData = await CheckAndDisplayCityData(countryInput, stateInput, cityInput);

                                    if (!displayCityData)
                                    {
                                        Console.WriteLine("\n[1] -> Search another city");
                                        Console.WriteLine("[2] -> Go back to the main menu.");
                                        int subChoice = Convert.ToInt32(Console.ReadLine());

                                        if (subChoice == 2)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("\n\nHow would you like to proceed?");
                                        Console.WriteLine($"\n[1] -> Download {cityInput} data into a csv file.");
                                        Console.WriteLine("[2] -> Search another city");
                                        Console.WriteLine("[3] -> Go back to the main menu.");
                                        int subChoice = Convert.ToInt32(Console.ReadLine());

                                        switch (subChoice)
                                        {
                                            case 1:
                                                Console.WriteLine("\nDownloading data...");
                                                await DownloadCityDataToCsv(cityInput);
                                                goto MainMenu;
                                            case 2:
                                                break;
                                            case 3:
                                                goto MainMenu;
                                            default:
                                                Console.WriteLine("\nInvalid choice. Please try again.");
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case 2:
                        Console.WriteLine("\nGoodbye!");
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("\nInvalid choice. Please try again.");
                        break;
                }
            }
        }

        public static async Task<bool> CheckAvailableCountry(string countryInput)
        {
            string apiUrl = $"http://api.airvisual.com/v2/countries?key={apiKey}";

            try
            {
                var response = await client.GetStringAsync(apiUrl);
                var countryData = JsonSerializer.Deserialize<CountryResponse>(response);

                if (countryData == null || countryData.Data == null)
                {
                    Console.WriteLine("\nFailed to retrieve country data.");
                    return false;
                }

                foreach (var country in countryData.Data)
                {
                    if (country.Country.ToUpper() == countryInput.ToUpper())
                    {
                        return true;
                    }
                }

                Console.WriteLine("\nCountry not available.");
                return false;

            }
            catch (HttpRequestException ex) when (ex.StatusCode == (System.Net.HttpStatusCode)429)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                Console.WriteLine("\nThis error is caused from the API since it can't manage too many calls in a short period of time. \n You can try again now, or the best option would be to wait and try again in 10 to 20 seconds.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }

            return false;
        }

        public static async Task PrintStates(string countryInput)
        {
            string apiUrl = $"http://api.airvisual.com/v2/states?country={countryInput}&key={apiKey}";

            try
            {
                var response = await client.GetStringAsync(apiUrl);
                var stateData = JsonSerializer.Deserialize<StateResponse>(response);

                if (stateData == null || stateData.Data == null)
                {
                    Console.WriteLine("Failed to retrieve state data.");
                    return;
                }

                Console.WriteLine("\nAvailable states:\n");
                for (int i = 0; i < stateData.Data.Count; i++)
                {
                    Console.Write(stateData.Data[i].State);
                    if (i < stateData.Data.Count - 1)
                    {
                        Console.Write(" | ");
                    }
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == (System.Net.HttpStatusCode)429)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                Console.WriteLine("\nThis error is caused from the API since it can't manage too many calls in a short period of time. \n You can try again now, or the best option would be to wait and try again in 10 to 20 seconds.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static async Task<bool> CheckAvailableState(string countryInput, string stateInput)
        {
            string apiUrl = $"http://api.airvisual.com/v2/states?country={countryInput}&key={apiKey}";

            try
            {
                var response = await client.GetStringAsync(apiUrl);
                var stateData = JsonSerializer.Deserialize<StateResponse>(response);

                if (stateData == null || stateData.Data == null)
                {
                    Console.WriteLine("Failed to retrieve state data.");
                    return false;
                }

                foreach (var state in stateData.Data)
                {
                    if (state.State.ToUpper() == stateInput.ToUpper())
                    {
                        return true;
                    }
                }

                Console.WriteLine("\nState not available.");
                return false;

            }
            catch (HttpRequestException ex) when (ex.StatusCode == (System.Net.HttpStatusCode)429)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                Console.WriteLine("\nThis error is caused from the API since it can't manage too many calls in a short period of time. \n You can try again now, or the best option would be to wait and try again in 10 to 20 seconds.");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == (System.Net.HttpStatusCode)400)
            {
                Console.WriteLine("\nState not available.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("check this");
                Console.WriteLine($"An error occurred: {ex.Message}");

            }

            return false;
        }

        public static async Task PrintCities(string countryInput, string stateInput)
        {
            string apiUrl = $"http://api.airvisual.com/v2/cities?state={stateInput}&country={countryInput}&key={apiKey}";

            try
            {
                var response = await client.GetStringAsync(apiUrl);
                var cityData = JsonSerializer.Deserialize<CityResponse>(response);

                if (cityData == null || cityData.Data == null)
                {
                    Console.WriteLine("Failed to retrieve city data.");
                    return;
                }

                Console.WriteLine("\nAvailable cities:\n");
                for (int i = 0; i < cityData.Data.Count; i++)
                {
                    Console.Write(cityData.Data[i].City);
                    if (i < cityData.Data.Count - 1)
                    {
                        Console.Write(" | ");
                    }
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == (System.Net.HttpStatusCode)429)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                Console.WriteLine("\nThis error is caused from the API since it can't manage too many calls in a short period of time. \n You can try again now, or the best option would be to wait and try again in 10 to 20 seconds.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public static async Task<bool> CheckAndDisplayCityData(string countryInput, string stateInput, string cityInput)
        {
            string apiUrl = $"http://api.airvisual.com/v2/city?city={cityInput}&state={stateInput}&country={countryInput}&key={apiKey}";

            try
            {
                var response = await client.GetStringAsync(apiUrl);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var airQualityData = JsonSerializer.Deserialize<ApiResponse>(response, options);

                if (airQualityData == null || airQualityData.Data == null)
                {
                    Console.WriteLine("\nDeserialization failed: airQualityData or airQualityData.Data is null.");
                    return false;
                }

                var data = airQualityData.Data;

                if (data.Location == null || data.Current == null || data.Current.Pollution == null || data.Current.Weather == null)
                {
                    Console.WriteLine("\nDeserialization failed: some properties are null.");
                    return false;
                }

                storedCityData = data;

                Console.WriteLine("\nDATA");
                Console.WriteLine($"\nCity: {data.City}");
                Console.WriteLine($"State: {data.State}");
                Console.WriteLine($"Country: {data.Country}");
                Console.WriteLine($"Location: {data.Location.Type} ({data.Location.Coordinates[0]}, {data.Location.Coordinates[1]})");
                Console.WriteLine($"Pollution:");
                Console.WriteLine($"  Timestamp: {data.Current.Pollution.Ts}");
                Console.WriteLine($"  AQI US: {data.Current.Pollution.Aqius}");
                Console.WriteLine($"  Main Pollutant US: {data.Current.Pollution.Mainus}");
                Console.WriteLine($"  AQI CN: {data.Current.Pollution.Aqicn}");
                Console.WriteLine($"  Main Pollutant CN: {data.Current.Pollution.Maincn}");
                Console.WriteLine($"Weather:");
                Console.WriteLine($"  Timestamp: {data.Current.Weather.Ts}");
                Console.WriteLine($"  Temperature: {data.Current.Weather.Tp}°C");
                Console.WriteLine($"  Pressure: {data.Current.Weather.Pr} hPa");
                Console.WriteLine($"  Humidity: {data.Current.Weather.Hu}%");
                Console.WriteLine($"  Wind Speed: {data.Current.Weather.Ws} m/s");
                Console.WriteLine($"  Wind Direction: {data.Current.Weather.Wd}°");
                Console.WriteLine($"  Icon: {data.Current.Weather.Ic}");

                return true;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == (System.Net.HttpStatusCode)429)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                Console.WriteLine("\nThis error is caused from the API since it can't manage too many calls in a short period of time. \n You can try again now, or the best option would be to wait and try again in 10 to 20 seconds.");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == (System.Net.HttpStatusCode)400)
            {
                Console.WriteLine("\nState not available.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }

            return false;
        }


        public static async Task DownloadCityDataToCsv(string cityInput)
        {
            try
            {
                if (storedCityData == null)
                {
                    Console.WriteLine("\nNo city data available to download.");
                    return;
                }

                string projectDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? throw new InvalidOperationException("Project directory not found.");
                string csvFilePath = Path.Combine(projectDirectory, $"{cityInput}_data.csv");

                string csvData = "City,State,Country,Location Type,Location Coordinates,Pollution Timestamp,AQI US,Main Pollutant US,AQI CN,Main Pollutant CN,Weather Timestamp,Temperature,Pressure,Humidity,Wind Speed,Wind Direction,Icon\n";
                csvData += $"{storedCityData.City},{storedCityData.State},{storedCityData.Country},{storedCityData.Location.Type},{storedCityData.Location.Coordinates[0]} {storedCityData.Location.Coordinates[1]},{storedCityData.Current.Pollution.Ts},{storedCityData.Current.Pollution.Aqius},{storedCityData.Current.Pollution.Mainus},{storedCityData.Current.Pollution.Aqicn},{storedCityData.Current.Pollution.Maincn},{storedCityData.Current.Weather.Ts},{storedCityData.Current.Weather.Tp},{storedCityData.Current.Weather.Pr},{storedCityData.Current.Weather.Hu},{storedCityData.Current.Weather.Ws},{storedCityData.Current.Weather.Wd},{storedCityData.Current.Weather.Ic}\n";

                await File.WriteAllTextAsync(csvFilePath, csvData);

                Console.WriteLine("\nData downloaded successfully, look inside your project.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
            }
        }
    }

    public class ApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public AirQualityResponse Data { get; set; } = new AirQualityResponse();
    }

    public class CountryResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<CountryT>? Data { get; set; }
    }

    public class CountryT
    {
        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
    }

    public class StateResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<StateT>? Data { get; set; }
    }

    public class StateT
    {
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;
    }

    public class CityResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<CityT>? Data { get; set; }
    }

    public class CityT
    {
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;
    }

    public class AirQualityResponse
    {
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public Location Location { get; set; } = new Location();

        [JsonPropertyName("current")]
        public Current Current { get; set; } = new Current();
    }

    public class Location
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("coordinates")]
        public List<double> Coordinates { get; set; } = new List<double>();
    }

    public class Current
    {
        [JsonPropertyName("pollution")]
        public Pollution Pollution { get; set; } = new Pollution();

        [JsonPropertyName("weather")]
        public Weather Weather { get; set; } = new Weather();
    }

    public class Pollution
    {
        [JsonPropertyName("ts")]
        public string Ts { get; set; } = string.Empty;

        [JsonPropertyName("aqius")]
        public int Aqius { get; set; }

        [JsonPropertyName("mainus")]
        public string Mainus { get; set; } = string.Empty;

        [JsonPropertyName("aqicn")]
        public int Aqicn { get; set; }

        [JsonPropertyName("maincn")]
        public string Maincn { get; set; } = string.Empty;
    }

    public class Weather
    {
        [JsonPropertyName("ts")]
        public string Ts { get; set; } = string.Empty;

        [JsonPropertyName("tp")]
        public int Tp { get; set; }

        [JsonPropertyName("pr")]
        public int Pr { get; set; }

        [JsonPropertyName("hu")]
        public int Hu { get; set; }

        [JsonPropertyName("ws")]
        public double Ws { get; set; }

        [JsonPropertyName("wd")]
        public int Wd { get; set; }

        [JsonPropertyName("ic")]
        public string Ic { get; set; } = string.Empty;
    }
}

