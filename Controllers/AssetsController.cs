using EITSTMS.Data;
using EITSTMS.Models.Entities;
using EITSTMS.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EITSTMS.Controllers
{
    [Authorize(Roles = "Technician,Admin")]
    public class AssetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Asset> assets =
                await _context.Assets
                    .OrderBy(a => a.AssetNumber)
                    .ToListAsync();

            return View(assets);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Asset asset)
        {
            if (!ModelState.IsValid)
            {
                return View(asset);
            }

            // 检查资产编号是否已存在
            bool assetNumberExists =
                await _context.Assets
                    .AnyAsync(a => a.AssetNumber == asset.AssetNumber);
            if (assetNumberExists)
            {
                ModelState.AddModelError(
                    "AssetNumber",
                    "该资产编号已存在，请使用其他编号。");

                return View(asset);
            }

            // 检查序列号是否已存在（如果提供了序列号）
            if (!string.IsNullOrWhiteSpace(asset.SerialNumber))
            {
                bool serialNumberExists =
                    await _context.Assets
                        .AnyAsync(a => a.SerialNumber == asset.SerialNumber);

                if (serialNumberExists)
                {
                    ModelState.AddModelError(
                        "SerialNumber",
                        "该序列号已存在，请检查后重新输入。");

                    return View(asset);
                }
            }

            _context.Assets.Add(asset);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}