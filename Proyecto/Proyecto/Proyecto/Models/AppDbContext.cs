using Microsoft.EntityFrameworkCore;

namespace Proyecto.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 
        
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<HistorialEstado> HistorialEstado { get; set; }
        public DbSet<Notificacion> Notificacion { get; set; }
        public DbSet<Adjunto> Adjunto { get; set; }
        public DbSet<Comentario> Comentario { get; set; }
        public DbSet<Asignacion> Asignacion { get; set; }

        /*protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relación entre Ticket y Usuario como Creador
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.CreadoPor)
                .WithMany(u => u.TicketsCreados)
                .HasForeignKey(t => t.Creado_Por)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación entre Ticket y Usuario como Asignado
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.AsignadoA)
                .WithMany(u => u.TicketsAsignados)
                .HasForeignKey(t => t.Asignado_A)
                .OnDelete(DeleteBehavior.Restrict);
        }*/
       

    }
}
