using AMS.Application.Common.Interfaces;
using AMS.Application.Common.Models;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Domain.Entities;
using AMS.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AMS.Application.Services;

public sealed class StudentService(IApplicationDbContext context) : IStudentService
{
    public async Task<Result<Guid>> CreateStudentAsync(CreateStudentDto dto, CancellationToken cancellationToken)
    {
        await using var transaction =
            await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var counter = await context.SequenceCounters
                .FirstOrDefaultAsync(s => s.Prefix == "S", cancellationToken).ConfigureAwait(false);

            if (counter == null)
            {
                counter = new SequenceCounter { Prefix = "S", CurrentValue = 0 };
                await context.SequenceCounters.AddAsync(counter, cancellationToken).ConfigureAwait(false);
            }

            counter.CurrentValue++;
            var newIndex = $"{counter.Prefix}{counter.CurrentValue}";

            var student = new Student
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                YearOfStudy = dto.YearOfStudy,
                UniversityIndex = newIndex,
                Address = new Address(dto.Street, dto.City, dto.PostalCode)
            };

            await context.Students.AddAsync(student, cancellationToken).ConfigureAwait(false);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result<Guid>.Success(student.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<Guid>.Failure($"Error creating student: {ex.Message}");
        }
    }

    public async Task<Result> DeleteStudentAsync(Guid studentId, CancellationToken cancellationToken)
    {
        await using var transaction =
            await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var student = await context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken).ConfigureAwait(false);

            if (student == null)
                return Result.Failure($"Student with ID {studentId} not found.");

            var counter = await context.SequenceCounters
                .FirstOrDefaultAsync(s => s.Prefix == "S", cancellationToken).ConfigureAwait(false);

            if (counter != null)
                if (int.TryParse(student.UniversityIndex.Substring(1), out var studentNumber))
                    if (counter.CurrentValue == studentNumber)
                        counter.CurrentValue--;

            context.Students.Remove(student);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return Result.Failure($"Error deleting student: {ex.Message}");
        }
    }

    public async Task<Result<StudentDto>> GetStudentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var student = await context.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken).ConfigureAwait(false);

        if (student == null) return Result<StudentDto>.Failure("Student not found.");

        var dto = new StudentDto
        (
            student.Id,
            student.FirstName,
            student.LastName,
            student.UniversityIndex,
            student.YearOfStudy,
            student.Address.Street,
            student.Address.City,
            student.Address.PostalCode
        );

        return Result<StudentDto>.Success(dto);
    }

    public async Task<Result<List<StudentDto>>> GetAllStudentsAsync(CancellationToken cancellationToken)
    {
        var students = await context.Students
            .AsNoTracking()
            .Select(s => new StudentDto(
                s.Id,
                s.FirstName,
                s.LastName,
                s.UniversityIndex,
                s.YearOfStudy,
                s.Address.Street,
                s.Address.City,
                s.Address.PostalCode
            ))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<List<StudentDto>>.Success(students);
    }

    public async Task<Result> UpdateStudentAsync(UpdateStudentDto dto, CancellationToken cancellationToken)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == dto.Id, cancellationToken)
            .ConfigureAwait(false);

        if (student == null) return Result.Failure("Student not found.");

        student.FirstName = dto.FirstName;
        student.LastName = dto.LastName;
        student.YearOfStudy = dto.YearOfStudy;
        student.Address = new Address(dto.Street, dto.City, dto.PostalCode);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}