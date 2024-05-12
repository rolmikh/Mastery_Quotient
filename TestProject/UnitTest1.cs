using Mastery_Quotient.Models;
using Mastery_Quotient.ModelsValidation;
using FluentValidation.TestHelper;


namespace TestProject
{
    [SetUpFixture]
    public class TestFixture
    {
        private static DisciplineEmployeeValidator _disciplineEmployeeValidator;
        private static EmployeeValidator _employeeValidator;
        private static MaterialValidator _materialValidator;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _disciplineEmployeeValidator = new DisciplineEmployeeValidator();
            _employeeValidator = new EmployeeValidator();
            _materialValidator = new MaterialValidator();
        }

        public static DisciplineEmployeeValidator GetDisciplineEmployeeValidator()
        {
            return _disciplineEmployeeValidator;
        }

        public static EmployeeValidator GetEmployeeValidator()
        {
            return _employeeValidator;
        }

        public static MaterialValidator GetMaterialValidator()
        {
            return _materialValidator;
        }
    }

    public class Tests
    {
       

        [Test]
        public void IsValidDisciplineEmployee_DisciplineID_Zero_False()
        {
            var _disciplineEmployeeValidator = new DisciplineEmployeeValidator();

            var disciplineEmployee = new DisciplineEmployee { DisciplineId = 0, EmployeeId = 1 };

            var result = _disciplineEmployeeValidator.TestValidate(disciplineEmployee);

            result.ShouldHaveValidationErrorFor(x => x.DisciplineId);
        }

        [Test]
        public void IsValidDisciplineEmployee_EmployeeID_Zero_False()
        {
            var _disciplineEmployeeValidator = new DisciplineEmployeeValidator();

            var disciplineEmployee = new DisciplineEmployee { DisciplineId = 1, EmployeeId = 0 };

            var result = _disciplineEmployeeValidator.TestValidate(disciplineEmployee);

            result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
        }

        [Test]
        public void IsValidDisciplineEmployee_DisciplineID_EmployeeID_Zero_False()
        {
            var _disciplineEmployeeValidator = new DisciplineEmployeeValidator();

            var disciplineEmployee = new DisciplineEmployee { DisciplineId = 0, EmployeeId = 0 };

            var result = _disciplineEmployeeValidator.TestValidate(disciplineEmployee);

            result.ShouldHaveValidationErrorFor(x => x.DisciplineId);
            result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
        }

        [Test]
        public void IsValidDisciplineEmployee_DisciplineID_EmployeeID_Save_True()
        {
            var _disciplineEmployeeValidator = new DisciplineEmployeeValidator();

            var disciplineEmployee = new DisciplineEmployee { DisciplineId = 1, EmployeeId = 1 };

            var result = _disciplineEmployeeValidator.TestValidate(disciplineEmployee);

            result.ShouldNotHaveValidationErrorFor(x => x.DisciplineId);
            result.ShouldNotHaveValidationErrorFor(x => x.EmployeeId);
        }

        [Test]
        public void IsValidEmail_ValidEmail_True()
        {
            var _employeeValidator = new EmployeeValidator();
            var employee = new Employee { EmailEmployee = "isip_l.m.rozkovskaiya@mpt.ru" };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldNotHaveValidationErrorFor(x => x.EmailEmployee);
        }

        [Test]
        public void IsValidEmail_InvalidEmail_False()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { EmailEmployee = "ludaarozhkovskaya@gmail.com" };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldHaveValidationErrorFor(x => x.EmailEmployee);
        }

        [Test]
        public void IsValidEmail_InvalidFormat_False()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { EmailEmployee = "isip_l.m.rozkovskaiyampt.ru" };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldHaveValidationErrorFor(x => x.EmailEmployee);
        }

        [Test]
        public void IsValidSurname_UppercaseFirstLetter_True()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { SurnameEmployee = "Ðîæêîâñêàÿ" };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldNotHaveValidationErrorFor(x => x.SurnameEmployee);
        }

        [Test]
        public void IsValidSurname_LowercaseFirstLetter_False()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { SurnameEmployee = "ðîæêîâñêàÿ" };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldHaveValidationErrorFor(x => x.SurnameEmployee);
        }

        [Test]
        public void IsValidName_UppercaseFirstLetter_True()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { NameEmployee = "Ëþäìèëà" };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldNotHaveValidationErrorFor(x => x.NameEmployee);
        }

        [Test]
        public void IsValidName_LowercaseFirstLetter_False()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { NameEmployee = "ëþäìèëà" };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldHaveValidationErrorFor(x => x.NameEmployee);
        }

        [Test]
        public void IsValidMiddleName_UppercaseFirstLetter_True()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { MiddleNameEmployee = "Ìèõàéëîâíà" };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldNotHaveValidationErrorFor(x => x.MiddleNameEmployee);
        }

        [Test]
        public void IsValidMiddleName_LowercaseFirstLetter_False()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { MiddleNameEmployee = "ìèõàéëîâíà" };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldHaveValidationErrorFor(x => x.MiddleNameEmployee);
        }

        [Test]
        public void IsValidPassword_MinimumLength_True()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { PasswordEmployee = "12345678" };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldNotHaveValidationErrorFor(x => x.PasswordEmployee);
        }

        [Test]
        public void IsValidPassword_LessThanMinimumLength_False()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { PasswordEmployee = "1234567" };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldHaveValidationErrorFor(x => x.PasswordEmployee);
        }

        [Test]
        public void IsValidRoleId_NonZero_True()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { RoleId = 1 };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldNotHaveValidationErrorFor(x => x.RoleId);
        }

        [Test]
        public void IsValidRoleId_Zero_False()
        {
            var _employeeValidator = new EmployeeValidator();

            var employee = new Employee { RoleId = 0 };

            var result = _employeeValidator.TestValidate(employee);

            result.ShouldHaveValidationErrorFor(x => x.RoleId);
        }


        [Test]
        public void NameMaterial_ShouldHaveError_WhenFirstLetterIsLowerCase_False()
        {
            var _materialValidator = new MaterialValidator();

            var material = new Material { NameMaterial = "äâîéñòâåííûå çàäà÷è" };

            var result = _materialValidator.TestValidate(material);

            result.ShouldHaveValidationErrorFor(x => x.NameMaterial);
        }

        [Test]
        public void NameMaterial_ShouldNotHaveError_WhenFirstLetterIsUpperCase()
        {
            var _materialValidator = new MaterialValidator();

            var material = new Material { NameMaterial = "Material" };
            var result = _materialValidator.TestValidate(material);
            result.ShouldNotHaveValidationErrorFor(x => x.NameMaterial);
        }

        [Test]
        public void NameMaterial_ShouldHaveError_WhenLengthIsLessThanFour()
        {
            var _materialValidator = new MaterialValidator();

            var material = new Material { NameMaterial = "mat" };

            var result = _materialValidator.TestValidate(material);

            result.ShouldHaveValidationErrorFor(x => x.NameMaterial);
        }

        

        [Test]
        public void NameMaterial_ShouldHaveError_WhenLengthIsMoreThanHundred()
        {
            var _materialValidator = new MaterialValidator();

            var material = new Material
            {
                NameMaterial = "àààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààààà"
            };

            var result = _materialValidator.TestValidate(material);

            result.ShouldHaveValidationErrorFor(x => x.NameMaterial);
        }

        

        

    }




}
