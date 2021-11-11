namespace CarrierEngine.Domain.Dtos
{
    public class RatingRequestDto
    {
        public string Carrier { get; set; }
        public int BanyanLoadId { get; set; }
        public int Quantity { get; set; }
        public string Product { get; set; }
    }

    public class RatingResponseDto
    {
        public int BanyanLoadId { get; set; }
        public string QuoteNumber { get; set; } 
        public double QuoteAmount { get; set; }
    }
} 