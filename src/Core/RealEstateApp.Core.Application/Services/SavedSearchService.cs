using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Application.Interfaces.Repositories;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Application.ViewModels.SavedSearch;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services
{
    public class SavedSearchService : ISavedSearchService
    {
        private readonly ISavedSearchRepository _savedSearchRepository;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IEmailService _emailService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ICurrencyService _currencyService;

        public SavedSearchService(
            ISavedSearchRepository savedSearchRepository,
            IPropertyRepository propertyRepository,
            IEmailService emailService,
            UserManager<IdentityUser> userManager,
            IMapper mapper,
            ICurrencyService currencyService)
        {
            _savedSearchRepository = savedSearchRepository;
            _propertyRepository = propertyRepository;
            _emailService = emailService;
            _userManager = userManager;
            _mapper = mapper;
            _currencyService = currencyService;
        }

        public async Task<List<SavedSearchViewModel>> GetUserSavedSearchesAsync(string userId)
        {
            var entities = await _savedSearchRepository.GetByUserIdAsync(userId);
            var viewModels = new List<SavedSearchViewModel>();

            foreach (var item in entities)
            {
                var vm = _mapper.Map<SavedSearchViewModel>(item);
                vm.SummaryCriteria = BuildCriteriaSummary(item);

                // Calcular cuántas propiedades coinciden actualmente
                var filter = new PropertyFilterViewModel
                {
                    PropertyTypeId = item.PropertyTypeId,
                    SaleTypeId = item.SaleTypeId,
                    ProvinceId = item.ProvinceId,
                    MunicipalityId = item.MunicipalityId,
                    Sector = item.Sector,
                    MinPrice = item.MinPrice,
                    MaxPrice = item.MaxPrice,
                    MinRooms = item.MinRooms,
                    MaxRooms = item.MaxRooms,
                    MinBathrooms = item.MinBathrooms,
                    MaxBathrooms = item.MaxBathrooms,
                    MinSizeInMeters = item.MinSizeInMeters,
                    MaxSizeInMeters = item.MaxSizeInMeters,
                    OnlyFinanciable = item.OnlyFinanciable,
                    OnlyWithVirtualTour = item.OnlyWithVirtualTour
                };

                var matchingProps = await _propertyRepository.GetWithFiltersAsync(filter);
                vm.CurrentMatchingCount = matchingProps.Count;

                viewModels.Add(vm);
            }

            return viewModels;
        }

        public async Task<SavedSearchViewModel?> GetByIdAsync(int id)
        {
            var entity = await _savedSearchRepository.GetByIdAsync(id);
            if (entity == null) return null;

            var vm = _mapper.Map<SavedSearchViewModel>(entity);
            vm.SummaryCriteria = BuildCriteriaSummary(entity);
            return vm;
        }

        public async Task<SaveSavedSearchViewModel> SaveSearchAsync(SaveSavedSearchViewModel vm)
        {
            var entity = _mapper.Map<SavedSearch>(vm);
            if (vm.Id == 0)
            {
                var created = await _savedSearchRepository.AddAsync(entity);
                vm.Id = created.Id;
            }
            else
            {
                await _savedSearchRepository.UpdateAsync(entity);
            }
            return vm;
        }

        public async Task<bool> ToggleEmailAlertsAsync(int id, string userId)
        {
            var entity = await _savedSearchRepository.GetByIdAsync(id);
            if (entity == null || entity.UserId != userId) return false;

            entity.EmailAlertsEnabled = !entity.EmailAlertsEnabled;
            await _savedSearchRepository.UpdateAsync(entity);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            var entity = await _savedSearchRepository.GetByIdAsync(id);
            if (entity == null || entity.UserId != userId) return false;

            await _savedSearchRepository.DeleteAsync(entity);
            return true;
        }

        public async Task CheckAndNotifyMatchesAsync(Property newProperty)
        {
            var matchingSearches = await _savedSearchRepository.GetMatchingSearchesAsync(newProperty);
            if (matchingSearches == null || matchingSearches.Count == 0) return;

            var exchangeRate = await _currencyService.GetExchangeRateAsync();
            var priceDOP = newProperty.Currency == "DOP" ? newProperty.Price : newProperty.Price * exchangeRate;
            var priceUSD = newProperty.Currency == "USD" ? newProperty.Price : (exchangeRate > 0 ? newProperty.Price / exchangeRate : 0);

            foreach (var search in matchingSearches)
            {
                if (search.EmailAlertsEnabled)
                {
                    var user = await _userManager.FindByIdAsync(search.UserId);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        var emailSubject = $"🔔 ¡Nueva propiedad para tu búsqueda \"{search.Name}\"!";
                        var emailBody = BuildEmailNotificationHtml(user.UserName ?? "Cliente", search.Name, newProperty, priceDOP, priceUSD);

                        try
                        {
                            await _emailService.SendAsync(user.Email, emailSubject, emailBody);
                        }
                        catch
                        {
                            // Continuar con los demás usuarios si falla el envío individual de correo
                        }
                    }
                }

                search.LastAlertSent = DateTime.UtcNow;
                await _savedSearchRepository.UpdateAsync(search);
            }
        }

        private static string BuildCriteriaSummary(SavedSearch s)
        {
            var parts = new List<string>();

            if (s.PropertyType != null) parts.Add(s.PropertyType.Name);
            if (s.SaleType != null) parts.Add(s.SaleType.Name);
            if (!string.IsNullOrEmpty(s.Sector)) parts.Add(s.Sector);
            else if (s.Municipality != null) parts.Add(s.Municipality.Name);
            else if (s.Province != null) parts.Add(s.Province.Name);

            if (s.MinPrice.HasValue && s.MaxPrice.HasValue)
                parts.Add($"RD$ {s.MinPrice:N0} - {s.MaxPrice:N0}");
            else if (s.MaxPrice.HasValue)
                parts.Add($"Hasta RD$ {s.MaxPrice:N0}");
            else if (s.MinPrice.HasValue)
                parts.Add($"Desde RD$ {s.MinPrice:N0}");

            if (s.MinRooms.HasValue) parts.Add($"{s.MinRooms}+ Habs");
            if (s.MinBathrooms.HasValue) parts.Add($"{s.MinBathrooms}+ Baños");
            if (s.OnlyFinanciable == true) parts.Add("Financiable");
            if (s.OnlyWithVirtualTour == true) parts.Add("Con Tour 3D");

            return parts.Count > 0 ? string.Join(" • ", parts) : "Todos los inmuebles";
        }

        private static string BuildEmailNotificationHtml(string userName, string searchName, Property prop, decimal priceDOP, decimal priceUSD)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e2e8f0; border-radius: 12px; overflow: hidden;'>");
            sb.AppendLine("  <div style='background: linear-gradient(135deg, #0d6efd, #0dcaf0); padding: 24px; color: #ffffff; text-align: center;'>");
            sb.AppendLine("    <h2 style='margin: 0; font-size: 24px;'>🏠 RealEstateApp</h2>");
            sb.AppendLine("    <p style='margin: 6px 0 0 0; opacity: 0.9;'>¡Nueva propiedad disponible para tu búsqueda guardada!</p>");
            sb.AppendLine("  </div>");
            sb.AppendLine("  <div style='padding: 24px; color: #334155;'>");
            sb.AppendLine($"    <p style='font-size: 16px;'>Hola <strong>{userName}</strong>,</p>");
            sb.AppendLine($"    <p>Se acaba de publicar un inmueble que coincide con tus criterios de <strong>\"{searchName}\"</strong>:</p>");
            sb.AppendLine("    <div style='background: #f8fafc; border: 1px solid #cbd5e1; border-radius: 10px; padding: 16px; margin: 20px 0;'>");
            sb.AppendLine($"      <h3 style='margin: 0 0 8px 0; color: #0f172a;'>{prop.Name}</h3>");
            sb.AppendLine($"      <div style='font-size: 20px; font-weight: bold; color: #059669; margin-bottom: 6px;'>RD$ {priceDOP:N0} <span style='font-size: 14px; color: #64748b; font-weight: normal;'>(~US$ {priceUSD:N0})</span></div>");
            sb.AppendLine($"      <p style='margin: 0 0 8px 0; color: #64748b; font-size: 14px;'>📍 {prop.Sector ?? "Sector no especificado"} | 🛏️ {prop.Rooms} Habitaciones | 🚿 {prop.Bathrooms} Baños | 📐 {prop.SizeInMeters:N0} m²</p>");
            sb.AppendLine($"      <p style='margin: 0; font-size: 14px; color: #475569;'>{prop.Description}</p>");
            sb.AppendLine("    </div>");
            sb.AppendLine("    <div style='text-align: center; margin: 24px 0;'>");
            sb.AppendLine($"      <a href='http://localhost:5173/properties/{prop.Id}' style='display: inline-block; background: #059669; color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 50px; font-weight: bold; font-size: 15px;'>Ver Propiedad Completa</a>");
            sb.AppendLine("    </div>");
            sb.AppendLine("    <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />");
            sb.AppendLine($"    <p style='font-size: 12px; color: #94a3b8; text-align: center;'>Recibes este correo porque tienes activadas las alertas para la búsqueda \"{searchName}\". Puedes gestionar tus alertas desde tu perfil en RealEstateApp.</p>");
            sb.AppendLine("  </div>");
            sb.AppendLine("</div>");
            return sb.ToString();
        }
    }
}
