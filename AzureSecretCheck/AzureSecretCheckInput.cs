//https://github.com/janpieterz/seq-input-certificatecheck/blob/main/Seq.Input.CertificateCheck/Seq.Input.CertificateCheck.csproj
//https://github.com/datalust/seq-app-httprequest/blob/dev/Build.ps1
using System;
using System.Threading.Tasks;
using Microsoft.Graph;
using Serilog.Events;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Seq.Apps;


namespace SEQ.App.AzureSecretCheck
{

     [SeqApp("AzureSecretCheck", Description = "Check Azure Application Registrations Secrets and Certificates expirations")]
     public class AzureSecretCheckInput : SeqApp, IPublishJson, IDisposable
     {

          #region Class Properties
          /// <summary>
          /// 
          /// </summary>
          /// <returns></returns>
          private Settings _settings = null;

          /// <summary>
          /// List of Running Tasks to check Expiry
          /// </summary>
          private readonly List<AzureSecretCheckTask> _azureSecretCheckTasks = new List<AzureSecretCheckTask>();
          //readonly AzureAppRegistrationSecretCheckClient _client;
          #endregion
          #region Constructors
          public AzureSecretCheckInput()
          {

          }

          #endregion
          #region Plugin Inputs   
          /// <summary>
          /// 
          /// </summary>
          [SeqAppSetting(DisplayName = "Azure Tenant Id",
               Syntax = "code",
               IsOptional = false,
               InputType = SettingInputType.Password,
               HelpText = "The Tenant ID of your Azure Instance")]
          public string? TenantId { get; set; }
          /// <summary>
          /// 
          /// </summary>
          [SeqAppSetting(DisplayName = "Azure Client Id",
               Syntax = "code",
               IsOptional = false,
               InputType = SettingInputType.Password,
               HelpText = "The Client ID of this App Registration, that has the necessary access to query expiry.")]
          public string? ClientId { get; set; }
          /// <summary>
          /// 
          /// </summary>
          [SeqAppSetting(DisplayName = "Client Secret ",
               Syntax = "code",
               IsOptional = false,
               InputType = SettingInputType.Password,
               HelpText = "The Client Secret for App Authentication.")]
          public string? ClientSecret { get; set; }
          /// <summary>
          /// 
          /// </summary>
          [SeqAppSetting(DisplayName = "Graph Scopes",
               Syntax = "code",
               InputType = SettingInputType.LongText,
               IsOptional = true,
               HelpText = "Scopes for Graph API; enter one per line.")]
          public string? GraphScopes { get; set; }
          /// <summary>
          /// 
          /// </summary>
          [SeqAppSetting(DisplayName = "Azure App Registration Object Ids",
               Syntax = "code",
               IsOptional = false,
               InputType = SettingInputType.LongText,
               HelpText = "The App Registrations to check; enter one per line.")]
          public string? AppObjectIds { get; set; }
          /// <summary>
          /// 
          /// </summary>
          [SeqAppSetting(
               DisplayName = "Interval (seconds)",
               IsOptional = true,
               HelpText = "The time between checks; the default is 3600 (once an hour).")]
          public int IntervalSeconds { get; set; } = 3600;
          /// <summary>
          /// 
          /// </summary>
          [SeqAppSetting(DisplayName = "Minimum validity period (days)", IsOptional = true,
              HelpText = "The minimum amount of days a certificate should be valid; the default is 30")]
          public int ValidityDays { get; set; } = 30;
          #endregion

          /// <summary>
          /// 
          /// </summary>
          /// <param name="inputWriter"></param>
          public void Start(TextWriter inputWriter)
          {
               //If the GraphScopes is null then put in an empty string to prevent issues with the split function.
               if (GraphScopes == null){
                    GraphScopes = string.Empty;
               }
               try{
               _settings = new Settings();
               _settings.ClientId = ClientId;
               _settings.TenantId = TenantId;
               _settings.ClientSecret = ClientSecret;
               _settings.GraphScopes = GraphScopes.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
               }catch(Exception ex){
                   Console.WriteLine(ex.Message);//, "The Start Task threw an unhandled exception");
               }
               // Connect to Graph.
               // https://learn.microsoft.com/en-us/graph/tutorials/dotnet?tabs=aad&tutorial-step=3
               // Microsoft.Graph.Settings settings = new Microsoft.Graph.Settings();
               //GraphHelper.InitializeGraphForUserAuth(_settings);

               var reporter = new AzureSecretCheckReporter(inputWriter);
               var appObjectIds = AppObjectIds.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
               foreach (var appObjectId in appObjectIds)
               {
                    var healthCheck = new AzureSecretValidityCheck(App.Title, appObjectId, ValidityDays, _settings);
                    _azureSecretCheckTasks.Add(new AzureSecretCheckTask(healthCheck,
                         appObjectId,
                         TimeSpan.FromSeconds(IntervalSeconds),
                         reporter,
                         Log
                    ));

               }

          }

          public void Stop()
          {
               foreach (var task in _azureSecretCheckTasks)
               {
                    task.Stop();
               }
          }

          public void Dispose()
          {
               foreach (var task in _azureSecretCheckTasks)
               {
                    task.Dispose();
               }
          }


     }
}