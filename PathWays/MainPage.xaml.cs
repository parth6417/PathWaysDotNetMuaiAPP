using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Newtonsoft.Json;
using PathWays.Const;
using PathWays.Model;
using PathWays.Model.APIRespone;
using PathWaysVMS.API;
using Plugin.Maui.Audio;
using System.Timers;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

using Camera.MAUI;

namespace PathWays
{
    public partial class MainPage : ContentPage
    {
        private readonly IAudioManager audioManager;
        System.Timers.Timer timer = new System.Timers.Timer();
        bool processIsRunningForImport = false;

        public MainPage(IAudioManager audioManager)
        {
            InitializeComponent();
            this.audioManager = audioManager;
            //timer.Elapsed += new ElapsedEventHandler(OnElapsedTimeTLAndPE);
            //timer.Interval = 2000;
            //timer.Enabled = true;
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            cameraView.Camera = cameraView.Cameras.FirstOrDefault(i => i.Position == Camera.MAUI.CameraPosition.Front);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await cameraView.StopCameraAsync();
                await cameraView.StartCameraAsync();
                currentPosition = Camera.MAUI.CameraPosition.Front;
            });


        }

        private CameraPosition currentPosition = Camera.MAUI.CameraPosition.Front;
        private async void ChangePosition(object sender, EventArgs e)
        {
            if (!processIsRunningForImport)
            {
                if (currentPosition == Camera.MAUI.CameraPosition.Back)
                {
                    cameraView.Camera = cameraView.Cameras.FirstOrDefault(i => i.Position == Camera.MAUI.CameraPosition.Front);
                    currentPosition = Camera.MAUI.CameraPosition.Front;
                }
                else
                {
                    cameraView.Camera = cameraView.Cameras.FirstOrDefault(i => i.Position == Camera.MAUI.CameraPosition.Back);
                    currentPosition = Camera.MAUI.CameraPosition.Back;
                }
                await cameraView.StopCameraAsync();
                await cameraView.StartCameraAsync();
            }
        }

        private async void OnElapsedTimeTLAndPE(object source, EventArgs e)
        {
            if (processIsRunningForImport) { return; }
            processIsRunningForImport = true;
            try
            {
                timer.Stop();
                timer.Enabled = false;
                var image = cameraView.GetSnapShot(Camera.MAUI.ImageFormat.PNG);
                var bytes = await ConvertBackTo(image);
                if (bytes != null)
                {
                    var rekognitionClient = new AmazonRekognitionClient(ConstantVariables.awsAccessKeyId, ConstantVariables.awsSecretAccessKey, ConstantVariables.region);
                    var searchFacesRequest = new SearchFacesByImageRequest
                    {
                        CollectionId = ConstantVariables.CollectionId,
                        Image = new Amazon.Rekognition.Model.Image
                        {
                            Bytes = bytes
                        }
                    };
                    var response = await rekognitionClient.SearchFacesByImageAsync(searchFacesRequest);
                    var data = JsonConvert.SerializeObject(response.FaceMatches);
                    var postResponse = await ApiAsync.CallApi("PersonImage/GetPersonUsingFacePrint", HttpMethod.Post, data);
                    PersonUsingFacePrint ScanUser = JsonConvert.DeserializeObject<PersonUsingFacePrint>(postResponse.Item1);

                    if (ScanUser?.Data?.Person_Id != null)
                    {
                        var getResponse = await ApiAsync.CallApi("PersonLoginRecord/GetClockInPersonById/" + ScanUser.Data.Person_Id, HttpMethod.Get);

                        if (getResponse.Item2)
                        {
                            GetClockInPerson<PersonLoginRecord> apiResponse = JsonConvert.DeserializeObject<GetClockInPerson<PersonLoginRecord>>(getResponse.Item1);
                            if (apiResponse.Data != null)
                            {
                                if (apiResponse.Data?.OutTime == null)
                                {
                                    var SaveResponse = await UpdateData(apiResponse?.Data);
                                    if (SaveResponse)
                                    {
                                        MainThread.BeginInvokeOnMainThread(async () =>
                                        {
                                            var tost = Toast.Make("Log Out successfully", ToastDuration.Long);
                                            await tost.Show();
                                        });
                                        var player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("LogOut.mp3"));
                                        player.Play();

                                        timer.Start();
                                        timer.Enabled = true;
                                    }
                                    else
                                        await DisplayAlert("Error", "Having somithing issue..", "OK");
                                }
                            }
                            else
                            {
                                var SaveResponse = await SaveData(ScanUser.Data.Person_Id);
                                if (SaveResponse)
                                {
                                    //MainThread.BeginInvokeOnMainThread(async () =>
                                    //{
                                    timer.Stop();
                                    timer.Enabled = false;
                                    MainThread.BeginInvokeOnMainThread(async () =>
                                    {
                                        var tost = Toast.Make("Log in successfully", ToastDuration.Long);
                                        await tost.Show();
                                    });
                                    var player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("LOGIN.mp3"));
                                    player.Play();
                                    await cameraView.StopCameraAsync();

                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        App.Current.MainPage = new LogInUserDetails(ScanUser.Data, this.audioManager);
                                    });
                                    //closebutton.IsVisible = true;
                                    //cameraButton.IsVisible = true;
                                    //cameraView.IsVisible = true;
                                    //timer.Start();
                                    //timer.Enabled = true;
                                    //});

                                }
                                else
                                    await DisplayAlert("Error", "Having somithing issue..", "OK");
                            }
                        }
                    }
                    else
                    {
                        await cameraView.StopCameraAsync();
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            App.Current.MainPage = new Register(bytes, response.FaceMatches, this.audioManager);
                        });

                    }
                }
                processIsRunningForImport = false;
            }
            catch (AmazonRekognitionException ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var tost = Toast.Make("AmazonRekognitionException" + ex.Message, ToastDuration.Long);
                    await tost.Show();
                });
                timer.Start();
                timer.Enabled = true;
                processIsRunningForImport = false;
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var tost = Toast.Make("Exception" + ex.Message, ToastDuration.Long);
                    await tost.Show();
                });
                timer.Start();
                timer.Enabled = true;
                processIsRunningForImport = false;
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Enabled = false;
            var loder = new Loader();
            this.ShowPopup(loder);

            cameraButton.IsEnabled = false;
            var image = cameraView.GetSnapShot(Camera.MAUI.ImageFormat.PNG);
            var bytes = await ConvertBackTo(image);

            if (bytes != null)
            {
                try
                {
                    var rekognitionClient = new AmazonRekognitionClient(ConstantVariables.awsAccessKeyId, ConstantVariables.awsSecretAccessKey, ConstantVariables.region);
                    var searchFacesRequest = new SearchFacesByImageRequest
                    {
                        CollectionId = ConstantVariables.CollectionId,
                        Image = new Amazon.Rekognition.Model.Image
                        {
                            Bytes = bytes
                        }
                    };
                    var response = await rekognitionClient.SearchFacesByImageAsync(searchFacesRequest);
                    var face = Newtonsoft.Json.JsonConvert.SerializeObject(response.FaceMatches);
                    var postResponse = await ApiAsync.CallApi("PersonImage/GetPersonUsingFacePrint", HttpMethod.Post, face);
                    PersonUsingFacePrint ScanUser = JsonConvert.DeserializeObject<PersonUsingFacePrint>(postResponse.Item1);

                    #region FaceMatch

                    if (ScanUser?.Data?.Person_Id != null)
                    {
                        var getResponse = await ApiAsync.CallApi("PersonLoginRecord/GetClockInPersonById/" + ScanUser.Data.Person_Id, HttpMethod.Get);
                        if (getResponse.Item2)
                        {
                            GetClockInPerson<PersonLoginRecord> apiResponse = JsonConvert.DeserializeObject<GetClockInPerson<PersonLoginRecord>>(getResponse.Item1);
                            if (apiResponse.Data != null)
                            {
                                if (apiResponse.Data?.OutTime == null)
                                {
                                    if (Convert.ToDateTime(apiResponse.Data.InTime).AddSeconds(30) < DateTime.UtcNow)
                                    {
                                        var SaveResponse = await UpdateData(apiResponse?.Data);
                                        if (SaveResponse)
                                        {
                                            var tost = Toast.Make("Log Out successfully", ToastDuration.Long);
                                            await tost.Show();
                                            //var player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("LogOut.mp3"));
                                            //player.Play();
                                        }
                                        else
                                            await DisplayAlert("Error", "Having somithing issue..", "OK");
                                    }
                                    else
                                    {
                                        var tost = Toast.Make("Duplicate Punch", ToastDuration.Long);
                                        await tost.Show();
                                        //var player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("duplicatePunch.mp3"));
                                        //player.Play();
                                    }
                                }
                            }
                            else
                            {
                                var SaveResponse = await SaveData(ScanUser.Data.Person_Id);
                                if (SaveResponse)
                                {
                                    var tost = Toast.Make("Log in successfully", ToastDuration.Long);
                                    await tost.Show();
                                    //var player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("LOGIN.mp3"));
                                    //player.Play();
                                    await cameraView.StopCameraAsync();
                                    App.Current.MainPage = new LogInUserDetails(ScanUser.Data, this.audioManager);
                                }
                                else
                                    await DisplayAlert("Error", "Having somithing issue..", "OK");
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        await cameraView.StopCameraAsync();
                        App.Current.MainPage = new Register(bytes, response.FaceMatches, this.audioManager);
                    }
                }
                catch (Exception ex)
                {
                    var tost = Toast.Make(ex.Message, ToastDuration.Long);
                    await tost.Show();
                }
            }
            timer.Start();
            timer.Enabled = true;
            cameraButton.IsEnabled = true;
            loder.Close();
        }



        private async void Close_Clicked(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Enabled = false;
            cameraView.Camera = cameraView.Cameras.FirstOrDefault(i => i.Position == Camera.MAUI.CameraPosition.Front);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await cameraView.StopCameraAsync();
            });
            App.Current.MainPage = new NavigationPage(new Home(this.audioManager));
        }

        public async Task<bool> SaveData(string id)
        {
            try
            {
                PersonLoginRecord personLoginRecord = new PersonLoginRecord();
                personLoginRecord.InTime = DateTime.Now;
                personLoginRecord.Person_Id = id;
                personLoginRecord.VisitorPersonId = id;
                personLoginRecord.OutTime = null;
                string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(personLoginRecord);
                var postResponse = await ApiAsync.CallApi("PersonLoginRecord", HttpMethod.Post, jsonBody);
                if (postResponse.Item2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateData(PersonLoginRecord Data)
        {
            try
            {
                PersonLoginRecord personLoginRecord = new PersonLoginRecord();
                personLoginRecord.Id = Data.Id;
                personLoginRecord.InTime = Data.InTime;
                personLoginRecord.Person_Id = Data?.Person_Id;
                personLoginRecord.VisitorPersonId = Data?.Person_Id;
                personLoginRecord.OutTime = DateTime.Now;
                string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(personLoginRecord);
                var postResponse = await ApiAsync.CallApi("PersonLoginRecord", HttpMethod.Put, jsonBody);
                if (postResponse.Item2)
                    return true;
                else
                    return false;
            }
            catch
            (Exception ex)
            {
                return false;
            }
        }

        public async Task<MemoryStream> ConvertBackTo(ImageSource value)
        {
            if (value is null)
            {
                Console.WriteLine("ImageSource is null.");
                return null;
            }

            if (!(value is StreamImageSource streamImageSource))
            {
                Console.WriteLine("Expected value to be of type StreamImageSource.");
                return null;
            }

            try
            {
                var streamFromImageSource = await streamImageSource.Stream(CancellationToken.None);

                if (streamFromImageSource is null)
                {
                    Console.WriteLine("Stream from ImageSource is null.");
                    return null;
                }

                var memoryStream = new MemoryStream();
                await streamFromImageSource.CopyToAsync(memoryStream);
                return memoryStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting ImageSource to MemoryStream: {ex.Message}");
                return null;
            }
        }
    }

}
