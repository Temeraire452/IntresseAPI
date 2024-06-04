
using System.Text.Json.Serialization;
using System.Text.Json;
using IntresseAPI.Data;
using IntresseAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IntresseAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
            builder.Services.AddDbContext<Data.ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };


            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapPost("/person", async (Person person, ApplicationDbContext context) =>
            {
                if (person == null || string.IsNullOrEmpty(person.PersonName) || string.IsNullOrEmpty(person.Contact))
                {
                    return Results.BadRequest("Ogiltiga personuppgifter");
                }

                context.Persons.Add(person);
                await context.SaveChangesAsync();

                return Results.Created($"/person/{person.PersonId}", person);
            });

            app.MapPost("/interest", async (Interest interest, ApplicationDbContext context) =>
            {
                if (interest == null || string.IsNullOrEmpty(interest.Title) || string.IsNullOrEmpty(interest.Description))
                {
                    return Results.BadRequest("Ogiltiga intresseuppgifter");
                }

                context.Interests.Add(interest);
                await context.SaveChangesAsync();

                return Results.Created($"/interest/{interest.InterestId}", interest);
            });


            app.MapGet("/person", async (ApplicationDbContext context) =>
            {
                var persons = await context.Persons.ToListAsync();
                if(persons == null || !persons.Any())
                {
                    return Results.NotFound("Hittar ingen person");
                }
                return Results.Ok(persons);

            });

            app.MapPost("/person/{personId:int}/interest", async (int personId, Interest interest, ApplicationDbContext context) =>
            {
                var person = await context.Persons.FindAsync(personId);

                if (person == null)
                {
                    return Results.NotFound($"No person with id: {personId} found");
                }

                if (interest == null || string.IsNullOrEmpty(interest.Title))
                {
                    return Results.BadRequest("Invalid interest information");
                }

                var existingInterest = await context.Interests.FirstOrDefaultAsync(i => i.Title == interest.Title);

                if (existingInterest == null)
                {
                    return Results.NotFound($"Interest '{interest.Title}' does not exist");
                }

                person.Interests ??= new List<Interest>();
                person.Interests.Add(existingInterest);

                await context.SaveChangesAsync();

                return Results.Created($"/person/{personId}/interest", existingInterest);
            });

            app.MapPost("/person/{personId:int}/interest/{interestId:int}/link", async (int personId, int interestId, Link link, ApplicationDbContext context) =>
            {
                var person = await context.Persons.FindAsync(personId);
                var interest = await context.Interests.FindAsync(interestId);

                if (person == null || interest == null)
                {
                    return Results.NotFound($"No person with id: {personId} or interest with id: {interestId} found");
                }

                if (link == null || string.IsNullOrEmpty(link.Url))
                {
                    return Results.BadRequest("Invalid link information");
                }

                var newLink = new Link
                {
                    Url = link.Url,
                    PersonId = personId,
                    InterestId = interestId
                };

                person.Links ??= new List<Link>();
                person.Links.Add(newLink);

                interest.Links ??= new List<Link>();
                interest.Links.Add(newLink);

                await context.SaveChangesAsync();

                return Results.Created($"/person/{personId}/interest/{interestId}/link", newLink);
            });

            app.MapGet("/person/{personId:int}/interests", async (int personId, ApplicationDbContext context) =>
            {
                var person = await context.Persons
                    .Include(p => p.Interests)
                    .FirstOrDefaultAsync(p => p.PersonId == personId);

                if (person == null)
                {
                    return Results.NotFound($"No person with id: {personId} found");
                }

                var interests = person.Interests.Select(interest => new
                {
                    Title = interest.Title,
                }).ToList();

                var json = JsonSerializer.Serialize(interests, options);

                return Results.Ok(interests);
            });

            app.MapGet("/person/{personId:int}/links", async (int personId, ApplicationDbContext context) =>
            {
                var person = await context.Persons
                    .Include(p => p.Links)
                    .FirstOrDefaultAsync(p => p.PersonId == personId);

                if (person == null)
                {
                    return Results.NotFound($"No person with id: {personId} found");
                }

                var links = person.Links.Select(link => new
                {
                    Url = link.Url
                }).ToList();

                var json = JsonSerializer.Serialize(links, options);

                return Results.Ok(links);
            });


            app.Run();
        }
    }
}
