using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices
{
    public class TimingHandler : DelegatingHandler
    {
        private readonly ILogger<TimingHandler> _logger;

        public TimingHandler(ILogger<TimingHandler> logger)
        {
            _logger = logger;
        }


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        { 
            HttpResponseMessage response = null;
            var sw = Stopwatch.StartNew();

            try
            {
               
                response = await base.SendAsync(request, cancellationToken);
                sw.Stop();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                throw;
            }
            finally
            {
                _logger.LogInformation($"Finished request in {sw.ElapsedMilliseconds}ms");

                var scope = new Dictionary<string, object>();

                scope.TryAdd("request_headers", request);
                if (request?.Content != null)
                {
                    scope.Add("request_body", await request.Content.ReadAsStringAsync(cancellationToken));
                }
                scope.TryAdd("response_headers", response);
                if (response?.Content != null)
                {
                    scope.Add("response_body", await response.Content.ReadAsStringAsync(cancellationToken));
                }
                using (_logger.BeginScope(scope))
                {
                    _logger.LogInformation("[TRACE] request/response");
                }
            }

            return response;
        }
    }
}
