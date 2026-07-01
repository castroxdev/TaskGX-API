using Microsoft.EntityFrameworkCore;
using TaskGX.API.Models;
using TaskGX.Data;

namespace TaskGX.API.Repositories
{
    public class RecuperacaoSenhaRepository
    {
        private readonly TaskGXContext _contexto;

        public RecuperacaoSenhaRepository(TaskGXContext contexto)
        {
            _contexto = contexto;
        }

        public Task<RecuperacaoSenha?> ObterPorUsuarioIdAsync(int usuarioId)
        {
            return _contexto.RecuperacoesSenha
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.UsuarioID == usuarioId);
        }

        public Task CriarOuSubstituirAsync(RecuperacaoSenha recuperacao)
        {
            return _contexto.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "RecuperacoesSenha"
                    ("UsuarioID", "CodigoHash", "Expiracao", "TentativasInvalidas", "CriadoEm")
                VALUES
                    ({recuperacao.UsuarioID}, {recuperacao.CodigoHash}, {recuperacao.Expiracao},
                     {recuperacao.TentativasInvalidas}, {recuperacao.CriadoEm})
                ON CONFLICT ("UsuarioID") DO UPDATE SET
                    "CodigoHash" = EXCLUDED."CodigoHash",
                    "Expiracao" = EXCLUDED."Expiracao",
                    "TentativasInvalidas" = EXCLUDED."TentativasInvalidas",
                    "CriadoEm" = EXCLUDED."CriadoEm"
                """);
        }

        public Task<int> RegistrarTentativaInvalidaAsync(
            int usuarioId,
            string codigoHashEsperado,
            int maximoTentativas)
        {
            return _contexto.RecuperacoesSenha
                .Where(item =>
                    item.UsuarioID == usuarioId &&
                    item.CodigoHash == codigoHashEsperado &&
                    item.TentativasInvalidas < maximoTentativas)
                .ExecuteUpdateAsync(atualizacao => atualizacao
                    .SetProperty(
                        item => item.TentativasInvalidas,
                        item => item.TentativasInvalidas + 1));
        }

        public Task<int> RemoverSeCorresponderAsync(int usuarioId, string codigoHashEsperado)
        {
            return _contexto.RecuperacoesSenha
                .Where(item =>
                    item.UsuarioID == usuarioId &&
                    item.CodigoHash == codigoHashEsperado)
                .ExecuteDeleteAsync();
        }

        public async Task<bool> RedefinirSenhaAsync(
            int usuarioId,
            string codigoHashEsperado,
            DateTime instanteAtual,
            int maximoTentativas,
            string novaSenhaHash)
        {
            var estrategia = _contexto.Database.CreateExecutionStrategy();

            return await estrategia.ExecuteAsync(async () =>
            {
                await using var transacao = await _contexto.Database.BeginTransactionAsync();

                var recuperacoesRemovidas = await _contexto.RecuperacoesSenha
                    .Where(item =>
                        item.UsuarioID == usuarioId &&
                        item.CodigoHash == codigoHashEsperado &&
                        item.Expiracao >= instanteAtual &&
                        item.TentativasInvalidas < maximoTentativas)
                    .ExecuteDeleteAsync();

                if (recuperacoesRemovidas != 1)
                {
                    await transacao.RollbackAsync();
                    return false;
                }

                var usuariosAtualizados = await _contexto.Usuarios
                    .Where(usuario => usuario.ID == usuarioId)
                    .ExecuteUpdateAsync(atualizacao => atualizacao
                        .SetProperty(usuario => usuario.SenhaHash, novaSenhaHash)
                        .SetProperty(usuario => usuario.DataAtualizacao, instanteAtual));

                if (usuariosAtualizados != 1)
                {
                    await transacao.RollbackAsync();
                    return false;
                }

                await transacao.CommitAsync();
                return true;
            });
        }
    }
}
