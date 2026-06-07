using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Features.Teachers;
using SchoolCom.Application.Features.Teachers.Commands.CreateTeacher;
using SchoolCom.Application.Features.Teachers.Commands.DeleteTeacher;
using SchoolCom.Application.Features.Teachers.Commands.SetTeacherAssignments;
using SchoolCom.Application.Features.Teachers.Commands.UpdateTeacher;
using SchoolCom.Application.Features.Teachers.Queries.GetTeacherById;
using SchoolCom.Application.Features.Teachers.Queries.GetTeachers;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/admin/teachers")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("api")]
public class TeachersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTeachers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? subject = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetTeachersQuery(page, pageSize, search, isActive, subject, sortBy, sortDesc), ct);
        return Ok(new ApiResponse<TeacherListResult>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTeacher(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTeacherByIdQuery(id), ct);
        return Ok(new ApiResponse<TeacherDto>(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeacher(
        [FromBody] CreateTeacherRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateTeacherCommand(
                request.FirstNameEn, request.FirstNameAr, request.LastNameEn, request.LastNameAr,
                request.Email, request.Phone, request.EmployeeNumber, request.SubjectEn, request.SubjectAr,
                request.IsActive, request.UserId), ct);
        return CreatedAtAction(nameof(GetTeacher), new { id = result.Id }, new ApiResponse<TeacherDto>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTeacher(
        Guid id,
        [FromBody] UpdateTeacherRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateTeacherCommand(
                id, request.FirstNameEn, request.FirstNameAr, request.LastNameEn, request.LastNameAr,
                request.Email, request.Phone, request.EmployeeNumber, request.SubjectEn, request.SubjectAr,
                request.IsActive, request.UserId), ct);
        return Ok(new ApiResponse<TeacherDto>(result));
    }

    [HttpPut("{id:guid}/assignments")]
    public async Task<IActionResult> SetTeacherAssignments(
        Guid id,
        [FromBody] AssignTeacherRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new SetTeacherAssignmentsCommand(id, request.Assignments), ct);
        return Ok(new ApiResponse<TeacherDto>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTeacher(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteTeacherCommand(id), ct);
        return Ok(new ApiResponse<object?>(null));
    }
}
