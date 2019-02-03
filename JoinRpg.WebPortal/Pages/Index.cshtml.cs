using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JoinRpg.Web.Models;
using JoinRpg.WebPortal.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JoinRpg.WebPortal.Pages
{
    public class IndexModel : PageModel
    {
        private ProjectListManager ProjectListManager { get; }

        public IndexModel(ProjectListManager projectListManager)
        {
            ProjectListManager = projectListManager;
        }

        public async Task OnGetAsync()
        {
            Model = await ProjectListManager.LoadModel();
        }

        public HomeViewModel Model { get; set; }
    }
}
