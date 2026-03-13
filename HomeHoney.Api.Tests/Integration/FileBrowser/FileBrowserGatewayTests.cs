using System.Net;
using System.Text;
using HomeHoney.Api.Infrastructure.FileBrowser;

namespace HomeHoney.Api.Tests.Integration.FileBrowser;

public sealed class FileBrowserGatewayTests
{
    [Fact]
    public async Task Upload_uses_login_token_when_json_auth_is_configured()
    {
        var handler = new RecordingHandler(
        [
            CreateResponse(HttpStatusCode.OK, "jwt-token"),
            CreateResponse(HttpStatusCode.OK, "uploaded")
        ]);
        var gateway = new FileBrowserGateway(new StubHttpClientFactory(handler));

        await using var content = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
        var result = await gateway.UploadFileAsync(new FileBrowserConnectionOptions
        {
            BaseUrl = "http://localhost:8999",
            ApiPath = "/api",
            FileServiceUsername = "demo",
            FileServicePassword = "secret",
        }, "insurance/123", "policy.pdf", content, "application/pdf");

        Assert.True(result.IsSuccess);
        Assert.Collection(handler.Requests,
            login =>
            {
                Assert.Equal(HttpMethod.Post, login.Method);
                Assert.Equal("http://localhost:8999/api/login", login.RequestUri?.ToString());
            },
            upload =>
            {
                Assert.Equal(HttpMethod.Post, upload.Method);
                Assert.Equal("jwt-token", upload.Headers.GetValues("X-Auth").Single());
                Assert.Equal("http://localhost:8999/api/resources/insurance/123/policy.pdf", upload.RequestUri?.ToString());
                Assert.Equal("application/pdf", upload.Content?.Headers.ContentType?.MediaType);
            });
    }

    [Fact]
    public async Task ValidateConnection_returns_auth_error_when_login_is_rejected()
    {
        var handler = new RecordingHandler([CreateResponse(HttpStatusCode.Forbidden, string.Empty)]);
        var gateway = new FileBrowserGateway(new StubHttpClientFactory(handler));

        var result = await gateway.ValidateConnectionAsync(new FileBrowserConnectionOptions
        {
            BaseUrl = "http://localhost:8999",
            ApiPath = "/api",
            FileServiceUsername = "demo",
            FileServicePassword = "bad-secret",
        });

        Assert.False(result.IsSuccess);
        Assert.Contains("FileBrowser 登录失败", result.Message);
        Assert.Contains("拒绝了当前认证信息", result.Message);
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StubHttpClientFactory(HttpMessageHandler handler)
        {
            _client = new HttpClient(handler, disposeHandler: false);
        }

        public HttpClient CreateClient(string name) => _client;
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string body)
        => new(statusCode)
        {
            Content = new StringContent(body),
        };

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;

        public RecordingHandler(IEnumerable<HttpResponseMessage> responses)
        {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(await CloneAsync(request, cancellationToken));
            return _responses.Dequeue();
        }

        private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content is not null)
            {
                var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
                var content = new ByteArrayContent(bytes);
                foreach (var header in request.Content.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                clone.Content = content;
            }

            return clone;
        }
    }
}