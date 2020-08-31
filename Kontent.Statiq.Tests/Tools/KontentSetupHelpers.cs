using FakeItEasy;
using Kentico.Kontent.Delivery.Abstractions;
using System.Collections.Generic;

namespace Kontent.Statiq.Tests.Tools
{
    public static class KontentSetupHelpers
    {
        public static IDeliveryClient WithFakeContent<TContent>(this IDeliveryClient client, params TContent[] content)
        {
            var response = A.Fake<IDeliveryItemListingResponse<TContent>>();
            A.CallTo(() => response.Items).Returns( content );
            A.CallTo(() => client.GetItemsAsync<TContent>(A<IEnumerable<IQueryParameter>>._))
                .Returns(response);

            return client;
        }
    }
}