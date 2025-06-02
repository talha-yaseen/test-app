using Microsoft.AspNetCore.Mvc;
using TestApp.Entities;
using TestApp.Data;
using TestApp.DTOs;
using MongoDB.Driver;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace TestApp.Controllers;

[ApiController]
[Route("[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ILogger<ReviewsController> _logger;
    private readonly DbContext _context;
    private readonly IMemoryCache _memoryCache;

    public ReviewsController(ILogger<ReviewsController> logger, DbContext dbContext, IMemoryCache memoryCache)
    {
        _logger = logger;
        _context = dbContext;
        _memoryCache = memoryCache;
    }

    [HttpGet("GetMovies")]
    public async Task<ActionResult> GetMovies()
    {
        List<Movie> movies;
        if (!_memoryCache.TryGetValue("Movies", out movies))
        {
            movies = _context.Movies.Find(FilterDefinition<Movie>.Empty).Limit(100).ToList();
            _memoryCache.Set("Movies", movies, TimeSpan.FromMinutes(5));
        }
        
        return Ok(movies);
    }

    [HttpGet("Test")]
    public async Task<ActionResult> Test()
    {
        List<int> questions = new List<int>() { 1, 2, 3 };
        List<Answer> answers = new List<Answer>();
        answers.Add(new Answer { Id = 1, QuestionId = 3, QuestionText = "Test Q3", Value = "Answer to 3" });
        answers.Add(new Answer { Id = 2, QuestionId = 1, QuestionText = "Test Q1", Value = "Answer to 1" });
        //var joined = questions.Join(answers, q => q, a => a.QuestionId, (q, a) => a.Value);
        var joined =
            from questionId in questions
            join a in answers
            on questionId equals a.QuestionId into temp
            from a in temp.DefaultIfEmpty()
            select a?.Value ?? "";
        return Ok(joined);
    }

    [HttpGet("TestApiCall")]
    public async Task<ActionResult> TestApiCall()
    {
        var handler = new HttpClientHandler();
        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.textgrid.com/2010-04-01/Accounts/k3IdEjalZGVfMfyP0ndSGg==/Messages.json");
        request.Content = JsonContent.Create(new
        {
            Body = "Is this Simpletalk?"
        });
        var response = await client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        return new JsonResult(new
        {
            Code = response.StatusCode,
            Content = responseContent
        });
    }

    [HttpGet("TestCSVParser")]
    public async Task<ActionResult> TestCSVParser()
    {
        var csvFileLines = await System.IO.File.ReadAllLinesAsync("/Users/talhayaseen/Downloads/csv-parser/list.csv");
        StringBuilder updatedList = new StringBuilder();
        for (int i = 0; i < csvFileLines.Count(); i++)
        {
            if (i == 0)
            {
                updatedList.Append(csvFileLines[i]);
            }
            else
            {
                string preprocessedLine = csvFileLines[i];
                int firstQuoteIndex = preprocessedLine.IndexOf("\"");
                int secondQuoteIndex = preprocessedLine.Substring(firstQuoteIndex + 1).IndexOf("\"");
                secondQuoteIndex += firstQuoteIndex + 1;
                preprocessedLine = preprocessedLine.Substring(secondQuoteIndex + 1);
                string[] columns = preprocessedLine.Split(",");
                int colsCount = columns.Length;
                string rawNumber = columns[colsCount - 1];
                string updatedNumber = "+1" + rawNumber;
                columns[colsCount - 1] = updatedNumber;
                string updatedRow = string.Join(",", columns);
                updatedList.Append(updatedRow + "\n");
            }
        }
        await System.IO.File.WriteAllTextAsync("/Users/talhayaseen/Downloads/csv-parser/updated-list.csv", updatedList.ToString());
        return Ok("Done");
    }

    [HttpPost("GetAvailableSlots")]
    public ActionResult GetAvailableSlots(BusySlotsInput busySlots)
    {
        List<BusySlot> parsedBusySlots = busySlots.Busy.Select(s => new BusySlot()
        {
            Start = DateTime.Parse(s.Start),
            End = DateTime.Parse(s.End)
        }).ToList();
        Dictionary<string, TimeSlotsForDate> calendarData = new();
        string timezoneString = "-07:00";
        for (int i = 0; i <= 6; i++)
        {
            DateTime chosenDay = DateTime.Now.AddDays(i);
            string formattedDate = chosenDay.ToString("yyyy-MM-dd");
            List<BusySlot> busySlotsForDate = parsedBusySlots.Where(s => s.Start.ToString("yyyy-MM-dd") == chosenDay.ToString("yyyy-MM-dd")).ToList();
            List<double> bookedSlots = GetBookedSlotsForDate(busySlotsForDate);
            List<double> availableSlots = new();
            for (double slot = 9; slot < 17; slot += 0.5)
            {
                if (!bookedSlots.Contains(slot))
                    availableSlots.Add(slot);
            }
            List<string> formattedAvailableSlots = new();
            foreach (double availableSlot in availableSlots)
            {
                if ((availableSlot * 10) % 10 == 0)
                {
                    string formattedSlot = $"{availableSlot}:00:00{timezoneString}";
                    if (availableSlot < 10) formattedSlot = $"0{formattedSlot}";
                    formattedAvailableSlots.Add($"{formattedDate}T{formattedSlot}");
                }
                else
                {
                    int availableSlotHour = (int)availableSlot;
                    string formattedSlot = $"{availableSlotHour}:30:00{timezoneString}";
                    if (availableSlotHour < 10) formattedSlot = $"0{formattedSlot}";
                    formattedAvailableSlots.Add($"{formattedDate}T{formattedSlot}");
                }
            }
            calendarData.Add(formattedDate, new TimeSlotsForDate
            {
                slots = formattedAvailableSlots
            });
        }
        return new JsonResult(new { calendar_data = calendarData }) ;
    }

    private List<double> GetBookedSlotsForDate(List<BusySlot> busySlotsForDate)
    {
        List<double> bookedSlots = new();
        foreach (BusySlot slot in busySlotsForDate)
        {
            double bookedPeriod = slot.End.Subtract(slot.Start).TotalHours;
            double bookedSlotStart;
            if (slot.Start.Minute == 0)
                bookedSlotStart = slot.Start.Hour;
            else
                bookedSlotStart = slot.Start.Hour + 0.5;
            double bookedSlotEnd = bookedSlotStart + bookedPeriod;
            for (double bookedSlot = bookedSlotStart; bookedSlot < bookedSlotEnd; bookedSlot += 0.5)
            {
                bookedSlots.Add(bookedSlot);
            }
        }
        return bookedSlots;
    }
}

