using AiKocStudio.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AiKocStudio.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator(IApplicationDbContext context)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Category).MaximumLength(200);

        RuleFor(x => x.TargetPersonaId)
            .MustAsync(async (id, cancellationToken) =>
                id is null || await context.Personas.AnyAsync(p => p.Id == id, cancellationToken))
            .WithMessage("TargetPersonaId does not reference an existing Persona.");
    }
}
