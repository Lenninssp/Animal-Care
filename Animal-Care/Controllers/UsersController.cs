using Animal_Care.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Animal_Care.Controllers
{
    public class UsersController : Controller
    {
        private readonly AnimalCare2Context _context;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public UsersController(AnimalCare2Context context)
        {
            _context = context;
        }

        // ===================== AUTH =====================

        // GET: Users/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Name) // "Admin", "Veterinarian", "Receptionist"
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login", "Users");
        }

        // ===================== SELF-REGISTER =====================

        // GET: Users/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            var model = new RegisterViewModel
            {
                Roles = _context.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = r.Name
                    })
                    .ToList()
            };

            return View(model);
        }

        // POST: Users/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = _context.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = r.Name
                    })
                    .ToList();

                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("", "Email already registered.");

                model.Roles = _context.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = r.Name
                    })
                    .ToList();

                return View(model);
            }

            var role = await _context.Roles.FindAsync(model.RoleId);
            if (role == null)
            {
                ModelState.AddModelError("RoleId", "Selected role does not exist.");

                model.Roles = _context.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = r.Name
                    })
                    .ToList();

                return View(model);
            }

            var user = new User
            {
                Email = model.Email,
                FullName = model.FullName,
                Phone = model.Phone,
                RoleId = model.RoleId!.Value
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        // ===================== ADMIN CRUD =====================

        // GET: Users
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = _context.Users.Include(u => u.Role);
            return View(await users.ToListAsync());
        }

        // GET: Users/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create (admin-created users)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name");
            return View();
        }

        // POST: Users/Create (admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            // We treat PasswordHash property as the plain password input here
            var rawPassword = user.PasswordHash;

            // Remove any validation errors for navigation props if any
            ModelState.Remove("Role");
            ModelState.Remove("Appointments");
            ModelState.Remove("Veterinarian");

            if (string.IsNullOrWhiteSpace(rawPassword))
            {
                ModelState.AddModelError("PasswordHash", "Password is required.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already registered.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name", user.RoleId);
                return View(user);
            }

            // Hash the raw password and save
            user.PasswordHash = _passwordHasher.HashPassword(user, rawPassword);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name", user.RoleId);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,FullName,Phone,RoleId")] User edited)
        {
            if (id != edited.Id)
            {
                return NotFound();
            }

            // Remove nav props from validation
            ModelState.Remove("Role");
            ModelState.Remove("Appointments");
            ModelState.Remove("Veterinarian");
            ModelState.Remove("PasswordHash"); // we don't edit password here

            if (!ModelState.IsValid)
            {
                ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name", edited.RoleId);
                return View(edited);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update allowed fields only
            user.Email = edited.Email;
            user.FullName = edited.FullName;
            user.Phone = edited.Phone;
            user.RoleId = edited.RoleId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'AnimalCare2Context.Users' is null.");
            }

            var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(currentUserIdString, out var currentUserId);

            var user = await _context.Users
                .Include(u => u.Appointments)
                .Include(u => u.Veterinarian)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Prevent deleting yourself
            if (currentUserId == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction("Details", new { id });
            }

            // Prevent deleting if referenced as vet or receptionist
            var hasAppointments = user.Appointments != null && user.Appointments.Any();
            var isVet = user.Veterinarian != null;

            if (hasAppointments || isVet)
            {
                TempData["ErrorMessage"] =
                    "Cannot delete this user because they are referenced in appointments or as a veterinarian. " +
                    "Reassign or remove those records first.";

                return RedirectToAction("Details", new { id });
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting user: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
