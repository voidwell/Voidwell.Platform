using AutoMapper;
using Markdig;

namespace Voidwell.Platform.Api.Services.Mappers;

public class BlogPostMapper : Profile
{
    public BlogPostMapper()
    {
        CreateMap<Data.Models.BlogPost, Models.BlogPost>()
            .ForMember(a => a.HtmlContent, a => a.MapFrom(src => src.HtmlContent))
            .ForMember(a => a.Tags, a => a.MapFrom((src, _, _, rc) => rc.Mapper.Map<IEnumerable<Models.BlogPostTag>>(src.BlogPostTagMaps)))
            .ForMember(a => a.AuthorName, a => a.Ignore());

        CreateMap<Data.Models.BlogPost, Models.EditableBlogPost>()
            .ForMember(a => a.Tags, a => a.MapFrom((src, _, _, rc) => rc.Mapper.Map<IEnumerable<Models.BlogPostTag>>(src.BlogPostTagMaps)));

        CreateMap<Models.BlogPostRequest, Data.Models.BlogPost>()
            .ForMember(a => a.Id, a => a.Ignore())
            .ForMember(a => a.HtmlContent, a => a.MapFrom(src => Markdown.ToHtml(src.MarkdownContent, null, null)))
            .ForMember(a => a.BlogPostTagMaps, a => a.MapFrom((src, _, _, rc) => rc.Mapper.Map<IEnumerable<Data.Models.BlogPostTagMap>>(src.Tags)))
            .ForMember(a => a.AuthorId, a => a.Ignore())
            .ForMember(a => a.PublishDate, a => a.Ignore());
    }
}
