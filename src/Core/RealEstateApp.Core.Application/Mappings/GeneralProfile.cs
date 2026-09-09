using AutoMapper;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Application.DTOs.Property;
using RealEstateApp.Core.Application.DTOs.PropertyType;
using RealEstateApp.Core.Application.DTOs.SaleType;
using RealEstateApp.Core.Application.DTOs.Improvement;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Application.ViewModels.Offer;
using RealEstateApp.Core.Application.ViewModels.Chat;
using RealEstateApp.Core.Application.ViewModels.Favorite;
using RealEstateApp.Core.Application.ViewModels.LeadPipeline;
using System.Linq;
using VmPropertyType = RealEstateApp.Core.Application.ViewModels.PropertyType;
using VmSaleType = RealEstateApp.Core.Application.ViewModels.SaleType;
using VmImprovement = RealEstateApp.Core.Application.ViewModels.Improvement;

namespace RealEstateApp.Core.Application.Mappings
{
    /// <summary>
    /// Perfil general de AutoMapper que define todos los mapeos entre
    /// entidades de dominio, ViewModels (MVC) y DTOs (API).
    /// </summary>
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            #region Property

            // Property → PropertyViewModel (lectura MVC)
            CreateMap<Property, PropertyViewModel>()
                .ForMember(dest => dest.PropertyTypeName, opt => opt.MapFrom(src => src.PropertyType != null ? src.PropertyType.Name : string.Empty))
                .ForMember(dest => dest.SaleTypeName, opt => opt.MapFrom(src => src.SaleType != null ? src.SaleType.Name : string.Empty))
                .ForMember(dest => dest.ProvinceName, opt => opt.MapFrom(src => src.Province != null ? src.Province.Name : string.Empty))
                .ForMember(dest => dest.MunicipalityName, opt => opt.MapFrom(src => src.Municipality != null ? src.Municipality.Name : string.Empty))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null ? src.Images.Select(i => i.ImageUrl).ToList() : new System.Collections.Generic.List<string>()))
                .ForMember(dest => dest.Improvements, opt => opt.MapFrom(src => src.PropertyImprovements != null ? src.PropertyImprovements.Where(pi => pi.Improvement != null).Select(pi => pi.Improvement!.Name).ToList() : new System.Collections.Generic.List<string>()))
                .ForMember(dest => dest.FavoritesCount, opt => opt.MapFrom(src => src.Favorites != null ? src.Favorites.Count : 0))
                .ForMember(dest => dest.AgentName, opt => opt.Ignore()) // Se resuelve en el servicio
                .ForMember(dest => dest.IsFavorite, opt => opt.Ignore()); // Se resuelve en el servicio

            // Property → PropertyDto (lectura API)
            CreateMap<Property, PropertyDto>()
                .ForMember(dest => dest.PropertyTypeName, opt => opt.MapFrom(src => src.PropertyType != null ? src.PropertyType.Name : string.Empty))
                .ForMember(dest => dest.SaleTypeName, opt => opt.MapFrom(src => src.SaleType != null ? src.SaleType.Name : string.Empty))
                .ForMember(dest => dest.ProvinceName, opt => opt.MapFrom(src => src.Province != null ? src.Province.Name : string.Empty))
                .ForMember(dest => dest.MunicipalityName, opt => opt.MapFrom(src => src.Municipality != null ? src.Municipality.Name : string.Empty))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images != null ? src.Images.Select(i => i.ImageUrl).ToList() : new System.Collections.Generic.List<string>()))
                .ForMember(dest => dest.Improvements, opt => opt.MapFrom(src => src.PropertyImprovements != null ? src.PropertyImprovements.Where(pi => pi.Improvement != null).Select(pi => pi.Improvement!.Name).ToList() : new System.Collections.Generic.List<string>()))
                .ForMember(dest => dest.AgentName, opt => opt.Ignore()); // Se resuelve en el servicio

            // PropertyViewModel <-> PropertyDto (conversión entre DTO de API y ViewModel de servicio)
            CreateMap<PropertyViewModel, PropertyDto>().ReverseMap();

            // SavePropertyViewModel → Property (escritura desde formulario)
            CreateMap<SavePropertyViewModel, Property>()
                .ForMember(dest => dest.Images, opt => opt.Ignore()) // Se maneja manualmente (IFormFile)
                .ForMember(dest => dest.PropertyImprovements, opt => opt.Ignore()) // Se maneja manualmente
                .ForMember(dest => dest.PropertyType, opt => opt.Ignore())
                .ForMember(dest => dest.SaleType, opt => opt.Ignore())
                .ForMember(dest => dest.Province, opt => opt.Ignore())
                .ForMember(dest => dest.Municipality, opt => opt.Ignore())
                .ForMember(dest => dest.Offers, opt => opt.Ignore())
                .ForMember(dest => dest.Chats, opt => opt.Ignore())
                .ForMember(dest => dest.Favorites, opt => opt.Ignore())
                .ForMember(dest => dest.MortgageSimulations, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Se establece por lógica de negocio
                .ForMember(dest => dest.AgentId, opt => opt.Ignore()) // Se establece desde la sesión
                .ForMember(dest => dest.Code, opt => opt.Ignore()); // Se autogenera

            // Property → SavePropertyViewModel (pre-fill form for editing)
            CreateMap<Property, SavePropertyViewModel>()
                .ForMember(dest => dest.Files, opt => opt.Ignore())
                .ForMember(dest => dest.ImprovementIds, opt => opt.MapFrom(src =>
                    src.PropertyImprovements != null ? src.PropertyImprovements.Select(pi => pi.ImprovementId).ToList() : new System.Collections.Generic.List<int>()))
                .ForMember(dest => dest.ExistingImages, opt => opt.MapFrom(src =>
                    src.Images != null ? src.Images.Select(i => i.ImageUrl).ToList() : new System.Collections.Generic.List<string>()))
                .ForMember(dest => dest.PropertyTypes, opt => opt.Ignore())
                .ForMember(dest => dest.SaleTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Improvements, opt => opt.Ignore())
                .ForMember(dest => dest.Provinces, opt => opt.Ignore())
                .ForMember(dest => dest.Municipalities, opt => opt.Ignore());

            #endregion

            #region PropertyType

            CreateMap<Domain.Entities.PropertyType, VmPropertyType.PropertyTypeViewModel>()
                .ForMember(dest => dest.PropertiesCount, opt => opt.MapFrom(src => src.Properties != null ? src.Properties.Count : 0));

            CreateMap<Domain.Entities.PropertyType, PropertyTypeDto>()
                .ForMember(dest => dest.PropertiesCount, opt => opt.MapFrom(src => src.Properties != null ? src.Properties.Count : 0));

            CreateMap<VmPropertyType.PropertyTypeViewModel, PropertyTypeDto>().ReverseMap();
            CreateMap<VmPropertyType.SavePropertyTypeViewModel, PropertyTypeDto>().ReverseMap();

            CreateMap<VmPropertyType.SavePropertyTypeViewModel, Domain.Entities.PropertyType>()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<Domain.Entities.PropertyType, VmPropertyType.SavePropertyTypeViewModel>();

            #endregion

            #region SaleType

            CreateMap<Domain.Entities.SaleType, VmSaleType.SaleTypeViewModel>()
                .ForMember(dest => dest.PropertiesCount, opt => opt.MapFrom(src => src.Properties != null ? src.Properties.Count : 0));

            CreateMap<Domain.Entities.SaleType, SaleTypeDto>()
                .ForMember(dest => dest.PropertiesCount, opt => opt.MapFrom(src => src.Properties != null ? src.Properties.Count : 0));

            CreateMap<VmSaleType.SaleTypeViewModel, SaleTypeDto>().ReverseMap();
            CreateMap<VmSaleType.SaveSaleTypeViewModel, SaleTypeDto>().ReverseMap();

            CreateMap<VmSaleType.SaveSaleTypeViewModel, Domain.Entities.SaleType>()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<Domain.Entities.SaleType, VmSaleType.SaveSaleTypeViewModel>();

            #endregion

            #region Improvement

            CreateMap<Domain.Entities.Improvement, VmImprovement.ImprovementViewModel>();

            CreateMap<Domain.Entities.Improvement, ImprovementDto>();

            CreateMap<VmImprovement.ImprovementViewModel, ImprovementDto>().ReverseMap();
            CreateMap<VmImprovement.SaveImprovementViewModel, ImprovementDto>().ReverseMap();

            CreateMap<VmImprovement.SaveImprovementViewModel, Domain.Entities.Improvement>()
                .ForMember(dest => dest.PropertyImprovements, opt => opt.Ignore());

            CreateMap<Domain.Entities.Improvement, VmImprovement.SaveImprovementViewModel>();

            #endregion

            #region Offer

            CreateMap<Offer, OfferViewModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PropertyCode, opt => opt.MapFrom(src => src.Property != null ? src.Property.Code : string.Empty))
                .ForMember(dest => dest.PropertyPrice, opt => opt.MapFrom(src => src.Property != null ? src.Property.Price : 0))
                .ForMember(dest => dest.AgentId, opt => opt.MapFrom(src => src.Property != null ? src.Property.AgentId : string.Empty))
                .ForMember(dest => dest.ClienteName, opt => opt.Ignore()); // Se resuelve en el servicio

            CreateMap<SaveOfferViewModel, Offer>()
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Siempre empieza como Pending
                .ForMember(dest => dest.ClienteId, opt => opt.Ignore()) // Se toma de la sesión
                .ForMember(dest => dest.FechaOferta, opt => opt.Ignore()) // Se establece automáticamente
                .ForMember(dest => dest.Property, opt => opt.Ignore());

            #endregion

            #region Chat

            CreateMap<Chat, ChatViewModel>()
                .ForMember(dest => dest.PropertyCode, opt => opt.MapFrom(src => src.Property != null ? src.Property.Code : (!src.PropertyId.HasValue || src.PropertyId == -1 ? "SOPORTE-ADMIN" : (src.PropertyId == 0 ? "SOPORTE-DEV" : string.Empty))))
                .ForMember(dest => dest.ClienteName, opt => opt.Ignore()) // Se resuelve en el servicio
                .ForMember(dest => dest.AgenteName, opt => opt.Ignore()) // Se resuelve en el servicio
                .ForMember(dest => dest.SenderName, opt => opt.Ignore()) // Se resuelve en el servicio
                .ForMember(dest => dest.IsMine, opt => opt.Ignore()); // Se determina por contexto

            CreateMap<SaveChatViewModel, Chat>()
                .ForMember(dest => dest.ClienteId, opt => opt.Ignore()) // Se determina por lógica
                .ForMember(dest => dest.AgenteId, opt => opt.Ignore()) // Se determina por lógica
                .ForMember(dest => dest.SenderId, opt => opt.Ignore()) // Se toma de la sesión
                .ForMember(dest => dest.SentAt, opt => opt.Ignore()) // Se establece automáticamente
                .ForMember(dest => dest.Property, opt => opt.Ignore());

            #endregion

            #region Favorite

            CreateMap<Favorite, FavoriteViewModel>()
                .ForMember(dest => dest.PropertyCode, opt => opt.MapFrom(src => src.Property != null ? src.Property.Code : string.Empty))
                .ForMember(dest => dest.PropertyPrice, opt => opt.MapFrom(src => src.Property != null ? src.Property.Price : 0))
                .ForMember(dest => dest.PropertyTypeName, opt => opt.MapFrom(src =>
                    src.Property != null && src.Property.PropertyType != null ? src.Property.PropertyType.Name : string.Empty))
                .ForMember(dest => dest.PropertyRooms, opt => opt.MapFrom(src => src.Property != null ? src.Property.Rooms : 0))
                .ForMember(dest => dest.PropertyBathrooms, opt => opt.MapFrom(src => src.Property != null ? src.Property.Bathrooms : 0))
                .ForMember(dest => dest.PropertySizeInMeters, opt => opt.MapFrom(src => src.Property != null ? src.Property.SizeInMeters : 0))
                .ForMember(dest => dest.PropertyStatus, opt => opt.MapFrom(src => src.Property != null ? src.Property.Status : string.Empty))
                .ForMember(dest => dest.IsFinanciable, opt => opt.MapFrom(src => src.Property != null && src.Property.IsFinanciable))
                .ForMember(dest => dest.PropertyMainImage, opt => opt.MapFrom(src =>
                    src.Property != null && src.Property.Images != null && src.Property.Images.Any()
                        ? src.Property.Images.First().ImageUrl
                        : null));

            #endregion

            #region UserActivity

            CreateMap<UserActivity, RealEstateApp.Core.Application.ViewModels.UserActivity.UserActivityViewModel>().ReverseMap();

            #endregion

            #region PropertyAppointment

            CreateMap<PropertyAppointment, RealEstateApp.Core.Application.ViewModels.Appointment.AppointmentViewModel>()
                .ForMember(dest => dest.PropertyCode, opt => opt.MapFrom(src => src.Property != null ? src.Property.Code : string.Empty))
                .ForMember(dest => dest.PropertyName, opt => opt.MapFrom(src => src.Property != null ? src.Property.Name : string.Empty))
                .ForMember(dest => dest.PropertyMainImage, opt => opt.MapFrom(src =>
                    src.Property != null && src.Property.Images != null && src.Property.Images.Any()
                        ? src.Property.Images.First().ImageUrl
                        : null))
                .ForMember(dest => dest.ClienteName, opt => opt.Ignore())
                .ForMember(dest => dest.AgentName, opt => opt.Ignore())
                .ForMember(dest => dest.StatusFormatted, opt => opt.Ignore());

            #endregion

            #region SavedSearch

            CreateMap<SavedSearch, RealEstateApp.Core.Application.ViewModels.SavedSearch.SavedSearchViewModel>()
                .ForMember(dest => dest.PropertyTypeName, opt => opt.MapFrom(src => src.PropertyType != null ? src.PropertyType.Name : null))
                .ForMember(dest => dest.SaleTypeName, opt => opt.MapFrom(src => src.SaleType != null ? src.SaleType.Name : null))
                .ForMember(dest => dest.ProvinceName, opt => opt.MapFrom(src => src.Province != null ? src.Province.Name : null))
                .ForMember(dest => dest.MunicipalityName, opt => opt.MapFrom(src => src.Municipality != null ? src.Municipality.Name : null))
                .ForMember(dest => dest.SummaryCriteria, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentMatchingCount, opt => opt.Ignore());

            CreateMap<RealEstateApp.Core.Application.ViewModels.SavedSearch.SaveSavedSearchViewModel, SavedSearch>()
                .ForMember(dest => dest.PropertyType, opt => opt.Ignore())
                .ForMember(dest => dest.SaleType, opt => opt.Ignore())
                .ForMember(dest => dest.Province, opt => opt.Ignore())
                .ForMember(dest => dest.Municipality, opt => opt.Ignore())
                .ForMember(dest => dest.LastAlertSent, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.LastModified, opt => opt.Ignore())
                .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore());

            #endregion

            #region AgentReview

            CreateMap<AgentReview, RealEstateApp.Core.Application.ViewModels.Review.AgentReviewViewModel>()
                .ForMember(dest => dest.PropertyCode, opt => opt.MapFrom(src => src.Property != null ? src.Property.Code : string.Empty))
                .ForMember(dest => dest.PropertyName, opt => opt.MapFrom(src => src.Property != null ? src.Property.Name : string.Empty))
                .ForMember(dest => dest.ClienteName, opt => opt.Ignore())
                .ForMember(dest => dest.ClienteEmail, opt => opt.Ignore());

            #endregion

            #region Commission

            CreateMap<Commission, RealEstateApp.Core.Application.ViewModels.Commission.CommissionViewModel>()
                .ForMember(dest => dest.PropertyCode, opt => opt.MapFrom(src => src.Property != null ? src.Property.Code : string.Empty))
                .ForMember(dest => dest.PropertyName, opt => opt.MapFrom(src => src.Property != null ? src.Property.Name : string.Empty))
                .ForMember(dest => dest.AgentName, opt => opt.Ignore());

            #endregion

            #region PropertyDocument

            CreateMap<PropertyDocument, RealEstateApp.Core.Application.ViewModels.Document.PropertyDocumentViewModel>()
                .ForMember(dest => dest.PropertyCode, opt => opt.MapFrom(src => src.Property != null ? src.Property.Code : string.Empty))
                .ForMember(dest => dest.PropertyName, opt => opt.MapFrom(src => src.Property != null ? src.Property.Name : string.Empty))
                .ForMember(dest => dest.UploadedByName, opt => opt.Ignore());

            #endregion

            #region LeadPipeline

            CreateMap<LeadPipeline, LeadPipelineDto>()
                .ForMember(dest => dest.PropertyName, opt => opt.MapFrom(src => src.Property != null ? src.Property.Name : null));

            #endregion
        }
    }
}
