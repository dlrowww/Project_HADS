namespace Search.API.Models
{
    public class SearchRequest
    {
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime? DepartureDate { get; set; } // 可选
    }
}

