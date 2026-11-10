using StudentManagementSystem.Models;

namespace StudentManagementSystem.Services;

// In-memory demo repository. Replace this class with EF Core/SQL Server later.
public sealed class StudentRepository
{
    private readonly List<Student> _students =
    [
        new() { Id = 1001, Name = "Ahmed Hassan", Email = "ahmed@university.edu", Department = "Computer Science", EnrollmentDate = new(2024, 9, 15) },
        new() { Id = 1002, Name = "Mariam Ali", Email = "mariam@university.edu", Department = "Information Systems", EnrollmentDate = new(2023, 9, 20) },
        new() { Id = 1003, Name = "Omar Mostafa", Email = "omar@university.edu", Department = "Information Technology", EnrollmentDate = new(2025, 2, 10) },
        new() { Id = 1004, Name = "Nour Samir", Email = "nour@university.edu", Department = "Computer Science", EnrollmentDate = new(2024, 10, 1) }
    ];

    private int _nextId = 1005;
    public IReadOnlyList<Student> GetAll() => _students.Select(s => s.Clone()).ToList();

    public void Add(Student student)
    {
        student.Id = _nextId++;
        _students.Add(student.Clone());
    }

    public bool Update(Student student)
    {
        var existing = _students.FirstOrDefault(s => s.Id == student.Id);
        if (existing is null) return false;
        existing.Name = student.Name;
        existing.Email = student.Email;
        existing.Department = student.Department;
        existing.EnrollmentDate = student.EnrollmentDate;
        return true;
    }

    public bool Delete(int id)
    {
        var item = _students.FirstOrDefault(s => s.Id == id);
        return item is not null && _students.Remove(item);
    }
}
