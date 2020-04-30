using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Entities;
using EmailService;
using Contracts;

namespace Pouzeux_MVC.Controllers
{
    public class BookingsController : Controller
    {
        private readonly RepositoryContext _context;
        private IEmailSender _emailsender;
        private IRepositoryWrapper _repowrapper;
        private ILoggerManager _logmanager;


        public BookingsController(RepositoryContext context, IEmailSender emailsender, IRepositoryWrapper repoWrapper, ILoggerManager logmanager)
        {
            _context = context;
            _emailsender = emailsender;
            _repowrapper = repoWrapper;
            _logmanager = logmanager;
        }

        [HttpGet]
        public IActionResult Booking()
        {
            ViewData["Message"] = "Booking";

            return View();
        }

        [HttpGet]
        public IActionResult Home()
        {
            return View("/Views/Home/Index.cshtml");
        }

        [HttpPost]
        public IActionResult Booking(Booking b)
        {
            if (ModelState.IsValid)
            {
                ViewData["Message"] = "Booking";
                _repowrapper.Booking.Create(b);
                _repowrapper.Save();

                _logmanager.LogInfo("BRDH Booking done");


                _emailsender.SendEmailAsync(b.Email, "Booking Received", BookingMail("Received", b));


                //using (var smtpClient = HttpContext.RequestServices.GetRequiredService<SmtpClient>())
                //{
                //    await smtpClient.SendMailAsync(new MailMessage(
                //           to: "sample1.app@noname.test",
                //           subject: "Test message subject",
                //           body: "Test message body"
                //           ));

                //    return Ok("OK");
                //}

                ViewData["Message"] = "Request Received";

                return View("/Views/Bookings/BookingThanks.cshtml", b);

            }
            else
            {
                // error!
                return View();
            }

        }





        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            return View(await _context.Bookings.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FromDate,ToDate,NoPeople,Name,Email,Phone,Confirmed, DepositPaid, DepositPaidAmount, InvoiceTotal, FullyPaid, BookingStatus, DepositDueDate, FullyPaidDueDate")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                booking.BookingRequestDate = DateTime.Now;
                booking.BookingLastAmendedDate = DateTime.Now;
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FromDate,ToDate,NoPeople,Name,Email,Phone, Confirmed, DepositPaid, DepositPaidAmount, InvoiceTotal, FullyPaid, BookingStatus, DepositDueDate, FullyPaidDueDate")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    booking.BookingLastAmendedDate = DateTime.Now;
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
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
            return View(booking);
        }


        // CONFIRMED BOOKING
        public async Task<IActionResult> Confirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Confirmed = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();

            await _emailsender.SendEmailAsync(booking.Email, "Booking Confirmed", BookingMail("Confirmed", booking));

            ViewData["Message"] = "BOOKING CONFIRMED!";

            return View("/Views/Bookings/BookingThanks.cshtml", booking);
        }
        // DEPOSIT PAID
        public async Task<IActionResult> DepositPaid(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.DepositPaid = true;
            _context.Update(booking);
            await _context.SaveChangesAsync();

            await _emailsender.SendEmailAsync(booking.Email, "Deposit Paid", BookingMail("Deposit Paid", booking));


            ViewData["Message"] = "DEPOSIT PAID";

            return View("/Views/Bookings/BookingThanks.cshtml", booking);

        }
        // FULLY PAID
        public async Task<IActionResult> FullyPaid(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            
            booking.FullyPaid = true;

            _context.Update(booking);
            await _context.SaveChangesAsync();

            await _emailsender.SendEmailAsync(booking.Email, "Fully Paid", BookingMail("Fully Paid", booking));

            ViewData["Message"] = "FULLY PAID";

            return View("/Views/Bookings/BookingThanks.cshtml", booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }

        public string BookingMail(string stage, Booking b)
        {
            string s="";

            s = "Booking for Pouzeux made by "+ b.Name + " from ";
            s += b.FromDate.ToString("ddd dd/MM/yyyy");
            s += " to ";
            s += b.ToDate.ToString("ddd dd/MM/yyyy");
            s += "<br/>";
            s += "For " + b.NoPeople;
            if (b.NoPeople > 1)
                s += " people";
            else
                s += " person";

            s += "<br/>";

            s += "<b>";
            s += stage;
            s += "</b>";

            s += "<br/>";

            int price = pricecalc(b);
            int depositPC = 20;
            int DaysToBookingStart = ((TimeSpan)(DateTime.Now - b.FromDate)).Days;

            switch (stage)
            {
                case "Received":
                    s += "Thank you for your request for this booking";
                    s += "<br/>";
                    s += "Once we have accepted the booking (you should hear within 48 hours):";
                    s += "<br/>";

                    s += "<br/>";
                    s += "The total cost for this booking will be " + price;
                    s += "<br/>";
                    s += "<br/>";
                    
                    if ((DaysToBookingStart) < 31)
                    {
                        s += "<t>";
                        s += "To confirm your booking the full payment of " + price + " must be paid within 7 days";
                    }
                    else
                    {
                        s += "<t>";
                        s += "To confirm your booking a deposit of " + depositPC * price / 100 + " must be paid within 7 days";
                        s += "<br/>";
                        s += "<t>";
                        s += "And the balance must be paid by Time " + b.ToDate.AddDays(-30);
                    }

                    s += "<br/>";
                    s += "<br/>";

                    s += "No refunds can be made for a booking cancelled within 30 days of the start date";
                    s += "<br/>";
                    s += "A voucher will be sent for bookings cancelled before this time";
                    s += "<br/>";


                    s += "<br/>";
                    // booking details
                    // confirmed total pricing, deposit amount and dates for payments
                    // amendment and/or cancellation detail
                    // next stage Confirmed (24 hours)

                    break;
                case "Confirmed":
                    // confirmed booking 
                    // confirmed pricing, deposit amount and dates 
                    s += "<Large/>";
                    s += "Congratluations - we have now accepted this booking!";
                    s += "<br/>";

                    if ((DaysToBookingStart) < 31)
                    {
                        s += "<t>";
                        s += "To confirm your booking the full payment of " + price + " must be paid within 7 days";
                    }
                    else
                    {
                        s += "<t>";
                        s += "To confirm your booking a deposit of " + depositPC * price / 100 + " must be paid within 7 days";
                        s += "<br/>";
                        s += "<t>";
                        s += "And the balance must be paid by Time " + b.ToDate.AddDays(-30);
                    }

                    s += "<br/>";
                    s += "<br/>";

                    s += "No refunds can be made for a booking cancelled within 30 days of the start date";
                    s += "<br/>";
                    s += "A voucher will be sent for bookings cancelled before this time";
                    s += "<br/>";


                    break;
                case "Deposit Paid":
                    // deposit paid = customer confirm
                    // deposit = 20%, if cancel can be set against future booking
                    
                    break;
                case "Fully Paid":
                    
                    break;
                default:
                    s = "Invalid Stage" + stage;
                    break;
            }
            
            return s;

        }
        
        int pricecalc(Booking b)
        {
            int NoDays = ((TimeSpan)(b.ToDate - b.FromDate)).Days;

            //add oin seasonality 
            int DayRate = 80;
            
            int PerPerson = 30;
            return (NoDays * DayRate) + PerPerson * b.NoPeople;

            //seasonality 
        }

    }
}
