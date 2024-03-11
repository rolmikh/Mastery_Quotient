namespace Mastery_Quotient.Models
{
    public class EmployeePersonalAccountModel
    {
        public Employee Employee { get; set; }

        public Role Role { get; set; }

        public EmployeePersonalAccountModel(Employee employee, Role role)
        {
            Employee = employee;
            Role = role;
        }
    }
}
