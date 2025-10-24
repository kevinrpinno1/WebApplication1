using Microsoft.AspNetCore.Identity;

namespace Data.Seed
{
    public class DemoIdentityUserSeeder
    {
        public static async Task SeedDemoUsersAsync(UserManager<IdentityUser> userManager)
        {
            var demoEmails = new List<string>
            {
                "CaptainConnor97@oilers.com",
                "McJesus97@oilers.com",
                "GermanGretzky29@oilers.com",
                "TheNugeIsHuge93@oilers.com",
                "BouchBomb2@oilers.com",
                "EkkyAnchors14@oilers.com",
                "NurseOnDuty25@oilers.com",
                "HymanHattrick18@oilers.com",
                "StuSaves74@oilers.com",
                "PickardPocket30@oilers.com",
                "KulakAttack27@oilers.com",
                "WalmanWall96@oilers.com",
                "RicoClutch19@oilers.com",
                "KappyWheels42@oilers.com",
                "JanmarkMyWords13@oilers.com",
                "DraiOneTimer29@oilers.com",
                "ConnorGenerational97@oilers.com",
                "Nuuuuge93@oilers.com",
                "BlueLineBouch2@oilers.com",
                "FortressEkholm14@oilers.com",
                "JamesTKirk@UssEnterprise.co",
                "Spock@Vulcan.science",
                "NyotaUhura@UssEnterprise.co",
                "HikaruSulu@UssExcelsior.space",
                "JeanLucPicard@UssEnterpriseD.fed",
                "WillRiker@UssTitan.space",
                "Data@UssEnterpriseD.ai",
                "Worf@UssDefiant.klingon",
                "BenjaminSisko@DeepSpaceNine.space",
                "KiraNerys@Bajor.mil",
                "JadziaDax@DeepSpaceNine.science",
                "JulianBashir@UssDefiant.med",
                "MilesOBrien@DeepSpaceNine.ops",
                "Quark@FerengiCommerce.biz",
                "KathrynJaneway@UssVoyager.delta",
                "SevenOfNine@UssVoyager.collective",
                "HarryKim@UssVoyager.ops",
                "TomParis@UssVoyager.helm",
                "Chakotay@UssVoyager.maquis",
                "MichaelBurnham@UssDiscovery.mycelium",
                "Saru@UssDiscovery.kelpien",
                "SylviaTilly@UssDiscovery.engineering",
                "ChristopherPike@UssEnterpriseNCC1701.command",
                "BeckettMariner@UssCerritos.lowerdecks",
                "DvanaTendi@UssCerritos.science",
                "BradBoimler@UssCerritos.conn",
                "JonathanArcher@EnterpriseNX01.star",
                "TripTucker@EnterpriseNX01.engineering",
                "DrPhlox@EnterpriseNX01.denobula",
                "Guinan@TenForward.lounge"
            };

            foreach (var email in demoEmails)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new IdentityUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true 
                    };

                    await userManager.CreateAsync(user, "ThisIsADemoPassword1!");
                }
            }
        }
    }
}
