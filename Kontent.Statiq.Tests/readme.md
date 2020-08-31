# Kontent module for Statiq - Tests

## How does this work?

These tests are based on the Kontent Delivery API for C# test suite, as described [here](https://github.com/Kentico/kontent-delivery-sdk-net/wiki/Faking-responses). In a nutshell, what you do is make an API call for to the `/items` endpoint on the Kontent Delivery API.
The resulting JSON is pasted into `response/getitems.json`. Then, there's the `FakeDeliveryClient` class which is wired up to always return that JSON data, pretending it's an actual call to the Delivery API.

The Kontent module for Statiq is hooked up to the FakeDeliveryClient, allowing the tests to validate that the whole thing actually works, without calling the actual API.

## Untyped content

The default Statiq mode of operation is to have a Document with a large block of content. Essentially, a string holding all the text and markup. The Statiq pipeline transforms that into whatever HTML you need.
You can use that mode with Kontent. By default the module puts all the properties of the content item into the Meta data and you can pick the property to use for the URL and the body.

While it's easy to get started this way, it can get complicated really fast because you have to access each property using a (magic) string to identify it. There's no compiler checking or code completion to help out, so building your site is mostly trial-and-error.

Kontent can have much more elaborate content models though so this module also allows you to use strong typed models.

## Strong typed modeling

The Delivery Client supports [strong typed models](https://github.com/Kentico/kontent-delivery-sdk-net/wiki/Working-with-strongly-typed-models) which makes working with the content a lot more intuitive.

In the test project, the `Models` folder contains the generated content models for the Kentico demo project. To regenerate the models, install the [Kontent generator](https://github.com/Kentico/kontent-generators-net) and use the following command:

```
KontentModelGenerator --projectid "00000000-0000-0000-0000-000000000000" --outputdir Models --namespace Kontent.Statiq.Tests.Models -s true -g false
```

You will need to fill in the correct project id for your account. 
This will generate a C# class for each content type defined in the project and also a `CustomTypeProvider` which allows the Delivery Client to map the JSON data to C# classes.

Note that the models and the response JSON need to line up, so make sure they both come from the same project. The tests also check specific properties of the models so if you regenerate the models from a different project the tests will likely no longer compile.
