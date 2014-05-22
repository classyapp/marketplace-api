using Classy.Models;
using Funq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Classy.UtilRunner.Utilities.ExportProfiles
{
    public class ExportProfiles : IUtility
    {
        private readonly MongoCollection<Profile> _profiles;

        public ExportProfiles(Container container)
        {
            var mongoDatabase = container.Resolve<MongoDatabase>();
            _profiles = mongoDatabase.GetCollection<Profile>("profiles");
        }

        public StatusCode Run(string[] args)
        {
            if (args == null || !args.Any()) return StatusCode.MissingArguments;

            var startTime = DateTime.Now;

            using (var stream = File.OpenWrite("C:\\profiles.csv"))
            {
                int i = 0, count = 0;
                var profiles = _profiles.Find(Query<Profile>.Where(x => x.AppId == args[0] && x.ProfessionalInfo != null && !x.ProfessionalInfo.IsProxy));
                foreach (var profile in profiles)
                {
                    i++;

                    if (string.IsNullOrEmpty(profile.ProfessionalInfo.CompanyName) ||
                        string.IsNullOrEmpty(profile.ProfessionalInfo.CompanyContactInfo.Email) ||
                        profile.ProfessionalInfo.CompanyContactInfo == null ||
                        profile.ProfessionalInfo.CompanyContactInfo.Location == null)
                        continue;

                    count++;

                    var buffer = Encoding.UTF8.GetBytes(profile.Id + "," + 
                        profile.ProfessionalInfo.CompanyName + "," + 
                        profile.ProfessionalInfo.CompanyContactInfo.Email + "," + 
                        profile.ProfessionalInfo.CompanyContactInfo.Location.Address.Country +
                        Environment.NewLine);
                    stream.Write(buffer, 0, buffer.Length);

                    if (i % 100 == 0)
                        Console.WriteLine("Scanned " + i + " profiles in " + (DateTime.Now - startTime).TotalSeconds + " sec");
                }
                Console.WriteLine("Found " + count);
            }

            return StatusCode.Success;
        }
    }
}
