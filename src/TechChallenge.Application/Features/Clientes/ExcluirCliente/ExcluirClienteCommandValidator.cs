using FluentValidation;

namespace TechChallenge.Application.Features.Clientes.ExcluirCliente;

public class ExcluirClienteCommandValidator : AbstractValidator<ExcluirClienteCommand>
{
    public ExcluirClienteCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
