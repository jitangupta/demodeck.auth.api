using System.Net;

namespace DemoDeck.Auth.Api.Tests.Helpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseContent;
    private readonly Exception? _exception;

    public MockHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
    {
        _statusCode = statusCode;
        _responseContent = responseContent;
    }

    public MockHttpMessageHandler(Exception exception)
    {
        _exception = exception;
        _statusCode = HttpStatusCode.InternalServerError;
        _responseContent = string.Empty;
    }

    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;

        if (_exception != null)
            throw _exception;

        return Task.FromResult(new HttpResponseMessage
        {
            StatusCode = _statusCode,
            Content = new StringContent(_responseContent, System.Text.Encoding.UTF8, "application/json")
        });
    }
}
