using AiKocStudio.Application.Personas.Commands.CreatePersona;
using AiKocStudio.Application.UnitTests.Common;
using AiKocStudio.Domain.Enums;
using FluentAssertions;

namespace AiKocStudio.Application.UnitTests.Personas;

public class CreatePersonaCommandTests
{
    private static CreatePersonaCommand ValidCommand(string name = "Mia Chen") => new(
        Name: name,
        Description: "Beauty & skincare KOC",
        ToneOfVoice: "Warm, specific",
        TargetAudience: "Gen Z skincare enthusiasts",
        Platform: Platform.TikTok,
        DefaultAiProvider: null,
        SystemPromptTemplate: "You are Mia.");

    [Fact]
    public void Validator_MissingName_Fails()
    {
        var validator = new CreatePersonaCommandValidator();

        var result = validator.Validate(ValidCommand(name: ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePersonaCommand.Name));
    }

    [Fact]
    public void Validator_ValidCommand_Passes()
    {
        var validator = new CreatePersonaCommandValidator();

        var result = validator.Validate(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsPersona()
    {
        using var context = TestApplicationDbContext.Create();
        var handler = new CreatePersonaCommandHandler(context);

        var id = await handler.Handle(ValidCommand(), CancellationToken.None);

        var persisted = await Task.FromResult(context.Personas.Single(p => p.Id == id));
        persisted.Name.Should().Be("Mia Chen");
        persisted.IsActive.Should().BeTrue();
    }
}
