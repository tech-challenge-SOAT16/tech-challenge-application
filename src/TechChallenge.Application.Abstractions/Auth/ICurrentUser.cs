using System;
using TechChallenge.Domain.Enums;

namespace TechChallenge.Application.Abstractions.Auth
{
    public interface ICurrentUser
    {
        Guid? UsuarioId { get; }
        Guid? FuncionarioId { get; }
        TipoUsuario? TipoUsuario { get; }
        bool EstaAutenticado { get; }
    }
}
