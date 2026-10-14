using TechChallenge.Domain.Helpers;

namespace TechChallenge.Domain.Enums
{
    public enum StatusOS
    {
        [EnumValueAttribute("1", "Recebida")]
        Recebida = 1,
        [EnumValueAttribute("2", "Em Diagnóstico")]
        EmDiagnostico = 2,
        [EnumValueAttribute("3", "Aguardando Aprovação")]
        AguardandoAprovacao = 3,
        [EnumValueAttribute("4", "Em Execução")]
        EmExecucao = 4,
        [EnumValueAttribute("5", "Finalizada")]
        Finalizada = 5,
        [EnumValueAttribute("6", "Entregue")]
        Entregue = 6,
        [EnumValueAttribute("7", "Reprovada")]
        Reprovada = 7,
        [EnumValueAttribute("8", "Cancelada")]
        Cancelada = 8
    }
}