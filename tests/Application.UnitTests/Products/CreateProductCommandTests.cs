using AiKocStudio.Application.Products.Commands.CreateProduct;
using AiKocStudio.Application.UnitTests.Common;
using FluentAssertions;

namespace AiKocStudio.Application.UnitTests.Products;

public class CreateProductCommandTests
{
    private static CreateProductCommand ValidCommand(string name = "Aurora Serum") => new(
        Name: name,
        Description: "Brightening vitamin C serum",
        Category: "Skincare",
        KeyFeatures: ["Vitamin C", "Fragrance-free"],
        TargetPersonaId: null);

    private static CreateProductCommandValidator CreateValidator(TestApplicationDbContext context) => new(context);

    [Fact]
    public async Task Validator_MissingName_Fails()
    {
        using var context = TestApplicationDbContext.Create();
        var validator = CreateValidator(context);

        var result = await validator.ValidateAsync(ValidCommand(name: ""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public async Task Validator_ValidCommand_Passes()
    {
        using var context = TestApplicationDbContext.Create();
        var validator = CreateValidator(context);

        var result = await validator.ValidateAsync(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_NonExistentTargetPersonaId_Fails()
    {
        using var context = TestApplicationDbContext.Create();
        var validator = CreateValidator(context);

        var result = await validator.ValidateAsync(ValidCommand() with { TargetPersonaId = Guid.NewGuid() });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductCommand.TargetPersonaId));
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsProduct()
    {
        using var context = TestApplicationDbContext.Create();
        var handler = new CreateProductCommandHandler(context);

        var id = await handler.Handle(ValidCommand(), CancellationToken.None);

        var persisted = context.Products.Single(p => p.Id == id);
        persisted.Name.Should().Be("Aurora Serum");
        persisted.KeyFeatures.Should().Contain("Vitamin C");
        persisted.IsActive.Should().BeTrue();
    }
}
