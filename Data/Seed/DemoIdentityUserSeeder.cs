using Microsoft.AspNetCore.Identity;
using System.Reflection;
using System.Text.Json;
using WebApplication1.Configuration;

namespace Data.Seed
{
    public class DemoIdentityUserSeeder()
    {
        private sealed class EmailSeed
        {
            public List<string> TrekkieEmails { get; set; } = new();
            public List<string> OilersEmails { get; set; } = new();
        }

        // seeding demo users from the emails.json file here 
        // reading the file into the two lists of emails, then creating users with the desired roles and passwords
        public static async Task SeedDemoUsersAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, Constants _constants)
        {
            await SeedRolesAsync(roleManager);

            var path = Path.Combine(AppContext.BaseDirectory, _constants.EmailsFilePath);
            if (!File.Exists(path)) return;

            var emailSeedJson = await File.ReadAllTextAsync(path);
            var emailData = JsonSerializer.Deserialize<EmailSeed>(emailSeedJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (emailData == null) return;

            await SeedUserGroupAsync(userManager, 
                    emailData.OilersEmails.Select(e => e.Trim()).ToList(), 
                    RoleConstants.OilerPlayer, 
                    _constants.OilerPlayerPassword);

            // I hadn't yet used the new C# 12 collection expression syntax before, so gave it a try here
            await SeedUserGroupAsync(userManager,
                    [.. emailData.TrekkieEmails.Select(e => e.Trim())],
                    RoleConstants.Trekkie, 
                    _constants.TrekkiePassword);
        }

        // helper method used to seed a group of users with the same role and password
        // reduces code duplication, allows for easy addition of more user groups
        // like pesky marvel fans for instance :)
        private static async Task SeedUserGroupAsync(UserManager<IdentityUser> userManager, List<string> emails, string roleName, string password)
        {
            foreach (var email in emails)
            {
                if (await userManager.FindByEmailAsync(email) == null) // does the user exist already?
                {
                    // create the user if not

                    var user = new IdentityUser 
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, password);

                    if (result.Succeeded) await userManager.AddToRoleAsync(user, roleName); // add the user role 
                }
            }

            await Task.CompletedTask;
        }

        // checking to make sure the desired roles exist in the db when the app starts up
        // ensures no duplicates are created if it's already there
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // reading all the roles from RoleConstants to avoid hardcoding them here
            var roles = typeof(RoleConstants)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) // return only public static fields, no nested values
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string)) // only constant string field types, written at compile time 
                .Select(fi => fi.GetRawConstantValue() as string) // want that value as a string
                .Where(r => !string.IsNullOrEmpty(r)) // filter out nulls
                .ToList();

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role!)) // does the role exist?
                {
                    await roleManager.CreateAsync(new IdentityRole(role!)); // create the role if not
                }
            }
        }
    }
}
