using System;
using System.Collections.Generic;

namespace CarrierEngine.Domain.Dtos
{

    public class SubscriptionDetails
    { 
        public string ApiUsername { get; set; }
        public string ApiPassword { get; set; }

        public string WebUsername { get; set; }
        public string WebPassword { get; set; }

        public string AccountId { get; set; }
        public string AccountNote { get; set; }

        public string Miscellaneous { get; set; }

    }

    public class TrackingRequestDto
    {
        public string CarrierClassName { get; set; }
        public int BanyanLoadId { get; set; }
        public string BolNumber { get; set; }
        public string ProNumber { get; set; }
        public string PickupNumber { get; set; }


        public SubscriptionDetails SubscriptionDetails { get; set; }

        public Guid CorrelationId { get; set; } = Guid.NewGuid();
    }

    public class TrackingResponseDto
    {
        public int BanyanLoadId { get; set; }
        public DateTime? StatuesDateTime { get; set; }
        public string Code { get; set; }
        public string Message { get; set; } 
        public string ErrorMessage { get; set; }  
        public bool IsSuccess { get; set; }

        public static TrackingResponseDto Failure(string errorMessage)
        {
            return new TrackingResponseDto
            {
                ErrorMessage = errorMessage,
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
