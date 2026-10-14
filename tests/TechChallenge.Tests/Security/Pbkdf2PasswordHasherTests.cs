using FluentAssertions;
using TechChallenge.Infrastructure.Auth;

namespace TechChallenge.Tests.Security;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void HashDeveSerDiferenteDaSenhaEVerificarComSucesso()
    {
        var hash = _hasher.Hash("Senha@123");

        hash.Should().NotBe("Senha@123");
        _hasher.Verify("Senha@123", hash).Should().BeTrue();
    }

    [Fact]
    public void VerifyDeveFalharComSenhaErrada()
    {
        var hash = _hasher.Hash("Senha@123");

        _hasher.Verify("Outra@123", hash).Should().BeFalse();
    }

    [Fact]
    public void HashesDaMesmaSenhaDevemSerDiferentes()
    {
        // Salt aleatório por hash.
        _hasher.Hash("Senha@123").Should().NotBe(_hasher.Hash("Senha@123"));
    }

    [Fact]
    public void VerifyDeveFalharComHashMalformado()
    {
        _hasher.Verify("Senha@123", "formato-invalido").Should().BeFalse();
    }
}
