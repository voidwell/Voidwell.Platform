using AutoMapper;

namespace Voidwell.Platform.Api.Services.Mappers;

public class BlogPostTagMapMapper : Profile
{
    public BlogPostTagMapMapper()
    {
        CreateMap<Data.Models.BlogPostTagMap, Models.BlogPostTag>()
            .ForMember(a => a.Id, a => a.MapFrom(src => src.BlogPostTagId))
            .ForMember(a => a.Name, a => a.MapFrom(src => src.BlogPostTag!.Name));

        CreateMap<Models.BlogPostTag, Data.Models.BlogPostTagMap>()
            .ForMember(a => a.BlogPostTagId, a => a.MapFrom(src => src.Id))
            .ForMember(a => a.BlogPostTag, a => a.MapFrom(src => src))
            .ForMember(a => a.BlogPost, a => a.Ignore())
            .ForMember(a => a.BlogPostId, a => a.Ignore());
        CreateMap<Models.BlogPostTag, Data.Models.BlogPostTag>()
            .ForMember(a => a.NormalizedName, a => a.MapFrom(src => src.Name.ToUpper()));
    }
}
