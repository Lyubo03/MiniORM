namespace MiniORM.App
{
    using System.Linq;
    using Data;
    using Data.Entities;
    public class StartUp
    {
        public static void Main()
        {
            string connectionString = @"Server=DESKTOP-2RO7KG1\SQLEXPRESS;Database=MiniORM;Integrated Security=True";

            var context = new SoftUniDbContext(connectionString);
            context.Employees.Add(new Employee
            {
                FirstName = "Pesho",
                LastName = "Goshow",
                DepartmentId = context.Departments.First().Id, 
                IsEmployed = true,
            });
            
            var employee = context.Employees.Last();
            employee.FirstName = "Modified";

            context.SaveChanges();
        }
    }
}