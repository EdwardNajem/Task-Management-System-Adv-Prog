using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.DTO;
using Project.Models;

namespace Project.Controllers
{
    [Route("api/project")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public ProjectsController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Projectt>>> GetProjects()
        {
            var projects = await _appDbContext.Projects.ToListAsync(); //Include(p => p.ProjectUsers).ThenInclude(pu => pu.User)

            return Ok(projects);//200
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Projectt>> DeleteProject(int id)
        {
            var project = await _appDbContext.Projects.FindAsync(id);
            if (project == null)
            { return NotFound(); } // Returns a 404 Not Found if the task with the given id is not foun}
            _appDbContext.Projects.Remove(project);
            await _appDbContext.SaveChangesAsync();

            return Ok(project); // Returns a 200 OK along with the deleted task
        }


        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Projectt>> CreateProject([FromBody] ProjectCreationDto project)
        {
            if (project == null)
            {
                return BadRequest("Invalid project data");
            }

            var newProject = new Projectt
            {
                Name = project.Name,
                Description = project.Description,
                ProjectUsers = new List<ProjectUser>()


            };

            var notFoundUserIds = new List<int>();

            foreach (var userId in project.UserIds.Distinct()) // Ensure uniqueness
            {
                var user = await _appDbContext.Users.FindAsync(userId);
                if (user != null)
                {
                    newProject.ProjectUsers.Add(new ProjectUser { User = user });
                }
                else
                {
                    notFoundUserIds.Add(userId);
                    // Optionally handle the case where a user ID doesn't exist.
                }
            }

            if (notFoundUserIds.Any())
            {
              return BadRequest($"User IDs not found: {string.Join(", ", notFoundUserIds)}");
            }

            _appDbContext.Projects.Add(newProject);
            await _appDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjects), new { id = newProject.Id }, newProject);
        }

        [HttpPut]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Projectt>> AddUserToProject([FromBody] ProjectUserDto data)
        {
            if (data == null)
            {
                return BadRequest("Invalid project data");
            }

            var project = await _appDbContext.Projects
                .Include(p => p.ProjectUsers) // Include ProjectUsers to check existing relationships
                .FirstOrDefaultAsync(p => p.Id == data.ProjectId);

            if (project == null)
            {
                return NotFound($"Project with ID {data.ProjectId} not found.");
            }

            var user = await _appDbContext.Users.FindAsync(data.UserId);
            if (user == null)
            {
                return NotFound($"User with ID {data.UserId} not found.");
            }

            // Check if the user is already added to the project
            if (project.ProjectUsers.Any(pu => pu.UserId == data.UserId))
            {
                return BadRequest($"User is already a member of this project.");
            }

            // If the project's ProjectUsers collection is null, initialize it
            if (project.ProjectUsers == null)
            {
                project.ProjectUsers = new List<ProjectUser>();
            }

            // Add the user to the project
            project.ProjectUsers.Add(new ProjectUser { User = user });
            await _appDbContext.SaveChangesAsync();

            return Ok(project);
        }


        [HttpDelete("RemoveUser")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Projectt>> RemoveUserFromProject([FromBody] ProjectUserDto data)
        {
            if (data == null)
            {
                return BadRequest("Invalid project data");
            }

            var project = await _appDbContext.Projects
                .Include(p => p.ProjectUsers)
                .FirstOrDefaultAsync(p => p.Id == data.ProjectId);

            if (project == null)
            {
                return NotFound($"Project with ID {data.ProjectId} not found.");
            }

            var projectUser = project.ProjectUsers.FirstOrDefault(pu => pu.UserId == data.UserId);
            if (projectUser == null)
            {
                return NotFound($"User with ID {data.UserId} not found in project with ID {data.ProjectId}.");
            }

            project.ProjectUsers.Remove(projectUser);
            await _appDbContext.SaveChangesAsync();

            return Ok(project);
        }



        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Projectt>> GetProjectById(int id)
        {
            var project = await _appDbContext.Projects
                .Include(p => p.ProjectUsers)
                    .ThenInclude(pu => pu.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound(); // Returns a 404 Not Found if no project with the given id is found
            }

            return Ok(project); // Returns a 200 OK along with the project data
        }




    }
}
