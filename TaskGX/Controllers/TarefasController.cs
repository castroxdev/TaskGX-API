using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskGX.API.DTOs;
using TaskGX.API.Models;
using TaskGX.API.Services;
using TaskGX.Data;

namespace TaskGX.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TarefasController : ControllerBase
    {
        private readonly TaskGXContext _contexto;

        public TarefasController(TaskGXContext contexto)
        {
            _contexto = contexto;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TarefaDTO>>> ObterTarefas([FromQuery] int listaId)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);

            var listaDoUsuario = await _contexto.Listas.AnyAsync(lista => lista.ID == listaId && lista.UsuarioID == usuarioId);
            if (!listaDoUsuario)
                return NotFound();

            var tarefas = await _contexto.Tarefas
                .Include(tarefa => tarefa.Lista)
                .Include(tarefa => tarefa.Prioridade)
                .Where(tarefa => tarefa.ListaID == listaId)
                .OrderBy(tarefa => tarefa.Concluida)
                .ThenBy(tarefa => tarefa.Ordem)
                .ThenBy(tarefa => tarefa.DataCriacao)
                .Select(tarefa => new TarefaDTO
                {
                    ID = tarefa.ID,
                    Titulo = tarefa.Titulo,
                    Descricao = tarefa.Descricao,
                    Tags = tarefa.Tags,
                    Concluida = tarefa.Concluida,
                    Arquivada = tarefa.Arquivada,
                    DataVencimento = tarefa.DataVencimento,
                    DataCriacao = tarefa.DataCriacao,
                    ListaId = tarefa.ListaID,
                    ListaNome = tarefa.Lista != null ? tarefa.Lista.Nome : null,
                    PrioridadeId = tarefa.PrioridadeID,
                    PrioridadeNome = tarefa.Prioridade != null ? tarefa.Prioridade.Nome : null
                })
                .ToListAsync();

            return Ok(tarefas);
        }

        [HttpPost]
        public async Task<ActionResult<TarefaDTO>> CriarTarefa([FromBody] CriarTarefaRequest requisicao)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);

            var lista = await _contexto.Listas
                .FirstOrDefaultAsync(item => item.ID == requisicao.ListaID && item.UsuarioID == usuarioId);

            if (lista == null)
                return NotFound("Lista nao encontrada.");

            var tarefa = new Tarefa
            {
                ListaID = requisicao.ListaID,
                Titulo = requisicao.Titulo.Trim(),
                Descricao = string.IsNullOrWhiteSpace(requisicao.Descricao) ? null : requisicao.Descricao.Trim(),
                Tags = string.IsNullOrWhiteSpace(requisicao.Tags) ? null : requisicao.Tags.Trim(),
                PrioridadeID = requisicao.PrioridadeID,
                Concluida = requisicao.Concluida,
                Arquivada = requisicao.Arquivada,
                DataVencimento = NormalizarDataUtc(requisicao.DataVencimento),
                Ordem = requisicao.Ordem,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };

            _contexto.Tarefas.Add(tarefa);
            await _contexto.SaveChangesAsync();

            string? prioridadeNome = null;
            if (tarefa.PrioridadeID.HasValue)
            {
                prioridadeNome = await _contexto.Prioridades
                    .Where(prioridade => prioridade.ID == tarefa.PrioridadeID.Value)
                    .Select(prioridade => prioridade.Nome)
                    .FirstOrDefaultAsync();
            }

            return CreatedAtAction(nameof(ObterTarefas), new { listaId = tarefa.ListaID }, new TarefaDTO
            {
                ID = tarefa.ID,
                Titulo = tarefa.Titulo,
                Descricao = tarefa.Descricao,
                Tags = tarefa.Tags,
                Concluida = tarefa.Concluida,
                Arquivada = tarefa.Arquivada,
                DataVencimento = tarefa.DataVencimento,
                DataCriacao = tarefa.DataCriacao,
                ListaId = tarefa.ListaID,
                ListaNome = lista.Nome,
                PrioridadeId = tarefa.PrioridadeID,
                PrioridadeNome = prioridadeNome
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarTarefa(int id, [FromBody] AtualizarTarefaRequest requisicao)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);
            if (id != requisicao.ID)
                return BadRequest();

            var tarefa = await _contexto.Tarefas
                .Include(item => item.Lista)
                .FirstOrDefaultAsync(item => item.ID == id && item.Lista != null && item.Lista.UsuarioID == usuarioId);

            if (tarefa == null)
                return NotFound();

            tarefa.Titulo = requisicao.Titulo.Trim();
            tarefa.Descricao = string.IsNullOrWhiteSpace(requisicao.Descricao) ? null : requisicao.Descricao.Trim();
            tarefa.Tags = string.IsNullOrWhiteSpace(requisicao.Tags) ? null : requisicao.Tags.Trim();
            tarefa.Concluida = requisicao.Concluida;
            tarefa.Arquivada = requisicao.Arquivada;
            tarefa.DataVencimento = NormalizarDataUtc(requisicao.DataVencimento);
            tarefa.PrioridadeID = requisicao.PrioridadeID;
            tarefa.Ordem = requisicao.Ordem;
            tarefa.DataAtualizacao = DateTime.UtcNow;

            if (tarefa.ListaID != requisicao.ListaID)
            {
                var novaLista = await _contexto.Listas.AnyAsync(lista => lista.ID == requisicao.ListaID && lista.UsuarioID == usuarioId);
                if (!novaLista)
                    return BadRequest("Lista destino invalida.");

                tarefa.ListaID = requisicao.ListaID;
            }

            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletarTarefa(int id)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);

            var tarefa = await _contexto.Tarefas
                .Include(item => item.Lista)
                .FirstOrDefaultAsync(item => item.ID == id && item.Lista != null && item.Lista.UsuarioID == usuarioId);

            if (tarefa == null)
                return NotFound();

            _contexto.Tarefas.Remove(tarefa);
            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/concluir")]
        public async Task<IActionResult> ConcluirTarefa(int id)
        {
            var usuarioId = TokenService.ObterUsuarioId(User);

            var tarefa = await _contexto.Tarefas
                .Include(item => item.Lista)
                .FirstOrDefaultAsync(item => item.ID == id && item.Lista != null && item.Lista.UsuarioID == usuarioId);

            if (tarefa == null)
                return NotFound();

            tarefa.Concluida = true;
            tarefa.DataAtualizacao = DateTime.UtcNow;
            await _contexto.SaveChangesAsync();
            return NoContent();
        }

        private static DateTime? NormalizarDataUtc(DateTime? data)
        {
            if (!data.HasValue)
                return null;

            return data.Value.Kind switch
            {
                DateTimeKind.Utc => data.Value,
                DateTimeKind.Local => data.Value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(data.Value, DateTimeKind.Utc)
            };
        }
    }
}
