using AutoMapper;
using Company.Ali.BLL.Interfaces;
using Company.Ali.DAL.Helper;
using Company.Ali.DAL.Models;
using Company.Ali.PL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.Reflection.Metadata;

namespace Company.Ali.PL.Controllers
{

    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        //private readonly IEmployeeRepository _employeeRepository;
        //private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;

        // ASK CLR Create Object From DepartmentRepository
        public EmployeeController(
            //IEmployeeRepository 
            //employeeRepository ,
            //IDepartmentRepository departmentRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper
            )
        {
            _unitOfWork = unitOfWork;
            //_employeeRepository = employeeRepository;
            //_departmentRepository = departmentRepository;
            _mapper = mapper;
        }

        [HttpGet] // GET :  /Department/Index
        public async Task<IActionResult> Index(string? Search)
        {
            IEnumerable<Employee> employees;
            if (string.IsNullOrEmpty(Search))
            {


                 employees = await _unitOfWork.EmployeeRepository.GetAllAsync();
            }
            else
            {
                 employees = await _unitOfWork.EmployeeRepository.GetByNameAsync(Search);
            }
            //// Dictionary :
            //// 1.ViewData : Transfer Extra Information From Controller (Action) To View
            //ViewData["Message"] = "Hello From ViewData";



            //// 2.ViewBag  : Transfer Extra Information From Controller (Action) To View

            //ViewBag.Message = new { Message = "Hello From ViewBag"};



            // 3.TempData 

            return View(employees);
        }


        public async Task<IActionResult> Search(string? Search)
        {
            IEnumerable<Employee> employees;
            if (string.IsNullOrEmpty(Search))
            {


                employees = await _unitOfWork.EmployeeRepository.GetAllAsync();
            }
            else
            {
                employees = await _unitOfWork.EmployeeRepository.GetByNameAsync(Search);
            }
            //// Dictionary :
            //// 1.ViewData : Transfer Extra Information From Controller (Action) To View
            //ViewData["Message"] = "Hello From ViewData";



            //// 2.ViewBag  : Transfer Extra Information From Controller (Action) To View

            //ViewBag.Message = new { Message = "Hello From ViewBag"};



            // 3.TempData 

            return PartialView("EmployeePartialView/EmployeeTablePartialView", employees);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
            ViewData["departments"] = departments;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeDto model)
        {
            if (ModelState.IsValid) // Server Side Validation
            {
                // Manula Mapping
                //var employees = new Employee()
                //{
                //  Name = model.Name,
                //  Address = model.Address,
                //  Age = model.Age,
                //  CreateAt = model.CreateAt,
                //  HiringDate = model.HiringDate,
                //  Email = model.Email,
                //  IsActive = model.IsActive,
                //  IsDeleted = model.IsDeleted,
                //  Phone = model.Phone,
                //  Salary = model.Salary,
                //  DepartmentId = model.DepartmentId
                //};

                if(model.Image is not null)
                {


                 model.ImageName =  DocumentSettings.UploadFile(model.Image, "Images");
                }

               

              var employees =  _mapper.Map<Employee>(model);

              await  _unitOfWork.EmployeeRepository.AddAsync(employees);
                //_unitOfWork.EmployeeRepository.Update(employees);
                //_unitOfWork.EmployeeRepository.Delete(employees);


                var Count = await _unitOfWork.CompleteAsync();

                if (Count > 0)
                {
                    TempData["Message"] = "Employee Is Created !!";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? Id)
        {
       
            if (Id is null) return BadRequest("Invalid Id"); //400

            var employee = await _unitOfWork.EmployeeRepository.GetAsync(Id.Value);

            if (employee is null) return NotFound(new { statusCode = 404, message = $"employee With Id : {Id} is not Found" });

         var dto = _mapper.Map<CreateEmployeeDto>(employee);

            ViewBag.EmployeeId = Id;

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? Id ,string ViewName = "Edit")
        {


            if (Id is null) return BadRequest("Invalid Id"); //400

            var employee = await _unitOfWork.EmployeeRepository.GetAsync(Id.Value);
            var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
            ViewData["departments"] =departments;

            if (employee is null) return NotFound(new { statusCode = 404, message = $"employee With Id : {Id} is not Found" });

            //var employeesDto = new CreateEmployeeDto()
            //{

            //    Name = employee.Name,
            //    Address = employee.Address,
            //    Age = employee.Age,
            //    CreateAt = employee.CreateAt,
            //    HiringDate = employee.HiringDate,
            //    Email = employee.Email,
            //    IsActive = employee.IsActive,
            //    IsDeleted = employee.IsDeleted,
            //    Phone = employee.Phone,
            //    Salary = employee.Salary

            //};
            var dto = _mapper.Map<CreateEmployeeDto>(employee);

            return View(ViewName, dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] int Id, CreateEmployeeDto model, string ViewName = "Edit")
        {

            if (ModelState.IsValid)
            {


                //var employees = new Employee()
                //{
                //    Id = Id,
                //    Name = model.Name,
                //    Address = model.Address,
                //    Age = model.Age,
                //    CreateAt = model.CreateAt,
                //    HiringDate = model.HiringDate,
                //    Email = model.Email,
                //    IsActive = model.IsActive,
                //    IsDeleted = model.IsDeleted,
                //    Phone = model.Phone,
                //    Salary = model.Salary
                //    , DepartmentId = model.DepartmentId
                //};


                if(model.ImageName is not null && model.Image is not null)
                {
                    DocumentSettings.DeleteFile(model.ImageName, "images");
                }

                if (model.Image is not null)
                {
                 model.ImageName =   DocumentSettings.UploadFile(model.Image, "images");
                }



                var employees = _mapper.Map<Employee>(model);

                employees.Id = Id; 


               _unitOfWork.EmployeeRepository.Update(employees);

                var Count = await _unitOfWork.CompleteAsync();

                if (Count > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(ViewName, model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? Id)
        {
           

            return await Edit(Id, "Delete");
        }

        [HttpPost]
 
        public async Task<IActionResult> Delete([FromRoute] int Id, CreateEmployeeDto model)
        {

            if (ModelState.IsValid)
            {
          

                var employees = _mapper.Map<Employee>(model);

                employees.Id = Id; 

              _unitOfWork.EmployeeRepository.Delete(employees);

                var Count = await _unitOfWork.CompleteAsync();

                if (Count > 0)
                {
                    if (model.ImageName is not null)
                    {
                        DocumentSettings.DeleteFile(model.ImageName, "images");
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }

    }
}
