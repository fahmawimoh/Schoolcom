using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Features.Timetables;
using SchoolCom.Application.Features.Timetables.Commands.CreateTimetableEntry;
using SchoolCom.Application.Features.Timetables.Commands.DeleteTimetableEntry;
using SchoolCom.Application.Features.Timetables.Commands.UpdateTimetableEntry;
using SchoolCom.Application.Features.Timetables.Queries.GetTimetableEntries;
using SchoolCom.Application.Features.Timetables.Queries.GetTimetableEntryById;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/admin/timetables")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("api")]
public class TimetablesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTimetableEntries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? schoolClassId = null,
        [FromQuery] Guid? sectionId = null,
        [FromQuery] Guid? teacherId = null,
        [FromQuery] Guid? subjectId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetTimetableEntriesQuery(
                page, pageSize, schoolClassId, sectionId, teacherId, subjectId, sortBy, sortDesc), ct);
        return Ok(new ApiResponse<TimetableEntryListResult>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTimetableEntry(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTimetableEntryByIdQuery(id), ct);
        return Ok(new ApiResponse<TimetableEntryDto>(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTimetableEntry(
        [FromBody] CreateTimetableEntryRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateTimetableEntryCommand(
                request.SchoolClassId,
                request.SectionId,
                request.TeacherId,
                request.SubjectId,
                request.RoomId,
                request.DayOfWeek,
                request.PeriodNumber,
                request.StartTime,
                request.EndTime), ct);
        return CreatedAtAction(nameof(GetTimetableEntry), new { id = result.Id }, new ApiResponse<TimetableEntryDto>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTimetableEntry(
        Guid id,
        [FromBody] UpdateTimetableEntryRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateTimetableEntryCommand(
                id,
                request.SchoolClassId,
                request.SectionId,
                request.TeacherId,
                request.SubjectId,
                request.RoomId,
                request.DayOfWeek,
                request.PeriodNumber,
                request.StartTime,
                request.EndTime), ct);
        return Ok(new ApiResponse<TimetableEntryDto>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTimetableEntry(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteTimetableEntryCommand(id), ct);
        return Ok(new ApiResponse<object?>(null));
    }
}
