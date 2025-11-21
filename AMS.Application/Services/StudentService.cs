using AMS.Application.Common.Interfaces;
using AMS.Application.DTOs;
using AMS.Domain.Entities;
using AMS.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AMS.Application.Services;

public class StudentService(IApplicationDbContext context) : IStudentService
{
    public async Task<Guid> CreateStudentAsync(CreateStudentDto dto, CancellationToken cancellationToken)
    {
        // 1. Rozpoczęcie Transakcji (Wymóg zadania)
        // DatabaseFacade jest dostępny dzięki interfejsowi IApplicationDbContext
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 2. Pobierz licznik dla Studentów ("S")
            var counter = await context.SequenceCounters
                .FirstOrDefaultAsync(s => s.Prefix == "S", cancellationToken);

            if (counter == null)
            {
                // Jeśli nie ma licznika, tworzymy go (fail-safe)
                counter = new SequenceCounter { Prefix = "S", CurrentValue = 0 };
                await context.SequenceCounters.AddAsync(counter, cancellationToken);
            }

            // 3. Inkrementacja (Logika biznesowa)
            counter.CurrentValue++;
            var newIndex = $"{counter.Prefix}{counter.CurrentValue}"; // np. S1001

            // 4. Utworzenie encji
            var student = new Student
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                YearOfStudy = dto.YearOfStudy,
                UniversityIndex = newIndex, // <-- Przypisanie wygenerowanego indeksu
                Address = new Address(dto.Street, dto.City, dto.PostalCode)
            };

            await context.Students.AddAsync(student, cancellationToken);

            // 5. Zapisz zmiany (Zapisze Studenta ORAZ zaktualizowany Licznik w jednej paczce)
            await context.SaveChangesAsync(cancellationToken);

            // 6. Zatwierdź transakcję
            await transaction.CommitAsync(cancellationToken);

            return student.Id;
        }
        catch
        {
            // W razie błędu wycofaj wszystko (licznik nie zostanie podbity)
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}