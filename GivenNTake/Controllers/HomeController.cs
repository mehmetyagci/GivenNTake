﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GiveNTake.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            //return Content("Hello from MVC!");
            return View();
        }
    }
}