using AiKocStudio.Application.Common.Models;
using AiKocStudio.Application.Personas.Commands.CreatePersona;
using AiKocStudio.Application.Personas.Commands.DeletePersona;
using AiKocStudio.Application.Personas.Commands.UpdatePersona;
using AiKocStudio.Application.Personas.Commands.UploadPersonaAvatar;
using AiKocStudio.Application.Personas.Queries.GetPersonaById;
using AiKocStudio.Application.Personas.Queries.GetPersonas;
using AiKocStudio.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AiKocStudio.WebApi.Controllers;

[ApiController]
[Route("api/personas")]
public class PersonasController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Platform? platformFilter = null,
        [FromQuery] bool? isActiveFilter = null,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetPersonasQuery(pageNumber, pageSize, searchTerm, platformFilter, isActiveFilter),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetPersonaByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePersonaCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePersonaCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("Route id does not match body id.");
        }

        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePersonaCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/avatar")]
    [RequestSizeLimit(AllowedImageContentTypes.MaxSizeBytes)]
    public async Task<IActionResult> UploadAvatar(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        if (file.Length > AllowedImageContentTypes.MaxSizeBytes)
        {
            return BadRequest($"File exceeds the {AllowedImageContentTypes.MaxSizeBytes} byte limit.");
        }

        await using var stream = file.OpenReadStream();
        var url = await mediator.Send(
            new UploadPersonaAvatarCommand(id, stream, file.FileName, file.ContentType),
            cancellationToken);

        return Ok(new { url });
    }
}
