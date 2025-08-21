using System.ComponentModel.DataAnnotations;

namespace MagnaPP.Api.DTOs;

public class CreateSessionRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Avatar { get; set; } = string.Empty;
}