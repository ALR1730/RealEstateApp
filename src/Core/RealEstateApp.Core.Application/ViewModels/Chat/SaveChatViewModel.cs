using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Chat
{
    /// <summary>
    /// ViewModel para formulario de envío de mensaje en el chat.
    /// Hilo bidireccional directo entre cliente y agente por propiedad.
    /// </summary>
    public class SaveChatViewModel
    {
        public int? PropertyId { get; set; }

        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(2000, ErrorMessage = "El mensaje no puede exceder 2000 caracteres")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mensaje")]
        public string MessageContent { get; set; } = string.Empty;

        /// <summary>
        /// ID del destinatario (agente o cliente según el emisor).
        /// </summary>
        [Required(ErrorMessage = "El destinatario es requerido")]
        public string RecipientId { get; set; } = string.Empty;
    }
}
