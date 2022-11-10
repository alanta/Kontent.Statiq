using FakeItEasy;
using Kontent.Ai.Delivery.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kontent.Statiq.Tests.Tools
{
    public static class KontentSetupHelpers
    {
        public static IDeliveryClient WithFakeContent<TContent>(this IDeliveryClient client, params TContent[] content)
        {
            var response = A.Fake<IDeliveryItemListingResponse<TContent>>();
            A.CallTo(() => response.ApiResponse.IsSuccess).Returns(true);
            A.CallTo(() => response.Items).Returns(content);
            A.CallTo(() => client.GetItemsAsync<TContent>(A<IEnumerable<IQueryParameter>>._))
                .Returns(response);

            return client;
        }

        public static IDeliveryClient WithFakeContentError<TContent>(this IDeliveryClient client)
        {
            var response = A.Fake<IDeliveryItemListingResponse<TContent>>();
            A.CallTo(() => response.ApiResponse.IsSuccess).Returns(false);
            A.CallTo(() => response.ApiResponse.Error.Message).Returns("Oops");
            A.CallTo(() => response.Items).Returns(null!);
            A.CallTo(() => client.GetItemsAsync<TContent>(A<IEnumerable<IQueryParameter>>._))
                .Returns(response);

            return client;
        }

        public static IDeliveryClient WithFakeContentFeed<TContent>(this IDeliveryClient client, params TContent[] content)
        {
            var response = A.Fake<IDeliveryItemsFeed<TContent>>();
            var moreData = true;
            A.CallTo(() => response.HasMoreResults).ReturnsLazily( _ => moreData );
            A.CallTo(() => response.FetchNextBatchAsync(A<string>.Ignored)).ReturnsLazily(_ =>
            {
                moreData = false; // return a single batch
                var data = A.Fake<IDeliveryItemsFeedResponse<TContent>>();
                A.CallTo(() => data.Items).Returns(content);
                A.CallTo(() => data.ApiResponse.IsSuccess).Returns(true);
                return Task.FromResult(data);
            });
            A.CallTo(() => client.GetItemsFeed<TContent>(A<IEnumerable<IQueryParameter>>._))
                .Returns(response);

            return client;
        }

        public static IDeliveryClient WithFakeContentFeedError<TContent>(this IDeliveryClient client)
        {
            var response = A.Fake<IDeliveryItemsFeed<TContent>>();
            var moreData = true;
            A.CallTo(() => response.HasMoreResults).ReturnsLazily(_ => moreData);
            A.CallTo(() => response.FetchNextBatchAsync(A<string>.Ignored)).ReturnsLazily(_ =>
            {
                moreData = false; // return a single batch
                var data = A.Fake<IDeliveryItemsFeedResponse<TContent>>();
                A.CallTo(() => data.Items).Returns(null!);
                A.CallTo(() => data.ApiResponse.IsSuccess).Returns(false);
                A.CallTo(() => data.ApiResponse.Error.Message).Returns("Oops");
                return Task.FromResult(data);
            });
            A.CallTo(() => client.GetItemsFeed<TContent>(A<IEnumerable<IQueryParameter>>._))
                .Returns(response);

            return client;
        }

        public static IDeliveryClient WithFakeTaxonomy(this IDeliveryClient client, params ITaxonomyGroup[] taxonomies)
        {
            var response = A.Fake<IDeliveryTaxonomyListingResponse>();
            A.CallTo(() => response.Taxonomies).Returns(taxonomies);
            A.CallTo(() => client.GetTaxonomiesAsync(A<IEnumerable<IQueryParameter>>._))
                .Returns(response);

            return client;
        }
    }
}