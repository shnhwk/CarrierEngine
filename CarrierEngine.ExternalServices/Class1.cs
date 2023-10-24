//using System;
//using System.Threading.Tasks;

//namespace CarrierEngine.ExternalServices;

///// <summary>
///// The request response logger.
///// </summary>
//public class RequestResponseLogger : IRequestResponseLogger
//{
//    private readonly IWebServiceRequestsRepository _webServiceRequestsRepository;

//    /// <summary>
//    /// Instantiates an instance of <see cref="RequestResponseLogger"/>.
//    /// </summary>
//    /// <param name="webServiceRequestsRepository">The <see cref="IWebServiceRequestsRepository"/>used to log requests and responses.</param>
//    public RequestResponseLogger(IWebServiceRequestsRepository webServiceRequestsRepository)
//    {
//        _webServiceRequestsRepository = webServiceRequestsRepository;
//    }

//    /// <summary>
//    /// Logs the <see cref="WebServiceRequest"/> with the specified details.
//    /// </summary>
//    public async Task Log(string url, int statusCode, string request, string response, string? ipAddress, int? userId, string? requestId, string? openIdClient,
//        DateTime requestDateTimeUtc)
//    {
//        var wsr = new WebServiceRequest
//        {
//            RequestDateUtc = requestDateTimeUtc,
//            IpAddress = ipAddress,
//            OpenIdClient = openIdClient,
//            UserId = userId,
//            RequestUrl = url,
//            RequestBody = request,
//            ResponseBody = response,
//            RequestId = requestId,
//            StatusCode = statusCode
//        };

//        _webServiceRequestsRepository.Create(wsr);
//        await _webServiceRequestsRepository.SaveChangesAsync();
//    }
//}

  

///// <summary>
///// Interface IRequestResponseLogger
///// </summary>
//public interface IRequestResponseLogger
//{
//    /// <summary>
//    /// Logs the specified URL.
//    /// </summary>
//    /// <param name="url">The URL.</param>
//    /// <param name="statusCode">The status code.</param>
//    /// <param name="request">The request.</param>
//    /// <param name="response">The response.</param>
//    /// <param name="ipAddress">The ip address.</param>
//    /// <param name="userId">The user identifier.</param>
//    /// <param name="findFirstValue">The find first value.</param>
//    /// <param name="openIdClient">The open identifier client.</param>
//    /// <param name="requestDateTimeUtc">The request date time UTC.</param>
//    /// <returns>Task.</returns>
//    Task Log(string url, int statusCode, string request, string response, string? ipAddress, int? userId, string? findFirstValue,
//        string? openIdClient, DateTime requestDateTimeUtc);
//}