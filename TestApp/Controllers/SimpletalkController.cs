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
public class SimpletalkController : ControllerBase
{
    private readonly ILogger<SimpletalkController> _logger;
    private readonly IMemoryCache _memoryCache;

    public SimpletalkController(ILogger<SimpletalkController> logger, DbContext dbContext, IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
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

