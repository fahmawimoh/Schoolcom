using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolCom.Api.Models;
using SchoolCom.Application.Features.Students;
using SchoolCom.Application.Features.Students.Commands.CreateStudent;
using SchoolCom.Application.Features.Students.Commands.DeleteStudent;
using SchoolCom.Application.Features.Students.Commands.UpdateStudent;
using SchoolCom.Application.Features.Students.Queries.GetStudentById;
using SchoolCom.Application.Features.Students.Queries.GetStudentFilters;
using SchoolCom.Application.Features.Students.Queries.GetStudents;

namespace SchoolCom.Api.Controllers.v1;

[ApiController]
[Route("v{version:apiVersion}/admin/students")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize(Policy = "AdminOnly")]
[EnableRateLimiting("api")]
public class StudentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] Guid? schoolClassId = null,
        [FromQuery] Guid? sectionId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? lang = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetStudentsQuery(page, pageSize, schoolClassId, sectionId, search, lang), ct);
        return Ok(new ApiResponse<StudentListResult>(result));
    }

    [HttpGet("filters")]
    public async Task<IActionResult> GetFilters(CancellationToken ct)
    {
        var result = await mediator.Send(new GetStudentFiltersQuery(), ct);
        return Ok(new ApiResponse<StudentFiltersDto>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetStudent(
        Guid id,
        [FromQuery] string? lang = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetStudentByIdQuery(id, lang), ct);
        return Ok(new ApiResponse<StudentDto>(result));
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent(
        [FromBody] CreateStudentRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateStudentCommand(
                request.StudentNumber,
                request.FirstNameEn,
                request.FirstNameAr,
                request.LastNameEn,
                request.LastNameAr,
                request.SchoolClassId,
                request.SectionId,
                request.ParentId,
                request.ParentName,
                request.ParentPhone,
                request.ParentEmail,
                request.IsActive),
            ct);
        return CreatedAtAction(nameof(GetStudent), new { id = result.Id }, new ApiResponse<StudentDto>(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateStudent(
        Guid id,
        [FromBody] UpdateStudentRequest request,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new UpdateStudentCommand(
                id,
                request.StudentNumber,
                request.FirstNameEn,
                request.FirstNameAr,
                request.LastNameEn,
                request.LastNameAr,
                request.SchoolClassId,
                request.SectionId,
                request.ParentId,
                request.ParentName,
                request.ParentPhone,
                request.ParentEmail,
                request.IsActive),
            ct);
        return Ok(new ApiResponse<StudentDto>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStudent(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteStudentCommand(id), ct);
        return Ok(new ApiResponse<object?>(null));
    }
}
