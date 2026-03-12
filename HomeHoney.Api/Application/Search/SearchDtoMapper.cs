using HomeHoney.Api.Contracts.Search;
using HomeHoney.Services.Search;

namespace HomeHoney.Api.Application.Search;

public static class SearchDtoMapper
{
    public static SearchGroupDto ToDto(SearchGroupResult group)
        => new()
        {
            Title = group.Title,
            Items = group.Items.Select(item => new SearchResultItemDto
            {
                Title = item.Title,
                Summary = item.Summary,
                Category = item.Category,
                Href = item.Href,
            }).ToList(),
        };
}
