using FluentValidation;

namespace TechChallenge.Application.Features.Servicos.CriarServico;

public class CriarServicoCommandValidator : AbstractValidator<CriarServicoCommand>
{
    public CriarServicoCommandValidator()
    {
        RuleFor(command => command.Descricao)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Valor)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.CategoriaId)
            .NotEmpty();
    }
}
