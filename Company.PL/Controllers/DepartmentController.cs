using Company.Ali.BLL.Interfaces;
using Company.Ali.BLL.Repositories;
using Company.Ali.DAL.Models;
using Company.Ali.PL.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.Ali.PL.Controllers
{
    // MVC Controller


    [Authorize]
    public class DepartmentController : Controller
    {

        //private readonly IDepartmentRepository _departmentrepository;
        private readonly IUnitOfWork _unitOfWork;

        // ASK CLR Create Object From DepartmentRepository
        public DepartmentController(/*IDepartmentRepository departmentRepository*/ IUnitOfWork unitOfWork)
        {
            //_departmentrepository = departmentRepository;
          _unitOfWork = unitOfWork;
        }

        [HttpGet] // GET :  /Department/Index
        public async Task<IActionResult> Index()
        {

         var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();

            return View(departments);
        }

        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDepartmentDto model)
        {
            if (ModelState.IsValid) // Server Side Validation
            {

                var department = new Department()
                {
                    Code = model.Code,
                    Name = model.Name,
                    CreateAt = model.CreateAt
                };

             await  _unitOfWork.DepartmentRepository.AddAsync(department);

                var Count = await _unitOfWork.CompleteAsync();

                if (Count > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? Id)
        {

            if (Id is null) return BadRequest("Invalid Id"); //400

            var departments = await _unitOfWork.DepartmentRepository.GetAsync(Id.Value);

            if(departments is null) return NotFound( new { statusCode = 404, message = $"Department With Id : {Id} is not Found" });

            return View(departments);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id is null) return BadRequest("Invalid Id"); //400

            var departments = await _unitOfWork.DepartmentRepository.GetAsync(Id.Value);

            if (departments is null) return NotFound(new { statusCode = 404, message = $"Department With Id : {Id} is not Found" });

            var dto = new CreateDepartmentDto()
            {
                Name = departments.Name,
                Code = departments.Code,
                CreateAt = departments.CreateAt
            };

            return View(dto);
        }

        [HttpPost]
    
        public async Task<IActionResult> Edit([FromRoute] int Id,CreateDepartmentDto model)
        {

            if (ModelState.IsValid)
            {

                var department = new Department()
                {
                    Id = Id,
                    Code = model.Code,
                    Name = model.Name,
                    CreateAt = model.CreateAt
                };

               _unitOfWork.DepartmentRepository.Update(department);

                var Count = await _unitOfWork.CompleteAsync();

                if (Count > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id is null) return BadRequest("Invalid Id"); //400

            var departments = await _unitOfWork.DepartmentRepository.GetAsync(Id.Value);

            if (departments is null) return NotFound(new { statusCode = 404, message = $"Department With Id : {Id} is not Found" });

            var dto = new CreateDepartmentDto()
            {
                Name = departments.Name,
                Code = departments.Code,
                CreateAt = departments.CreateAt
            };

            return View(dto);
        }

        [HttpPost]
      
        public async Task<IActionResult> Delete([FromRoute] int Id, CreateDepartmentDto model )
        {

            if (ModelState.IsValid)
            {

                var department = new Department()
                {
                    Id = Id,
                    Code = model.Code,
                    Name = model.Name,
                    CreateAt = model.CreateAt
                };


              _unitOfWork.DepartmentRepository.Delete(department);

                var  Count = await _unitOfWork.CompleteAsync();

                if (Count > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }

    }
}
