namespace HNGBackendOne.Models
{
    public class IpGeolocationResponse
    {
        public string hostname { get; set; }
        public string city { get; set; }
        public string continent_code { get; set; }
        public string continent_name { get; set; }
        public string country_code2 { get; set; }
        public string country_code3 { get; set; }
        public string country_name { get; set; }
        public string country_capital { get; set; }
        public string state_prov { get; set; }
        public string district { get; set; }
        public string isp { get; set; }

    }

    public class WeatherApiResponse
    {
        public Main Main { get; set; }
    }

    public class Main
    {
        public double Temp { get; set; }
    }
}
