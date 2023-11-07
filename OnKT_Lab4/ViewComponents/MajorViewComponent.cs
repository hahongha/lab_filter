using Microsoft.AspNetCore.Mvc;
using OnKT_Lab4.Data;
using OnKT_Lab4.Models;

namespace OnKT_Lab4.ViewComponents
{
    public class MajorViewComponent: ViewComponent
    {
        SchoolContext db;
        List<Major> majors;
        public MajorViewComponent(SchoolContext context)
        {
            db = context;
            majors = db.Majors.ToList();
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("RenderMajor", majors);
        }
    }
}
