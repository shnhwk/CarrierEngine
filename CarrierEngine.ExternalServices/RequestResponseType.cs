namespace CarrierEngine.ExternalServices;

public enum RequestResponseType
{
    RateRequest = 6,
    RateResponse = 7,

    PickupRequest = 8,
    PickupResponse = 9,

    TrackingRequest = 10,
    TrackingResponse = 11,

    FailedRateRequest = 18,
    FailedRateResponse = 19,



    PostLoadRequest = 35,
    PostLoadResponse = 36,
    CancelPostLoadRequest = 37,
    CancelPostLoadResponse = 38,
    NegotiationRequest = 39,
    NegotiationResponse = 40,
    ConfirmationRequest = 41,
    ConfirmationResponse = 42,
    LoadTrackRequest = 43,
    LoadTrackResponse = 44
}