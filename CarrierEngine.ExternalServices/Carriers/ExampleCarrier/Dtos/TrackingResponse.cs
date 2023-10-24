using System;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier.Dtos
{
    internal sealed class TrackingResponse
    {
        public string Bol { get; set; }
        public DateTime Date { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
 