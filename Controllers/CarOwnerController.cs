using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RentACar01.Data;
using System.Linq;
using RentACar01.ViewModels;
using Microsoft.EntityFrameworkCore;
using RentACar01.Models;
using System.Diagnostics;

namespace RentACar01.Controllers
{
    public class CarOwnerController : Controller
    {
        private readonly AppDbContext _context;

        public CarOwnerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Profile_CarOwner(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }
            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] =user.UserName;
            return View(user);
        }
        public IActionResult EditProfile_CarOwner(string userName, bool isUpdated = false)
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
        public IActionResult EditProfile_CarOwner(EditProfile_ViewModel model)
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
                return RedirectToAction("EditProfile_CarOwner", new { userName = user.UserName, isUpdated = true });
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult CarRegistration_CarOwner(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
                return NotFound();
            }
            var model = new CarRegistrationHouseViewModel
            {
                CarOwnerName = user.UserName
            };
            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult>CarRegistration_CarOwner(CarRegistrationHouseViewModel model)
        {
            ViewData["UserType"] = model.UserType;
            ViewData["UserName"] = model.CarOwnerName;

            Console.WriteLine("UserType: " + model.UserType);
            Console.WriteLine("HouseName: " + model.HouseName);
            Console.WriteLine("Ac: " + model.Ac);


            var house = _context.Houses.FirstOrDefault(h => h.Division == model.Division &&
                    h.District == model.District &&
                    h.Area == model.Area &&
                    h.HouseName == model.HouseName);
            Console.WriteLine("House Name: " + house.HouseName);

            if (house == null)
            {
                Console.WriteLine("House Name Not found: " + house.HouseName);
                return NotFound();
            }
            if (ModelState.IsValid)
            {

                if (await _context.Cars.AnyAsync(c => c.CarRegNum == model.CarRegNum))
                {
                    ModelState.AddModelError("", " Car Already Registered.");
                    return View(model);
                }


                Car newCar = new Car
                {
                    CarRegNum = model.CarRegNum,
                    CarOwnerName = model.CarOwnerName,
                    HouseID = house.House_Id,
                    CarType = model.CarType,
                    Color = model.Color,
                    Brand = model.Brand,
                    NumOfSeat = model.NumOfSeat,
                    Ac = model.Ac,
                    Availability = "Available",
                    RentPerKilo = model.RentPerKilo,
                    Status = "Pending",


                    DriverName = model.DriverName,
                    DriverAddress = model.DriverAddress,
                    DriverNID = model.DriverNID,
                    DriverPhoneNum = model.DriverPhoneNum,
                    LicenseNo = model.LicenseNo,
                    DriverImage = "abc",


                    //Image = model.Image,
                    CreatedAt = DateTime.UtcNow,

                };


                _context.Cars.Add(newCar);
                await _context.SaveChangesAsync();
                Console.WriteLine("UserName: " + model.CarOwnerName);

                TempData["IsUpdated"] = true;
                return RedirectToAction("CarRegistration_CarOwner", new { userName = model.CarOwnerName });
            }
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                //Console.WriteLine(error.ErrorMessage);
                Debug.WriteLine("Validation Error: " + error.ErrorMessage);
            }
            return View(model);

        }
        /*public IActionResult CarRegistration_CarOwner(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }
            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;
            return View(user);
        }*/
        public async Task<IActionResult> ViewCars_CarOwner(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }

            var cars = await _context.Cars
                                     .Where(c => c.CarOwnerName == userName)
                                     .ToListAsync();

            var carRegNums = cars.Select(c => c.CarRegNum).ToList();

            var upcomingBookings = await _context.BookedCars
                                     .Where(b => carRegNums.Contains(b.CarRegNum) && b.StartDate >= DateTime.UtcNow)
                                     .OrderBy(b => b.StartDate)
                                     .ToListAsync();

            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;

            var viewModel = new ViewCar_CarOwnerViewModel
            {
                Cars = cars,
                BookedCars = upcomingBookings
            };

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult UpdateCarInfo_CarOwner(string userName, string CarRegNum, string Availability, int RentPerKilo)
        {

            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }

            var car = _context.Cars.FirstOrDefault(c => c.CarRegNum == CarRegNum);
            if (car != null)
            {
                car.Availability = Availability;
                car.RentPerKilo = RentPerKilo;
                _context.SaveChanges();
            }
            return RedirectToAction("ViewCars_CarOwner", new { userName = user.UserName });
        }


        public async Task<IActionResult> History_CarOwner(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }

            var cars = await _context.Cars
                                     .Where(c => c.CarOwnerName == userName)
                                     .ToListAsync();

            var carRegNums = cars.Select(c => c.CarRegNum).ToList();

            var previousBookings = await _context.BookedCars
                                     .Where(b => carRegNums.Contains(b.CarRegNum) && b.StartDate < DateTime.UtcNow)
                                      .OrderByDescending(b => b.StartDate)
                                     .ToListAsync();

            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;

            /*var viewModel = new ViewCar_CarOwnerViewModel
            {
                Cars = cars,
                BookedCars = upcomingBookings
            };*/

            return View(previousBookings);
        }


    }
}

