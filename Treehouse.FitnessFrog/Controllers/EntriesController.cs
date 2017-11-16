using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Treehouse.FitnessFrog.Data;
using Treehouse.FitnessFrog.Models;

namespace Treehouse.FitnessFrog.Controllers
{
  public class EntriesController : Controller
  {
    private EntriesRepository _entriesRepository = null;

    public EntriesController()
    {
        _entriesRepository = new EntriesRepository();
    }

    public ActionResult Index()
    {
      List<Entry> entries = _entriesRepository.GetEntries();

      // Calculate the total activity.
      double totalActivity = entries
          .Where(e => e.Exclude == false)
          .Sum(e => e.Duration);

      // Determine the number of days that have entries.
      int numberOfActiveDays = entries
          .Select(e => e.Date)
          .Distinct()
          .Count();

      ViewBag.TotalActivity = totalActivity;
      ViewBag.AverageDailyActivity = (totalActivity / (double)numberOfActiveDays);

      return View(entries);
    }

    public ActionResult Add() //GET version
    {
      var entry = new Entry() {
      Date = DateTime.Today,
      };

      ViewBag.ActivitiesSelectListItems = new SelectList(Data.Data.Activities, "Id", "Name");

      return View(entry);
    }

    [HttpPost]
    public ActionResult Add(Entry entry) //POST version
    {
      ValidateEntry(entry);

      if (ModelState.IsValid) //If the entry is valid, add it to the repo, redirec tto home 'index' page
      {
        _entriesRepository.AddEntry(entry);

        return RedirectToAction("Index");
      }

      SetupActivitiesSelectListItems();

      return View(entry);
    }

    public ActionResult Edit(int? id)
    {
      if (id == null)
      {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }

      //get the requested entry form the repository
      Entry entry = _entriesRepository.GetEntry((int)id);

      //return a status of not found if the entry wasnt found
      if (entry == null) 
      {
        return HttpNotFound();
      }

      //populate the activities select list items 
      SetupActivitiesSelectListItems();

      return View(entry);
    }

    [HttpPost]
    public ActionResult Edit(Entry entry) 
    {
      //validate the entry
      ValidateEntry(entry);

      //if the entry is valid...
      //1.use repo to update the entry
      //2. redirect the user to the entries list page
      if (ModelState.IsValid) 
      {
        _entriesRepository.UpdateEntry(entry);
        return RedirectToAction("Index");
      }


      //populate the activities select list items ViewBag property
      SetupActivitiesSelectListItems();

      return View(entry);
    }

    public ActionResult Delete(int? id)
    {
      if (id == null)
      {
          return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
      }

      //rereive entry for the provided id parameter value
      Entry entry = _entriesRepository.GetEntry((int)id);

      if (entry == null) 
      {
        return HttpNotFound();
      }
      //Pass the entry to the view
      return View(entry);
    }

    [HttpPost]
    public ActionResult Delete(int id) 
    {
      //delete the entry
      _entriesRepository.DeleteEntry(id);
      return RedirectToAction("Index");
    }

    private void ValidateEntry(Entry entry) 
    {
      //If there arn't any Duration field validation errors, then make sure that duration is > 0
      if (ModelState.IsValidField("Duration") && entry.Duration <= 0) {
        ModelState.AddModelError("Duration", "The Duration field value must be greater than '0'.");
      }
    }

    private void SetupActivitiesSelectListItems() {
      ViewBag.ActivitiesSelectListItems = new SelectList(Data.Data.Activities, "Id", "Name");
    }
  }
}