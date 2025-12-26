using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("create-theme")]
public class TeacherController : ControllerBase
{
    [HttpGet("only")]
    [Authorize(Roles = "teacher")]
    public IActionResult OnlyTeachers()
        => Ok(new { ok = true, message = "Teacher access granted." });
}