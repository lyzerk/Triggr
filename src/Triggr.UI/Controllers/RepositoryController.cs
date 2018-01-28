using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Triggr.Data;
using Triggr.Providers;

namespace Triggr.UI.Controllers
{
    public class RepositoryController : Controller
    {
        private readonly TriggrContext _context;
        private readonly IProviderFactory _providerFactory;
        public RepositoryController(TriggrContext context, IProviderFactory providerFactory)
        {
            _context = context;
            _providerFactory = providerFactory;

        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetRepositories()
        {
            var lists = await _context.Repositories.ToListAsync();
            return Json(lists);
        }

        public IActionResult GetProvider(string url)
        {
            return Json(_providerFactory.GetProviderType(url));
        }

        [HttpPost]
        public async Task<IActionResult> AddRepository([FromBody]Models.AddRepositoryViewModel model)
        {
            bool result = false;
            var providerType = _providerFactory.GetProviderType(model.Url);
            if (ModelState.IsValid && !string.IsNullOrEmpty(providerType))
            {
                Repository repository = new Repository();
                repository.Provider = providerType;
                repository.UpdatedTime = DateTimeOffset.Now;
                repository.Url = model.Url;
                await _context.SaveChangesAsync(); 
                _context.Add(repository);
                var affected = await _context.SaveChangesAsync();
                result = affected > 0;
                //result = true;
            }

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRepository([FromBody]Models.IdFormViewModel model)
        {
            bool result = false;

            if (ModelState.IsValid)
            {
                var repo = await _context.Repositories.FindAsync(model.Id);
                if (repo != null)
                {
                    _context.Remove(repo);
                    var affected = await _context.SaveChangesAsync();
                    result = affected > 0;
                }
            }

            return Json(result);
        }

    }
}