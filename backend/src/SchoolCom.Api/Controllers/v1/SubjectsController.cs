using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Features.Subjects;
using SchoolCom.Application.Features.Subjects.Commands.CreateSubject;
using SchoolCom.Application.Features.Subjects.Commands.DeleteSubject;
using SchoolCom.Application.Features.Subjects.Commands.UpdateSubject;
using SchoolCom.Application.Features.Subjects.Queries.GetSubjectById;
using SchoolCom.Application.Features.Subjects.Queries.GetSubjects;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/admin/subjects")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("api")]
public class SubjectsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSubjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetSubjectsQuery(page, pageSize, search, isActive, sortBy, sortDesc), ct);
        return Ok(new ApiResponse<SubjectListResult>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSubject(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSubjectByIdQuery(id), ct);
        return Ok(new ApiResponse<SubjectDto>(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubject(
        [FromBody] CreateSubjectRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateSubjectCommand(request.NameEn, request.NameAr, request.Code, request.IsActive), ct);
        return CreatedAtAction(nameof(GetSubject), new { id = result.Id }, new ApiResponse<SubjectDto>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSubject(
        Guid id,
        [FromBody] UpdateSubjectRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateSubjectCommand(id, request.NameEn, request.NameAr, request.Code, request.IsActive), ct);
        return Ok(new ApiResponse<SubjectDto>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSubject(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteSubjectCommand(id), ct);
        return Ok(new ApiResponse<object?>(null));
    }
}
