using FluentValidation;

namespace TechChallenge.Application.Features.OS.RegistrarDiagnostico;

public class RegistrarDiagnosticoCommandValidator : AbstractValidator<RegistrarDiagnosticoCommand>
{
    public RegistrarDiagnosticoCommandValidator()
    {
        RuleFor(command => command.OrdemServicoId)
            .NotEmpty();

        RuleFor(command => command)
            .Must(command =>
                command.Servicos.Count != 0 ||
                command.Produtos.Count != 0)
            .WithMessage("Informe ao menos um serviço ou produto para registrar o diagnóstico.");

        RuleForEach(command => command.Servicos).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            item.RuleFor(i => i.Quantidade).GreaterThan(0);
        });

        RuleForEach(command => command.Produtos).ChildRules(item =>
        {
            item.RuleFor(i => i.Id).NotEmpty();
            item.RuleFor(i => i.Quantidade).GreaterThan(0);
        });
    }
}
