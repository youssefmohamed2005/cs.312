namespace StudentManagementSystem.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Department { get; set; } = "";
    public DateTime EnrollmentDate { get; set; }

    public Student Clone() => (Student)MemberwiseClone();
}
