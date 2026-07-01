using Microsoft.EntityFrameworkCore;
using TaskGX.API.Models;

namespace TaskGX.Data
{
    public class TaskGXContext : DbContext
    {
        public TaskGXContext(DbContextOptions<TaskGXContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Lista> Listas { get; set; } = null!;
        public DbSet<Tarefa> Tarefas { get; set; } = null!;
        public DbSet<Prioridade> Prioridades { get; set; } = null!;
        public DbSet<RecuperacaoSenha> RecuperacoesSenha { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ID).HasColumnName("ID");
                entity.Property(e => e.Nome).HasColumnName("Nome").HasMaxLength(100);
                entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(150);
                entity.Property(e => e.EmailPendente).HasColumnName("EmailPendente").HasMaxLength(150);
                entity.Property(e => e.SenhaHash).HasColumnName("Senha");
                entity.Property(e => e.Avatar).HasColumnName("Avatar").HasMaxLength(255);
                entity.Property(e => e.Ativo).HasColumnName("Ativo");
                entity.Property(e => e.EmailVerificado).HasColumnName("EmailVerificado");
                entity.Property(e => e.CodigoVerificacao).HasColumnName("CodigoVerificacao").HasMaxLength(10);
                entity.Property(e => e.CodigoVerificacaoExpiracao).HasColumnName("CodigoVerificacaoExpiracao");
                entity.Property(e => e.CriadoEm).HasColumnName("Criado_em");
                entity.Property(e => e.DataAtualizacao).HasColumnName("DataAtualizacao");

                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<RecuperacaoSenha>(entity =>
            {
                entity.ToTable("RecuperacoesSenha");
                entity.HasKey(e => e.UsuarioID);

                entity.Property(e => e.UsuarioID).HasColumnName("UsuarioID");
                entity.Property(e => e.CodigoHash).HasColumnName("CodigoHash").HasMaxLength(64);
                entity.Property(e => e.Expiracao).HasColumnName("Expiracao");
                entity.Property(e => e.TentativasInvalidas).HasColumnName("TentativasInvalidas");
                entity.Property(e => e.CriadoEm).HasColumnName("CriadoEm");

                entity.HasOne<Usuario>()
                    .WithOne()
                    .HasForeignKey<RecuperacaoSenha>(e => e.UsuarioID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Prioridade>(entity =>
            {
                entity.ToTable("Prioridades");
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ID).HasColumnName("ID");
                entity.Property(e => e.Nome).HasColumnName("Nome").HasMaxLength(50);
            });

            modelBuilder.Entity<Lista>(entity =>
            {
                entity.ToTable("Listas");
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ID).HasColumnName("ID");
                entity.Property(e => e.Nome).HasColumnName("Nome").HasMaxLength(100);
                entity.Property(e => e.Cor).HasColumnName("Cor").HasMaxLength(20);
                entity.Property(e => e.Favorita).HasColumnName("Favorita");
                entity.Property(e => e.Ordem).HasColumnName("Ordem");
                entity.Property(e => e.DataCriacao).HasColumnName("DataCriacao");
                entity.Property(e => e.UsuarioID).HasColumnName("Usuario_id");

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Tarefa>(entity =>
            {
                entity.ToTable("Tarefas");
                entity.HasKey(e => e.ID);

                entity.Property(e => e.ID).HasColumnName("ID");
                entity.Property(e => e.Titulo).HasColumnName("Titulo").HasMaxLength(200);
                entity.Property(e => e.Descricao).HasColumnName("Descricao");
                entity.Property(e => e.Tags).HasColumnName("Tags").HasMaxLength(255);
                entity.Property(e => e.Concluida).HasColumnName("Concluida");
                entity.Property(e => e.Arquivada).HasColumnName("Arquivada");
                entity.Property(e => e.DataVencimento).HasColumnName("DataVencimento");
                entity.Property(e => e.DataCriacao).HasColumnName("DataCriacao");
                entity.Property(e => e.DataAtualizacao).HasColumnName("DataAtualizacao");
                entity.Property(e => e.Ordem).HasColumnName("Ordem");
                entity.Property(e => e.ListaID).HasColumnName("Lista_id");
                entity.Property(e => e.PrioridadeID).HasColumnName("Prioridade_id");

                entity.HasOne(e => e.Lista)
                    .WithMany()
                    .HasForeignKey(e => e.ListaID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Prioridade)
                    .WithMany()
                    .HasForeignKey(e => e.PrioridadeID)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
