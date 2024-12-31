using GymManagement.CustomControllers;
using GymManagement.Data;
using GymManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace GymManagement.Controllers
{
    [Authorize]
    public class FitnessCategoryController : CognizantController
	{
		private readonly GymContext _context;

        public FitnessCategoryController(GymContext context)
        {
            _context = context;
        }

        // GET: FitnessCategory
        public async Task<IActionResult> Index()
        {
            var fitnessCategories = await _context.FitnessCategories
                .OrderBy(fc => fc.Category)
                .Include(c=>c.ExerciseCategories)
                .AsNoTracking()
                .ToListAsync();
            return View(fitnessCategories);
        }

		// GET: FitnessCategory/Details/5
		[Authorize(Roles = "Admin,Supervisor,Staff")]
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCategory = await _context.FitnessCategories
				.Include(fc => fc.ExerciseCategories).ThenInclude(ec => ec.Exercise)
				.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (fitnessCategory == null)
            {
                return NotFound();
            }

            return View(fitnessCategory);
        }

		// GET: FitnessCategory/Create
		[Authorize(Roles = "Admin,Supervisor,Staff")]
		public IActionResult Create()
        {
            return View();
        }

        // POST: FitnessCategory/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin,Supervisor,Staff")]
		public async Task<IActionResult> Create([Bind("Category")] FitnessCategory fitnessCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(fitnessCategory);
                    await _context.SaveChangesAsync();
                    var returnUrl = ViewData["returnURL"]?.ToString();
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    return Redirect(returnUrl);
                }
			}
			catch (DbUpdateException dex)
			{
				string message = dex.GetBaseException().Message;
				if (message.Contains("UNIQUE") && message.Contains("FitnessCategories.Category"))
				{
					ModelState.AddModelError("", "Unable to create the record. " +
						"The Fitness Category is already in our records, it must be unique.");
				}
				else
				{
					ModelState.AddModelError("", "Unable to create the record. " +
						"Try again, and if the problem persists contact your system administrator.");
				}
			}

			//Decide if we need to send the Validaiton Errors directly to the client
			if (!ModelState.IsValid && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				//Was an AJAX request so build a message with all validation errors
				string errorMessage = "";
				foreach (var modelState in ViewData.ModelState.Values)
				{
					foreach (ModelError error in modelState.Errors)
					{
						errorMessage += error.ErrorMessage + "|";
					}
				}
				//Note: returning a BadRequest results in HTTP Status code 400
				return BadRequest(errorMessage);
			}

			return View(fitnessCategory);
        }

		// GET: FitnessCategory/Edit/5
		[Authorize(Roles = "Admin,Supervisor,Staff")]
		public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCategory = await _context.FitnessCategories.FindAsync(id);
            if (fitnessCategory == null)
            {
                return NotFound();
            }
            return View(fitnessCategory);
        }

        // POST: FitnessCategory/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin,Supervisor,Staff")]
		public async Task<IActionResult> Edit(int id)
        {
            //Go get the Doctor to update
            var fitnessCategoryToUpdate = await _context.FitnessCategories
                .FirstOrDefaultAsync(p => p.ID == id);

            //Check that you got it or exit with a not found error
            if (fitnessCategoryToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<FitnessCategory>(fitnessCategoryToUpdate, "",
                d => d.Category))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    var returnUrl = ViewData["returnURL"]?.ToString();
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    return Redirect(returnUrl);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FitnessCategoryExists(fitnessCategoryToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
				}
				catch (DbUpdateException dex)
				{
					string message = dex.GetBaseException().Message;
					if (message.Contains("UNIQUE") && message.Contains("FitnessCategories.Category"))
					{
						ModelState.AddModelError("", "Unable to update the record. " +
							"The Fitness Category is already in our records, it must be unique.");
					}
					else
					{
						ModelState.AddModelError("", "Unable to update the record. " +
							"Try again, and if the problem persists contact your system administrator.");
					}
				}

			}
            return View(fitnessCategoryToUpdate);
        }

		// GET: FitnessCategory/Delete/5
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fitnessCategory = await _context.FitnessCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (fitnessCategory == null)
            {
                return NotFound();
            }

            return View(fitnessCategory);
        }

        // POST: FitnessCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fitnessCategory = await _context.FitnessCategories.FindAsync(id);
            try
            {
                if (fitnessCategory != null)
                {
                    _context.FitnessCategories.Remove(fitnessCategory);
                }

                await _context.SaveChangesAsync();
                var returnUrl = ViewData["returnURL"]?.ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction(nameof(Index));
                }
                return Redirect(returnUrl);
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete Fitness Category. " +
                        "Remember, you cannot delete a Category if there are Group Classes in it.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(fitnessCategory);

        }

		// Insert From Excel
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> InsertFromExcel(IFormFile theExcel)
		{
			string badFeedBack = string.Empty;
			string goodFeedBack = string.Empty;
			if (theExcel != null)
			{
				string mimeType = theExcel.ContentType;
				long fileLength = theExcel.Length;
				if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
				{
					if (mimeType.Contains("excel") || mimeType.Contains("spreadsheet"))
					{
						ExcelPackage excel;
						using (var memoryStream = new MemoryStream())
						{
							await theExcel.CopyToAsync(memoryStream);
							excel = new ExcelPackage(memoryStream);
						}
						var workSheet = excel.Workbook.Worksheets[0];
						var start = workSheet.Dimension.Start;
						var end = workSheet.Dimension.End;
						int successCount = 0;
						int errorCount = 0;

						if (workSheet.Cells[1, 1].Text == "Exercise" && workSheet.Cells[1, 2].Text == "FitnessCategory")
						{
							for (int row = start.Row + 1; row <= end.Row; row++)
							{
								// Row by row...
								string exerciseName = workSheet.Cells[row, 1].Text;
								string fitnessCategoryName = workSheet.Cells[row, 2].Text;
								try
								{
									// Exercise
									Exercise exer = _context.Exercises.FirstOrDefault(e => e.Name == exerciseName);
									if (exer == null)
									{
										exer = new Exercise { Name = exerciseName };
										_context.Exercises.Add(exer);
										_context.SaveChanges();
									}
									// Fitness categories
									FitnessCategory fitnessCat = _context.FitnessCategories.FirstOrDefault(fc => fc.Category == fitnessCategoryName);
									if (fitnessCat == null)
									{
										fitnessCat = new FitnessCategory { Category = fitnessCategoryName };
										_context.FitnessCategories.Add(fitnessCat);
										_context.SaveChanges();
									}
									// ExerciseCategory
									if (!_context.ExerciseCategories.Any(ec => ec.ExerciseID == exer.ID && ec.FitnessCategoryID == fitnessCat.ID))
									{
										var exCat = new ExerciseCategory { FitnessCategoryID = fitnessCat.ID, ExerciseID = exer.ID };
										_context.ExerciseCategories.Add(exCat);
										_context.SaveChanges();
										
										successCount++;
									}
									else
									{
										errorCount++;
										badFeedBack += "Error: Record " + exerciseName + " (" + fitnessCategoryName + ")"
											+ " was rejected as a duplicate.";
									}
								}
								catch (DbUpdateException dex)
								{
									errorCount++;
									if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
									{
										badFeedBack += "Error: Record " + exerciseName + " (" + fitnessCategoryName + ")"
											+ " was rejected as a duplicate." + "<br />";
									}
									else
									{
										badFeedBack += "Error: Record " + exerciseName + " (" + fitnessCategoryName + ")"
											+ " caused an error." + "<br />";
									}
									//Here is the trick to using SaveChanges in a loop.  You must remove the 
									//offending object from the cue or it will keep raising the same error.
									//_context.Remove(exer);
									//_context.Remove(fitnessCat);
									//_context.Remove(exCat);
								}
								catch (Exception ex)
								{
									errorCount++;
									if (ex.GetBaseException().Message.Contains("correct format"))
									{
										badFeedBack += "Error: Record " + exerciseName + " (" + fitnessCategoryName + ")"
											+ " was rejected becuase it was not in the correct format." + "<br />";
									}
									else
									{
										badFeedBack += "Error: Record " + exerciseName + " (" + fitnessCategoryName + ")" 
											+ " caused and error." + "<br />";
									}
								}
							}
							goodFeedBack += "Finished Importing " + (successCount + errorCount).ToString() +
								" Records with " + successCount.ToString() + " inserted and " +
								errorCount.ToString() + " rejected.";
						}
						else
						{
							badFeedBack = "Error: You may have selected the wrong file to upload.<br /> " +
								"Remember, you must have the heading 'Exercise' in the first cell of the first row" +
								" and 'FitnessCategory' in the second cell of the first row.";
						}
					}
					else
					{
						badFeedBack = "Error: That file is not an Excel spreadsheet.";
					}
				}
				else
				{
					badFeedBack = "Error:  file appears to be empty.";
				}
			}
			else
			{
				badFeedBack = "Error: No file uploaded.";
			}

			TempData["BadFeedback"] = badFeedBack;
			TempData["GoodFeedback"] = goodFeedBack + (goodFeedBack == "" ? "<br />" : "<br /><br />");

			return RedirectToAction(nameof(Index));
		}

		private bool FitnessCategoryExists(int id)
        {
            return _context.FitnessCategories.Any(e => e.ID == id);
        }
    }
}
