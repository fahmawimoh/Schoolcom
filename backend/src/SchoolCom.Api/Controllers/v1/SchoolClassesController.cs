using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Features.SchoolClasses;
using SchoolCom.Application.Features.SchoolClasses.Commands.CreateSchoolClass;
using SchoolCom.Application.Features.SchoolClasses.Commands.DeleteSchoolClass;
using SchoolCom.Application.Features.SchoolClasses.Commands.UpdateSchoolClass;
using SchoolCom.Application.Features.SchoolClasses.Queries.GetSchoolClassById;
using SchoolCom.Application.Features.SchoolClasses.Queries.GetSchoolClasses;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/admin/school-classes")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("api")]
public class SchoolClassesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSchoolClasses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetSchoolClassesQuery(page, pageSize, search, isActive, sortBy, sortDesc), ct);
        return Ok(new ApiResponse<SchoolClassListResult>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSchoolClass(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSchoolClassByIdQuery(id), ct);
        return Ok(new ApiResponse<SchoolClassDto>(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSchoolClass(
        [FromBody] CreateSchoolClassRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateSchoolClassCommand(
                request.NameEn, request.NameAr, request.DescriptionEn, request.DescriptionAr,
                request.SortOrder, request.IsActive), ct);
        return CreatedAtAction(nameof(GetSchoolClass), new { id = result.Id }, new ApiResponse<SchoolClassDto>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSchoolClass(
        Guid id,
        [FromBody] UpdateSchoolClassRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateSchoolClassCommand(
                id, request.NameEn, request.NameAr, request.DescriptionEn, request.DescriptionAr,
                request.SortOrder, request.IsActive), ct);
        return Ok(new ApiResponse<SchoolClassDto>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSchoolClass(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteSchoolClassCommand(id), ct);
        return Ok(new ApiResponse<object?>(null));
    }
}
