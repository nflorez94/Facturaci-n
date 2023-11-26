using AutoMapper;
using Facturación.Models.Dtos;
using Facturación.Models.Entities;

namespace Facturación.Transversal
{
    public class Mappers : Profile
    {
        public Mappers()
        {
            CreateMap<Factura, FacturaDto>();
        }
    }
}
