namespace CarrierEngine.Domain.Dtos
{
    public class TrackingRequestDto
    {
        public string Carrier { get; set; }
        public int BanyanLoadId { get; set; }
        public string BolNumber { get; set; }
        public string ProNumber { get; set; }
    }

    public class TrackingResponseDto
    {
        public int BanyanLoadId { get; set; }
        public string Message { get; set; }
    }
}
