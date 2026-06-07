using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Features.Sections;
using SchoolCom.Application.Features.Sections.Commands.CreateSection;
using SchoolCom.Application.Features.Sections.Commands.DeleteSection;
using SchoolCom.Application.Features.Sections.Commands.UpdateSection;
using SchoolCom.Application.Features.Sections.Queries.GetSectionById;
using SchoolCom.Application.Features.Sections.Queries.GetSections;
using SchoolCom.Application.Features.Sections.Queries.GetSectionsBySchoolClass;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/admin/sections")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("api")]
public class SectionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSections(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? schoolClassId = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetSectionsQuery(page, pageSize, schoolClassId, search, isActive, sortBy, sortDesc), ct);
        return Ok(new ApiResponse<SectionListResult>(result));
    }

    [HttpGet("by-school-class/{schoolClassId:guid}")]
    public async Task<IActionResult> GetSectionsBySchoolClass(Guid schoolClassId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSectionsBySchoolClassQuery(schoolClassId), ct);
        return Ok(new ApiResponse<IReadOnlyList<SchoolClassSectionOptionDto>>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSection(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSectionByIdQuery(id), ct);
        return Ok(new ApiResponse<SectionDto>(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSection(
        [FromBody] CreateSectionRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateSectionCommand(
                request.SchoolClassId,
                request.NameEn,
                request.NameAr,
                request.DescriptionEn,
                request.DescriptionAr,
                request.IsActive), ct);
        return CreatedAtAction(nameof(GetSection), new { id = result.Id }, new ApiResponse<SectionDto>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSection(
        Guid id,
        [FromBody] UpdateSectionRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateSectionCommand(
                id, request.NameEn, request.NameAr, request.DescriptionEn, request.DescriptionAr,
                request.IsActive), ct);
        return Ok(new ApiResponse<SectionDto>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSection(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteSectionCommand(id), ct);
        return Ok(new ApiResponse<object?>(null));
    }
}
