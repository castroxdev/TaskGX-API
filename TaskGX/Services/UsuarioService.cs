using Microsoft.EntityFrameworkCore;
using TaskGX.Data;

namespace TaskGX.API.Services
{
    public enum ResultadoEliminacaoConta
    {
        Eliminada,
        UsuarioNaoEncontrado
    }

    public class UsuarioService
    {
        private readonly TaskGXContext _contexto;

        public UsuarioService(TaskGXContext contexto)
        {
            _contexto = contexto;
        }

        public Task<ResultadoEliminacaoConta> EliminarContaAsync(int usuarioId)
        {
            var estrategia = _contexto.Database.CreateExecutionStrategy();

            return estrategia.ExecuteAsync(async () =>
            {
                await using var transacao = await _contexto.Database.BeginTransactionAsync();

                var usuarioExiste = await _contexto.Usuarios.AnyAsync(usuario => usuario.ID == usuarioId);
                if (!usuarioExiste)
                {
                    await transacao.RollbackAsync();
                    return ResultadoEliminacaoConta.UsuarioNaoEncontrado;
                }

                var listasDoUsuario = _contexto.Listas
                    .Where(lista => lista.UsuarioID == usuarioId)
                    .Select(lista => lista.ID);

                await _contexto.Tarefas
                    .Where(tarefa => listasDoUsuario.Contains(tarefa.ListaID))
                    .ExecuteDeleteAsync();

                await _contexto.Listas
                    .Where(lista => lista.UsuarioID == usuarioId)
                    .ExecuteDeleteAsync();

                var usuariosEliminados = await _contexto.Usuarios
                    .Where(usuario => usuario.ID == usuarioId)
                    .ExecuteDeleteAsync();

                if (usuariosEliminados == 0)
                {
                    await transacao.RollbackAsync();
                    return ResultadoEliminacaoConta.UsuarioNaoEncontrado;
                }

                await transacao.CommitAsync();
                return ResultadoEliminacaoConta.Eliminada;
            });
        }
    }
}
