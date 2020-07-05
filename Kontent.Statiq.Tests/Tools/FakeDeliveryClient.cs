using FakeItEasy;
using Kentico.Kontent.Delivery;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Builders.DeliveryClient;
using Kentico.Kontent.Delivery.Configuration;
using RichardSzalay.MockHttp;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kontent.Statiq.Tests.Tools
{
    public static class MockDeliveryClient
    {
        // This is not the nicest code :(
        // Unfortunately the Delivery API is mostly based on concrete sealed classes, making it very difficult to test 
        // This code relies on blobs of JSON, requiring us to know the internal structure of the JSON responses from Kontent.

        // based on https://github.com/Kentico/kontent-delivery-sdk-net/blob/13.0.0/Kentico.Kontent.Delivery.Tests/FakeHttpClientTests.cs
        // More info https://github.com/Kentico/kontent-delivery-sdk-net/wiki/Faking-responses


        public static IDeliveryClient Create(string response, Func<IOptionalClientSetup, IOptionalClientSetup>? configureClient = null)
        {
            const string testUrl = "https://tests.fake.url";

            var httpClient = MockHttpClient(testUrl, response);
            var deliveryOptions = MockDeliveryOptions(testUrl);
            return CreateMockDeliveryClient(deliveryOptions, httpClient, configureClient);
        }

        private static HttpClient MockHttpClient(string baseUrl, string response)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When($"{baseUrl}/*").Respond("application/json", response);

            return mockHttp.ToHttpClient();
        }

        private static DeliveryOptions MockDeliveryOptions(string baseUrl)
            => DeliveryOptionsBuilder
                .CreateInstance()
                .WithProjectId(Guid.NewGuid())
                .UseProductionApi()
                .WithCustomEndpoint($"{baseUrl}/{{0}}")
                .Build();

        private static IDeliveryClient CreateMockDeliveryClient(DeliveryOptions deliveryOptions, HttpClient httpClient, Func<IOptionalClientSetup, IOptionalClientSetup>? configureClient)
        {
            var retryPolicy = A.Fake<IRetryPolicy>();
            var retryPolicyProvider = A.Fake<IRetryPolicyProvider>();
            A.CallTo(() => retryPolicyProvider.GetRetryPolicy())
                .Returns(retryPolicy);
            A.CallTo(() => retryPolicy.ExecuteAsync(A<Func<Task<HttpResponseMessage>>>._))
                .ReturnsLazily(c => c.GetArgument<Func<Task<HttpResponseMessage>>>(0)!.Invoke());

            var client = DeliveryClientBuilder
                .WithOptions(_ => deliveryOptions)
                .WithDeliveryHttpClient(new DeliveryHttpClient(httpClient))
                //.WithContentLinkUrlResolver(contentLinkUrlResolver)
                .ApplyIfNotNull(configureClient)
                .WithRetryPolicyProvider(retryPolicyProvider)
                .Build();

            return client;
        }
    }

    internal static class Helpers
    {
        public static TBuilder ApplyIfNotNull<TBuilder>(this TBuilder builder, Func<TBuilder, TBuilder>? configure)
        {
            return configure != null ? configure(builder) : builder;
        }
    }
}
