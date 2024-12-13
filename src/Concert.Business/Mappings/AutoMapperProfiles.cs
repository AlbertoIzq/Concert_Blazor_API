using AutoMapper;
using Concert.Business.Models.Domain;

namespace Concert.Business.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<SongRequest, SongRequestDto>().ReverseMap();
            CreateMap<AddSongRequestDto, SongRequest>();
        }
    }
}
