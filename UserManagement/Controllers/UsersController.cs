using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserManagement.Entities;
using UserManagement.Services;
using UserManagement.Services.Helpers;
using UserManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace UserManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserManagementService _umService;
        private readonly IMapper _mapper;
        private readonly IHelperService _helperService;

        public UsersController(
            IConfiguration configuration,
            ILogger<AccountController> logger,
            IUserManagementService umService,
            IMapper mapper,
            IHelperService helperService)
        {
            _configuration = configuration;
            _logger = logger;
            _umService = umService;
            _mapper = mapper;
            _helperService = helperService;
        }

        // GET: api/UserManagement/Applicants
        // [Authorize(Roles = "admin")]
        [HttpGet("")]
        public async Task<ActionResult<PageUsersModel>> GetUsers(int? offset, int? limit, string sortOrder, string searchString)
        {
            PageUsersModel pageUsersModel = new PageUsersModel();

            pageUsersModel.TotalUsers = await _umService.GetAllUsersCountAsync(searchString);

            List<ApplicationUser> users = await _umService.GetUsersAsync(offset ?? 0, limit ?? 10, sortOrder, searchString);

            List<UserModel> userModels = new List<UserModel>();
            // Refactor to return role as part of the user (using a Dto) and avoid multiple
            // round-trips to the database
            foreach (ApplicationUser user in users)
            {
                UserModel userModel = _mapper.Map<UserModel>(user);
                string userRole = await _umService.GetUserRoleAsync(user.Id, false);
                userModel.Role = userRole;
                pageUsersModel.Users.Add(userModel);
            }
            return pageUsersModel;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUser(string id)
        {
            var user = await _umService.FindUserAsync(id);

            if (user == null)
                return NotFound();

            UserModel userModel = _mapper.Map<UserModel>(user);
            string userRole = await _umService.GetUserRoleAsync(user.Id, true);
            userModel.Role = userRole;

            return userModel;
        }

        [HttpPost("")]
        public async Task<ActionResult<UserModel>> PostUser(UserModel userModel)
        {
            if (!ModelState.IsValid) // Is automatically done by the [ApiController] controller attribute
                return BadRequest(ModelState);

            if (await _umService.IsEmailInUseAsync(userModel.Email))
            {
                ModelState.AddModelError("Email", "Email already in use.");
                return BadRequest(ModelState);
            }

            ApplicationUser user = _mapper.Map<ApplicationUser>(userModel);
            user.EmailConfirmed = true;
            user.UserName = userModel.Email;
            IdentityResult result = await _umService.AddUserAsync(user, userModel.Password, userModel.Role);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, UpdateUserModel userModel)
        {
            if (id != userModel.Id || !ModelState.IsValid)
                return BadRequest();

            ApplicationUser user = await _umService.FindUserAsync(userModel.Id);

            if (user == null)
                return NotFound();

            if (await _umService.IsEmailInUseAsync(userModel.Email, userModel.Id))
            {
                ModelState.AddModelError("Email", "Email already in use");
                return BadRequest(ModelState);
            }

            _mapper.Map(userModel, user);

            try
            {
                await _umService.UpdateUserAsync(user, userModel.Role, userModel.RowVersion);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var databaseUser = _helperService.RetrieveEntity(ex);
                if (databaseUser == null)
                {
                    ModelState.AddModelError("", "User deleted by another user.");
                }
                else
                {
                    ModelState.AddModelError("", "User modified by another user.");
                }

                return BadRequest(ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _umService.DeleteUserAsync(id);

            if (result.Succeeded)
                return NoContent();

            return NotFound();
        }

        // GET: api/UserManagement
        [HttpGet("roles")]
        public ActionResult<List<RoleHelpers.RolePair>> GetRoles()
        {
            return RoleHelpers.Roles;
        }

        [HttpPost("{id}/change-password")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangeUserPassword(string id, PasswordChangeModel model)
        {
            ApplicationUser user = await _umService.FindUserAsync(id);

            if (user == null)
                return NotFound();

            var result = await _umService.ChangePasswordAsync(user, model.Password);

            if (result.Succeeded)
                return Ok();

            return BadRequest();
        }

        // GET: api/UserManagement/Admins
        [Authorize(Roles = "admin")]
        [HttpGet("admins")]
        public ActionResult<IEnumerable<string>> Admins()
        {
            return new string[] { "Admin1", "Admin2" };
        }

        // GET: api/UserManagement/Applicants
        [Authorize(Roles = "applicant")]
        [HttpGet("applicants")]
        public ActionResult<IEnumerable<string>> Applicants()
        {
            return new string[] { "Applicant1", "Applicant2" };
        }

        //// GET: api/UserManagement
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET: api/UserManagement/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/UserManagement
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT: api/UserManagement/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
