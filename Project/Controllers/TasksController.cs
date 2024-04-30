using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.DTO;
using System.Security.Claims;
using Task = Project.Models.Task;


namespace Project.Controllers
{
    [Route("api/task")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public TasksController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Task>>> GetTasks()
        {
            var tasks = await _appDbContext.Tasks.ToListAsync();
            if (tasks == null || tasks.Count == 0)
            { return NotFound(); } // Returns a 404 Not Found 
            return Ok(tasks); // Returns a 200 OK 
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Task>>> GetTaskById(int id)
        {
            var task = await _appDbContext.Tasks.FindAsync(id);
            if (task == null)
            { return NotFound(); } // Returns a 404 Not Found 
            return Ok(task); // Returns a 200 OK 
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Task>> DeleteTask(int id)
        {
            var task = await _appDbContext.Tasks.FindAsync(id);
            if (task == null)
            { return NotFound(); } // Returns a 404 Not Found if the task with the given id is not foun}
            _appDbContext.Tasks.Remove(task);
            await _appDbContext.SaveChangesAsync();

            return Ok(task); // Returns a 200 OK along with the deleted task
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Task>> CreateTask([FromBody] TaskCreationDto taskDto)
        {
            if (taskDto == null)
            {return BadRequest("Invalid task data");}

            var project = await _appDbContext.Projects.FindAsync(taskDto.ProjectId);
            if (project == null)
            {return NotFound($"Project with ID {taskDto.ProjectId} not found.");}

            var user = await _appDbContext.Users.FindAsync(taskDto.UserId);
            if (user == null)
            {return NotFound($"User with ID {taskDto.UserId} not found.");}

            var assignedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the assignedById is not null
            if (assignedById == null)
            {
                return BadRequest("Invalid user data");
            }

            var task = new Task
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                Status = taskDto.Status,
                Project = project,
                User = user,
                AssignedById = int.Parse(assignedById)
            };

            _appDbContext.Tasks.Add(task);
            await _appDbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
        }


        [HttpPut]
        [Authorize]
        public async Task<ActionResult<Task>> UpdateTaskStatus([FromBody] TaskStatusUpdateDto taskUpdateDto)
        {
            // Retrieve the user's email from the JWT token claims
            var name = User.FindFirst(ClaimTypes.Name)?.Value;

            // Retrieve the user from the database based on the email
            var user = await _appDbContext.Users.FirstOrDefaultAsync(u => u.Name == name);

            // Check if the user object is null
            if (user == null)
            {
                return NotFound($"User with email {ClaimTypes.Name} not found.");
            }

            if (taskUpdateDto == null)
            {
                return BadRequest("Invalid request data");
            }

            var task = await _appDbContext.Tasks.FindAsync(taskUpdateDto.Id);

            if (task == null)
            {
                return NotFound($"Task with ID {taskUpdateDto.Id} not found.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the current user's ID is null or the task's AssignedById does not match the current user's ID when status is being updated to 'Done'
            if (currentUserId == null || (taskUpdateDto.Status == "Done" && task.AssignedById != int.Parse(currentUserId)))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Only the admin who assigned the task can update its status to 'Done'." });
            }

            else if ((task.UserId == user.Id && taskUpdateDto.Status != "Done") || (taskUpdateDto.Status == "Done" && task.AssignedById == int.Parse(currentUserId))) // Now user is confirmed not to be null
            {
                task.Status = taskUpdateDto.Status;
                _appDbContext.Tasks.Update(task);
                await _appDbContext.SaveChangesAsync();
                return Ok(task);
            }

            return StatusCode(StatusCodes.Status403Forbidden, new { message = "This Task is not assigned to You." });
        }

        [HttpGet("ByProject/{projectId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Task>>> GetTasksByProjectId(int projectId)
        {
            var tasks = await _appDbContext.Tasks
                            .Where(t => t.ProjectId == projectId)
                            .ToListAsync();

            return Ok(tasks);
        }




    }
}
