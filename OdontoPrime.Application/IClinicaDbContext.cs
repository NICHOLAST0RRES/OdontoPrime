using Microsoft.EntityFrameworkCore;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Application;

public interface IClinicaDbContext
{
    DbSet<Paciente> Pacientes { get; }
    DbSet<Profissional> Profissionais { get; }
    DbSet<Consulta> Consultas { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}