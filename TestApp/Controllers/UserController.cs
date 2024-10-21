using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TestApp.Models;
using TestApp.Data;
using TestApp.Services;
using BGProcess.Services;
using BGProcess.Interface;
using Microsoft.Extensions.Logging;

namespace TestApp.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;
        public readonly SendMail _sendMail;
        private readonly IEmailQueue _emailQueue;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService userService, SendMail sendMail, ILogger<UserController> logger, IEmailQueue emailQueue)
        {
            _userService = userService;
            _sendMail = sendMail;
            _logger = logger;
            _emailQueue = emailQueue;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(UsersEntity user)
        {
            if (ModelState.IsValid)
            {
                await _userService.CreateUserAsync(user);
                TempData["Message"] = "Pengguna berhasil dibuat!";
                return RedirectToAction("Index");
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UsersEntity user)
        {
            if (ModelState.IsValid)
            {
                await _userService.UpdateUserAsync(user);
                TempData["Message"] = "Pengguna berhasil diperbarui!";
                return RedirectToAction("Index");
            }
            return View(user);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _userService.DeleteUserAsync(id);
            TempData["Message"] = "Pengguna berhasil dihapus!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Send(int id)
        {
            var user = await _userService.GetUserByIdAsync(id); 
            return View(user);
        }

         [HttpPost, ActionName("Send")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(string email, string subject, string message)
        {
            await _emailQueue.EnqueueEmail(email, subject, message); // Enqueue email for background processing
            TempData["AlertMessage"] = "Email has been sent successfully";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> SendTestEmail()
        {
            await _emailQueue.EnqueueEmail("mochamadaris112@gmail.com", "WELCOME TO ARS OFFICE", "Welcome");
            Thread.Sleep(100);
            return Ok("Email queued.");
        }

        public IActionResult SendBulkEmails()
        {
            for (int i = 0; i < 5; i++)
            {
                _emailQueue.EnqueueEmail("mochamadaris112@gmail.com", $"Test Subject {i}", $"Test Message {i}");
            }
            return Ok("Bulk emails have been queued.");
        }      
    }
}