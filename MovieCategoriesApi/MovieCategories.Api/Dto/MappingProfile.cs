using AutoMapper;
using MovieCategories.Domain;

namespace MovieCategories.Api.Dto;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateMovieCategoryRequest, MovieCategory>();
    }
}