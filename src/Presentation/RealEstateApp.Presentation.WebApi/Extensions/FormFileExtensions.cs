using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using RealEstateApp.Core.Application.DTOs.Common;

namespace RealEstateApp.Presentation.WebApi.Extensions
{
    public static class FormFileExtensions
    {
        public static FileUploadDto? ToFileUploadDto(this IFormFile? file)
        {
            if (file == null)
            {
                return null;
            }

            return new FileUploadDto(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType,
                file.Length);
        }

        public static List<FileUploadDto>? ToFileUploadDtos(this IEnumerable<IFormFile>? files)
        {
            if (files == null)
            {
                return null;
            }

            return files
                .Select(f => f.ToFileUploadDto())
                .Where(dto => dto != null)
                .Cast<FileUploadDto>()
                .ToList();
        }
    }
}
