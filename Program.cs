using System.Collections;
using System.Text;
using System.Text.Json;
using HitRefresh.Schedule;
using Ical.Net;
using HitRefresh.HitGeneralServices.Jwts;
using HitRefresh.HitGeneralServices.WeChatServiceHall;
using DevTrends.ConfigurationExtensions;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Mvc;

IEnumerable<(int, JwtsSemester, Semester, bool, SubscriptionEntry)> GetTasks(SubscriptionEntry[] entries)
{
    var year = DateTime.Today.Year;
    var autumn = DateTime.Today.Month is > 7 or < 2;
    bool withWeek = true;
    foreach (var entry in entries)
    {
        if (autumn)
        {
            yield return (year, JwtsSemester.Autumn, Semester.Autumn, withWeek, entry);
        }
        else
        {
            yield return (year, JwtsSemester.Spring, Semester.Spring, withWeek, entry);
            yield return (year, JwtsSemester.Summer, Semester.Summer, withWeek, entry);

        }
        withWeek = false;
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Configuration.Bind<Config>("ScheduleServer");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/{subscriptionName}",
        async ([FromRoute] string subscriptionName, [FromQuery] string secret) =>
        {

            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"));
            var sub = config.Subscriptions.FirstOrDefault(s => s.Name == subscriptionName);
            if (sub is null) return Results.NotFound();
            if (sub.Secret != secret) return Results.Forbid();
            var calendars = await Task.WhenAll(
                GetTasks(sub.Entries).Select(async (args) =>
                {
                    var (year, jwtsSem, sem, withWeek, entry) = args;
                    var schedule = await WeChatServices.GetScheduleAnonymousAsync((uint)year,
                        jwtsSem,
                        entry.StudentId);
                    var convert = ScheduleEntity.FromWeb(year, sem, schedule);
                    convert.EnableNotification = entry.Notification >= 0;
                    convert.NotificationTime = entry.Notification;
                    convert.DisableWeekIndex = !(withWeek && sub.WeekIndex);
                    var calendar = convert.ToCalendar(entry.Prefix);
                    return calendar;
                }));
            var sum = calendars[0];
            for (var i = 1; i < calendars.Length; i++)
                sum.MergeWith(calendars[i]);
            await using var calStream = new MemoryStream();
            new CalendarSerializer().Serialize(sum, calStream, Encoding.UTF8);
            var calData=calStream.ToArray();
            return Results.File(calData, "text/calendar", $"{subscriptionName}.ics");
        })
.WithName("Get");

app.Run();

public record Config(Subscription[] Subscriptions);
public record Subscription(string Name, bool WeekIndex, SubscriptionEntry[] Entries, string Secret);

public record SubscriptionEntry(int Notification, string Prefix, string StudentId);

