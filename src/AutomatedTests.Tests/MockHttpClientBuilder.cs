using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace AutomatedTests.Tests;

public class MockHttpClientBuilderTests
{
    [Test]
    public async Task No_unused_http_calls()
    {
        // Arrange
        var clientOneBuilder = MockHttpClientBuilder.Create();
        var clientTwoBuilder = MockHttpClientBuilder.Create();

        var httpClientFactory = clientOneBuilder
            .SetupRequest(HttpMethod.Get).WithJsonResponse("{'name':'zero'}")
            .SetupRequest(HttpMethod.Get).WithJsonResponse(HttpStatusCode.Accepted, "{'name':'one'}")
            .SetupRequest(HttpMethod.Get, "/Three").WithJsonResponse(HttpStatusCode.Created, "{'name':'three'}")
            .ToIHttpClientFactory();

        clientTwoBuilder
            .SetupRequest(HttpMethod.Post).WithJsonResponse("{'name':'two'}")
            .ToClientForExistingIHttpClientFactory(httpClientFactory, "Named");

        var sut = new Service(httpClientFactory);

        // Act
        await sut.MakeRequest();

        // Assert
        clientOneBuilder.AssertNoUnusedCalls();
        clientTwoBuilder.AssertNoUnusedCalls();
    }

    class Service
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public Service(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task MakeRequest()
        {
            var clientOne = _httpClientFactory.CreateClient();
            var clientTwo = _httpClientFactory.CreateClient("Named");

            await clientOne.GetStringAsync("/One");
            var responseOne = await clientOne.GetStringAsync("/One");
            var responseTwo = await clientTwo.PostAsync("/Two", null);
            var responseTwoBody = await responseTwo.Content.ReadAsStringAsync();
            var responseThree = await clientOne.GetStringAsync("/Three");
        }
    }
}



public class MockHttpClientBuilder
{
    public static MockHttpClientBuilder Create() => new();

    private readonly MockHttpClientHandler _handler = new();
    private readonly HttpClient _httpClient;
    private bool _isBuilt;

    private MockHttpClientBuilder()
    {
        _httpClient = new(_handler)
        {
            BaseAddress = new Uri($"http://{IPAddress.Loopback}"),
        };
    }

    public void AssertNoUnusedCalls() =>
        CollectionAssert.IsEmpty(
            _handler.Responses,
            "There were responses set up on the MockHttpClient that were never requested");

    public MockHttpClientRequestBuilder SetupRequest() =>
        new(this, _GetRequestPredicate());

    public MockHttpClientRequestBuilder SetupRequest(HttpMethod requestMethod) =>
        new(this, _GetRequestPredicate(requestMethod));
    
    public MockHttpClientRequestBuilder SetupRequest(HttpMethod requestMethod, string requestPath) =>
        new(this, _GetRequestPredicate(requestMethod, requestPath));

    private Predicate<HttpRequestMessage> _GetRequestPredicate(
        HttpMethod? httpMethod = null,
        string? requestPath = null)
    {
        return MatchesAllConditions;

        bool MatchesAllConditions(HttpRequestMessage message)
        {
            var allConditions = new[]
            {
                () => Matches(httpMethod, val => message.Method == val),
                () => Matches(requestPath, val => message.RequestUri == new Uri(_httpClient.BaseAddress!, val)),
            };

            return allConditions.All(x => x());
        }

        bool Matches<T>(T val, Func<T, bool> x) => val is null || x(val);
    }

    public HttpClient ToHttpClient()
    {
        if (_isBuilt)
        {
            throw new InvalidOperationException($"{nameof(ToHttpClient)} can only be called once per instance");
        }

        _isBuilt = true;

        return _httpClient;
    }

    public IHttpClientFactory ToIHttpClientFactory(string clientName = "") =>
        new MockHttpClientFactory(clientName, ToHttpClient());

    public void ToClientForExistingIHttpClientFactory(
        IHttpClientFactory existingFactory,
        string clientName = "")
    {
        if (existingFactory is not MockHttpClientFactory factory)
        {
            throw new ArgumentException($"Factory provdied must be of type {nameof(MockHttpClientFactory)}", nameof(existingFactory));
        }

        factory.AddNamedClient(clientName, ToHttpClient());
    }

    class MockHttpClientFactory : IHttpClientFactory
    {
        private readonly Dictionary<string, HttpClient> _httpClients = [];

        public MockHttpClientFactory(string name, HttpClient httpClient)
        {
            AddNamedClient(name, httpClient);
        }

        HttpClient IHttpClientFactory.CreateClient(string name)
        {
            if (_httpClients.TryGetValue(name, out var httpClient))
            {
                return httpClient;
            }
            
            throw new InvalidOperationException($"Factory has not been set up for client with name: '{name}'");
        }

        public void AddNamedClient(string name, HttpClient httpClient)
        {
            if (_httpClients.ContainsKey(name))
            {
                throw new ArgumentException("Factory has already been set up for a client with that name", nameof(name));
            }

            _httpClients[name] = httpClient;
        }
    }

    public class MockHttpClientRequestBuilder
    {
        private readonly MockHttpClientBuilder _builder;
        private readonly Predicate<HttpRequestMessage> _predicate;

        public MockHttpClientRequestBuilder(MockHttpClientBuilder builder, Predicate<HttpRequestMessage> predicate)
        {
            _builder = builder;
            _predicate = predicate;
        }

        public MockHttpClientBuilder WithJsonResponse(HttpStatusCode responseCode, string json) =>
            _WithJsonResponse(responseCode, json);

        public MockHttpClientBuilder WithJsonResponse(string json) =>
            _WithJsonResponse(responseCode: null, json);

        private MockHttpClientBuilder _WithJsonResponse(HttpStatusCode? responseCode, string json)
        {
            var response = new HttpResponseMessage()
            {
                Content = new StringContent(json, new MediaTypeHeaderValue(MediaTypeNames.Application.Json)),
            };

            if (responseCode.HasValue)
            {
                response.StatusCode = responseCode.Value;
            }

            _builder._handler.Responses.Add((_predicate, response));

            return _builder;
        }
    }

    class MockHttpClientHandler : HttpMessageHandler
    {
        public List<(Predicate<HttpRequestMessage> Predicate, HttpResponseMessage Response)> Responses { get; } = [];

        private readonly object _lock = new();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                var index = Responses.FindIndex(x => x.Predicate(request));
                if (index is -1)
                {
                    throw new InvalidOperationException("No response is set up for this request");
                }

                var response = Responses[index].Response;
                Responses.RemoveAt(index);

                return Task.FromResult(response);
            }
        }
    }
}
