using AutoMapper;
using DC = SocialDistancing.API.DataContracts;
using DCH = SocialDistancing.API.DataContracts.Helpers;
using S = SocialDistancing.Services.Model;

namespace SocialDistancing.IoC.Configuration.AutoMapper.Profiles
{
    public class APIMappingProfile : Profile
    {
        public APIMappingProfile()
        {
            CreateMap<DC.Terminal, S.Terminal>().ReverseMap();
            CreateMap<DCH.TerminalPatch, S.TerminalPatch>().ReverseMap();
        }
    }
}
