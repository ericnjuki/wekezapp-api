using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using wekezapp.data.DTOs;
using wekezapp.data.Entities;

namespace wekezapp.core.Mapping {
    public class MappingProfile : Profile {
        public MappingProfile() {
            // Add as many of these lines as you need to map your objects
            CreateMap<User, UserDto>().ReverseMap();

            CreateMap<Chama, ChamaDto>().ReverseMap();
        }
    }
}
