namespace Mastery_Quotient.Models
{
    public class EmployeeModelView
    {
        public List<Employee> Employees { get; set; }

        public List<Role> Roles { get; set; }

        public EmployeeModelView(List<Employee> employees, List<Role> roles)
        {
            Employees = employees;
            Roles = roles;
        }


    }
}
