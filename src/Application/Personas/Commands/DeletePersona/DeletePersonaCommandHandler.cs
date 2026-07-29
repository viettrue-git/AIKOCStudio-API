using AiKocStudio.Application.Common.Exceptions;
using AiKocStudio.Application.Common.Interfaces;
using AiKocStudio.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Personas.Commands.DeletePersona;

public class DeletePersonaCommandHandler(IApplicationDbContext context) : IRequestHandler<DeletePersonaCommand>
{
    public async Task Handle(DeletePersonaCommand request, CancellationToken cancellationToken)
    {
        var persona = await context.Personas.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Persona), request.Id);

        persona.IsDeleted = true;
        persona.DeletedAt = DateTimeOffset.UtcNow;

        // The DB-level FK SetNull (ProductConfiguration) only fires on a real DELETE,
        // which never happens here — emulate it at the application level so a
        // soft-deleted Persona doesn't leave Products pointing at a now-invisible row
        // (which would otherwise also block those Products from ever being updated,
        // since UpdateProductCommandValidator checks TargetPersonaId against the
        // same tenant-scoped, non-deleted Personas query).
        var referencingProducts = await context.Products
            .Where(p => p.TargetPersonaId == request.Id)
            .ToListAsync(cancellationToken);

        foreach (var product in referencingProducts)
        {
            product.TargetPersonaId = null;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
