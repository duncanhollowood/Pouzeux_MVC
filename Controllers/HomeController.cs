using System.IO;
using Contracts;
using EmailService;
using Entities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Pouzeux_MVC.Controllers
{
    public class HomeController : Controller
    {
        private IRepositoryWrapper _repowrapper;
        private IEmailSender _emailsender;
        private IConfiguration _config;
        private ILoggerManager _logmanager;
        private IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _contextAccessor;


        public HomeController(IRepositoryWrapper repoWrapper, IEmailSender emailsender, ILoggerManager logmanager, IWebHostEnvironment env, IConfiguration config)
        {
            _repowrapper = repoWrapper;
            _emailsender = emailsender;
            _logmanager = logmanager;
            _config = config;
            _env = env;
        }

        
        public MyImages GetImages(string imagepath)
        {
            var folderPath = System.IO.Path.Combine(_env.WebRootPath, "images\\" + imagepath);

            var model = new MyImages()
            {
                Images = Directory.EnumerateFiles(folderPath, "*.j*").Select(filename => "~/images/" + imagepath +"/" + Path.GetFileName(filename))
            };


            return model;
            
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        public void SetUserRole(string Role)
        {
            HttpContext.Session.SetString("UserRole", Role);
        }
        public string GetUserRole()
        {
            return (HttpContext.Session.GetString("UserRole"));
        }

        [HttpGet]
        public IActionResult GetRegisteredUser()
        {
            return View("GetRegisteredUser");
        }
        [HttpPost]
        public IActionResult GetRegisteredUser(RegisteredUser r)
        {
            if (ModelState.IsValid)
            {
                ViewData["Message"] = "New Registered User";
                _repowrapper.RegisteredUser.Create(r);
                _repowrapper.Save();
                return View("NewUserAdded", r);
            }
            else
            {
                // error!
                return View();
            }

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Website Details";

            return View();
        }
        public IActionResult RegisteredUsers()
        {
            ViewData["Message"] = "Registered Users";
            return View();

            // return Pouzeux_POC.Pages.IndexModel();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }
        public IActionResult TheArea()
        {
            ViewData["Message"] = "Photos - The Area";

            return View();
        }
        public IActionResult Photos()
        {
            ViewData["Message"] = "Photos";

            return View("Photos", GetImages("Summer 2019"));
        }
        public IActionResult Downstairs()
        {
            ViewData["Message"] = "Photos - Downstairs";

            return View("Photos", GetImages("Summer 2019\\Downstairs"));
        }
        public IActionResult Upstairs()
        {
            ViewData["Message"] = "Photos - Upstairs";

            return View("Photos", GetImages("Summer 2019\\Upstairs"));
        }
        public IActionResult All()
        {
            ViewData["Message"] = "Photos - All";

            return View("Photos", GetImages("Summer 2019"));
        }
        public IActionResult Outside()
        {
            ViewData["Message"] = "Photos - Outside";

            return View("Photos", GetImages("Summer 2019\\Outside"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Enquiry(Enquiry e)
        {
            e.DateEnquiryMade = System.DateTime.Now;

            string htmlBody = "<html><body>";

            htmlBody += "<b>";
            htmlBody += e.EnquiryText;
            htmlBody += "</b>";
            htmlBody += " my enquiry";
            htmlBody += "</body></html>";

            _emailsender.SendEmailAsync(e.EmailAddress, "Enquiry", htmlBody);
                        
            return View("EnquirySent", e);
        }


        [HttpGet]
        public IActionResult Enquiry()
        {
            ViewData["Message"] = "Enquiry";

            return View();
        }

        public IActionResult Accommodation()
        {
            ViewData["Message"] = "Accommodation";

            return View();
        }
        public IActionResult Facilities()
        {
            ViewData["Message"] = "Facilities";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Pricing()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
