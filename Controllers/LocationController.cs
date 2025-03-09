using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentACar01.Data;
using System.Collections.Generic;

namespace LocationAPI.Controllers
{
    [Route("api/location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        // Division to District mapping
        private static readonly Dictionary<string, List<string>> Districts = new()
        {
            { "Dhaka", new List<string> { "Dhaka", "Gazipur", "Narayanganj", "Tangail", "Manikganj", "Rajbari" } },
            { "Chattogram", new List<string> { "Chattogram", "Cox's Bazar", "Comilla", "Feni", "Noakhali", "Chandpur" } },
            { "Rajshahi", new List<string> { "Rajshahi", "Bogura", "Natore", "Pabna", "Naogaon" } },
            { "Khulna", new List<string> { "Khulna", "Jessore", "Satkhira", "Bagerhat", "Jhenaidah" } },
            { "Barishal", new List<string> { "Barishal", "Patuakhali", "Bhola", "Jhalokathi" } },
            { "Sylhet", new List<string> { "Sylhet", "Moulvibazar", "Habiganj", "Sunamganj" } },
            { "Rangpur", new List<string> { "Rangpur", "Dinajpur",  "Kurigram", "Gaibandha" } },
            { "Mymensingh", new List<string> { "Mymensingh", "Jamalpur", "Sherpur" } }
        };



        // District to Area mapping
        private static readonly Dictionary<string, List<string>> Areas = new()
        {
             // Dhaka Division
            { "Dhaka", new List<string> { "Dhanmondi", "Mirpur", "Uttara", "Gulshan", "Banani", "Mohammadpur", "Badda", "Motijheel", "Paltan", "Tejgaon", "Shahbagh", "Lalbagh", "Ramna", "Khilgaon" } },
            { "Gazipur", new List<string> { "Tongi", "Kaliakoir", "Sreepur", "Kapasia", "Konabari", "Bason", "Chandona", "Rajendrapur", "Joydebpur" } },
            { "Narayanganj", new List<string> { "Fatullah", "Sonargaon", "Rupganj", "Siddhirganj", "Bandar", "Kanchpur", "Shitalakshya", "Narayanganj Sadar" } },
            { "Tangail", new List<string> { "Tangail Sadar", "Mirzapur", "Gopalpur", "Ghatail", "Nagarpur", "Basail", "Sakhipur", "Delduar", "Kalihati", "Madhupur", "Dhanbari", "Bhuapur" } },
            { "Manikganj", new List<string> { "Manikganj Sadar", "Shibalaya", "Singair", "Saturia", "Harirampur", "Daulatpur", "Ghior" } },
            { "Rajbari", new List<string> { "Rajbari Sadar", "Goalanda", "Pangsha", "Kalukhali", "Baliakandi" } },

            // Chattogram Division
            { "Chattogram", new List<string> { "Agrabad", "Halishahar", "Pahartali", "Patenga", "Kotwali", "Chawkbazar", "Bakolia", "Panchlaish", "Lalkhan Bazar", "Bayezid", "Karnafuli", "Anwara", "Hathazari", "Raozan", "Boalkhali" } },
            { "Cox's Bazar", new List<string> { "Cox's Bazar Sadar", "Chakaria", "Ramu", "Teknaf", "Ukhia", "Pekua", "Maheshkhali", "Kutubdia" } },
            { "Comilla", new List<string> { "Comilla Sadar", "Debidwar", "Daudkandi", "Muradnagar", "Meghna", "Laksam", "Homna", "Brahmanpara", "Monohorgonj", "Titas", "Chandina", "Barura" } },
            { "Feni", new List<string> { "Feni Sadar", "Daganbhuiyan", "Parshuram", "Fulgazi", "Chhagalnaiya", "Sonagazi" } },
            { "Noakhali", new List<string> { "Noakhali Sadar", "Begumganj", "Companiganj", "Senbagh", "Hatiya", "Subarnachar", "Chatkhil" } },
            { "Chandpur", new List<string> { "Chandpur Sadar", "Matlab Uttar", "Matlab Dakkhin", "Hajiganj", "Shahrasti", "Faridganj", "Kachua", "Haimchar" } },

            // Rajshahi Division
            { "Rajshahi", new List<string> { "Rajshahi Sadar", "Boalia", "Rajpara", "Motihar", "Shah Makhdum", "Paba", "Godagari", "Durgapur", "Charghat", "Bagmara", "Tanore" } },
            { "Bogura", new List<string> { "Bogura Sadar", "Sherpur", "Shibganj", "Gabtali", "Nandigram", "Sonatala", "Dhupchanchia", "Adamdighi", "Sariakandi", "Kahaloo", "Shajahanpur" } },
            { "Natore", new List<string> { "Natore Sadar", "Lalpur", "Singra", "Baraigram", "Bagatipara", "Gurudaspur" } },
            { "Pabna", new List<string> { "Pabna Sadar", "Ishwardi", "Bera", "Bhangura", "Chatmohar", "Sujanagar", "Santhia", "Faridpur" } },
            { "Sirajganj", new List<string> { "Sirajganj Sadar", "Kazipur", "Tarash", "Belkuchi", "Shahjadpur", "Chauhali", "Raiganj", "Ullapara", "Kamarkhanda" } },

            // Khulna Division
            { "Khulna", new List<string> { "Khulna Sadar", "Sonadanga", "Khalishpur", "Daulatpur", "Boyra", "Dumuria", "Batiaghata", "Paikgacha", "Dighalia", "Terokhada", "Fultala", "Koyra" } },
            { "Jessore", new List<string> { "Jessore Sadar", "Bagherpara", "Sharsha", "Jhikargacha", "Monirampur", "Chaugachha", "Abhaynagar", "Keshabpur" } },
            { "Satkhira", new List<string> { "Satkhira Sadar", "Tala", "Kaliganj", "Shyamnagar", "Debhata", "Assasuni" } },
            { "Bagerhat", new List<string> { "Bagerhat Sadar", "Mongla", "Rampal", "Fakirhat", "Mollahat", "Chitalmari", "Kachua", "Morrelganj", "Sharankhola" } },
            { "Jhenaidah", new List<string> { "Jhenaidah Sadar", "Maheshpur", "Shailkupa", "Harinakunda", "Kaliganj", "Kotchandpur" } },

            // Barishal Division
            { "Barishal", new List<string> { "Barishal Sadar", "Bakerganj", "Babuganj", "Banaripara", "Gournadi", "Agailjhara", "Muladi", "Mehendiganj", "Hizla" } },
            { "Patuakhali", new List<string> { "Patuakhali Sadar", "Dumki", "Galachipa", "Dashmina", "Bauphal", "Kalapara", "Rangabali", "Mirzaganj" } },
            { "Bhola", new List<string> { "Bhola Sadar", "Char Fasson", "Lalmohan", "Manpura", "Tazumuddin", "Borhanuddin", "Daulatkhan" } },
            { "Jhalokathi", new List<string> { "Jhalokathi Sadar", "Kathalia", "Nalchity", "Rajapur" } },

            // Sylhet Division
            { "Sylhet", new List<string> { "Sylhet Sadar", "Golapganj", "Beanibazar", "Jaintiapur", "Zakiganj", "Companiganj", "Kanaighat", "Fenchuganj", "Bishwanath", "Dakshin Surma" } },
            { "Moulvibazar", new List<string> { "Moulvibazar Sadar", "Sreemangal", "Kamalganj", "Rajnagar", "Kulaura", "Juri", "Barlekha" } },
            { "Habiganj", new List<string> { "Habiganj Sadar", "Madhabpur", "Baniachang", "Lakhai", "Nabiganj", "Chunarughat", "Azmiriganj" } },
            { "Sunamganj", new List<string> { "Sunamganj Sadar", "Tahirpur", "Jamalganj", "Shalla", "Dharmapasha", "Derai", "Jagannathpur", "Sullah", "Chhatak", "Bishwambarpur", "Doarabazar" } },

            // Rangpur Division
            { "Rangpur", new List<string> { "Rangpur Sadar", "Badarganj", "Mithapukur", "Pirganj", "Gangachara", "Kaunia", "Taraganj" } },
            { "Dinajpur", new List<string> { "Dinajpur Sadar", "Birampur", "Birganj", "Ghoraghat", "Hakimpur", "Kaharole", "Khansama", "Nawabganj", "Parbatipur" } },
            { "Kurigram", new List<string> { "Kurigram Sadar", "Nageshwari", "Bhurungamari", "Phulbari", "Rajarhat", "Ulipur", "Chilmari", "Char Rajibpur", "Roumari" } },
            { "Gaibandha", new List<string> { "Gaibandha Sadar", "Palashbari", "Gobindaganj", "Sadullapur", "Sundarganj", "Saghata", "Phulchhari" } },

            // Mymensingh Division
            { "Mymensingh", new List<string> { "Mymensingh Sadar", "Muktagachha", "Dhobaura", "Fulbaria", "Gaffargaon", "Gouripur", "Haluaghat", "Ishwarganj", "Nandail", "Phulpur", "Trishal" } },
            { "Jamalpur", new List<string> { "Jamalpur Sadar", "Bakshiganj", "Dewanganj", "Islampur", "Madarganj", "Melandaha", "Sarishabari" } },
            { "Sherpur", new List<string> { "Sherpur Sadar", "Nalitabari", "Jhenaigati", "Nakla", "Sreebardi" } }


        };

        // Endpoint: Get districts by division
        [HttpGet("districts/{division}")]
        public IActionResult GetDistricts(string division)
        {
            if (Districts.ContainsKey(division))
            {
                return Ok(Districts[division]);
            }
            return NotFound("No districts found for the selected division.");
        }

        // Endpoint: Get areas by district
        [HttpGet("areas/{district}")]
        public IActionResult GetAreas(string district)
        {
            if (Areas.ContainsKey(district))
            {
                return Ok(Areas[district]);
            }
            return NotFound("No areas found for the selected district.");
        }



        private readonly AppDbContext _context;

        public LocationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("houses/{division}/{district}/{area}")]
        public IActionResult GetHouses(string division, string district, string area)
        {
            var houses = _context.Houses
                .Where(h => h.Division == division && h.District == district && h.Area == area)
                .Select(h => new { h.House_Id, h.HouseName })
                .ToList();

            if (!houses.Any()) return NotFound(new { message = "No houses found" });

            return Ok(houses);
        }
    }
}






/*using Microsoft.AspNetCore.Mvc;

[Route("api/location")]
[ApiController]
public class LocationController : ControllerBase
{
    private static readonly Dictionary<string, List<string>> Districts = new()
    {
        { "Dhaka", new List<string> { "Dhaka", "Gazipur", "Narayanganj", "Tangail", "Manikganj", "Munshiganj", "Madaripur", "Shariatpur", "Kishoreganj", "Narsingdi", "Rajbari" } },
        { "Chattogram", new List<string> { "Chattogram", "Cox's Bazar", "Comilla", "Feni", "Noakhali", "Chandpur", "Lakshmipur", "Brahmanbaria", "Khagrachari", "Rangamati", "Bandarban" } },
        { "Rajshahi", new List<string> { "Rajshahi", "Bogura", "Natore", "Pabna", "Sirajganj", "Naogaon", "Kushtia", "Meherpur", "Chapai Nawabganj" } },
        { "Khulna", new List<string> { "Khulna", "Jessore", "Satkhira", "Bagerhat", "Jhenaidah", "Chuadanga", "Meherpur" } },
        { "Barishal", new List<string> { "Barishal", "Patuakhali", "Bhola", "Jhalokathi", "Pirojpur", "Kushumchira" } },
        { "Sylhet", new List<string> { "Sylhet", "Moulvibazar", "Habiganj", "Sunamganj" } },
        { "Rangpur", new List<string> { "Rangpur", "Dinajpur", "Thakurgaon", "Kurigram", "Gaibandha", "Panchagarh" } },
        { "Mymensingh", new List<string> { "Mymensingh", "Jamalpur", "Sherpur", "Netrokona" } },

    };

    private static readonly Dictionary<string, List<string>> Areas = new()
    {
        { "Dhaka", new List<string> { "Dhanmondi", "Mirpur", "Uttara", "Gulshan", "Banani", "Mohammadpur", "Badda", "Motijheel", "Paltan", "Tejgaon", "Shahbagh", "Lalbagh", "Ramna", "Khilgaon" } },
        { "Gazipur", new List<string> { "Tongi", "Kaliakoir", "Sreepur", "Kapasia", "Konabari", "Bason", "Chandona", "Rajendrapur", "Joydebpur" } },
        { "Narayanganj", new List<string> { "Fatullah", "Sonargaon", "Rupganj", "Siddhirganj", "Bandar", "Kanchpur", "Shitalakshya", "Narayanganj Sadar" } },
        { "Tangail", new List<string> { "Tangail Sadar", "Mirzapur", "Gopalpur", "Ghatail", "Nagarpur", "Basail", "Sakhipur", "Delduar", "Kalihati", "Madhupur", "Dhanbari", "Bhuapur" } },
        { "Manikganj", new List<string> { "Manikganj Sadar", "Shibalaya", "Singair", "Saturia", "Harirampur", "Daulatpur", "Ghior" } },
        { "Munshiganj", new List<string> { "Munshiganj Sadar", "Gazaria", "Sreenagar", "Sirajdikhan", "Lohajang", "Tongibari" } },
        { "Madaripur", new List<string> { "Madaripur Sadar", "Rajoir", "Kalkini", "Shibchar" } },
        { "Shariatpur", new List<string> { "Shariatpur Sadar", "Naria", "Zanjira", "Gosairhat", "Bhedarganj", "Damudya" } },
        { "Kishoreganj", new List<string> { "Kishoreganj Sadar", "Bajitpur", "Kuliarchar", "Hossainpur", "Pakundia", "Karimganj", "Katiadi", "Austagram", "Mithamain", "Itna", "Tarail", "Nikli" } },
        { "Narsingdi", new List<string> { "Narsingdi Sadar", "Palash", "Monohardi", "Belabo", "Raipura", "Shibpur" } },
        { "Rajbari", new List<string> { "Rajbari Sadar", "Goalanda", "Pangsha", "Kalukhali", "Baliakandi" } },
        { "Chattogram", new List<string> { "Agrabad", "Halishahar", "Pahartali", "Patenga", "Kotwali", "Chawkbazar", "Bakolia", "Panchlaish", "Lalkhan Bazar", "Bayezid", "Karnafuli", "Anwara", "Hathazari", "Raozan", "Boalkhali" } },

        { "Cox's Bazar", new List<string> { "Cox's Bazar Sadar", "Chakaria", "Ramu", "Teknaf", "Ukhia", "Pekua", "Maheshkhali", "Kutubdia" } },

        { "Comilla", new List<string> { "Comilla Sadar", "Debidwar", "Daudkandi", "Muradnagar", "Meghna", "Laksam", "Homna", "Brahmanpara", "Monohorgonj", "Titas", "Chandina", "Barura" } },

        { "Feni", new List<string> { "Feni Sadar", "Daganbhuiyan", "Parshuram", "Fulgazi", "Chhagalnaiya", "Sonagazi" } },

        { "Noakhali", new List<string> { "Noakhali Sadar", "Begumganj", "Companiganj", "Senbagh", "Hatiya", "Subarnachar", "Chatkhil" } },

        { "Chandpur", new List<string> { "Chandpur Sadar", "Matlab Uttar", "Matlab Dakkhin", "Hajiganj", "Shahrasti", "Faridganj", "Kachua", "Haimchar" } },

        { "Lakshmipur", new List<string> { "Lakshmipur Sadar", "Ramganj", "Raipur", "Ramgati", "Kamalnagar" } },

        { "Brahmanbaria", new List<string> { "Brahmanbaria Sadar", "Nabinagar", "Kasba", "Ashuganj", "Sarail", "Bijoynagar", "Bancharampur", "Akhaura" } },

        { "Khagrachari", new List<string> { "Khagrachari Sadar", "Mahalchhari", "Dighinala", "Lakshmichhari", "Panchhari", "Manikchhari", "Ramgarh", "Matiranga" } },

        { "Rangamati", new List<string> { "Rangamati Sadar", "Kaptai", "Baghaichhari", "Barkal", "Rajasthali", "Juraichhari", "Langadu", "Naniarchar", "Bilaichhari" } },

        { "Bandarban", new List<string> { "Bandarban Sadar", "Thanchi", "Ruma", "Rowangchhari", "Lama", "Alikadam", "Naikhongchhari" } },
        { "Rajshahi", new List<string> { "Rajshahi Sadar", "Boalia", "Rajpara", "Motihar", "Shah Makhdum", "Paba", "Godagari", "Durgapur", "Charghat", "Bagmara", "Tanore" } },

        { "Bogura", new List<string> { "Bogura Sadar", "Sherpur", "Shibganj", "Gabtali", "Nandigram", "Sonatala", "Dhupchanchia", "Adamdighi", "Sariakandi", "Kahaloo", "Shajahanpur" } },

        { "Natore", new List<string> { "Natore Sadar", "Lalpur", "Singra", "Baraigram", "Bagatipara", "Gurudaspur" } },

        { "Pabna", new List<string> { "Pabna Sadar", "Ishwardi", "Bera", "Bhangura", "Chatmohar", "Sujanagar", "Santhia", "Faridpur" } },

        { "Sirajganj", new List<string> { "Sirajganj Sadar", "Kazipur", "Tarash", "Belkuchi", "Shahjadpur", "Chauhali", "Raiganj", "Ullapara", "Kamarkhanda" } },

        { "Naogaon", new List<string> { "Naogaon Sadar", "Atrai", "Raninagar", "Badalgachhi", "Manda", "Dhamoirhat", "Mohadevpur", "Porsha", "Sapahar", "Patnitala", "Niamatpur" } },

        { "Kushtia", new List<string> { "Kushtia Sadar", "Kumarkhali", "Khoksa", "Mirpur", "Daulatpur", "Bheramara" } },

        { "Meherpur", new List<string> { "Meherpur Sadar", "Gangni", "Mujibnagar" } },

        { "Chapai Nawabganj", new List<string> { "Chapai Nawabganj Sadar", "Shibganj", "Gomastapur", "Nachole", "Bholahat" } },
         { "Khulna", new List<string> { "Khulna Sadar", "Sonadanga", "Khalishpur", "Daulatpur", "Boyra", "Dumuria", "Batiaghata", "Paikgacha", "Dighalia", "Terokhada", "Fultala", "Koyra" } },

        { "Jessore", new List<string> { "Jessore Sadar", "Bagherpara", "Sharsha", "Jhikargacha", "Monirampur", "Chaugachha", "Abhaynagar", "Keshabpur" } },

        { "Satkhira", new List<string> { "Satkhira Sadar", "Tala", "Kaliganj", "Shyamnagar", "Debhata", "Assasuni" } },

       { "Bagerhat", new List<string> { "Bagerhat Sadar", "Mongla", "Rampal", "Fakirhat", "Mollahat", "Chitalmari", "Kachua", "Morrelganj", "Sharankhola" } },

        { "Jhenaidah", new List<string> { "Jhenaidah Sadar", "Maheshpur", "Shailkupa", "Harinakunda", "Kaliganj", "Kotchandpur" } },

        { "Chuadanga", new List<string> { "Chuadanga Sadar", "Damurhuda", "Jibannagar", "Alamdanga" } },

        { "Meherpur", new List<string> { "Meherpur Sadar", "Gangni", "Mujibnagar" } },
        { "Barishal", new List<string> { "Barishal Sadar", "Bakerganj", "Babuganj", "Banaripara", "Gournadi", "Agailjhara", "Muladi", "Mehendiganj", "Hizla" } },

        { "Patuakhali", new List<string> { "Patuakhali Sadar", "Dumki", "Galachipa", "Dashmina", "Bauphal", "Kalapara", "Rangabali", "Mirzaganj" } },

        { "Bhola", new List<string> { "Bhola Sadar", "Char Fasson", "Lalmohan", "Manpura", "Tazumuddin", "Borhanuddin", "Daulatkhan" } },

        { "Jhalokathi", new List<string> { "Jhalokathi Sadar", "Kathalia", "Nalchity", "Rajapur" } },

        { "Pirojpur", new List<string> { "Pirojpur Sadar", "Bhandaria", "Mathbaria", "Nazirpur", "Kawkhali", "Nesarabad" } },

        { "Kushumchira", new List<string> { "Kushumchira Sadar", "Borhanuddin", "Lalmohan" } },
        { "Sylhet", new List<string> { "Sylhet Sadar", "Golapganj", "Beanibazar", "Jaintiapur", "Zakiganj", "Companiganj", "Kanaighat", "Fenchuganj", "Bishwanath", "Dakshin Surma" } },

        { "Moulvibazar", new List<string> { "Moulvibazar Sadar", "Sreemangal", "Kamalganj", "Rajnagar", "Kulaura", "Juri", "Barlekha" } },

        { "Habiganj", new List<string> { "Habiganj Sadar", "Madhabpur", "Baniachang", "Lakhai", "Nabiganj", "Chunarughat", "Azmiriganj" } },

        { "Sunamganj", new List<string> { "Sunamganj Sadar", "Tahirpur", "Jamalganj", "Shalla", "Dharmapasha", "Derai", "Jagannathpur", "Sullah", "Chhatak", "Bishwambarpur", "Doarabazar" } },

        { "Rangpur", new List<string> { "Rangpur Sadar", "Badarganj", "Mithapukur", "Pirganj", "Gangachara", "Kaunia", "Taraganj" } },

        { "Dinajpur", new List<string> { "Dinajpur Sadar", "Birampur", "Birganj", "Ghoraghat", "Hakimpur", "Kaharole", "Khansama", "Nawabganj", "Parbatipur" } },

        { "Thakurgaon", new List<string> { "Thakurgaon Sadar", "Baliadangi", "Haripur", "Pirganj", "Ranisankail" } },

        { "Kurigram", new List<string> { "Kurigram Sadar", "Nageshwari", "Bhurungamari", "Phulbari", "Rajarhat", "Ulipur", "Chilmari", "Char Rajibpur", "Roumari" } },

        { "Gaibandha", new List<string> { "Gaibandha Sadar", "Palashbari", "Gobindaganj", "Sadullapur", "Sundarganj", "Saghata", "Phulchhari" } },

        { "Panchagarh", new List<string> { "Panchagarh Sadar", "Tetulia", "Boda", "Atwari", "Debiganj" } },
        { "Mymensingh", new List<string> { "Mymensingh Sadar", "Muktagachha", "Dhobaura", "Fulbaria", "Gaffargaon", "Gouripur", "Haluaghat", "Ishwarganj", "Nandail", "Phulpur", "Trishal" } },

        { "Jamalpur", new List<string> { "Jamalpur Sadar", "Bakshiganj", "Dewanganj", "Islampur", "Madarganj", "Melandaha", "Sarishabari" } },

        { "Sherpur", new List<string> { "Sherpur Sadar", "Nalitabari", "Jhenaigati", "Nakla", "Sreebardi" } },

        { "Netrokona", new List<string> { "Netrokona Sadar", "Atpara", "Barhatta", "Durgapur", "Khaliajuri", "Kalmakanda", "Kendua", "Madan", "Mohanganj", "Purbadhala" } },
    };

    // 1️⃣ Division অনুযায়ী District রিটার্ন করবে
    [HttpGet("districts/{division}")]
    public IActionResult GetDistricts(string division)
    {
        if (Districts.ContainsKey(division))
        {
            return Ok(Districts[division]);
        }
        return NotFound("No districts found for the selected division.");
    }

    // 2️⃣ District অনুযায়ী Area রিটার্ন করবে
    [HttpGet("areas/{district}")]
    public IActionResult GetAreas(string district)
    {
        if (Areas.ContainsKey(district))
        {
            return Ok(Areas[district]);
        }
        return NotFound("No areas found for the selected district.");
    }
}*/
