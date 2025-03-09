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

namespace RentACar01.Controllers
{
    
    public class HouseOwnerController : Controller
    {
        private readonly AppDbContext _context;

        public HouseOwnerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Profile_HouseOwner(string userName)
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
        public IActionResult EditProfile_HouseOwner(string userName, bool isUpdated = false)
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
        public IActionResult EditProfile_HouseOwner(EditProfile_ViewModel model)
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
                return RedirectToAction("EditProfile_HouseOwner", new { userName = user.UserName, isUpdated = true });
            }

            return View(model);
        }
        public IActionResult HouseRegistration_HouseOwner(string userName, bool isUpdated = false)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }

            var model = new HouseRegistrationViewModel
            {
                 HouseOwnerName = user.UserName,
            };
            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;
            return View(model);

        }

        [HttpPost]
     
        public async Task<IActionResult> HouseRegistration_HouseOwner(HouseRegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email or username already exists
                if (await _context.Houses.AnyAsync(u => 
                   u.HouseOwnerName == model.HouseOwnerName &&
                   u.HouseName == model.HouseName &&
                   u.Division == model.Division &&
                   u.District == model.District &&
                   u.Area == model.Area))
                {
                    ModelState.AddModelError("", " House Already Registered.");
                    return View(model);
                }

               
                House newHouse = new House
                {
                    HouseOwnerName = model.HouseOwnerName,
                    HouseName = model.HouseName,
                    HousePhoneNum = model.HousePhoneNum,
                    Division = model.Division,
                    District = model.District,
                    Area = model.Area,
                    //Image = model.Image,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Pending"
                };


                _context.Houses.Add(newHouse);
                await _context.SaveChangesAsync();

                TempData["IsUpdated"] = true;
                return RedirectToAction("HouseRegistration_HouseOwner", new { userName = model.HouseOwnerName, isUpdated = true });
            }
            return View(model);

        }
        

        public IActionResult BookCar_HouseOwner(string userName)
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


        [HttpGet]
        public IActionResult CarRegistration_HouseOwner(string userName)
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
         public async Task<IActionResult> CarRegistration_HouseOwner(CarRegistrationHouseViewModel model)
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

            if (house== null)
            {
                Console.WriteLine("House Name Not found: " + house.HouseName);
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                
                if (await _context.Cars.AnyAsync(c => c.CarRegNum == model.CarRegNum ))
                {
                    ModelState.AddModelError("", " Car Already Registered.");
                    return View(model);
                }


                 Car newCar = new Car
                 {
                     CarRegNum = model.CarRegNum,
                     CarOwnerName = model.CarOwnerName,
                     HouseID= house.House_Id,
                     CarType = model.CarType,
                     Color = model.Color,
                     Brand = model.Brand,
                     NumOfSeat = model.NumOfSeat,
                     Ac= model.Ac,
                     Availability = "Available",
                     RentPerKilo= model.RentPerKilo,
                     Status = "Pending",
                   

                     DriverName = model.DriverName,
                     DriverAddress = model.DriverAddress,
                     DriverNID = model.DriverNID,
                     DriverPhoneNum = model.DriverPhoneNum,
                     LicenseNo = model.LicenseNo,
                     DriverImage="abc",
                     

                     //Image = model.Image,
                     CreatedAt = DateTime.UtcNow,
                     
                 }; 


                 _context.Cars.Add(newCar);
                 await _context.SaveChangesAsync();
                 Console.WriteLine("UserName: " + model.CarOwnerName);

                TempData["IsUpdated"] = true;
                return RedirectToAction("CarRegistration_HouseOwner", new { userName = model.CarOwnerName });
            }
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                //Console.WriteLine(error.ErrorMessage);
                Debug.WriteLine("Validation Error: " + error.ErrorMessage);
            }
            return View(model);
            
        }
        public async Task<IActionResult> SearchCars_HouseOwner(string userName, string searchInfo)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }

            var houseIds = await _context.Houses
                .Where(h => h.HouseOwnerName == userName)
                .Select(h => h.House_Id)
                .ToListAsync();

            if (houseIds == null || !houseIds.Any())
            {
                return NotFound("No house found for this owner.");
            }

            List<Car> cars = new List<Car>();

            if (!string.IsNullOrEmpty(searchInfo))
            {
                cars = await _context.Cars
                    .Where(c => houseIds.Contains(c.HouseID) &&
                                (c.CarRegNum.Contains(searchInfo) ||
                                 c.CarOwnerName.Contains(searchInfo) ||
                                 c.CarType.Contains(searchInfo) ||
                                 c.Color.Contains(searchInfo) ||
                                  c.Brand.Contains(searchInfo) ||
                                   c.CarType.Contains(searchInfo) ||
                                 c.DriverName.Contains(searchInfo)))
                    .ToListAsync();
            }
            else
            {
                // If no search input, show all cars of this owner
                cars = await _context.Cars
                    .Where(c => houseIds.Contains(c.HouseID))
                    .ToListAsync();
            }

            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;

            // Return the same view as ViewCars_HouseOwner.cshtml
            return View("ViewCars_HouseOwner", cars);
        }

        public async Task<IActionResult> ViewCars_HouseOwner(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }
            var houseIds = await _context.Houses
                             .Where(h => h.HouseOwnerName == userName)
                             .Select(h => h.House_Id)
                             .ToListAsync();

            if (houseIds == null || !houseIds.Any())
            {
                return NotFound("No house found for this owner.");
            }

           
            var cars = await _context.Cars
                                     .Where(c => houseIds.Contains(c.HouseID))
                                     .ToListAsync();

            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;

            return View(cars);
        }
     
        public IActionResult ViewDetails_HouseOwner(string regNum , string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }
            var car = _context.Cars.FirstOrDefault(c => c.CarRegNum == regNum);
            if (car == null)
            {
                return NotFound();
            }
            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;
            return View(car);
        }
        public async Task<IActionResult> ViewHouse_HouseOwner(string userName)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);

            if (user == null)
            {
                return NotFound();
            }
            var house = await _context.Houses.Where(h => h.HouseOwnerName== userName).ToListAsync();
            if (house == null)
            {
                return NotFound();
            }
            ViewData["UserType"] = user.UserType;
            ViewData["UserName"] = user.UserName;
            return View(house);
        }


    }



}



