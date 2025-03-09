using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RentACar01.Data;
using System.Linq;
using RentACar01.ViewModels;
using Microsoft.EntityFrameworkCore;
using RentACar01.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics;
using RentACar01.PaymentGateway;
using System.Collections.Specialized;

namespace RentACar01.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Profile_Customer(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }
            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;
            return View(user);
        }
        public IActionResult EditProfile_Customer(string userName, bool isUpdated = false)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EditProfile_ViewModel
            {
                UserName = user.UserName,
                FullName = user.FullName,
                DOB = user.DOB,
                Gender = user.Gender,
                NID = user.NID,
                Email = user.Email,
                PhoneNum = user.PhoneNum,
                IsUpdated = isUpdated
            };
            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult EditProfile_Customer(EditProfile_ViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.UserName == model.UserName);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = model.FullName;
                user.DOB = model.DOB;
                user.Gender = model.Gender;
                user.NID = model.NID;
                user.Email = model.Email;
                user.PhoneNum = model.PhoneNum;

                _context.SaveChanges();

                // Redirect with update success flag
                return RedirectToAction("EditProfile_Customer", new { userName = user.UserName, isUpdated = true });
            }

            return View(model);
        }

        
        public IActionResult BookCar_Customer(string userName)
        {

            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
                return NotFound();
            }
            var allCars = _context.Cars.ToList();

            foreach (var car in allCars)
            {
                if (car.Availability == "Unavailable")
                {
                    car.Availability = "Available";
                };
            }
            _context.SaveChanges();

            
            var model = new BookCarCustomerViewModel
            {
                CustomerName = user.UserName
            };
            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> BookCar_Customer(BookCarCustomerViewModel model)
        {
            ViewData["UserType"] = model.UserType;
            ViewData["UserName"] = model.CustomerName;
            Debug.WriteLine("HouseName: " + model.HouseName);
            Debug.WriteLine("Ac: " + model.CustomerName);
        
           
            if (ModelState.IsValid)
            {
                
                var house = await _context.Houses.FirstOrDefaultAsync(h => 
                   h.HouseName == model.HouseName &&
                   h.Division == model.Division &&
                   h.District == model.District &&
                   h.Area == model.Area);

                if (house == null)
                {
                    ModelState.AddModelError("HouseName", "Selected house does not exist.");
                    return View(model);
                }

                var requirement = new CustomerRequirement
                {
                    CustomerName = model.CustomerName,
                    HouseID = house.House_Id, 
                    StartAdress = model.StartAddress,
                    EndAdress = model.EndAddress,
                    Distance=model.Distance,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CarType = model.CarType,
                    NumOfSeat = model.NumOfSeat,
                    Brand = model.Brand,
                    Color = model.Color,
                    AC = model.AC,
                    BookingStatus = "Pending" 
                };

                _context.CustomerRequirements.Add(requirement);
                await _context.SaveChangesAsync();

                //TempData["SuccessMessage"] = "Your car booking request has been submitted!";
                return RedirectToAction("BookCar_Customer2", new { userName = model.CustomerName, requirementId = requirement.RequirementID });
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                //Console.WriteLine(error.ErrorMessage);
                Debug.WriteLine("Validation Error: " + error.ErrorMessage);
            }
            return View(model);
        }

        public IActionResult BookCar_Customer2(string userName , int requirementId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
                return NotFound();
            }

            var requirement = _context.CustomerRequirements
                .FirstOrDefault(r => r.RequirementID == requirementId);

            if (requirement == null)
            {
                return NotFound();
            }

            DateTime startDate = requirement.StartDate;
            DateTime endDate = requirement.EndDate;


            var bookedCarIds = _context.BookedCars
                .Where(bc => (startDate >= bc.StartDate && startDate <= bc.EndDate) || (endDate >= bc.StartDate && endDate <= bc.EndDate) ||(startDate<=bc.StartDate && endDate>=bc.EndDate)) 
                .Select(bc => bc.CarRegNum) 
                .ToList();

            var bookedCars = _context.Cars.Where(c => bookedCarIds.Contains(c.CarRegNum) && c.Availability=="Available").ToList();

            foreach (var car in bookedCars)
            {
                car.Availability = "Unavailable";
            }

            var allCars = _context.Cars.ToList();

            foreach (var car in allCars)
            {
                if (car.Availability == "Available")
                {
               
                    if (car.HouseID != requirement.HouseID)
                        car.Availability = "Unavailable";

                    if (!string.IsNullOrEmpty(requirement.Color) && car.Color != requirement.Color)
                        car.Availability = "Unavailable";

                    if (!string.IsNullOrEmpty(requirement.Brand) && car.Brand != requirement.Brand)
                        car.Availability = "Unavailable";

                    if (requirement.NumOfSeat > 0 && car.NumOfSeat != requirement.NumOfSeat)
                        car.Availability = "Unavailable";

                    if (!string.IsNullOrEmpty(requirement.AC) && car.Ac != requirement.AC)
                        car.Availability = "Unavailable";

                    if (!string.IsNullOrEmpty(requirement.CarType) && car.CarType != requirement.CarType)
                        car.Availability = "Unavailable";
                };
            }

           
           _context.SaveChanges();

            var availableCars = _context.Cars.Where(c =>c.Availability=="Available").ToList();

            var model = new BookCarCustomerViewModel
            {
                CustomerName = user.UserName
            };


            ViewData["Distance"] = requirement.Distance;
            ViewData["requirementId"] = requirementId;
            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;
            return View(availableCars);
        }

        public async Task<IActionResult> AllBookings_Customer(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }

            var previousBookings = await _context.BookedCars
                                     .Where(b => b.CustomerName==userName && b.StartDate < DateTime.UtcNow)
                                     .OrderByDescending(b => b.StartDate)
                                     .ToListAsync();
            var upcomingBookings = await _context.BookedCars
                                     .Where(b => b.CustomerName == userName && b.StartDate >= DateTime.UtcNow)
                                     .OrderBy(b => b.StartDate)
                                     .ToListAsync();

            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;

            var viewModel = new AllBookings_CustomerViewModel
            {
                UpcomingBookings = upcomingBookings,
                PreviousBookings = previousBookings
            };

            return View(viewModel);
           
        }

        public IActionResult Checkout(int requirementId , int carId)
        {
            var requirement = _context.CustomerRequirements.FirstOrDefault(r => r.RequirementID == requirementId);
            var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);

            var CarReg = car.CarRegNum;
            var CustomerName = requirement.CustomerName;
            var amount = car.RentPerKilo*requirement.Distance;

            string tranId = Guid.NewGuid().ToString("N");
            
            var baseUrl = Request.Scheme + "://" + Request.Host;

            // CREATING LIST OF POST DATA
            NameValueCollection PostData = new NameValueCollection();

            PostData.Add("total_amount", $"{amount}");
            PostData.Add("tran_id", tranId);
            //PostData.Add("success_url", baseUrl + "/Customer/CheckoutConfirmation");
            PostData.Add("success_url", $"{baseUrl}/Customer/CheckoutConfirmation?requirementId={requirementId}&carId={carId}");
            PostData.Add("fail_url", baseUrl + "/Customer/CheckoutFail");
            PostData.Add("cancel_url", baseUrl + "/Customer/CheckoutCancel");

            PostData.Add("version", "3.00");
            PostData.Add("cus_name", $"{CustomerName}");
            PostData.Add("cus_email", "abc.xyz@mail.co");
            PostData.Add("cus_add1", "Address Line On");
            PostData.Add("cus_add2", "Address Line Tw");
            PostData.Add("cus_city", "City Nam");
            PostData.Add("cus_state", "State Nam");
            PostData.Add("cus_postcode", "Post Cod");
            PostData.Add("cus_country", "Countr");
            PostData.Add("cus_phone", "0111111111");
            PostData.Add("cus_fax", "0171111111");
            PostData.Add("ship_name", "ABC XY");
            PostData.Add("ship_add1", "Address Line On");
            PostData.Add("ship_add2", "Address Line Tw");
            PostData.Add("ship_city", "City Nam");
            PostData.Add("ship_state", "State Nam");
            PostData.Add("ship_postcode", "Post Cod");
            PostData.Add("ship_country", "Countr");
            PostData.Add("value_a", "ref00");
            PostData.Add("value_b", "ref00");
            PostData.Add("value_c", "ref00");
            PostData.Add("value_d", "ref00");
            PostData.Add("shipping_method", "NO");
            PostData.Add("num_of_item", "1");
            PostData.Add("product_name", $"{CarReg}");
            PostData.Add("product_profile", "general");
            PostData.Add("product_category", "Demo");

            //we can get from email notificaton
            var storeId = "rentc67c2958cd95c5";
            var storePassword = "rentc67c2958cd95c5@ssl";
            var isSandboxMood = true;

            SSLCommerzGatewayProcessor sslcz = new SSLCommerzGatewayProcessor(storeId, storePassword, isSandboxMood);

            string response = sslcz.InitiateTransaction(PostData);

            return Redirect(response);
        }

        public async Task<IActionResult> CheckoutConfirmation(int requirementId, int carId)
        {
            var requirement = _context.CustomerRequirements.FirstOrDefault(r => r.RequirementID == requirementId);
            var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);


            if (!(!String.IsNullOrEmpty(Request.Form["status"]) && Request.Form["status"] == "VALID"))
            {
                ViewBag.SuccessInfo = "There some error while processing your payment. Please try again.";
                return View();
            }

            string TrxID = Request.Form["tran_id"];

            var amount = (car.RentPerKilo * requirement.Distance).ToString();

            string currency = "BDT";

            var storeId = "rentc67c2958cd95c5";
            var storePassword = "rentc67c2958cd95c5@ssl";

            SSLCommerzGatewayProcessor sslcz = new SSLCommerzGatewayProcessor(storeId, storePassword, true);
            var resonse = sslcz.OrderValidate(TrxID, amount, currency, Request);
            var successInfo = $"Validation Response: {resonse}";

            if (await _context.BookedCars.AnyAsync(b => b.TransactionId == TrxID))
            {
                ModelState.AddModelError("", "Transaction Already entered.");
                ViewBag.SuccessInfo = successInfo;

                return View();
            }
            else
            {
               // var requirement = _context.CustomerRequirements.FirstOrDefault(r => r.RequirementID == requirementId);
                //var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);

                var newBookedCar = new BookedCar
                {
                    RequirementID = requirement.RequirementID,
                    TransactionId = TrxID,
                    CustomerName = requirement.CustomerName,
                    HouseID = car.HouseID, // Assuming your Car model has HouseId
                    CarRegNum = car.CarRegNum,
                    TotalRent = int.Parse(amount),
                    // or get dynamic if needed
                    Status = "Booked",
                    StartAdress = requirement.StartAdress,
                    EndAdress = requirement.EndAdress,
                    StartDate = requirement.StartDate,
                    EndDate = requirement.EndDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BookedCars.Add(newBookedCar);
                _context.SaveChanges();


                ViewBag.SuccessInfo = successInfo;
                ViewData["TrxID"] = TrxID;
                ViewData["UserName"] = requirement.CustomerName;
                return View();
            }
            
        }
        public IActionResult CheckoutFail()
        {
            ViewBag.FailInfo = "There some error while processing your payment. Please try again.";
            return View();
        }
        public IActionResult CheckoutCancel()
        {
            ViewBag.CancelInfo = "Your payment has been cancel";
            return View();
        }

    }
}


