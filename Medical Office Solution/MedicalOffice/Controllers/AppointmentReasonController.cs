﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOffice.Data;
using MedicalOffice.Models;
using MedicalOffice.CustomControllers;
using System.Numerics;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;

namespace MedicalOffice.Controllers
{
    [Authorize(Roles = "Admin,Supervisor")]
    public class AppointmentReasonController : LookupsController
    {
        private readonly MedicalOfficeContext _context;

        public AppointmentReasonController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: AppointmentReason
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: AppointmentReason/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AppointmentReason/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ReasonName")] AppointmentReason appointmentReason)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(appointmentReason);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(appointmentReason);
        }

        // GET: AppointmentReason/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AppointmentReasons == null)
            {
                return NotFound();
            }

            var appointmentReason = await _context.AppointmentReasons.FindAsync(id);
            if (appointmentReason == null)
            {
                return NotFound();
            }
            return View(appointmentReason);
        }

        // POST: AppointmentReason/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            //Go get the Doctor to update
            var appointmentReasonToUpdate = await _context.AppointmentReasons
                .FirstOrDefaultAsync(p => p.ID == id);

            //Check that you got it or exit with a not found error
            if (appointmentReasonToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<AppointmentReason>(appointmentReasonToUpdate, "",
                d => d.ReasonName))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentReasonExists(appointmentReasonToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(appointmentReasonToUpdate);
        }

        // GET: AppointmentReason/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AppointmentReasons == null)
            {
                return NotFound();
            }

            var appointmentReason = await _context.AppointmentReasons
                .FirstOrDefaultAsync(m => m.ID == id);
            if (appointmentReason == null)
            {
                return NotFound();
            }

            return View(appointmentReason);
        }

        // POST: AppointmentReason/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AppointmentReasons == null)
            {
                return Problem("Entity set 'MedicalOfficeContext.AppointmentReasons'  is null.");
            }
            var appointmentReason = await _context.AppointmentReasons
                  .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (appointmentReason != null)
                {
                    _context.AppointmentReasons.Remove(appointmentReason);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete " + ViewData["ControllerFriendlyName"] +
                        ". Remember, you cannot delete a " + ViewData["ControllerFriendlyName"] + " that has related records.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(appointmentReason);

        }

        [HttpPost]
        public async Task<IActionResult> InsertFromExcel(IFormFile theExcel)
        {
            //Note: This is a very basic example and has 
            //no ERROR HANDLING.  It also assumes that
            //duplicate values are allowed, both in the 
            //uploaded data and the DbSet.
            ExcelPackage excel;
            using (var memoryStream = new MemoryStream())
            {
                await theExcel.CopyToAsync(memoryStream);
                excel = new ExcelPackage(memoryStream);
            }
            var workSheet = excel.Workbook.Worksheets[0];
            var start = workSheet.Dimension.Start;
            var end = workSheet.Dimension.End;

            //Start a new list to hold imported objects
            List<AppointmentReason> appointmentReasons = new List<AppointmentReason>();

            for (int row = start.Row; row <= end.Row; row++)
            {
                // Row by row...
                AppointmentReason a = new AppointmentReason
                {
                    ReasonName = workSheet.Cells[row, 1].Text
                };
                appointmentReasons.Add(a);
            }
            _context.AppointmentReasons.AddRange(appointmentReasons);
            _context.SaveChanges();
            //Note that we are assuming that you are using the Preferred Approach to Lookup Values
            //And the custom LookupsController
            return Redirect(ViewData["returnURL"].ToString());
        }

        private bool AppointmentReasonExists(int id)
        {
          return _context.AppointmentReasons.Any(e => e.ID == id);
        }
    }
}
