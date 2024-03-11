using System;
using System.Collections.Generic;

namespace CarrierEngine.Domain.Dtos
{

    public class BookingParams
    {
        //todo: better name for this class

        public string ApiUsername { get; set; }
        public string ApiPassword { get; set; }

        public string WebUsername { get; set; }
        public string WebPassword { get; set; }

        public string Miscellaneous { get; set; }
        public string AccountNote { get; set; }

    }

    public class TrackingRequestDto
    {
        public BookingParams BookingParams { get; set; }
        public string CarrierClassName { get; set; }
        public int BanyanLoadId { get; set; }
        public string BolNumber { get; set; }
        public string ProNumber { get; set; }
        public string PickupNumber { get; set; }

        public Guid CorrelationId { get; set; } = Guid.NewGuid();

        public ICarrierConfig CarrierConfig { get; set; }
    }

    public class TrackingResponseDto
    {
        public int BanyanLoadId { get; set; }
        public DateTime? StatuesDateTime { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsSuccess { get; set; }

        public static TrackingResponseDto Failure(List<string> errors)
        {
            return new TrackingResponseDto
            { 
                Errors = errors,
                IsSuccess = false
            };
        }
         
        public static TrackingResponseDto Success(int banyanLoadId, DateTime? statuesDateTime)
        {
            return new TrackingResponseDto
            {
                BanyanLoadId = banyanLoadId,
                StatuesDateTime = statuesDateTime,
                IsSuccess = true
            };
        }
    }



    public interface ICarrierConfig
    {

    }


    public class BaseConfig
    {
        public bool TestMode { get; set; }

        public string BaseUrlProduction { get; set; }
        public string BaseUrlSandbox { get; set; }
        public string AuthenticationEndpoint { get; set; }
        public string RatingEndpoint { get; set; }
        public string BookingEndpoint { get; set; }
        public string ImagingEndpoint { get; set; }
        public string TrackingEndpoint { get; set; }
    }


    public class ChRobinsonConfig : BaseConfig, ICarrierConfig
    {
        public string PlatformId { get; set; }
        public string TokenAudience { get; set; }

        /*
         * 
         *  private const string BaseUrl = "https://api.navisphere.com/";
        private const string SandBoxBaseUrl = "https://sandbox-api.navisphere.com/";
        public const string TokenAudience = "https://inavisphere.chrobinson.com";
        public const string PlatformId = "69D9CAB7-6BD0-4EDC-BF14-649FA656AEEF";
         */
    }


    class BanyanTrackingCodes
    {
        public int CodeId { get; set; }

        public string Code { get; set;}
        
        public string Message { get; set; }

    }

}
