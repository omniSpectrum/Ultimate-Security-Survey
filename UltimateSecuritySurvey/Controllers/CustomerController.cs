﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UltimateSecuritySurvey.Models;

namespace UltimateSecuritySurvey.Controllers
{  
    /// <summary>
    /// This controller to Display Customers and to manage them, CRUD
    /// </summary>
    [Authorize]
    public class CustomerController : Controller
    {
        private SecuritySurveyEntities db = new SecuritySurveyEntities();

        /// <summary>
        /// This method to list the customers on the index page
        /// </summary>
        [Authorize(Roles="Teacher, Student")]
        public ActionResult Index()
        {
            ViewBag.Error = TempData["Message"];
            return View(db.Customers.ToList());
        }

        /// <summary>
        /// This method to find a customer with id from the database
        /// </summary>
        /// <param name="id">Primary id of Customer</param>
        [Authorize(Roles = "Teacher, Student")]
        public ActionResult Details(int id = 0)
        {
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            //Additional Info for sidebar
            var surveys = customer.CustomerSurveys;

            int implementedSurveysCount = surveys.Count;
            ViewBag.ImplementedSurveysCount = implementedSurveysCount;

            double averageBaselevel = (implementedSurveysCount > 0) ?
                                            (from item in surveys
                                            select item.GenericSurvey.baseLevel)
                                            .Average() :
                                            0;

            ViewBag.AvgBaseLevel = Math.Round(averageBaselevel, 2);
            
            /* Factor here derrives from ObserverStatusValue Range for surveys implemented for company. 
             * Minima = -3, Maxima = +3 
             * --> Four answers with value +3 should give 100 %
             * --> Four answers with value -3 should give 0 %
             * 
             * --> -12/4 * factor = 0 % --> factor = 0;
             * --> 12/4 * factor = 100 % --> factor = 4/12 = 1/3
             * Not good..... To have normal calculations minima and Maxima both should be Natural numbers
             * 
             * additionValue = 3 --> Minima = 0, Maxima = 6
             * --> 24/4 * factor = 100 % --> factor = 100 * 4/24 = 100/6 = 50/3;
             * percentage = AVG*50/3; */
            int additionValue = 3;

            /*Here is two factors in order to control arithmetic operations order, 
             * cause if 50 will be divided by 3 first, then we will never get a clean 100%
             * 
             * Example: 6*50/3 = 6*16,66... =  99,99...
             * But having two factors Example: 6*50/3 = 300/3 = 100 */
            int factor1 = 50;
            int factor2 = 3;

            double? avgObserverStatus = (from item in surveys
                                         from answer in item.CustomerAnswers
                                         select answer.observerStatusValue + additionValue)
                                         .Average();

            double percentage = (avgObserverStatus ?? 0) * factor1 / factor2;
            int companySecurity = (int)Math.Round(percentage, 0);

            ViewBag.BarColor = (companySecurity < 50) ? "alert" : "success";
            ViewBag.CompanySecurity = String.Format("{0}%", companySecurity);

            return View(customer);
        }

        /// <summary>
        /// This method to go to the create view
        /// </summary>
        [Authorize(Roles = "Teacher")]
        public ActionResult Create()
        {
            return View("CreateEdit", new Customer());
        }

        /// <summary>
        /// This method to go to edit view, gets also the customer details from db
        /// </summary>
        /// <param name="Customer">Customer class object</param>
        [Authorize(Roles = "Teacher")]
        public ActionResult Edit(int id = 0)
        {
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            return View("CreateEdit",customer);
        }

        /// <summary>
        /// This method to edit the customer and save changes
        /// </summary>
        /// <param name="Customer">Customer class object from textboxes</param>
        [Authorize(Roles = "Teacher")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEdit(Customer customer)
        {
            bool uniqueViolation = db.Customers.Any(x => x.email == customer.email
                                   && x.customerId != customer.customerId);

            if (ModelState.IsValid && !uniqueViolation)
            {
                //No Id => Add
                if (customer.customerId <= 0)
                {
                    db.Customers.Add(customer);
                }
                //Is Id => Update
                else
                {
                    db.Entry(customer).State = EntityState.Modified;
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Warning = "Email must be unique!";
            return View(customer);
        }

        /// <summary>
        /// This method to open delete view and show the customer details to confirm delete
        /// </summary>
        /// <param name="Customer">Primary id of Customer</param>
        [Authorize(Roles = "Teacher")]
        public ActionResult Delete(int id = 0)
        {
            Customer customer = db.Customers.Find(id);
            if (customer == null)
                return HttpNotFound();

            if (Request.IsAjaxRequest())
                return PartialView(customer);

            return View(customer);
        }

        /// <summary>
        /// This method to delete the customer from database
        /// and also check if customersurveys exist for the customer,
        /// if yes, then customer can't be deleted
        /// </summary>
        /// <param name="Customer">Primary id of Customer</param>
        [Authorize(Roles = "Teacher")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);
            bool childExist = db.CustomerSurveys.Any(x => x.customerId == customer.customerId);

            if (!childExist)
            {
                db.Customers.Remove(customer);
                db.SaveChanges();
            }
            else
            {
                TempData["Message"] = String.Format("Cannot delete '{0}' because related Customer Surveys exist!!",
                                                    customer.companyName);
            }

            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}