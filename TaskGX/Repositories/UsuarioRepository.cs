using Microsoft.EntityFrameworkCore;
using TaskGX.API.Models;
using TaskGX.Data;

namespace TaskGX.API.Repositories
{
    public class UsuarioRepository
    {
        private readonly TaskGXContext _contexto;

        public UsuarioRepository(TaskGXContext contexto)
        {
            _contexto = contexto;
        }

        public Task<Usuario?> ObterPorEmailAsync(string email)
        {
            return _contexto.Usuarios
                .AsNoTracking()
                .Where(usuario => usuario.Email == email)
                .Select(usuario => new Usuario
                {
                    ID = usuario.ID,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    EmailPendente = usuario.EmailPendente,
                    SenhaHash = usuario.SenhaHash,
                    Avatar = usuario.Avatar,
                    Ativo = usuario.Ativo,
                    EmailVerificado = usuario.EmailVerificado,
                    CodigoVerificacao = usuario.CodigoVerificacao,
                    CodigoVerificacaoExpiracao = usuario.CodigoVerificacaoExpiracao,
                    CriadoEm = usuario.CriadoEm,
                    DataAtualizacao = usuario.DataAtualizacao
                })
                .FirstOrDefaultAsync();
        }

        public Task<Usuario?> ObterPorEmailIgnorandoMaiusculasAsync(string email)
        {
            var emailNormalizado = (email ?? string.Empty).Trim().ToLowerInvariant();

            return _contexto.Usuarios
                .AsNoTracking()
                .Where(usuario => usuario.Email.ToLower() == emailNormalizado)
                .Select(usuario => new Usuario
                {
                    ID = usuario.ID,
                    Nome = usuario.Nome,
                    Email = usuario.Email,
                    EmailPendente = usuario.EmailPendente,
                    SenhaHash = usuario.SenhaHash,
                    Avatar = usuario.Avatar,
                    Ativo = usuario.Ativo,
                    EmailVerificado = usuario.EmailVerificado,
                    CodigoVerificacao = usuario.CodigoVerificacao,
                    CodigoVerificacaoExpiracao = usuario.CodigoVerificacaoExpiracao,
                    CriadoEm = usuario.CriadoEm,
                    DataAtualizacao = usuario.DataAtualizacao
                })
                .FirstOrDefaultAsync();
        }

        public Task<Usuario?> ObterPorIdAsync(int usuarioId)
        {
            return _contexto.Usuarios.AsNoTracking().FirstOrDefaultAsync(usuario => usuario.ID == usuarioId);
        }

        public Task<Usuario?> ObterParaEdicaoPorIdAsync(int usuarioId)
        {
            return _contexto.Usuarios.FirstOrDefaultAsync(usuario => usuario.ID == usuarioId);
        }

        public Task<bool> ExisteEmailAsync(string email)
        {
            return _contexto.Usuarios.AnyAsync(usuario => usuario.Email == email);
        }

        public Task<bool> ExisteEmailEmOutroUsuarioAsync(string email, int usuarioId)
        {
            return _contexto.Usuarios.AnyAsync(usuario => usuario.Email == email && usuario.ID != usuarioId);
        }

        public async Task InserirAsync(Usuario usuario)
        {
            _contexto.Usuarios.Add(usuario);
            await _contexto.SaveChangesAsync();
        }

        public async Task AtualizarDadosAsync(int usuarioId, string nome, string email)
        {
            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(item => item.ID == usuarioId);
            if (usuario == null)
                return;

            usuario.Nome = nome;
            usuario.Email = email;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _contexto.SaveChangesAsync();
        }

        public async Task AtualizarSenhaAsync(int usuarioId, string senhaHash)
        {
            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(item => item.ID == usuarioId);
            if (usuario == null)
                return;

            usuario.SenhaHash = senhaHash;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _contexto.SaveChangesAsync();
        }

        public async Task AtualizarVerificacaoEmailAsync(
            int usuarioId,
            bool emailVerificado,
            bool ativo,
            string? codigoVerificacao,
            DateTime? expiracao)
        {
            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(item => item.ID == usuarioId);
            if (usuario == null)
                return;

            usuario.EmailVerificado = emailVerificado;
            usuario.Ativo = ativo;
            usuario.CodigoVerificacao = codigoVerificacao;
            usuario.CodigoVerificacaoExpiracao = expiracao;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _contexto.SaveChangesAsync();
        }

        public async Task AtualizarSolicitacaoAlteracaoEmailAsync(
            int usuarioId,
            string novoEmail,
            string codigoVerificacao,
            DateTime expiracao)
        {
            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(item => item.ID == usuarioId);
            if (usuario == null)
                return;

            usuario.EmailPendente = novoEmail;
            usuario.CodigoVerificacao = codigoVerificacao;
            usuario.CodigoVerificacaoExpiracao = expiracao;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _contexto.SaveChangesAsync();
        }

        public async Task ConfirmarAlteracaoEmailAsync(int usuarioId, string novoEmail)
        {
            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(item => item.ID == usuarioId);
            if (usuario == null)
                return;

            usuario.Email = novoEmail;
            usuario.EmailPendente = null;
            usuario.EmailVerificado = true;
            usuario.CodigoVerificacao = null;
            usuario.CodigoVerificacaoExpiracao = null;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _contexto.SaveChangesAsync();
        }
    }
}
