//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources.Core;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Storage;

namespace H4U.VoiceCommands
{
    public sealed class H4UVoiceCommandService : IBackgroundTask
    {
        VoiceCommandServiceConnection voiceServiceConnection;
        BackgroundTaskDeferral serviceDeferral;
        ResourceMap cortanaResourceMap;
        ResourceContext cortanaContext;        
        DateTimeFormatInfo dateFormatInfo;

        HttpClient _Client;
        bool _IsAutorized = false;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _Client = new HttpClient();

            serviceDeferral = taskInstance.GetDeferral();
            
            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            
            cortanaResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");
            
            cortanaContext = ResourceContext.GetForViewIndependentUse();
            
            dateFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;
            
            if (triggerDetails != null && triggerDetails.Name == "H4UVoiceCommandService")
            {
                try
                {
                    voiceServiceConnection =
                        VoiceCommandServiceConnection.FromAppServiceTriggerDetails(
                            triggerDetails);

                    voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();
                    
                    switch (voiceCommand.CommandName)
                    {
                        case "switchLight":
                            var roomSwitchOn = voiceCommand.Properties["room"][0];
                            var valueSet = voiceCommand.Properties["switch"][0];
                            await SendCompletionMessageForSwitchLight(roomSwitchOn, valueSet);
                            break;       
                        case "setTemperature":
                            var roomSetTemperature = voiceCommand.Properties["room"][0];
                            var valueSetTemperature = voiceCommand.Properties["temperature"][0];
                            await SendCompletionMessageForSetTemperature(roomSetTemperature, valueSetTemperature);
                            break; 
                        case "setDimmer":
                            var roomSetDimmer = voiceCommand.Properties["room"][0];
                            var valueSetPercent = voiceCommand.Properties["percent"][0];
                            await SendCompletionMessageForSetDimmer(roomSetDimmer, valueSetPercent);
                            break;
                        default:
                            LaunchAppInForeground();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Handling Voice Command failed " + ex.ToString());
                }
            }
        }

        private async Task<bool> SendCompletionMessageForSwitchLight(string room, string state)
        {
            int sw = 0;
            string endOfResponse = string.Empty;
            string endOfFailedResponse = string.Empty;

            VoiceCommandUserMessage userMessage;
            VoiceCommandResponse response;

            if (string.Compare(state, "ein", true) == 0 ||
                   string.Compare(state, "einschalten", true) == 0 ||
                   string.Compare(state, "an", true) == 0 ||
                   string.Compare(state, "anschalten", true) == 0)
            {
                sw = 100;
                endOfResponse = "eingeschaltet";
                endOfFailedResponse = "einschalten";
            }
            else if (string.Compare(state, "aus", true) == 0 ||
                string.Compare(state, "ausschalten", true) == 0)
            {
                sw = 0;
                endOfResponse = "ausgeschaltet";
                endOfFailedResponse = "ausschalten";
            }

            if(string.IsNullOrEmpty(endOfResponse) || string.IsNullOrEmpty(endOfFailedResponse))
            {
                userMessage = new VoiceCommandUserMessage();
                string unknownCommand = $"Ich kenne das Schaltkommando {state} nicht";
                userMessage.DisplayMessage = userMessage.SpokenMessage = unknownCommand;
                response = VoiceCommandResponse.CreateResponse(userMessage);
                await voiceServiceConnection.ReportSuccessAsync(response);
                return false;
            }
              
            try
            {
                StringContent content = new StringContent(sw.ToString());
                await _Client.PostAsync($"http://openhabianpi:8080/rest/items/ZWaveNode33FGD212Dimmer2_Dimmer", content);
                userMessage = new VoiceCommandUserMessage();
                string switchedLight = $"Ich habe das Licht im {room} {endOfResponse}";
                userMessage.DisplayMessage = userMessage.SpokenMessage = switchedLight;
                response = VoiceCommandResponse.CreateResponse(userMessage);
                await voiceServiceConnection.ReportSuccessAsync(response);
                return true;
            }
            catch (Exception) { }

            userMessage = new VoiceCommandUserMessage();
            string failed = $"Ich konnte das Licht im {room} nicht {endOfFailedResponse}";
            userMessage.DisplayMessage = userMessage.SpokenMessage = failed;
            response = VoiceCommandResponse.CreateResponse(userMessage);
            await voiceServiceConnection.ReportSuccessAsync(response);
            return false;
        }
                
        private async Task<bool> SendCompletionMessageForSetTemperature(string room, string temperature)
        {
            VoiceCommandUserMessage userMessage;
            VoiceCommandResponse response;

            if (Login())
            {
                try
                {
                    await _Client.GetAsync($"http://10.0.0.10:8083/ZWaveAPI/Run/devices[20].instances[0].commandClasses[0x43].Set(1,{temperature})");
                    userMessage = new VoiceCommandUserMessage();
                    string setTemperatureDone = $"Ich habe das Thermostat im {room} auf {temperature} °C gestellt";
                    userMessage.DisplayMessage = userMessage.SpokenMessage = setTemperatureDone;
                    response = VoiceCommandResponse.CreateResponse(userMessage);
                    await voiceServiceConnection.ReportSuccessAsync(response);
                    return true;
                }
                catch (Exception) { }
            }
            userMessage = new VoiceCommandUserMessage();
            string setTemperatureMissed = $"Ich konnte das Thermostat im {room} nicht stellen";
            userMessage.DisplayMessage = userMessage.SpokenMessage = setTemperatureMissed;
            response = VoiceCommandResponse.CreateResponse(userMessage);
            await voiceServiceConnection.ReportSuccessAsync(response);
            return false;
        }

        private async Task<bool> SendCompletionMessageForSetDimmer(string room, string value)
        {
            VoiceCommandUserMessage userMessage;
            VoiceCommandResponse response;

            if (Login())
            {
                try
                {
                    value = string.Compare("ein", value, true) == 0 ? "99" : value;
                    value = string.Compare("aus", value, true) == 0 ? "0" : value;
                    value = Convert.ToInt16(value) >= 100 ? "99" : value;
                    value = Convert.ToInt16(value) < 0 ? "0" : value;

                    await _Client.GetAsync($"http://10.0.0.10:8083/ZWaveAPI/Run/devices[14].instances[0].commandClasses['SwitchMultilevel'].Set({value})");
                    userMessage = new VoiceCommandUserMessage();
                    string setDimmerDone = $"Ich habe das Hintergrundlicht im {room} auf {value} % gedimmt";
                    userMessage.DisplayMessage = userMessage.SpokenMessage = setDimmerDone;
                    response = VoiceCommandResponse.CreateResponse(userMessage);
                    await voiceServiceConnection.ReportSuccessAsync(response);
                    return true;
                }
                catch (Exception) { }
            }
            userMessage = new VoiceCommandUserMessage();
            string setTemperatureMissed = $"Ich konnte das Hintergrundlicht im {room} nicht dimmen";
            userMessage.DisplayMessage = userMessage.SpokenMessage = setTemperatureMissed;
            response = VoiceCommandResponse.CreateResponse(userMessage);
            await voiceServiceConnection.ReportSuccessAsync(response);
            return false;
        }

        private async Task ShowProgressScreen(string message)
        {
            var userProgressMessage = new VoiceCommandUserMessage();
            userProgressMessage.DisplayMessage = userProgressMessage.SpokenMessage = message;

            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userProgressMessage);
            await voiceServiceConnection.ReportProgressAsync(response);
        }
                         
        private async void LaunchAppInForeground()
        {
            var userMessage = new VoiceCommandUserMessage();
            userMessage.SpokenMessage = cortanaResourceMap.GetValue("LaunchingH4U", cortanaContext).ValueAsString;

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            response.AppLaunchArgument = "";

            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }

        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }
                
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Task cancelled, clean up");
            if (this.serviceDeferral != null)
            {
                //Complete the service deferral
                this.serviceDeferral.Complete();
            }
        }

        private bool Login()
        {
            if (!_IsAutorized)
            {
                string payload = @"{""login"":""admin"", ""password"":""somepwd""}";
                var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                try
                {
                    var response = _Client.PostAsync("http://10.0.0.10:8083/ZAutomation/api/v1/login", httpContent).Result;

                    if (response.IsSuccessStatusCode)
                        _IsAutorized = true;
                }
                catch (Exception ex)
                {
                    _IsAutorized = false;
                }
            }
            return _IsAutorized;
        }
    }
}
