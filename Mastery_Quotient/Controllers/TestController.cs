using AngleSharp.Dom;
using FluentValidation;
using Mastery_Quotient.Models;
using Mastery_Quotient.ModelsValidation;
using Mastery_Quotient.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Mastery_Quotient.Controllers
{
    public class TestController : Controller
    {
        private readonly ILogger<TestController> _logger;

        private readonly IConfiguration configuration;

        private readonly IValidator<Test> testValidator;

        private readonly IValidator<Question> questionValidator;

        private readonly IValidator<AnswerOption> answerOptionValidator;

        public TestController(ILogger<TestController> logger, IConfiguration configuration, IValidator<Test> testValidator, IValidator<Question> questionValidator, IValidator<AnswerOption> answerOptionValidator)
        {
            _logger = logger;
            this.configuration = configuration;
            this.testValidator = testValidator;
            this.questionValidator = questionValidator;
            this.answerOptionValidator = answerOptionValidator;
        }




        /// <summary>
        /// Загрузка представления страницы добавления тестирований
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> TeacherWindowTest()
        {

            try
            {
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = new Employee();
                List<Discipline> disciplines = new List<Discipline>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                List<TestParameter> testParameters = new List<TestParameter>();

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employee = JsonConvert.DeserializeObject<Employee>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineEmployee"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineEmployees = JsonConvert.DeserializeObject<List<DisciplineEmployee>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "TestParameters"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testParameters = JsonConvert.DeserializeObject<List<TestParameter>>(apiResponse);
                    }

                    
                }
                List<DisciplineEmployee> discipline = disciplineEmployees.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                TestTeacher testTeacher = new TestTeacher(employee, disciplines, discipline, testParameters);
                return View(testTeacher);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// POST запрос добавления тестирования
        /// </summary>
        /// <param name="nameTest"></param>
        /// <param name="parameterTest"></param>
        /// <param name="disciplineTest"></param>
        /// <param name="deadlineCheck"></param>
        /// <param name="deadlineDate"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TeacherWindowTest(string nameTest, int parameterTest, int disciplineTest, bool deadlineCheck, string deadlineDate)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                Test test = new Test();

                test.NameTest = nameTest;
                test.DateCreatedTest = DateTime.Now;
                if(deadlineCheck)
                {
                    test.Deadline = null;
                }
                else
                {
                    test.Deadline = DateTime.Parse(deadlineDate);
                }
                test.DisciplineId = disciplineTest;
                test.EmployeeId = id;
                test.TestParameterId = parameterTest;
                test.IsDeleted = 0;
                test.Active = 1;

                var validationResult = await testValidator.ValidateAsync(test);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("TeacherWindowTest", "Test");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(test), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(apiUrl + "Tests", content);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("TestTeacher", "Test");
                    }
                    else
                    {
                        return RedirectToAction("TeacherWindowTest", "Test");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }

        /// <summary>
        /// Загрузка представления страницы просмотра тестирований
        /// </summary>
        /// <returns></returns>


        [HttpGet]
        public async Task<IActionResult> TestTeacher()
        {
            try
            {
                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = new Employee();
                List<Discipline> disciplines = new List<Discipline>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                List<TestParameter> testParameters = new List<TestParameter>();
                List<Test> tests = new List<Test>();

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employee = JsonConvert.DeserializeObject<Employee>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines/NoDeleted"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineEmployee"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineEmployees = JsonConvert.DeserializeObject<List<DisciplineEmployee>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "TestParameters"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testParameters = JsonConvert.DeserializeObject<List<TestParameter>>(apiResponse);
                    }
                    if (TempData.ContainsKey("Search"))
                    {
                        tests = JsonConvert.DeserializeObject<List<Test>>(TempData["Search"].ToString());
                    }
                    else if (TempData.ContainsKey("Filtration"))
                    {
                        tests = JsonConvert.DeserializeObject<List<Test>>(TempData["Filtration"].ToString());
                    }
                    else
                    {
                        using (var response = await httpClient.GetAsync(apiUrl + "Tests"))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                        }
                    }

                }
                List<Test> testList = tests.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                List<DisciplineEmployee> discipline = disciplineEmployees.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                TestViewTeacher testViewTeacher = new TestViewTeacher(employee, disciplines, discipline, testParameters, testList);
                return View(testViewTeacher);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// POST запрос поиска тестирования
        /// </summary>
        /// <param name="Search"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TestTeacher(string Search)
        {
            try
            {
                if (Search != null)
                {
                    var apiUrl = configuration["AppSettings:ApiUrl"];
                    List<Test> tests = new List<Test>();

                    StringContent content = new StringContent(JsonConvert.SerializeObject(Search), Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.GetAsync(apiUrl + "Tests/Search?search=" + Search);

                        string apiResponse = await response.Content.ReadAsStringAsync();
                        tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);

                        TempData["Search"] = JsonConvert.SerializeObject(tests);

                        if (tests.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("TestTeacher", "Test");

                        }

                        return RedirectToAction("TestTeacher", "Test");
                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("TestTeacher", "Test");
                }
            }
            catch
            {
                return BadRequest("Ошибка");

            }



        }

        /// <summary>
        /// Загрузка представления страницы добавления вопросов
        /// </summary>
        /// <param name="testId"></param>
        /// <returns></returns>
        public async Task<IActionResult> QuestionTest(int testId)
        {

            try
            {

                if (testId != 0)
                {
                    TempData["testID"] = testId;
                }


                int testID = int.Parse(TempData["testID"].ToString());
                TempData.Keep("testID");


                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Test test = new Test();
                List<TypeQuestion> typeQuestions = new List<TypeQuestion>();
                List<TestParameter> testParameters = new List<TestParameter>();
                List<Question> questions = new List<Question>();
                List<TestQuestion> testQuestions = new List<TestQuestion>();
                List<AnswerOption> answerOptions = new List<AnswerOption>();
                List<QuestionAnswerOption> questionAnswerOptions = new List<QuestionAnswerOption>();
                Employee employee = new Employee();
                List<Discipline> disciplines = new List<Discipline>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();


                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "Tests/" + testID))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        test = JsonConvert.DeserializeObject<Test>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "TypeQuestions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        typeQuestions = JsonConvert.DeserializeObject<List<TypeQuestion>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "TestParameters"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testParameters = JsonConvert.DeserializeObject<List<TestParameter>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Questions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        questions = JsonConvert.DeserializeObject<List<Question>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "TestQuestions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testQuestions = JsonConvert.DeserializeObject<List<TestQuestion>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "AnswerOptions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        answerOptions = JsonConvert.DeserializeObject<List<AnswerOption>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "QuestionAnswerOptions"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        questionAnswerOptions = JsonConvert.DeserializeObject<List<QuestionAnswerOption>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employee = JsonConvert.DeserializeObject<Employee>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineEmployee"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineEmployees = JsonConvert.DeserializeObject<List<DisciplineEmployee>>(apiResponse);
                    }
                }
                List<DisciplineEmployee> discipline = disciplineEmployees.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                List<TestParameter> testsParameters = testParameters.Where(n => n.IdTestParameter == test.TestParameterId).ToList();
                List<TestQuestion> testQuestionsList = testQuestions.Where(n => n.TestId == test.IdTest).ToList();

                ViewTestModel viewTestModel = new ViewTestModel(test, typeQuestions, testsParameters, questions, testQuestionsList, answerOptions, questionAnswerOptions, employee,disciplines, discipline);

                return View(viewTestModel);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
           
        }

        /// <summary>
        /// POST запрос обновления данных о тестировании, публикация тестирования
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> TestPublication()
        {
            try
            {
                int testID = int.Parse(TempData["testID"].ToString());
                TempData.Keep("testID");
                var apiUrl = configuration["AppSettings:ApiUrl"];

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(apiUrl + "Tests/" + testID);
                    response.EnsureSuccessStatusCode();
                    var TestData = await response.Content.ReadAsStringAsync();

                    var test = JsonConvert.DeserializeObject<Test>(TestData);

                    test.Active = 0;
                    test.IsDeleted = 0;

                    string json = JsonConvert.SerializeObject(test);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    response = await httpClient.PutAsync(apiUrl + "Tests/" + testID, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("TestTeacher", "Test");
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// POST запрос удаления данных о тестирований
        /// </summary>
        /// <param name="IdTest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteTest(int IdTest)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.DeleteAsync(apiUrl + "Tests/" + IdTest))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("TestTeacher", "Test");
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка удаления теста!" + e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int IdTestQuestion, int IdQuestion)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];
                List<QuestionAnswerOption?> options = new List<QuestionAnswerOption?>();
                using (var httpClient = new HttpClient())
                {
                    
                   
                    using (var response = await httpClient.GetAsync(apiUrl + "QuestionAnswerOptions/Filtration?idQuestion=" + IdQuestion))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        options = JsonConvert.DeserializeObject<List<QuestionAnswerOption>>(apiResponse);
                    }
                    List<int?> answerOptions = options.Select(n => n.AnswerOptionsId).ToList();

                    List<int?> optionsDelete = options.Select(n => n.IdQuestionAnswerOptions).ToList();

                    foreach (int answerQuestionId in optionsDelete)
                    {
                        using (var response = await httpClient.DeleteAsync(apiUrl + "QuestionAnswerOptions/" + answerQuestionId))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                        }
                    }
                    foreach (int answerId in answerOptions)
                    {
                        using (var response = await httpClient.DeleteAsync(apiUrl + "AnswerOptions/" + answerId))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                        }
                    }
                    using (var response = await httpClient.DeleteAsync(apiUrl + "TestQuestions/" + IdTestQuestion))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                    using (var response = await httpClient.DeleteAsync(apiUrl + "Questions/" + IdQuestion))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                    }
                }

                return RedirectToAction("QuestionTest", "Test");
            }
            catch (Exception e)
            {
                return BadRequest("Ошибка удаления вопроса!" + e.Message);

            }
        }

        /// <summary>
        /// POST запрос добавления вопроса с письменным вариантом ответа
        /// </summary>
        /// <param name="numberQuestion"></param>
        /// <param name="nameQuestion"></param>
        /// <param name="typeQuestion"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> QuestionInput(int numberQuestion,string nameQuestion, string typeQuestion)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                
                
                Question question = new Question();
                question.NumberQuestion = numberQuestion;
                question.NameQuestion = nameQuestion;

                if(typeQuestion.Contains("Письменный"))
                {
                    question.TypeQuestionId = 1;
                }

                

                var validationResult = await questionValidator.ValidateAsync(question);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("QuestionTest", "Test");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(question), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(apiUrl + "Questions", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        var createdQuestion = JsonConvert.DeserializeObject<Question>(responseContent);

                        int id = int.Parse(TempData["testID"].ToString());

                        TempData.Keep("testID");
                        TestQuestion testQuestion = new TestQuestion();
                        testQuestion.QuestionId = createdQuestion.IdQuestion;
                        testQuestion.TestId = id;
                        StringContent contentTest = new StringContent(JsonConvert.SerializeObject(testQuestion), Encoding.UTF8, "application/json");

                        var responseTest = await httpClient.PostAsync(apiUrl + "TestQuestions", contentTest);

                        if (response.IsSuccessStatusCode)
                        {
                            return RedirectToAction("QuestionTest", "Test");
                        }
                        else
                        {
                            return BadRequest(responseTest);
                        }
                    }
                    else
                    {
                        return BadRequest(response.StatusCode);
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
       
        /// <summary>
        /// POST запрос добавления вопроса с одним вариантом ответа
        /// </summary>
        /// <param name="numberQuestion"></param>
        /// <param name="nameQuestion"></param>
        /// <param name="typeQuestion"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> QuestionOne(int numberQuestion, string nameQuestion, string typeQuestion, NewModelQuestionOne model)
        {
            try
            {
                int i = 1;
                var apiUrl = configuration["AppSettings:ApiUrl"];
                if (model.AnswerOptionViewModels == null)
                {
                    TempData["MessageValidation"] = "Добавьте вариант ответа";

                    return RedirectToAction("QuestionTest", "Test");
                }
                Question question = new Question();
                question.NumberQuestion = numberQuestion;
                question.NameQuestion = nameQuestion;

                if (typeQuestion.Contains("одним вариантом"))
                {
                    question.TypeQuestionId = 2;
                }
                else
                {
                    return BadRequest();
                }
                var validationResult = await questionValidator.ValidateAsync(question);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("QuestionTest", "Test");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(question), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(apiUrl + "Questions", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        var createdQuestion = JsonConvert.DeserializeObject<Question>(responseContent);
                        foreach(var answer in model.AnswerOptionViewModels)
                        {
                            AnswerOption answerOption = new AnswerOption();
                            answerOption.NumberAnswer = i++;
                            answerOption.ContentAnswer = answer.AnswerOption;
                            if (answer.IsCorrectAnswer == true)
                            {
                                answerOption.CorrectnessAnswer = 0;
                            }
                            else
                            {
                                answerOption.CorrectnessAnswer = 1;
                            }

                            var validationResultTwo = await answerOptionValidator.ValidateAsync(answerOption);

                            if (!validationResultTwo.IsValid)
                            {
                                var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                                TempData["ErrorValidation"] = errorMessages;
                                return RedirectToAction("QuestionTest", "Test");
                            }

                            StringContent contentAnswerOption = new StringContent(JsonConvert.SerializeObject(answerOption), Encoding.UTF8, "application/json");
                            var responseAnswerOption = await httpClient.PostAsync(apiUrl + "AnswerOptions", contentAnswerOption);

                            if (responseAnswerOption.IsSuccessStatusCode)
                            {
                                var responseContentAnswer = await responseAnswerOption.Content.ReadAsStringAsync();
                                var createdAnswer = JsonConvert.DeserializeObject<AnswerOption>(responseContentAnswer);


                                QuestionAnswerOption questionAnswerOption = new QuestionAnswerOption();
                                questionAnswerOption.AnswerOptionsId = createdAnswer.IdAnswerOptions;
                                questionAnswerOption.QuestionId = createdQuestion.IdQuestion;

                                StringContent contentQuestionOption = new StringContent(JsonConvert.SerializeObject(questionAnswerOption), Encoding.UTF8, "application/json");
                                var responseQuestionOption = await httpClient.PostAsync(apiUrl + "QuestionAnswerOptions", contentQuestionOption);

                                

                            }
                            else
                            {
                                return BadRequest(responseAnswerOption.StatusCode);
                            }

                        }

                        int id = int.Parse(TempData["testID"].ToString());

                        TempData.Keep("testID");
                        TestQuestion testQuestion = new TestQuestion();
                        testQuestion.QuestionId = createdQuestion.IdQuestion;
                        testQuestion.TestId = id;
                        StringContent contentTest = new StringContent(JsonConvert.SerializeObject(testQuestion), Encoding.UTF8, "application/json");

                        var responseTest = await httpClient.PostAsync(apiUrl + "TestQuestions", contentTest);

                        return RedirectToAction("QuestionTest", "Test");
                    }
                    else
                    {
                        return BadRequest(response.StatusCode);
                    }
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);  

            }

        }

        /// <summary>
        /// POST запрос добавления вопроса с несколькими вариантами ответа
        /// </summary>
        /// <param name="numberQuestion"></param>
        /// <param name="nameQuestion"></param>
        /// <param name="typeQuestion"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> QuestionTwo(int numberQuestion, string nameQuestion, string typeQuestion, NewModelQuestionOne model)
        {
            try
            {
                int i = 1;
                var apiUrl = configuration["AppSettings:ApiUrl"];
                if (model.AnswerOptionViewModels == null)
                {
                    TempData["MessageValidation"] = "Добавьте вариант ответа";

                    return RedirectToAction("QuestionTest", "Test");
                }
                Question question = new Question();
                question.NumberQuestion = numberQuestion;
                question.NameQuestion = nameQuestion;

                if (typeQuestion.Contains("несколькими вариантами"))
                {
                    question.TypeQuestionId = 3;
                }
                else
                {
                    return BadRequest();
                }
                var validationResult = await questionValidator.ValidateAsync(question);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("QuestionTest", "Test");
                }
                StringContent content = new StringContent(JsonConvert.SerializeObject(question), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(apiUrl + "Questions", content);

                    if (response.IsSuccessStatusCode)
                    {


                        var responseContent = await response.Content.ReadAsStringAsync();

                        var createdQuestion = JsonConvert.DeserializeObject<Question>(responseContent);
                        foreach (var answer in model.AnswerOptionViewModels)
                        {
                           
                            AnswerOption answerOption = new AnswerOption();
                            answerOption.NumberAnswer = i++;
                            answerOption.ContentAnswer = answer.AnswerOption;
                            if (answer.IsCorrectAnswer == true)
                            {
                                answerOption.CorrectnessAnswer = 0;
                            }
                            else
                            {
                                answerOption.CorrectnessAnswer = 1;
                            }
                            var validationResultTwo = await answerOptionValidator.ValidateAsync(answerOption);

                            if (!validationResultTwo.IsValid)
                            {
                                var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                                TempData["ErrorValidation"] = errorMessages;
                                return RedirectToAction("QuestionTest", "Test");
                            }
                            StringContent contentAnswerOption = new StringContent(JsonConvert.SerializeObject(answerOption), Encoding.UTF8, "application/json");
                            var responseAnswerOption = await httpClient.PostAsync(apiUrl + "AnswerOptions", contentAnswerOption);

                            if (responseAnswerOption.IsSuccessStatusCode)
                            {
                                var responseContentAnswer = await responseAnswerOption.Content.ReadAsStringAsync();
                                var createdAnswer = JsonConvert.DeserializeObject<AnswerOption>(responseContentAnswer);


                                QuestionAnswerOption questionAnswerOption = new QuestionAnswerOption();
                                questionAnswerOption.AnswerOptionsId = createdAnswer.IdAnswerOptions;
                                questionAnswerOption.QuestionId = createdQuestion.IdQuestion;

                                StringContent contentQuestionOption = new StringContent(JsonConvert.SerializeObject(questionAnswerOption), Encoding.UTF8, "application/json");
                                var responseQuestionOption = await httpClient.PostAsync(apiUrl + "QuestionAnswerOptions", contentQuestionOption);

                                if (responseQuestionOption.IsSuccessStatusCode)
                                {
                                   
                                }
                                else
                                {
                                    return BadRequest(responseQuestionOption.StatusCode);
                                }

                            }
                            else
                            {
                                return BadRequest(responseAnswerOption.StatusCode);
                            }

                        }
                        int id = int.Parse(TempData["testID"].ToString());

                        TempData.Keep("testID");



                        TestQuestion testQuestion = new TestQuestion();
                        testQuestion.QuestionId = createdQuestion.IdQuestion;
                        testQuestion.TestId = id;
                        StringContent contentTest = new StringContent(JsonConvert.SerializeObject(testQuestion), Encoding.UTF8, "application/json");

                        var responseTest = await httpClient.PostAsync(apiUrl + "TestQuestions", contentTest);


                        return RedirectToAction("QuestionTest", "Test");
                    }
                    else
                    {
                        return BadRequest(response.StatusCode);
                    }
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }

        }


        /// <summary>
        /// POST запрос фильтрации тестирований
        /// </summary>
        /// <param name="parameterId"></param>
        /// <param name="disciplineID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FiltrationTest(int parameterId, int disciplineID)
        {
            try
            {
                if (parameterId != 0 || disciplineID != 0)
                {
                    var apiUrl = configuration["AppSettings:ApiUrl"];

                    List<Test> tests = new List<Test>();

                    using (var httpClient = new HttpClient())
                    {
                        if (parameterId != 0 && disciplineID == 0)
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + "Tests/Filtration?idParameter=" + parameterId))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                            }
                        }
                        else if (parameterId == 0 && disciplineID != 0)
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + "Tests/Filtration?idDiscipline=" + disciplineID))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                            }
                        }
                        else
                        {
                            using (var response = await httpClient.GetAsync(apiUrl + $"Tests/Filtration?idDiscipline={disciplineID}&idParameter={parameterId}"))
                            {
                                var apiResponse = await response.Content.ReadAsStringAsync();
                                tests = JsonConvert.DeserializeObject<List<Test>>(apiResponse);
                            }

                        }
                        TempData["Filtration"] = JsonConvert.SerializeObject(tests);

                        if (tests.Count == 0)
                        {
                            TempData["Message"] = "По вашему запросу ничего не найдено";
                            return RedirectToAction("TestTeacher", "Test");

                        }

                        return RedirectToAction("TestTeacher", "Test");

                    }
                }
                else
                {
                    TempData["Message"] = "По вашему запросу ничего не найдено";

                    return RedirectToAction("TestTeacher", "Test");
                }
            }
            catch
            {
                return BadRequest("Ошибка");
            }
        }

        /// <summary>
        /// Загрузка представления изменения данных о тестировании
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> UpdateTest(int id)
        {
            try
            {
                int idEmployee = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                var apiUrl = configuration["AppSettings:ApiUrl"];

                Employee employee = new Employee();
                List<Discipline> disciplines = new List<Discipline>();
                List<DisciplineEmployee> disciplineEmployees = new List<DisciplineEmployee>();
                List<TestParameter> testParameters = new List<TestParameter>();
                Test tests = null;

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(apiUrl + "Employees/" + idEmployee))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        employee = JsonConvert.DeserializeObject<Employee>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "Disciplines"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplines = JsonConvert.DeserializeObject<List<Discipline>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "DisciplineEmployee"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        disciplineEmployees = JsonConvert.DeserializeObject<List<DisciplineEmployee>>(apiResponse);
                    }

                    using (var response = await httpClient.GetAsync(apiUrl + "TestParameters"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        testParameters = JsonConvert.DeserializeObject<List<TestParameter>>(apiResponse);
                    }
                    using (var response = await httpClient.GetAsync(apiUrl + "Tests/" + id))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        tests = JsonConvert.DeserializeObject<Test>(apiResponse);
                    }



                }
                
                List<DisciplineEmployee> discipline = disciplineEmployees.Where(n => n.EmployeeId == employee.IdEmployee).ToList();
                TestViewTeacher testViewTeacher = new TestViewTeacher(employee, disciplines, discipline, testParameters, new List<Test> { tests });
                return View(testViewTeacher);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// POST запрос изменения данных о тестировании
        /// </summary>
        /// <param name="IdTest"></param>
        /// <param name="nameTest"></param>
        /// <param name="testParameter"></param>
        /// <param name="disciplineTest"></param>
        /// <param name="deadlineCheck"></param>
        /// <param name="deadlineDate"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UpdateTest(int IdTest, string nameTest, string DateCreatedTest, int testParameter, int disciplineTest, bool deadlineCheck, string deadlineDate)
        {
            try
            {
                var apiUrl = configuration["AppSettings:ApiUrl"];

                int id = int.Parse(TempData["AuthUser"].ToString());

                TempData.Keep("AuthUser");

                Test test = new Test();

                test.IdTest = IdTest;
                test.NameTest = nameTest;
                test.DateCreatedTest = DateTime.Parse(DateCreatedTest);
                if (deadlineCheck)
                {
                    test.Deadline = null;
                }
                else
                {
                    test.Deadline = DateTime.Parse(deadlineDate);
                }
                test.DisciplineId = disciplineTest;
                test.EmployeeId = id;
                test.TestParameterId = testParameter;
                test.IsDeleted = 0;
                test.Active = 1;

                var validationResult = await testValidator.ValidateAsync(test);

                if (!validationResult.IsValid)
                {
                    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    TempData["ErrorValidation"] = errorMessages;
                    return RedirectToAction("TestTeacher", "Test");
                }

                StringContent content = new StringContent(JsonConvert.SerializeObject(test), Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PutAsync(apiUrl + "Tests/" + IdTest, content);

                    return RedirectToAction("TestTeacher", "Test");
                    
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Ошибка изменения данных" + ex.Message);
            }
        }
    }
}
