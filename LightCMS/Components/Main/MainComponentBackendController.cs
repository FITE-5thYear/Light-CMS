using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LightCMS.Config;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using LightCMS.Components.Main.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace LightCMS.Components.Main
{
    public class MainComponentBackendController : Controller
    {
        private Settings Settings { get; set; }

        public MainComponentBackendController(IOptions<Settings> settings)
        {
            Settings = settings.Value;
        }
    }
}
