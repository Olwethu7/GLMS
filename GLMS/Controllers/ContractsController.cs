using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Data;
using GLMS.Models;
using GLMS.Services;
using GLMS.Models.ViewModels;

namespace GLMS.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public ContractsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: Contracts with Search/Filter
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            var contracts = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

            var viewModel = new ContractsIndexViewModel
            {
                Contracts = contracts,
                StartDate = startDate,
                EndDate = endDate,
                SelectedStatus = status
            };

            return View(viewModel);
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contracts/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Clients = await _context.Clients.Where(c => c.IsActive).ToListAsync();
            return View();
        }

        // POST: Contracts/Create - FULLY UPDATED
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract, IFormFile? SignedAgreement)
        {
            // Remove ContractReference from validation (auto-generated)
            ModelState.Remove("ContractReference");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file upload
                    if (SignedAgreement != null && SignedAgreement.Length > 0)
                    {
                        if (!_fileService.IsValidFile(SignedAgreement))
                        {
                            ModelState.AddModelError("SignedAgreement", "Only PDF files up to 10MB are allowed.");
                            ViewBag.Clients = await _context.Clients.Where(c => c.IsActive).ToListAsync();
                            return View(contract);
                        }

                        var filePath = await _fileService.UploadFileAsync(SignedAgreement, "contracts");
                        contract.SignedAgreementPath = filePath;
                    }

                    // Generate unique contract reference
                    contract.ContractReference = $"CT-{DateTime.Now.Year}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
                    contract.CreatedAt = DateTime.UtcNow;

                    // Ensure dates are in UTC
                    contract.StartDate = contract.StartDate.ToUniversalTime();
                    contract.EndDate = contract.EndDate.ToUniversalTime();

                    _context.Add(contract);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"✅ Contract {contract.ContractReference} created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating contract: {ex.Message}");
                    ViewBag.Clients = await _context.Clients.Where(c => c.IsActive).ToListAsync();
                    return View(contract);
                }
            }

            // If we got this far, something failed, redisplay form
            var errors = ModelState.Values.SelectMany(v => v.Errors).ToList();
            foreach (var error in errors)
            {
                System.Diagnostics.Debug.WriteLine($"Validation Error: {error.ErrorMessage}");
            }

            ViewBag.Clients = await _context.Clients.Where(c => c.IsActive).ToListAsync();
            return View(contract);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            ViewBag.Clients = await _context.Clients.Where(c => c.IsActive).ToListAsync();
            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract, IFormFile? SignedAgreement)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            // Remove from validation
            ModelState.Remove("ContractReference");
            ModelState.Remove("CreatedAt");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingContract = await _context.Contracts.FindAsync(id);

                    if (existingContract == null)
                    {
                        return NotFound();
                    }

                    // Handle new file upload
                    if (SignedAgreement != null && SignedAgreement.Length > 0)
                    {
                        if (!_fileService.IsValidFile(SignedAgreement))
                        {
                            ModelState.AddModelError("SignedAgreement", "Only PDF files up to 10MB are allowed.");
                            ViewBag.Clients = await _context.Clients.Where(c => c.IsActive).ToListAsync();
                            return View(contract);
                        }

                        if (!string.IsNullOrEmpty(existingContract.SignedAgreementPath))
                        {
                            await _fileService.DeleteFileAsync(existingContract.SignedAgreementPath);
                        }

                        var filePath = await _fileService.UploadFileAsync(SignedAgreement, "contracts");
                        contract.SignedAgreementPath = filePath;
                    }
                    else
                    {
                        contract.SignedAgreementPath = existingContract.SignedAgreementPath;
                    }

                    contract.ContractReference = existingContract.ContractReference;
                    contract.CreatedAt = existingContract.CreatedAt;
                    contract.UpdatedAt = DateTime.UtcNow;

                    _context.Entry(existingContract).CurrentValues.SetValues(contract);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "✅ Contract updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            ViewBag.Clients = await _context.Clients.Where(c => c.IsActive).ToListAsync();
            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                if (!string.IsNullOrEmpty(contract.SignedAgreementPath))
                {
                    await _fileService.DeleteFileAsync(contract.SignedAgreementPath);
                }

                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Contract deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }
    }
}