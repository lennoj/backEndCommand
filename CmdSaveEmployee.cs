using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace BackendChallenge.Function
{
    public static class CmdSaveEmployee
    {
		public sealed class EmployeeModel
		{
			public string id { get; set; }
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public int BirthdayInEpoch { get; set; }
			public string Email { get; set; }

			public EmployeeModel(string FirstName, string LastName, int BirthdayInEpoch, string Email)
			{
				id = Guid.NewGuid().ToString();
				this.FirstName = FirstName;
				this.LastName = LastName;
				this.BirthdayInEpoch = BirthdayInEpoch;
				this.Email = Email;
			}
		}

		[FunctionName("CmdSaveEmployee")]
        public static async Task<IActionResult> Run(
		    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
			HttpRequest req,
			[CosmosDB(
				databaseName: "TestDB",
				collectionName: "Employee",
				ConnectionStringSetting = "CosmosDbConnectionString"
			)]
			IAsyncCollector<dynamic> outputDocuments,
			ILogger log)
		{
			log.LogInformation("CmdSaveEmployee request.");

			EmployeeModel record = null;
			string id;
			string firstName;
			string lastName;
			int birthDatInEpoch;
			string email;

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			dynamic data = JsonConvert.DeserializeObject(requestBody);
			id = data?.id ?? null;
			firstName = data?.FirstName ?? null;
			lastName = data?.LastName ?? null;
			birthDatInEpoch = data?.BirthDayInEpoch ?? 0;
			email = data?.Email ?? null;

			if (string.IsNullOrEmpty(id))
			{
				record = new EmployeeModel(firstName, lastName, birthDatInEpoch, email);
				await outputDocuments.AddAsync(record);
			}

			dynamic result = new
			{
				message = string.IsNullOrEmpty(id) ? "new employee record created" : "employee record already exist",
				result = record
			};

			string responseMessage = JsonConvert.SerializeObject(result);

			return new OkObjectResult(responseMessage);
		}
    }
}
