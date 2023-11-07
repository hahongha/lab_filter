using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnKT_Lab4.Data;
using OnKT_Lab4.Models;

namespace OnKT_Lab4.Controllers
{
    public class LearnerController : Controller
    {
        private int pageSize = 1;
        private SchoolContext db;
        public LearnerController(SchoolContext context)
        {
            db = context;
        }

		//public IActionResult Index()
		//{
		//    var learners = db.Learners.Include(m => m.Major).ToList();
		//    return View(learners);
		//}

		//public IActionResult Index(int? mid)
		//{
		//	if (mid == null)
		//	{
		//		var Learners = db.Learners.Include(m => m.Major).ToList();
		//		return View(Learners);
		//	}
		//	else
		//	{
		//		var Learners = db.Learners.Where(l => l.MajorID == mid).Include(m => m.Major).ToList();
		//		return View(Learners);
		//	}

		//}
		public IActionResult Index(int? mid)
		{
			var learners = (IQueryable<Learner>)db.Learners.Include(m => m.Major);
			if (mid != null)
			{
				learners = (IQueryable<Learner>)db.Learners.Where(l => l.MajorID == mid).Include(m => m.Major);
			}
            //tinh so trang
			int pageNum = (int)Math.Ceiling(learners.Count() / (float)pageSize);
            //tra so trang de hien thi nav trang
			ViewBag.pageNum = pageNum;
            //lay du lieu trang dau
			var result = learners.Take(pageSize).ToList();
			return View(result);
		}

		public IActionResult LearnerFilter(int? mid, string? keyword, int? pageIndex)
		{
            //lay toan bo learner trong dbset chuyen ve iquery de query
			var learners = (IQueryable<Learner>)db.Learners;
            //lay chi so trang neu null thi =1
			int page = (int)(pageIndex == null || pageIndex <= 0 ? 1 : pageIndex);

            //neu co mid thi loc theo mid
			if (mid != null)
			{
                //loc
				learners = learners.Where(l => l.MajorID == mid);
                //Gui mid de nav phan trang
				ViewBag.mid = mid;
			}
            //neu co keyword thi kiem theo ten
			if (keyword != null)
			{
                //find
				learners = learners.Where(l => l.FirstMidName.ToLower().Contains(keyword.ToLower()));
                //gui keyword ve view
				ViewBag.keyword = keyword;
			}
            //tinh so trang
			int pageNum = (int)Math.Ceiling(learners.Count() / (float)pageSize);
            //gui so trang ve view
			ViewBag.pageNum = pageNum;

            //chon du lieu cho trang hien tai
			var result = learners.Skip(pageSize * (page - 1)).Take(pageSize).Include(m => m.Major);
			return PartialView("LearnerTable", result);
		}

		// //Load dữ liệu không đồng bộ sử dụng AJAX
		public IActionResult LearnerByMajorID(int mid)
		{
			var Learners = db.Learners.Where(l => l.MajorID == mid).Include(m => m.Major).ToList();
			return PartialView("LearnerTable", Learners);
		}

		[HttpGet]
        public IActionResult Create()
        {
            //dùng 1 trong 2 cách để tạo SelectList gửi về View qua ViewBag để
            //hiển thị danh sách chuyên ngành (Majors)
            var majors = new List<SelectListItem>(); //cách 1
            foreach (var item in db.Majors)
            {
                majors.Add(new SelectListItem
                {
                    Text = item.MajorName,

                    Value = item.MajorID.ToString()
                });

            }
            ViewBag.MajorID = majors;
            ViewBag.MajorID = new SelectList(db.Majors, "MajorID", "MajorName"); //cách 2
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("FirstMidName,LastName,MajorID,EnrollmentDate")]
        Learner learner)
        {
            if (ModelState.IsValid)
            {
                db.Learners.Add(learner);
                db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            //lại dùng 1 trong 2 cách tạo SelectList gửi về View để hiển thị danh sách Majors
            ViewBag.MajorID = new SelectList(db.Majors, "MajorID", "MajorName");
            return View();
        }
        //edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if(id == null || db.Learners == null)
            {
                return NotFound();
            }
            var learner = db.Learners.Find(id);
            if (learner == null)
            {
                return NotFound();
            }

            // Tạo SelectList để hiển thị danh sách chuyên ngành (Majors)
            //var majors = new SelectList(db.Majors, "MajorID", "MajorName");
            //ViewBag.MajorID = majors;
            ViewBag.MajorID = new SelectList(db.Majors, "MajorID", "MajorName", learner.MajorID);
            return View(learner);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("LearnerID,FirstMidName,LastName,MajorID,EnrollmentDate")] Learner learner)
        {
            if (id != learner.LearnerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Update(learner);
                    db.SaveChanges();
                }catch(DbUpdateConcurrencyException)
                {
                    if(!LearnerExists(learner.LearnerID))
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

            // Tạo SelectList để hiển thị danh sách Majors
            ViewBag.MajorID = new SelectList(db.Majors, "MajorID", "MajorName", learner.MajorID);
            return View(learner);
        }

        private bool LearnerExists(int id)
        {
            return (db.Learners?.Any(e=> e.LearnerID == id)).GetValueOrDefault();
        }

        // Delete
        public IActionResult Delete(int id)
        {
            if (id == null || db.Learners == null)
            {
                return NotFound();
            }

            //var learners = db.Learners.Find(id);
            var learner = db.Learners.Include(l=>l.Major).Include(e=> e.Enrollments).FirstOrDefault(m=> m.LearnerID==id);
            if (learner == null)
            {
                return NotFound();
            }
            if(learner.Enrollments.Count() > 0)
            {
                return Content("this learner has somw enrollments, cant's delete!");
            }
            //ViewBag.MajorID = new SelectList(db.Majors, "MajorID", "MajorName");
            return View(learner);

        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if(db.Learners == null)
            {
                return Problem("Entity set Learner is null");
            }
            var learner = db.Learners.Find(id);
            if (learner == null)
            {
                return NotFound();
            }

            db.Learners.Remove(learner);
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
