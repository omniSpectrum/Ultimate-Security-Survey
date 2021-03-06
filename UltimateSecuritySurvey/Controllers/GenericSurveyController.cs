﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UltimateSecuritySurvey.Models;

namespace UltimateSecuritySurvey.Controllers
{
    /// <summary>
    /// Page to manage surveys
    /// </summary>
    [Authorize]
    [Authorize(Roles = "Teacher")]
    public class GenericSurveyController : Controller
    {
        private SecuritySurveyEntities db = new SecuritySurveyEntities();

        //
        // GET: /GenericSurvey/
        /// <summary>
        /// List of surveys
        /// </summary>
        /// <returns>Index view</returns>
        public ActionResult Index()
        {
            ViewBag.Error = TempData["Message"];
            var genericsurveys = db.GenericSurveys.Include(g => g.UserAccount);
            return View(genericsurveys.ToList());
        }

        //
        // GET: /GenericSurvey/Details/5
        /// <summary>
        /// Details view
        /// </summary>
        /// <param name="id">survey id</param>
        /// <returns>Details view</returns>
        public ActionResult Details(int id = 0)
        {
            GenericSurvey genericsurvey = db.GenericSurveys.Find(id);
            if (genericsurvey == null)
            {
                return HttpNotFound();
            }
            return View(genericsurvey);
        }

        //
        // GET: /GenericSurvey/Create
        /// <summary>
        /// Create view
        /// </summary>
        /// <returns>CreateEdit</returns>
        public ActionResult Create()
        {
            var genericsurvey = new GenericSurvey();
            PopulateAssignedData(genericsurvey);

            var teachers = db.UserAccounts.Where(x => x.isTeacher).ToList();
            ViewBag.supervisorUserId = new SelectList(teachers, "userId", "userName");
            return View("CreateEdit", genericsurvey);
        }

        //
        // GET: /GenericSurvey/Edit/5
        /// <summary>
        /// Edit view
        /// </summary>
        /// <param name="id">generic survey id</param>
        /// <returns>CreateEdit</returns>
        public ActionResult Edit(int id = 0)
        {
            GenericSurvey genericsurvey = db.GenericSurveys.Include(i => i.Questions).Single(i => i.surveyId == id);
            if (genericsurvey == null)
            {
                return HttpNotFound();
            }
            PopulateAssignedData(genericsurvey);

            var teachers = db.UserAccounts.Where(x => x.isTeacher).ToList();
            ViewBag.supervisorUserId = new SelectList(teachers, "userId", "userName", genericsurvey.supervisorUserId);
            return View("CreateEdit", genericsurvey);
        }

        private void PopulateAssignedData(GenericSurvey survey)
        {
            var allquestions = db.Questions;
            var surveyQuestions = new HashSet<int>(survey.Questions.Select(i => i.questionId));
            var viewModel = allquestions.Select(question => new GenericSurveyQuestion
            {
                Assigned = surveyQuestions.Contains(question.questionId),
                QuestionId = question.questionId,
                QuestionText = question.questionTextMain
            }).ToList();

            ViewBag.questions = viewModel;
        }

        //
        // POST: /GenericSurvey/Edit/5
        /// <summary>
        /// To add or update entry in db
        /// </summary>
        /// <param name="genericsurvey">object</param>
        /// <returns>Index view</returns>
        [HttpPost]
        public ActionResult CreateEdit(GenericSurvey genericsurvey, string[] selectedQuestions)
        {
            if (ModelState.IsValid)
            {
                //If No Id => Add
                if (genericsurvey.surveyId <= 0)
                {
                    UpdateSurveyQuestion(selectedQuestions, genericsurvey);
                    db.GenericSurveys.Add(genericsurvey);
                }
                //If IS Id => Update
                else
                {
                    var surveyToUpdate = db.GenericSurveys
                        .Include(i => i.Questions).Single(i => i.surveyId == genericsurvey.surveyId);

                    UpdateSurveyQuestion(selectedQuestions, surveyToUpdate);

                    db.Entry(surveyToUpdate).CurrentValues.SetValues(genericsurvey);

                    PopulateAssignedData(surveyToUpdate);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var teachers = db.UserAccounts.Where(x => x.isTeacher).ToList();
            ViewBag.supervisorUserId = new SelectList(teachers, "userId", "userName", genericsurvey.supervisorUserId);
            
            return View(genericsurvey);
        }

        public void UpdateSurveyQuestion(string[] selectedQuestions, GenericSurvey surveyToUpdate)
        {
            if (selectedQuestions == null)
            {
                surveyToUpdate.Questions = new List<Question>();
                return;
            }

            var selectedQuestionsHS = new HashSet<string>(selectedQuestions);
            var surveyQuestions = new HashSet<int>(surveyToUpdate.Questions.Select(i => i.questionId));
            foreach (var question in db.Questions)
            {
                if (selectedQuestionsHS.Contains(question.questionId.ToString()))
                {
                    if (!surveyQuestions.Contains(question.questionId))
                    {
                        surveyToUpdate.Questions.Add(question);
                    }
                }
                else
                {
                    if (surveyQuestions.Contains(question.questionId))
                    {
                        surveyToUpdate.Questions.Remove(question);
                    }
                }
            }
        }

        //
        // GET: /GenericSurvey/Delete/5
        /// <summary>
        /// Delete view
        /// </summary>
        /// <param name="id">survey id</param>
        /// <returns></returns>
        public ActionResult Delete(int id = 0)
        {
            GenericSurvey genericsurvey = db.GenericSurveys.Find(id);
            if (genericsurvey == null)
                return HttpNotFound();

            if (Request.IsAjaxRequest())
                return PartialView(genericsurvey);

            return View(genericsurvey);
        }

        //
        // POST: /GenericSurvey/Delete/5
        /// <summary>
        /// Post delete to delete entry
        /// </summary>
        /// <param name="id">survey id</param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            GenericSurvey genericsurvey = db.GenericSurveys
                .Include(i => i.Questions).Single(i => i.surveyId == id);

            bool childExist = db.CustomerSurveys.Any(x => x.surveyId == id);

            if (!childExist)
            {
                genericsurvey.Questions = null;
                db.GenericSurveys.Remove(genericsurvey);
                db.SaveChanges();
            }
            else
            {
                TempData["Message"] = String.Format("Cannot delete '{0}' because this survey was used as a Customer survey",
                                                        genericsurvey.title);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Method to reveal resources
        /// </summary>
        /// <param name="disposing">bool</param>
        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}