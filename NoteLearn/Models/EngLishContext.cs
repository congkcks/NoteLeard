
using Microsoft.EntityFrameworkCore;
namespace NoteLearn.Models;
public partial class EngLishContext : DbContext
{
    public EngLishContext(DbContextOptions<EngLishContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiMetadatum> AiMetadata { get; set; }

    public virtual DbSet<Collection> Collections { get; set; }

    public virtual DbSet<Content> Contents { get; set; }

    public virtual DbSet<DocumentPage> DocumentPages { get; set; }

    public virtual DbSet<Note> Notes { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<ContentChunk> ContentChunks { get; set; }


    public virtual DbSet<UserProgress> UserProgresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum("auth", "oauth_authorization_status", new[] { "pending", "approved", "denied", "expired" })
            .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS", "VECTOR" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("graphql", "pg_graphql")
            .HasPostgresExtension("vault", "supabase_vault")
            .HasPostgresExtension("vector");


        modelBuilder.Entity<AiMetadatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ai_metadata_pkey");

            entity.ToTable("ai_metadata");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.Summary).HasColumnName("summary");

            entity.HasOne(d => d.Content).WithMany(p => p.AiMetadata)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("ai_metadata_content_id_fkey");
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("collections_pkey");

            entity.ToTable("collections");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Collections)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("collections_user_id_fkey");

            entity.HasMany(d => d.Contents).WithMany(p => p.Collections)
                .UsingEntity<Dictionary<string, object>>(
                    "CollectionItem",
                    r => r.HasOne<Content>().WithMany()
                        .HasForeignKey("ContentId")
                        .HasConstraintName("collection_items_content_id_fkey"),
                    l => l.HasOne<Collection>().WithMany()
                        .HasForeignKey("CollectionId")
                        .HasConstraintName("collection_items_collection_id_fkey"),
                    j =>
                    {
                        j.HasKey("CollectionId", "ContentId").HasName("collection_items_pkey");
                        j.ToTable("collection_items");
                        j.IndexerProperty<long>("CollectionId").HasColumnName("collection_id");
                        j.IndexerProperty<long>("ContentId").HasColumnName("content_id");
                    });
        });

        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contents_pkey");

            entity.ToTable("contents");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.TotalPages).HasColumnName("total_pages");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.YoutubeUrl).HasColumnName("youtube_url");

            entity.HasOne(d => d.User).WithMany(p => p.Contents)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("contents_user_id_fkey");
        });

        modelBuilder.Entity<DocumentPage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("document_pages_pkey");

            entity.ToTable("document_pages");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.PageNumber).HasColumnName("page_number");
            entity.Property(e => e.Text).HasColumnName("text");

            entity.HasOne(d => d.Content).WithMany(p => p.DocumentPages)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("document_pages_content_id_fkey");
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notes_pkey");

            entity.ToTable("notes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.PageNumber).HasColumnName("page_number");
            entity.Property(e => e.Text).HasColumnName("text");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Content).WithMany(p => p.Notes)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notes_content_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Notes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notes_user_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FullName).HasColumnName("full_name");
        });

        

        modelBuilder.Entity<UserProgress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_progress_pkey");

            entity.ToTable("user_progress");

            entity.HasIndex(e => new { e.UserId, e.ContentId }, "user_progress_user_id_content_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.LastPage).HasColumnName("last_page");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Content).WithMany(p => p.UserProgresses)
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_progress_content_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserProgresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_progress_user_id_fkey");
        });
        modelBuilder.Entity<ContentChunk>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("content_chunks_pkey");
            entity.ToTable("content_chunks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContentId).HasColumnName("content_id");
            entity.Property(e => e.ChunkIndex).HasColumnName("chunk_index");
            entity.Property(e => e.PageNumber).HasColumnName("page_number");
            entity.Property(e => e.StartTimeSec).HasColumnName("start_time_sec");
            entity.Property(e => e.EndTimeSec).HasColumnName("end_time_sec");
            entity.Property(e => e.Text).HasColumnName("text");

            // pgvector column
            entity.Property(e => e.Embedding)
                  .HasColumnName("embedding")
                  .HasColumnType("vector(1536)");

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("now()")
                  .HasColumnType("timestamp with time zone")
                  .HasColumnName("created_at");

            entity.HasIndex(e => e.ContentId).HasDatabaseName("content_chunks_content_id_idx");

            entity.HasOne(d => d.Content).WithMany()
                .HasForeignKey(d => d.ContentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("content_chunks_content_id_fkey");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
