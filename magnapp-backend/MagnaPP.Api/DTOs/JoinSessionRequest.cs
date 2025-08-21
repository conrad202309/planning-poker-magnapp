using System.ComponentModel.DataAnnotations;

namespace MagnaPP.Api.DTOs;

public class JoinSessionRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Avatar { get; set; } = string.Empty;
}