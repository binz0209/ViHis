
namespace ViHis.Application.DTOs;
public record PersonListDto(string Id, string Slug, string FullName, string? AltName);
public record PersonDetailDto(string Id, string Slug, string FullName, string? AltName, string? Summary, string? DynastyId, string? PeriodId);
public record CreatePersonDto(string FullName, string? AltName, string? Summary, string? DynastyId, string? PeriodId);
public record UpdatePersonDto(string FullName, string? AltName, string? Summary, string? DynastyId, string? PeriodId);
