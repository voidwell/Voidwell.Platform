using System.Reflection;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Voidwell.Platform.Api.Services.Mappers;
using Xunit;

namespace Voidwell.Platform.Api.Test.MapperTests;

public class BlogPostMapperTest
{
    private readonly IMapper _mapper;

    public BlogPostMapperTest()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddMaps(Assembly.GetAssembly(typeof(BlogPostMapper))), NullLoggerFactory.Instance)
            .CreateMapper();
    }

    [Fact]
    public void AutoMapper_Configuration_IsValid()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<BlogPostMapper>(), NullLoggerFactory.Instance);
        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_DataModel_BlogPost_to_Model_BlogPost()
    {
        var expectedId = Guid.NewGuid();
        var expectedTitle = "TestTitle";
        var expectedPublishDate = DateTimeOffset.UtcNow;
        var expectedHtmlContent = "<b>Test</b>";

        var expectedTag1Id = Guid.NewGuid();
        var expectedTag2Id = Guid.NewGuid();

        var dataModel = new Data.Models.BlogPost
        {
            Id = expectedId,
            Title = expectedTitle,
            PublishDate = expectedPublishDate,
            HtmlContent = expectedHtmlContent,
            BlogPostTagMaps = new[]
            {
                new Data.Models.BlogPostTagMap
                {
                    BlogPostId = expectedId,
                    BlogPostTagId = expectedTag1Id,
                    BlogPostTag = new Data.Models.BlogPostTag
                    {
                        Id = expectedTag1Id,
                        Name = expectedTag1Id.ToString(),
                        NormalizedName = expectedTag1Id.ToString()
                    }
                },
                new Data.Models.BlogPostTagMap
                {
                    BlogPostId = expectedId,
                    BlogPostTagId = expectedTag2Id,
                    BlogPostTag = new Data.Models.BlogPostTag
                    {
                        Id = expectedTag2Id,
                        Name = expectedTag2Id.ToString(),
                        NormalizedName = expectedTag2Id.ToString()
                    }
                }
            }
        };

        var expectedModel = new Models.BlogPost
        {
            Id = expectedId,
            Title = expectedTitle,
            PublishDate = expectedPublishDate,
            HtmlContent = expectedHtmlContent,
            Tags = new[]
            {
                new Models.BlogPostTag
                {
                    Id = expectedTag1Id,
                    Name = expectedTag1Id.ToString()
                },
                new Models.BlogPostTag
                {
                    Id = expectedTag2Id,
                    Name = expectedTag2Id.ToString()
                }
            }
        };

        var result = _mapper.Map<Models.BlogPost>(dataModel);

        result.Should()
            .BeEquivalentTo(expectedModel);
    }

    [Fact]
    public void Map_Model_BlogPostRequest_to_DataModel_BlogPost()
    {
        var expectedId = Guid.NewGuid();
        var expectedTitle = "TestTitle";
        var expectedMarkdownContent = "*Test*";
        var expectedTag1Id = Guid.NewGuid();
        var expectedTag1Name = "Tag1";
        var expectedTag2Id = Guid.NewGuid();
        var expectedTag2Name = "Tag2";

        var model = new Models.BlogPostRequest
        {
            Id = expectedId,
            Title = expectedTitle,
            MarkdownContent = expectedMarkdownContent,
            Tags = new[]
            {
                new Models.BlogPostTag
                {
                    Id = expectedTag1Id,
                    Name = expectedTag1Name
                },
                new Models.BlogPostTag
                {
                    Id = expectedTag2Id,
                    Name = expectedTag2Name
                }
            }
        };

        var expectedDataModel = new Data.Models.BlogPost
        {
            Id = expectedId,
            Title = expectedTitle,
            MarkdownContent = expectedMarkdownContent,
            HtmlContent = "<p><em>Test</em></p>\n",
            BlogPostTagMaps = new[]
            {
                new Data.Models.BlogPostTagMap
                {
                    BlogPostTagId = expectedTag1Id,
                    BlogPostTag = new Data.Models.BlogPostTag
                    {
                        Id = expectedTag1Id,
                        Name = expectedTag1Name,
                        NormalizedName = expectedTag1Name.ToUpper()
                    }
                },
                new Data.Models.BlogPostTagMap
                {
                    BlogPostTagId = expectedTag2Id,
                    BlogPostTag = new Data.Models.BlogPostTag
                    {
                        Id = expectedTag2Id,
                        Name = expectedTag2Name,
                        NormalizedName = expectedTag2Name.ToUpper()
                    }
                }
            }
        };

        var result = _mapper.Map<Data.Models.BlogPost>(model);

        result.Should()
            .BeEquivalentTo(expectedDataModel);
    }

    [Fact]
    public void Map_DataModel_BlogPost_to_Model_BlogPostRequest()
    {
        var expectedId = Guid.NewGuid();
        var expectedTitle = "TestTitle";
        var expectedPublishDate = DateTimeOffset.UtcNow;
        var expectedMarkdownContent = "*Test*";

        var expectedTag1Id = Guid.NewGuid();
        var expectedTag2Id = Guid.NewGuid();

        var dataModel = new Data.Models.BlogPost
        {
            Id = expectedId,
            Title = expectedTitle,
            PublishDate = expectedPublishDate,
            MarkdownContent = expectedMarkdownContent,
            BlogPostTagMaps = new[]
            {
                new Data.Models.BlogPostTagMap
                {
                    BlogPostId = expectedId,
                    BlogPostTagId = expectedTag1Id,
                    BlogPostTag = new Data.Models.BlogPostTag
                    {
                        Id = expectedTag1Id,
                        Name = expectedTag1Id.ToString(),
                        NormalizedName = expectedTag1Id.ToString()
                    }
                },
                new Data.Models.BlogPostTagMap
                {
                    BlogPostId = expectedId,
                    BlogPostTagId = expectedTag2Id,
                    BlogPostTag = new Data.Models.BlogPostTag
                    {
                        Id = expectedTag2Id,
                        Name = expectedTag2Id.ToString(),
                        NormalizedName = expectedTag2Id.ToString()
                    }
                }
            }
        };

        var expectedModel = new Models.BlogPostRequest
        {
            Id = expectedId,
            Title = expectedTitle,
            MarkdownContent = expectedMarkdownContent,
            Tags = new[]
            {
                new Models.BlogPostTag
                {
                    Id = expectedTag1Id,
                    Name = expectedTag1Id.ToString()
                },
                new Models.BlogPostTag
                {
                    Id = expectedTag2Id,
                    Name = expectedTag2Id.ToString()
                }
            }
        };

        var result = _mapper.Map<Models.BlogPostRequest>(dataModel);

        result.Should()
            .BeEquivalentTo(expectedModel);
    }
}
