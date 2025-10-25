using Bookify.Domain.Entities;

namespace Bookify.Domain.Dtos
{
    public record ManageUserResponseDto(
        bool IsSucceeeded,
        ApplicationUser? User,
        string? VerificationCode,
        IEnumerable<string>? Errors
    );
}
