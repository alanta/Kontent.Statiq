# Kontent.Statiq

![CI](https://github.com/alanta/Kontent.Statiq/workflows/CI/badge.svg?branch=master)
[![NuGet](https://img.shields.io/nuget/v/Kontent.Statiq.svg)](https://www.nuget.org/packages/Kontent.Statiq)

Module to retrieve content from [Kentico Kontent](https://kontent.ai) for building static websites with [Statiq](https://Statiq.dev).

## Getting started

To get started, you can follow the tutorial below or check out the [starter site](https://github.com/petrsvihlik/statiq-starter-kontent-lumen). It'll give you a nice impression of what you can do with Kontent and Statiq. 

## Starting from scratch

This is a quick guide to get some content from Kontent and render it to HTML. You should be familiar with ASP.NET Core and it helps if you're a bit familiar with Kontent and Statiq Framework.

* Setup a project on [Kontent](https://app.kontent.ai/sign-up), there's a free [Developer Plan](https://kontent.ai/pricing#developer-plan). 
  This document assumes you're using the [demo project](https://docs.kontent.ai/tutorials/set-up-kontent/projects/manage-projects#a-creating-a-sample-project).
* Get the [Project ID](https://docs.kontent.ai/tutorials/develop-apps/get-content/get-content-items#a-getting-content-items) of your project, you'll need it later
* Follow the [getting started](https://statiq.dev/framework/) steps for Statiq.Framework, then come back here to set up a [pipeline](https://statiq.dev/framework/pipelines/)
* Add `Kontent.Statiq` to the project:

```dotnet add package Kontent.Statiq --version 1.0.0-*```

### Generate the content models

* Install the [Kontent generator](https://github.com/Kentico/kontent-generators-net) tool to generate strong-typed models for your project
* Generate the models for your project into the Models folder:

```cmd
KontentModelGenerator --projectid "<your-project-id>" --outputdir Models --namespace My.Models -s true -g true
```

The KontentModelGenerator tool generates a C# class for each of the content types in your Kontent project as well as a `TypeProvider`. The type provider maps content types returned by the Kontent Delivery API to the generated C# classes. It also provides all the code names for the properties on your content models so you can more easily write queries for the delivery SDK.

The KomtentModelGenerator makes it easy to keep your code in sync with your Kontent project. You'll need to re-run it whenever you make changes to the content model in Kontent. 

### Setup Statiq

* Configure the Delivery client for your Kontent project

In `program.cs`, add in the service configurations needed to register the Delivery client and the custom type provider:

```csharp
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Statiq.App;
using Statiq.Common;
using My.Models; // <== your model namespace

public static class Program
{
    public static async Task<int> Main(string[] args) =>
        await Bootstrapper
            .Factory
            .CreateDefault(args)
            .ConfigureServices((services, config) =>
            {
                // Add the type provider
                services.AddSingleton<ITypeProvider, CustomTypeProvider>();
                // Configure Delivery SDK
                services.AddDeliveryClient(opts =>
                    opts.WithProjectId("<your-project-id>")
                        .UseProductionApi()
                        .Build());
            })
            .RunAsync();
}
```

### Output content straight to file

* Add a pipeline to your project. The pipeline will automatically detected and executed by Statiq.

```c#
using Kentico.Kontent.Delivery.Abstractions;
using Kontent.Statiq;
using Statiq.Common;
using Statiq.Core;
using My.Models; // <== your model namespace

namespace My // <== your namespace
{
  public class ArticlesPipeline : Pipeline
  {
      public ArticlesPipeline(IDeliveryClient client)
      {
          InputModules = new ModuleList{
              // Load all articles from Kontent,
              new Kontent<Article>(client)
                  // Use the BodyContent field as the document content
                  .WithContent(page => page.BodyContent),

              // Set the output path for each article
              new SetDestination(Config.FromDocument((doc, ctx)  
                => new NormalizedPath( $"article/{doc.AsKontent<Article>().UrlPattern}.html"))),
          };

          OutputModules = new ModuleList {
              // Write each article to disk
              new WriteFiles()
          };
      }
  }
}
```

### Run the generator

* Run the generator: `dotnet run`

You should now see that for every Article in the Kontent site there's an html file in the `\output\article` folder.

### Rendering a Razor view

For more control over your output, you can feed the rich content model provided by Kontent into Razor views. This is very similar to an ASP.NET (Core) web application.

* Add a Razor view named `_Article.cshtml` into `/input`:

```razor
@model My.Models.Article

<h3>@Model.Title</h3>

@Model.BodyCopy
```
The Razor view uses the c# model class generated by Kontent. If you open the project in Visual Studio, you'll get code completion and feedback to help you write your view.

* Replace the Articles pipeline from the previous section with the following code:

```c#
using Kentico.Kontent.Delivery.Abstractions;
using Kontent.Statiq;
using Statiq.Common;
using Statiq.Core;
using Statiq.Razor;
using My.Models; // <== your model namespace

namespace My // <== your namespace
{
  public class ArticlesPipeline : Pipeline
  {
      public ArticlesPipeline(IDeliveryClient client)
      {
          InputModules = new ModuleList{
              // Load all articles from Kontent
              new Kontent<Article>(client), 
              // Set the output path for each article
              new SetDestination(KontentConfig.Get( 
                  (Article item) => new NormalizedPath( $"article/{item.UrlPattern}.html"))),
          };

          ProcessModules = new ModuleList {
              // Pull in the Razor view, the path is relative to /input
              new MergeContent(new ReadFiles(patterns: "_Article.cshtml") ),
              // Render the Razor view into the content of the document
              new RenderRazor()
                  // Use the strongly-typed model for the Razor view
                  .WithModel(KontentConfig.As<Article>())
          };

          OutputModules = new ModuleList {
              // Write each article to disk
              new WriteFiles()
          };
      }
  }
}
```

* Run the generator again: `dotnet run`

The generated html files are in the `\output\article` folder.

This is a very basic pipeline and gives you everything you need to get started with content served from a Kontent project. But Kontent has more advanced features and some nice extras that you can leverage to get better integration and more robust code.

## Filtering content

If you need to filter out the the input document s from Kontent, it is possible to specify the query of your request using `WithQuery(IQueryParameter[])` builder method.

Let's say, you just want first three latest articles that have the Title element set and you want to load only a subset of.

```csharp
// ...
new Kontent<Article>(client)
    .WithQuery(
        new NotEmptyFilter($"elements.{Article.TitleCodename}"),
        new OrderParameter($"elements.{Article.PostDateCodename}", SortOrder.Descending),
        new LimitParameter(3),
        new ElementsParameter(Article.TitleCodename, Article.SummaryCodeName, Article.PostDateCodename, Article.UrlPatternCodeName)
    )
// ...
```

> For all filtering possibilities, take a look to the [Kontent .Net SDK docs](https://github.com/Kentico/kontent-delivery-sdk-net/wiki/Querying-content#filtering-retrieved-data).

## Inline content

Kontent allows you to have content within content. Which is very powerfull but requires a bit of work on the client side to make it work.
You basically have two options:

### Inline resolvers

Inline resolvers are used by the Kontent Delivery API Client to transform inline content items directly into HTML. This is used when your model class uses a `string` property for a rich-text field.

Inline resolvers are perfect for simple content with very basic HTML.

### Structured content

You can also use the structured content in your application. This is achieved by making the content property of type `IRichTextContent`. This allows you to render the inline content in views, partials, display templates or what ever code is appropriate.

Both these content models can be used with Statiq, it's up to your preferences.

Where ever Statiq hands you an `IDocument`, use the extension method `.AsKontent<TModel>()` to get the typed model from the document.

## Accessing content

Kontent.Statiq wraps the strong typed content model in a Statiq Document. In order to access the strong typed model within the document, a couple of helpers are available.
| Example | Description |
|------|------|
| `.AsKontent<Page>()` | An extention on IDocument that gets the original Kontent item from the metadata |

Within Statiq, configuration delegates are a powerful way to specify custom logic that is evaluated during processing of the content. Kontent.Statiq provides a couple of helpers to get access to the strongly typed Kontent model wrapped inside the Statiq Document.

| Example | Description |
|------|------|
| `KontentConfig.As<Page>()` | Fetch the entire Kontent item |
| `KontentConfig.Get((Page page) => page.Title)` | Fetch a single value from a Kontent item |
| `KontentConfig.GetChildren<Page>(page => page.RelatedPages)` | Fetch a list of related content items from a property of the Kontent item.

## Working with images (optional)

Kontent can also manage your images. It hase a very comprehensive set of [image manipulations](https://docs.kontent.ai/reference/image-transformation) baked into the Delivery API and you can leverage that with Statiq.

For example:

```html
<img src="@Model.TeaserImage.First().Url?w=350&h=350&fit=crop"/>
```

In this example `TeaserImage` is an Asset field on a strong-typed model. We pick the first asset. You can set the image transformation parameters by hand or use the `ImageUrl` extension method to get access to the [ImageUrlBuilder](https://github.com/Kentico/kontent-delivery-sdk-net/blob/master/Kentico.Kontent.ImageTransformation/ImageUrlBuilder.cs) from the Kontent .NET SDK.

The `ImageUrlBuilder` provides a fluent API to specify image manipulations including resizing and a focus point. Finally, the `Url` property returns the full URL.

### Remote or local images?
Now you can leave it at that and get them served up from the Kontent API. For the full static site experience however, you can also download the images and store them in the site.

Kontent Statiq contains the `KontentImageProcessor` module that will pickup all images in your HTML content. It will generate a local unique URL for each image and replace the original url with the local url.
After that you can use the `KontentDownloadImages` module to download the images. Using the Statiq built-in task you can write them to disk.

> These modules will process _all_ images in your html. Use the `WithFilter` extension to limit what images processed and downloaded into your application.

If you're following along with the sample project:

* Process all images by adding the `KontentImageProcessor` module to the end of the `ProcessModules` phase of the Article pipeline (see demo code above for full listing).
```csharp
...
ProcessModules = new ModuleList {
        new MergeContent(new ReadFiles(patterns: "Article.cshtml") ),
        new RenderRazor()
            .WithModel(Config.FromDocument((document, context) => document.AsKontent<My.Models.Article>())),

        // Get urls from all images and replace with a local path
        new KontentImageProcessor()
    };
...
```

* Now create a new pipeline:
```csharp
public class DownloadImages : Pipeline
{
    public DownloadImages()
    {
        Dependencies.Add(nameof(Articles)); 

        PostProcessModules = new ModuleList(
            // Pull documents from other pipelines
            new ReplaceDocuments(Dependencies.ToArray()),
            // Download the images 
            new KontentDownloadImages()
        );
        OutputModules = new ModuleList(
            // Write the collected images to disk
            new WriteFiles()
        );
    }
}
```

The `KontentImageProcessor` will add any replaced image url to a collection in the Document metadata for processing by the `KontentDownloadImages` module at a later stage.

## Taxonomy

The `KontentTaxonomy` module allows you to get the taxonomies defined in Kontent. You can download a specific taxonomy group or a single group using filters.

```csharp
public class Taxonomies : Pipeline
{
    public Taxonomies(IDeliveryClient client)
    {
        PostProcessModules = new ModuleList(
            // Load the taxonomy named 'menu'
            new KontentTaxonomy(client)
                .WithQuery(new EqualsFilter("system.codename", "menu"))
        );
    }
}
```
This will return all the terms for the taxonomy group _menu_ as a list. For convenience the root node is skipped because you cannot select the root node as a tag in Kontent.

### Returning a tree or the root node

Using the `.WithNesting(nesting:true, collapseRoot:false)` method on the module, you can make the module return a hierarchical structure of documents and control whether the root node is returned as well.

```csharp
public class Taxonomies : Pipeline
{
    public Taxonomies(IDeliveryClient client)
    {
        PostProcessModules = new ModuleList(
            // Load the taxonomy named 'menu'
            new KontentTaxonomy(client)
                .WithQuery(new EqualsFilter("system.codename", "menu"))
        );
    }
}
```

To flatten this into a list, use the [`FlattenTree`](https://github.com/statiqdev/Statiq.Framework/blob/main/src/core/Statiq.Core/Modules/Metadata/FlattenTree.cs) module.

### Grouping documents by taxonomy

If you add a taxonomy field to one of your content types, this will result in a property of type `IEnumerable<ITaxonomyTerm>` in the generated model class. You can match these terms up to a taxonomy group to get a structured set of pages. This enables you to [manage the structure of the site using taxonomy](https://docs.kontent.ai/tutorials/develop-apps/optimize-your-app/sitemaps-for-seo#a-tag-items-with-a-taxonomy-to-create-a-sitemap) or generate an overview of documents per tag. Note that you don't need to load the entire taxonomy if you don't need that structure.

```csharp
new Pipeline
{
    InputModules =
    {
        new Kontent<Article>(deliveryClient),
        // Set taxonomy terms as metadata
        new SetMetadata("Tags", KontentConfig.Get((Article art) => art.Tags)),
        // Group by taxonomy
        new GroupDocuments( "Tags" )
            .WithComparer(new TaxonomyTermComparer())
    }
}
```
The output for this pipeline is a flat list of documents, one for each taxonomy term with the documents for that term as its children. 
The taxonomy term on each document is avalailable through `doc.AsTaxonomyTerm(Keys.GroupKey)`.

### Structuring documents by taxonomy

If you do need the structure of the taxonomy, for example to build a sitemap, you can join content to the taxonomy data returned by the `KontentTaxonomy` module.

```csharp
var articlePipeline = new Pipeline
{
    InputModules =
    {
        // Load all articles
        new Kontent<Article>(deliveryClient),
        // Set taxonomy terms as metadata
        new SetMetadata("Tags", KontentConfig.Get((Article art) => art.Sitemap)),
        // Group the articles by tag
        new GroupDocuments( "Tags" ).WithComparer(new TaxonomyTermComparer())
    }
};

var sitemapPipeline = new Pipeline
{
    Dependencies = { "Articles" },
    InputModules =
    {
        // Load the taxonomy as a flat list
        new KontentTaxonomy(deliveryClient)
            .WithQuery(new EqualsFilter("system.codename", "menu")),
    },
    ProcessModules = {
        new SetMetadata( Keys.Children, Config.FromDocument((doc, ctx) =>
        {
            var taxonomyTerm = doc.AsKontentTaxonomyTerm().Codename;

            // Find the articles for this term
            return ctx.Outputs.FromPipeline("Articles")
                .FirstOrDefault( x => x.AsKontentTaxonomyTerm(Keys.GroupKey)?.Codename==taxonomyTerm)
                ?.GetChildren()
                .ToArray();
        }))
    }
};
```

The output of the sitemap pipeline is a flat list of nodes representing the taxonomy terms. Each nodes children are the documents that are tagged with that term.

Note that the path to each node in the tree is available through `doc.Get<string>(Keys.TreePath)`.

### Taxonomy helpers

Extensions on `IDocument` that can be used with documents loaded through the `KontentTaxonomy` module:

| Example | Description |
|------|------|
| `.AsKontentTaxonomy()` | Fetch the entire `ITaxonomyGroup` from a root taxonomy document. |
| `.AsKontentTaxonomyTerm()` | Fetch a single term from an IDocument |
| `.AsKomtentTaxonomyTerms()` | Fetch a list of child terms from an IDocument, if any.

### Taxonomy metadata

All the keys used by the Modules provided are listed in the [KontentKeys](https://github.com/alanta/Kontent.Statiq/blob/main/Kontent.Statiq/KontentKeys.cs) class.
For most cases there is a helper or extension to access the metadata but if you need direct access, you can use these keys to do so.

| Key | Value |
|------|------|
| `Keys.Title` | The name of the term or group |
| `Keys.GroupKey` | The code name of the term or group |
| `Keys.TreePath` | A `string[]` containing the path of the node in the taxonomy by code name |
| `Keys.Children` | The list of child terms as Statiq `IDocument` |
| `KontentKeys.Taxonomy.Terms` | The child terms as `ITaxonomyTermDetails[]` |
| `KontentKeys.Taxonomy.Group` | The taxonomy group as `ITaxonomyGroup` |
| `KontentKeys.Taxonomy.Term` | The taxonomy term as `ITaxonomyTermDetails` |
| `KontentKeys.System.Name` | The name of the term or group |
| `KontentKeys.System.CodeName` | The code name of the term or group |
| `KontentKeys.System.Id` | The id of the group (not available for terms) |
| `KontentKeys.System.LastModified` | The `DateTime` the group was last modified (not available for terms) |

## Troubleshooting

#### There are weird object tags like this in my content: 

```xml
<object type="application/kenticocloud" data-type="item" data-rel="component" data-codename="n2ef9e997_4691_0118_8777_c0ac9cee683b"></object>
```

This means that the Kontent Delivery SDK was unable to resolve the type of content included in your rich text field. Make sure you read the section on structured content and follow the configuration steps.

#### Links to other pages don't work

Implement and register a link resolver. See the [Kontent docs](https://github.com/Kentico/kontent-delivery-sdk-net/wiki/Resolving-Links-to-Content-Items) for more information.

#### Null reference while rendering Razor

Make sure all your classes are in a namespace.

#### A _Bad request_ occurs while loading data from Kontent

If you have a lot of content items, use the items-feed by calling `.WithItemsFeed()` method on the Kontent module.

#### Why is this project Beta?

Mostly because Statiq itself is still beta and release packages should not rely on beta dependencies. As soon as Statiq moves to 1.0, so will this project.
The Kontent Statiq is being used in production projects.

## How do I build this repo?

You'll need a .NET Core development setup: Windows, Mac, Linux with VisualStudio, VS Code or Rider.

## Contribution guidelines

* You're welcome to send pull requests. Please create an issue first and include a unit test with your pull request to verify your code.
* Massive refactorings, code cleanups etc. will be rejected unless explictly agreed upon
* Adding additional Nuget package dependencies to the main assemblies is strongly discouraged
* Please read and respect the [code of conduct](CODE_OF_CONDUCT.md)

## Who do I talk to?

* Marnix van Valen on twitter : @marnixvanvalen or e-mail : marnix [at] alanta [dot] nl


## Blog posts, videos & docs
* [Why Kontent and Jamstack are primed for .NET and the Enterprise @ Kontent Horizons 2021](https://www.youtube.com/watch?v=khkF8-eEdjI&t=1083s) Brian McKeiver, 25 nov 2021
* [Building fast Jamstack sites on .NET @ CloudSummit 2021](https://www.youtube.com/watch?v=MJnVsAmSZB0&t=12402s) Ondrej Polesny on YouTube, 21 sept 2021
* [Jamstack on .NET : how I built this site](https://alanta.nl/posts/2021/05/how-i-built-this-site) Marnix van Valen, 17 may 2021
* [Returning to .NET](https://www.mattnield.co.uk/blog/returning-to-net/) Matt Nield, 11 mar 2021
* [JAMStack on .NET](https://jamstackon.net/) microsite by Kentico
* [Jamstack and .NET - friends or enemies?](https://www.youtube.com/watch?v=Y_yMveuTOTA) Ondrej Polesny on YouTube, 5 nov 2020
* [Statiq Starter Kontent Lumen](https://jamstackthemes.dev/theme/statiq-starter-kontent-lumen/) sources on [Github](https://github.com/petrsvihlik/statiq-starter-kontent-lumen), 20 oct 2020
* [On .NET Live - Generating static web applications with Statiq](https://www.youtube.com/watch?v=43oQTRZqK9g) on Youtube, 6 aug. 2020
* [Static sites with Kentico Cloud, Statiq and Netlify](https://www.kenticotricks.com/blog/static-sites-with-kentico-cloud) Kristian Bortnik, 31 jan 2018
