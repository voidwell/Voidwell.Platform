using System.Reflection;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Voidwell.Platform.Api.Services.Mappers;
using Xunit;

namespace Voidwell.Platform.Api.Test.MapperTests;

public class BlogPostTagMapMapperTest
{
    private readonly IMapper _mapper;

    public BlogPostTagMapMapperTest()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddMaps(Assembly.GetAssembly(typeof(BlogPostTagMapMapper))), NullLoggerFactory.Instance)
            .CreateMapper();
    }

    [Fact]
    public void AutoMapper_Configuration_IsValid()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<BlogPostTagMapMapper>(), NullLoggerFactory.Instance);
        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_DataModel_BlogPostTagMap_to_Model_BlogPostTag()
    {
        var expectedTagId = Guid.NewGuid();
        var expectedTagName = "TestTag";

        var dataModel = new Data.Models.BlogPostTagMap
        {
            BlogPostTagId = expectedTagId,
            BlogPostTag = new Data.Models.BlogPostTag
            {
                Id = expectedTagId,
                Name = expectedTagName
            }
        };

        var expectedModel = new Models.BlogPostTag
        {
            Id = expectedTagId,
            Name = expectedTagName
        };

        var result = _mapper.Map<Models.BlogPostTag>(dataModel);

        result.Should()
            .BeEquivalentTo(expectedModel);
    }

    [Fact]
    public void Map_Model_BlogPostTag_to_DataModel_BlogPostTagMap()
    {
        var expectedTagId = Guid.NewGuid();
        var expectedTagName = "TestTag";

        var model = new Models.BlogPostTag
        {
            Id = expectedTagId,
            Name = expectedTagName
        };

        var expectedDataModel = new Data.Models.BlogPostTagMap
        {
            BlogPostTagId = expectedTagId,
            BlogPostTag = new Data.Models.BlogPostTag
            {
                Id = expectedTagId,
                Name = expectedTagName,
                NormalizedName = expectedTagName.ToUpper()
            }
        };

        var result = _mapper.Map<Data.Models.BlogPostTagMap>(model);

        result.Should()
            .BeEquivalentTo(expectedDataModel);
    }
}
