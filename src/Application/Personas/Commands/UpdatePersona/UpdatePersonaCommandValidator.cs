using FluentValidation;

namespace AiKocStudio.Application.Personas.Commands.UpdatePersona;

public class UpdatePersonaCommandValidator : AbstractValidator<UpdatePersonaCommand>
{
    public UpdatePersonaCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ToneOfVoice).MaximumLength(200);
        RuleFor(x => x.TargetAudience).MaximumLength(500);
        RuleFor(x => x.Platform).IsInEnum();
        RuleFor(x => x.DefaultAiProvider).MaximumLength(50);
        RuleFor(x => x.SystemPromptTemplate).MaximumLength(4000);
    }
}
