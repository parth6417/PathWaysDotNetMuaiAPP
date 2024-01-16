using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Model;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Newtonsoft.Json;
using PathWays.Const;
using PathWays.Model;
using PathWays.Model.APIRespone;
using PathWaysVMS.API;
using Plugin.Maui.Audio;

namespace PathWays;

public partial class Register : ContentPage
{
    public MemoryStream bytes;

    List<FaceMatch> facePrint = new List<FaceMatch>();

    public IAudioManager audioManager;
    public Register(MemoryStream bytes, List<FaceMatch> FP, IAudioManager audioManager)
    {
        this.facePrint = FP;
        InitializeComponent();
        this.bytes = bytes;
        MemoryStream memoryStream = bytes;
        memoryStream.Position = 0;
        ImageSource imageSource = ImageSource.FromStream(() => memoryStream);
        UploadedOrSelectedImage.Source = imageSource;
        //BindingContext = new Person();

        //var dataList = new List<string> { "Item 1", "Item 2", "Item 3" };

        //// Dynamically create labels and add them to the StackLayout
        //foreach (var item in facePrint)
        //{
        //    var label = new Microsoft.Maui.Controls.Label { Text = item.Face.FaceId, FontSize = 18 };
        //    labelStackLayout.Children.Add(label);
        //}
        //labelStackLayout.IsVisible = false;

    }

    #region Validation
    public bool validationTheField(bool isvalid = false)
    {
        //var viewModel = (Person)BindingContext;

        if (First_Name.Text == string.Empty || First_Name.Text == null)
        {
            validFName.IsVisible = true;
            isvalid = true;
        }

        if (Middle_Name.Text == string.Empty || First_Name.Text == null)
        {
            Middle_Name.Text = "";
        }

        if (Last_Name.Text == string.Empty || Last_Name.Text == null)
        {
            validLName.IsVisible = true;
            isvalid = true;
        }

        if (Gender.SelectedItem == null)
        {
            validGender.IsVisible = true;
            isvalid = true;
        }
        if (Email_1.Text == string.Empty || Email_1.Text == null)
        {
            validEmail.IsVisible = true;
            isvalid = true;
        }
        if (Mobile_Phone.Text == string.Empty || Mobile_Phone.Text == null)
        {
            validphone.IsVisible = true;
            isvalid = true;
        }
        if (Address_1.Text == string.Empty || Address_1.Text == null)
        {
            validAddress_1.IsVisible = true;
            isvalid = true;
        }
        if (Postal_Code.Text == string.Empty || Postal_Code.Text == null)
        {
            validpostal.IsVisible = true;
            isvalid = true;
        }
        return isvalid;
    }
    private void firstname_textchage(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue != string.Empty)
            validFName.IsVisible = false;
        else
            validFName.IsVisible = true;
    }


    private void onselectGender(object sender, EventArgs e)
    {
        var selectedPickerItem = (string)((Picker)sender).SelectedItem;

        if (selectedPickerItem != string.Empty)
            validGender.IsVisible = false;
        else
            validGender.IsVisible = true;
    }

    //private void middlename_textchage(object sender, TextChangedEventArgs e)
    //{
    //    if (e.NewTextValue != string.Empty)
    //        validMName.IsVisible = false;
    //    else
    //        validMName.IsVisible = true;
    //}

    private void Lastname_textchage(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue != string.Empty)
            validLName.IsVisible = false;
        else
            validLName.IsVisible = true;
    }

    private void Email_textchage(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue != string.Empty)
        {
            validformate.IsVisible = !IsValidEmail(e.NewTextValue);
            validEmail.IsVisible = false;
        }
        else
        {
            validEmail.IsVisible = true;
            validformate.IsVisible = false;
        }
    }

    bool IsValidEmail(string email)
    {
        try
        {
            var mail = new System.Net.Mail.MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }


    private void Mobile_textchage(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue != string.Empty)
            validphone.IsVisible = false;
        else
            validphone.IsVisible = true;
    }

    private void Address_textchage(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue != string.Empty)
            validAddress_1.IsVisible = false;
        else
            validAddress_1.IsVisible = true;
    }

    private void Postal_textchage(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue != string.Empty)
            validpostal.IsVisible = false;
        else
            validpostal.IsVisible = true;
    }
    #endregion


    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        bool isvalid = this.validationTheField(false);
        if (!isvalid)
        {
            await CallApiAndPassValues();
            var tost = Toast.Make("Register Successfully", ToastDuration.Long);
            await tost.Show();

            //var player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("onlyregisetr.mp3"));
            //player.Play();

        }
    }

    private async Task CallApiAndPassValues()
    {
        try
        {

            Person person = new Person()
            {
                First_Name = First_Name.Text,
                Middle_Name = Middle_Name.Text,
                Last_Name = Last_Name.Text,
                Email_1 = Email_1.Text,
                Mobile_Phone = Mobile_Phone.Text,
                Gender = Gender.SelectedItem.ToString(),
                Address_1 = Address_1.Text,
                Postal_Code = Postal_Code.Text,
                TypeId=3
            };
            btnclose.IsEnabled = false;
            btnRegister.IsEnabled = false;
            btnRegister.Text = "Wait....";
            using (HttpClient client = new HttpClient())
            {
                string jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(person);
                var postResponse = await ApiAsync.CallApi("Person", HttpMethod.Post, jsonBody);
                if (postResponse.Item2)
                {
                    GetClockInPerson<PersonInsertResponse> apiResponse = JsonConvert.DeserializeObject<GetClockInPerson<PersonInsertResponse>>(postResponse.Item1);
                    var upload = await uploadImageToS3(apiResponse.Data.InsertId);
                    if (upload)
                    {
                        await UploadFacePrint(apiResponse.Data.InsertId);
                        UploadedOrSelectedImage.Source = "";
                    }
                }
                App.Current.MainPage = new MainPage(this.audioManager);

            }

        }
        catch (Exception ex)
        {
            App.Current.MainPage = new MainPage(this.audioManager);

        }
        finally
        {
            btnRegister.Text = "Register";
            btnclose.IsEnabled = true;
            btnRegister.IsEnabled = true;
        }
    }


    public async Task UploadFacePrint(string Person_Id)
    {
        try
        {
            var FacePrint = Newtonsoft.Json.JsonConvert.SerializeObject(facePrint);
            await ApiAsync.CallApi("PersonImage/AddFacePrint?person_id=" + Person_Id, HttpMethod.Post, FacePrint);
        }
        catch (Exception ex)
        {

        }
    }

    public async Task<bool> uploadImageToS3(string Person_Id)
    {
        try
        {
            IAmazonS3 client = new AmazonS3Client(ConstantVariables.awsAccessKeyId, ConstantVariables.awsSecretAccessKey, ConstantVariables.region);
            //FileInfo files = new FileInfo(savefilepath);
            string destPath = "index/" + Person_Id + ".JPG";

            MemoryStream memoryStream = bytes;
            byte[] byteArray = memoryStream.ToArray();
            Stream newStream = new MemoryStream(byteArray);

            PutObjectRequest request = new PutObjectRequest()
            {
                InputStream = newStream,
                BucketName = ConstantVariables.BacketName,
                Key = destPath,
            };
            request.Metadata.Add(ConstantVariables.TableFieldName2, Person_Id);
            PutObjectResponse response = await client.PutObjectAsync(request);

            return true;
        }
        catch (AmazonS3Exception ex)
        {
            return false;
        }

    }


    private async Task<byte[]> ImageSourceToByteArray(ImageSource imageSource)
    {
        StreamImageSource streamImageSource = (StreamImageSource)imageSource;

        var stream = await streamImageSource.Stream(CancellationToken.None);
        using (var memoryStream = new MemoryStream())
        {
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
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

            using var memoryStream = new MemoryStream();

            // Copy the content of the original stream to a byte array
            byte[] buffer = new byte[4096];
            int bytesRead;

            while ((bytesRead = await streamFromImageSource.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead);
            }

            // Reset the position to the beginning of the new memory stream
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting ImageSource to MemoryStream: {ex.Message}");
            return null;
        }
    }
    private void NoButton_Clicked(object sender, EventArgs e)
    {
        App.Current.MainPage = new NavigationPage(new MainPage(this.audioManager));

    }
}